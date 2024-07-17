# ApplicationHub

ApplicationHub is a powerful application launcher and organizer built in WPF using C#. It empowers users to efficiently manage and access their applications through a variety of intuitive features and customizable options.

## Features

### Interface Modes

- **View Modes**:
  - **Simplified View**: Displays applications as tiles in a wrap panel layout for easy access.
  - **Detailed View**: Organizes applications categorically with category panels and detailed application lists.
  - **Widget Mode**: Enables a floating widget that can be docked on screen corners or preferred locations.

### Widget Control

- **Docking and Floating Icon**: Place a floating icon on screen corners or any location. The icon magnetizes to screen boundaries or corners for user convenience.
- **Overlay Display**: On hover, displays pinned applications and recent apps for quick access.

![Screenshot 1](link.png)
*Caption for Screenshot 1.*

### Application Management

- **Predefined Application List**: Easily configure applications via the `ApplicationList.txt` file.
  - Supports direct paths to executables (`*.exe`)
  - Folder paths for automatic detection of executable (`*.exe`) or shortcut (`*.lnk`) files.
- **Additional apps**: Add custom applications under a "custom" category for personalized organization.

`ApplicationList.txt` is and must be located in the root directory. 
Example:

```plaintext
[CategoryName]
C:\Path\To\Application.exe
D:\Path\To\FolderWithApps\

[CategoryName2]
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

## Contributing

Contributions are welcome!
