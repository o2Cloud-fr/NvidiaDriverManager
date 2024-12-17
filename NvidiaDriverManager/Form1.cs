using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;
using Microsoft.Win32;

namespace NvidiaDriverManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ShowStartupMessage();
            LoadNvidiaDriverInfo();
            ProcessArguments(Environment.GetCommandLineArgs());

            this.FormBorderStyle = FormBorderStyle.FixedDialog;  // Empêche le redimensionnement
            this.MaximizeBox = false;  // Désactive le bouton de maximisation
            this.MinimizeBox = false;  // Désactive le bouton de minimisation (optionnel)
        }

        private void ShowStartupMessage()
        {
            // Vérifie si l'application est en mode sans échec
            if (!IsSafeMode())
            {
                MessageBox.Show(
                    "NvidiaDriverManager a détecté que vous n'êtes PAS en mode sans échec...\n" +
                    "Pour un nettoyage sans erreur, il est recommandé de redémarrer en mode sans échec.",
                    "Attention",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private bool IsSafeMode()
        {
            // Vérifie si l'application est en mode sans échec en vérifiant une clé du registre.
            string bootMode = Environment.GetEnvironmentVariable("SAFEBOOT");

            // Si "SAFEBOOT" est défini dans l'environnement, nous sommes en mode sans échec.
            return !string.IsNullOrEmpty(bootMode);
        }

        private void LoadNvidiaDriverInfo()
        {
            try
            {
                string finalVersion = "";

                // Tentative 1: WMI
                var nvidiaDriverVersions = GetNvidiaDriverVersions();
                if (nvidiaDriverVersions.Count > 0)
                {
                    foreach (var version in nvidiaDriverVersions)
                    {
                        lstDriverVersions.Items.Add(version);
                        if (string.IsNullOrEmpty(finalVersion))
                        {
                            finalVersion = version;
                        }
                    }
                }
                else
                {
                    lstDriverVersions.Items.Add("No NVIDIA drivers found via WMI.");
                }

                // Tentative 2: nvidia-smi
                try
                {
                    string nvidiaSmiOutput = ExecuteNvidiaSmiCommand();
                    if (!string.IsNullOrEmpty(nvidiaSmiOutput))
                    {
                        if (string.IsNullOrEmpty(finalVersion))
                        {
                            finalVersion = $"Version (nvidia-smi): {nvidiaSmiOutput}";
                        }
                    }
                }
                catch
                {
                    // Continue si nvidia-smi échoue
                }

                // Tentative 3: Registre
                try
                {
                    string registryDriverVersion = GetNvidiaDriverVersionFromRegistry();
                    if (!string.IsNullOrEmpty(registryDriverVersion))
                    {
                        if (string.IsNullOrEmpty(finalVersion))
                        {
                            finalVersion = $"Version (Registry): {registryDriverVersion}";
                        }
                    }
                }
                catch
                {
                    // Continue si la lecture du registre échoue
                }

                // Extraire et afficher uniquement les 6 derniers chiffres de la version
                if (!string.IsNullOrEmpty(finalVersion))
                {
                    string versionToDisplay = finalVersion.Length >= 6 ? finalVersion.Substring(finalVersion.Length - 6) : finalVersion;
                    lblDriverVersion.Text = $"NVIDIA Driver {versionToDisplay}";
                }
                else
                {
                    lblDriverVersion.Text = "Aucune version de pilote NVIDIA trouvée.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading NVIDIA driver info: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> GetNvidiaDriverVersions()
        {
            var driverVersions = new List<string>();
            string query = "SELECT DriverVersion, DeviceName FROM Win32_PnPSignedDriver WHERE DeviceName LIKE '%NVIDIA GeForce%'";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject obj in searcher.Get())
            {
                string version = obj["DriverVersion"]?.ToString();
                string deviceName = obj["DeviceName"]?.ToString();

                if (!string.IsNullOrEmpty(version) && !string.IsNullOrEmpty(deviceName))
                {
                    driverVersions.Add($"Device: {deviceName}, Version: {version}");
                }
            }

            return driverVersions;
        }

        private string ExecuteNvidiaSmiCommand()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c nvidia-smi --query-gpu=driver_version --format=csv,noheader",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                    {
                        return output.Trim();
                    }
                    else
                    {
                        throw new Exception($"nvidia-smi error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute nvidia-smi: {ex.Message}");
            }
        }

        private string GetNvidiaDriverVersionFromRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NVIDIA Corporation\Installer2\Drivers"))
                {
                    if (key != null)
                    {
                        foreach (string subKeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                            {
                                if (subKey != null)
                                {
                                    object driverVersion = subKey.GetValue("Display.Driver");
                                    if (driverVersion != null)
                                    {
                                        return driverVersion.ToString();
                                    }
                                }
                            }
                        }
                        throw new Exception("Driver version not found in the registry.");
                    }
                    else
                    {
                        throw new Exception("NVIDIA registry key not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get NVIDIA driver version from registry: {ex.Message}");
            }
        }

        private void DisplayDriverVersion()
        {
            try
            {
                string driverVersion = GetNvidiaDriverVersionFromRegistry();
                lblDriverVersion.Text = $"NVIDIA Driver Version: {driverVersion}";
            }
            catch (Exception ex)
            {
                lblDriverVersion.Text = $"Error: {ex.Message}";
            }
        }

        // Méthode pour analyser les arguments de ligne de commande
        private void ProcessArguments(string[] args)
        {
            // Vérifier si le tableau args contient suffisamment d'éléments avant d'accéder à args[1]
            if (args.Length > 1)
            {
                if (args[1].ToLower() == "/uninstallrestart")
                {
                    ExecuteUninstallRestart();
                }
                else if (args[1].ToLower() == "/uninstallnorestart")
                {
                    ExecuteUninstallNoRestart();
                }
                else if (args[1].ToLower() == "/uninstallshutdown")
                {
                    ExecuteUninstallShutdown();
                }
            }
            else
            {
                // Si les arguments sont insuffisants, afficher un message d'erreur ou prendre une autre action.
                //MessageBox.Show("Arguments insuffisants. Veuillez fournir l'argument approprié.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExecuteUninstallRestart()
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to uninstall the NVIDIA driver and restart?",
                    "Confirm Uninstall",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    Process uninstallProcess = Process.Start("cmd.exe", "/c pnputil /delete-driver oem*.inf /uninstall /force");
                    uninstallProcess.WaitForExit();
                    MessageBox.Show("Uninstall command executed. Your computer will restart now.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Process.Start("shutdown", "/r /f /t 0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during uninstallation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExecuteUninstallNoRestart()
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to uninstall the NVIDIA driver without restarting?",
                    "Confirm Uninstall",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    Process.Start("cmd.exe", "/c pnputil /delete-driver oem*.inf /uninstall /force");
                    MessageBox.Show("Uninstall command executed. Please restart your computer manually.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during uninstallation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExecuteUninstallShutdown()
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to uninstall the NVIDIA driver and restart?",
                    "Confirm Uninstall",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    ExecuteUninstallShutdown();
                    Process uninstallProcess = Process.Start("cmd.exe", "/c pnputil /delete-driver oem*.inf /uninstall /force");
                    uninstallProcess.WaitForExit();
                    MessageBox.Show("Uninstall command executed. Your computer will restart now.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Process.Start("shutdown", "/s /f /t 0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during uninstallation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnUninstall_Click_1(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to uninstall the NVIDIA driver?",
                    "Confirm Uninstall",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    Process uninstallProcess = Process.Start("cmd.exe", "/c pnputil /delete-driver oem*.inf /uninstall /force");
                    uninstallProcess.WaitForExit();
                    MessageBox.Show("Uninstall command executed. Your computer will restart now.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Process.Start("shutdown", "/r /f /t 0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during uninstallation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to uninstall the NVIDIA driver?",
                    "Confirm Uninstall",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    Process.Start("cmd.exe", "/c pnputil /delete-driver oem*.inf /uninstall /force");
                    MessageBox.Show("Uninstall command executed. Please restart your computer.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during uninstallation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button2_Click_1(object sender, EventArgs e)
        {
                try
                {
                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to uninstall the NVIDIA driver?",
                        "Confirm Uninstall",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Yes)
                    {
                        Process uninstallProcess = Process.Start("cmd.exe", "/c pnputil /delete-driver oem*.inf /uninstall /force");
                        uninstallProcess.WaitForExit();
                        MessageBox.Show("Uninstall command executed. Your computer will shut down now.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        System.Threading.Thread.Sleep(2000);
                        Process.Start("shutdown", "/s /f /t 0");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during uninstallation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Ouvre l'URL dans le navigateur par défaut
            System.Diagnostics.Process.Start("https://www.nvidia.com/fr-fr/drivers/");
        }
    }
    }
