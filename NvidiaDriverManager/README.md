# NvidiaDriverManager

**NvidiaDriverManager** est une application Windows développée en C# pour gérer et afficher les informations des pilotes NVIDIA installés sur votre système. Cette application fournit des informations détaillées sur les versions des pilotes NVIDIA à partir de diverses sources, y compris WMI, `nvidia-smi` et le registre Windows. Elle permet également de désinstaller les pilotes NVIDIA directement depuis l'interface graphique, avec des options pour redémarrer ou éteindre l'ordinateur après l'opération.

![Screen](https://i.imgur.com/KP2Qatg.png)

- [X] Affichage des versions de pilotes NVIDIA
- [X] Désinstallation des pilotes NVIDIA
- [X] Redémarrage ou extinction de l'ordinateur après désinstallation

## Fonctionnalités

- Vérification des pilotes NVIDIA installés via WMI, `nvidia-smi` et le registre Windows.
- Affichage de la version du pilote NVIDIA dans l'interface graphique.
- Vérification de l'état du système pour conseiller l'utilisation en mode sans échec pour un nettoyage en toute sécurité.
- Désinstallation des pilotes NVIDIA avec confirmation.
- Options pour redémarrer ou éteindre l'ordinateur après la désinstallation.

## Pré-requis

- Windows 10 ou supérieur
- .NET Framework 4.8.1 ou supérieur
- NVIDIA GPU installé et drivers disponibles

## Utilisation

1. Lancer l'application pour afficher les versions des pilotes NVIDIA installés.
2. Si vous souhaitez désinstaller les pilotes NVIDIA, cliquez sur le bouton "Uninstall" et confirmez votre choix.
3. Vous aurez l'option de redémarrer ou d'éteindre l'ordinateur après la désinstallation.

### Commandes

- **`/uninstallrestart`** : Désinstalle le pilote NVIDIA avec redémarrage.
- **`/uninstallnorestart`** : Désinstalle le pilote NVIDIA sans redémarrage.
- **`/uninstallshutdown`** : Désinstalle le pilote NVIDIA puis étin l'ordinateur.

## Installation

1. Clonez ce dépôt sur votre machine locale :

   ```bash
   git clone https://github.com/o2Cloud-fr/NvidiaDriverManager.git

## Authors

- [@MyAlien](https://www.github.com/MyAlien)
- [@o2Cloud](https://www.github.com/o2Cloud-fr )

## Badges

Add badges from somewhere like: [shields.io](https://shields.io/)

[![MIT License](https://img.shields.io/badge/License-o2Cloud-yellow.svg)]()


## Contributing

Contributions are always welcome!

See `contributing.md` for ways to get started.

Please adhere to this project's `code of conduct`.


## Feedback

If you have any feedback, please reach out to us at github@o2cloud.fr


## 🔗 Links
[![portfolio](https://img.shields.io/badge/my_portfolio-000?style=for-the-badge&logo=ko-fi&logoColor=white)](https://vcard.o2cloud.fr/)
[![linkedin](https://img.shields.io/badge/linkedin-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/remi-simier-2b30142a1/)


## 🛠 Skills
C#


## License

[Apache-2.0 license](https://github.com/o2Cloud-fr/NvidiaDriverManager/blob/main/LICENSE)


![Logo](https://o2cloud.fr/logo/o2Cloud.png)


## Related

Here are some related projects

[Awesome README](https://github.com/o2Cloud-fr/NvidiaDriverManager/blob/main/README.md)


## Roadmap

- Additional browser support

- Add more integrations


## Support

For support, email github@o2cloud.fr or join our Slack channel.


## Tech Stack

## Used By

This project is used by the following companies:

- o2Cloud
- MyAlienTech

