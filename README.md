# Theme Converter for Visual Studio
Theme Converter is a CLI tool which allows you to convert your VS Code theme to work in Visual Studio. It's a simple tool that maps the colors of a theme to Visual Studio. Themes can be packaged into VSIXs and uploaded to the Marketplace so that more people can use your theme! This allows developers who use Visual Studio, or use both VS Code and Visual Studio, to quickly get access to a wide range of fun and interesting themes! 

Here's a quick overview on how the Theme Converter works:
[<img width="700" alt="Tutorial on converting a theme" src="https://user-images.githubusercontent.com/12738587/134434509-c948f72d-82db-404a-95f2-ccdd1ffe353a.png">](https://www.youtube.com/watch?v=2Gwqr5uuBt4)

Our community of authors have already converted a handful of VS Code themes to work in Visual Studio. [Feel free to take a look here](https://aka.ms/vsthemes).
 
 ## Prerequisites
 1. VS Code
 2. [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)
 3. The following workloads can be installed via the Visual Studio installer:
    - .NET Desktop development
    - Visual Studio extension development

 ## Instructions
 ### Using the tool
1. Open command line in **Admin** mode. 
2. Clone the repo
3. Go to `<your_clone_path>\ThemeConverter\ThemeConverter` and build the converter project with `dotnet build ThemeConverter.csproj`. 
4. Go to `<your_clone_path>\ThemeConverter\ThemeConverter\bin\Debug\net6.0`. 
5. Get the theme file with steps described in section [Getting a theme's json file](https://github.com/microsoft/theme-converter#getting-a-themes-json-file)
7. Run `ThemeConverter.exe -h` to see the usage of the tool and use the tool according to your needs.
    - If you want to convert a theme and patch it to a target VS:
      - Run `ThemeConverter.exe -i <path_to_theme_json> -t <target_vs_installation_path>`
      - Example: `ThemeConverter.exe -i "C:\myTheme\TestTheme.json" -t "C:\Program Files\Microsoft Visual Studio\2022\Community"`. This command will convert the TestTheme, patch the generated json to the target VS and launch the VS with the converted theme.
      - Note: the VS installation path can be found in your VS installer (the location field on the modify page).
      - Now you can see the your converted theme under `Tools -> Themes`! Enjoy!

    - If you just want to convert a theme and get the generated pkgdef:
      - Run `ThemeConverter.exe -i <path_to_theme_json> -o <output_path>`
      - Example: `ThemeConverter.exe -i "C:\myTheme\TestTheme.json" -o "C:\myTheme\results"`. This command will convert the theme and the generated pkgdef will be  `C:\myTheme\results\TestTheme.pkgdef`.

### Getting a theme's json file
1. Open VS Code. 
2. Install the desired color theme and switch to this theme in VS Code. Please note that this tool will not convert icon themes. 
3. “Ctrl + Shift + P” and run “Developer: Generate Color Theme from current settings.” 
4. In the generated JSON, uncomment all code. When you uncomment, please be careful about missing commas! Make sure the JSON is valid. 
5. Save this as a “JSON” file for the conversion, using the theme's name as the file name. Please ensure that the file’s extension is .json. (The file shouldn’t be saved as a JSONC file.) 
6. **Note**: Because some part of VS UI does not support customized alpha channel, we recommend reducing the usage of not fully opaque colors for better conversion result.

### Creating a VSIX for the new theme
This section describes how you can create a VSIX with the converted theme for publishing and sharing.
1. In VS 2022, create a new "Empty VSIX Project."
2. Select the project node and open the "Add existing Item" window: Use "Shift + Alt + A" or right-click on the project node, select Add > Existing Item.
3. Set filter to All Files (*.*) and select the converted .pkgdef file(s) that you want to include in this VSIX.
5. Select the newly added pkgdef file in the Solution Explorer and open the Properties window. If the Properties window is not already open, navigate to the View menu at the top > Properties Window.
<img width="220" alt="blueReadme_propertieswindow" src="https://user-images.githubusercontent.com/12738587/133951341-d8ae0748-14f4-4e31-9e83-2d646b0caab1.png">

6. Set `Copy to Output Directory` to `Copy always`.
7. Set `Include in VSIX` to `true`.
8. Open the `source.extension.vsixmanifest` file, then select Assets, select New.
9. Set `Type` to `Microsoft.VisualStudio.VsPackage`, and `Source` to `File on filesystem`.
10. Select Browse and select the .pkgdef you added. Select OK.
11. Edit other fields in the vsixmanifest as desired (author, version, company, etc).
12. Build solution and you now have a vsix in the output folder! Your new theme is most compatible with Visual Studio 2022.

### Removing a converted theme from VS
1. Open target VS and switch to some theme that will not be deleted (like Blue theme).
2. Go to `<vs_install_dir>\Common7\IDE\CommonExtensions\Platform`. e.g: `C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform`
and delete the pkgdef of the theme that the you want to remove (The name of the pkgdef will match the name of the theme).
3. Open Developer Command Prompt of the target VS and run `devenv /updateConfiguration`.
4. Launch VS again, and the themes should now be removed.

# Support

## How to file issues and get help  
This project uses GitHub Issues to track bugs and feature requests. Please search the existing issues before filing new issues to avoid duplicates. For new issues, file your bug or feature request as a new Issue.

## Microsoft Support Policy  
Support for this project is limited to the resources listed above.


# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
