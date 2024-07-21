# ApplicationHub

ApplicationHub is a powerful application launcher and organizer built in WPF using C#. It empowers users to efficiently manage and access their applications through a variety of intuitive features and customizable options.

<p align="center">
  <img src="https://github.com/user-attachments/assets/c938b7cb-9e1e-46c9-9a3a-c9599622c453">
</p>

## Features

### Interface Modes

- **View Modes**:
  - **Simplified View**: Displays applications as tiles in a wrap panel layout for easy access.
  - **Detailed View**: Organizes applications categorically with category panels and detailed application lists.
  - **Widget Mode**: Enables a floating widget that can be docked on screen corners or preferred locations.
<p align="center">
  <img src="https://github.com/user-attachments/assets/de14ff49-573f-4dbc-bc8c-2a76b6ea64c0" Height="45%" width="45%">
  <img src="https://github.com/user-attachments/assets/87880514-b603-4fd8-8f0b-a9ce291585bd" Height="45%" width="45%">
</p>
### Widget Control

- **Toggle**: Toggle the widget by clicking on the app icon or minimizing it.
<p align="center">
  <img src="https://github.com/user-attachments/assets/68405439-f736-4810-a5c5-5326dd4f3f0e" Height="45%" width="45%">
</p>
- **Overlay Display**: On hover, displays pinned applications and recent apps for quick access.
<p align="center">
  <img src="https://github.com/user-attachments/assets/175a72bb-c3f2-4ae4-87a3-fe747d478c15" Height="45%" width="45%">
  <img src="https://github.com/user-attachments/assets/ef5fc253-2d56-485a-a51d-b8cc9bbe4536" Height="45%" width="45%">
</p>
- **Docking and Floating Icon**: Place a floating icon on screen corners or any location. The icon magnetizes to screen boundaries or corners for user convenience.
<p align="center">
  <img src="https://github.com/user-attachments/assets/092a0097-4377-465d-a1fc-7ebda16aa4f1" Height="20%" width="20%">
  <img src="https://github.com/user-attachments/assets/b6cb5888-9138-45c9-ada4-92f1a791cce3" Height="20%"  width="20%">
  <img src="https://github.com/user-attachments/assets/f395914f-d0ab-4d95-b5e9-f0d974b94450" Height="20%"  width="20%">
</p>
### Application Management

- **Predefined Application List**: Easily configure applications via the `ApplicationList.txt` file.
  - Supports direct paths to executables (`*.exe`)
  - Folder paths for automatic detection of executable (`*.exe`) or shortcut (`*.lnk`) files.
- **Additional apps**: Add custom applications under a "custom" category for personalized organization.

`ApplicationList.txt` is and must be located in the root directory. 
Example:

```plaintext
[Category1 Name]
C:\Path\To\Application.exe
C:\Path\To\FolderWithApps\

[Category2 Name]
...
```

### Metadata Integration

Upon launch, ApplicationHub generates metadata folders for each application if they are missing.
- Customize or add metadata files (`description.txt`, `image.png`) in the `metadata/` folder located alongside `ApplicationHub.exe`.

Example structure:
```
metadata/
├── AppName1/
│   ├── image.png
│   └── description.txt
└── AppName2/
    ├── image.png
    └── description.txt
```

## Getting Started

To begin using ApplicationHub, simply download the latest release and follow the installation instructions in the documentation.

## Contributing

We value contributions from the community. If you have suggestions, find a bug, or want to add a new feature, please open an issue or submit a pull request.

---

**Note:** Screenshots are for illustration purposes only and may not reflect the latest version of the application.
