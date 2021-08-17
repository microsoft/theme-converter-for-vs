# Theme Converter Private Preview
 This is a CLI tool that takes in a VS Code theme’s json file and converts it into a VS theme. 
 
 ## Prerequisites
 1. VS Code
 2. VS 2022 Preview 3 or later for best results


 ## Instructions
 ### Using the tool
1. Open command line in Admin mode. 
2. Clone the repo
3. Go to `<your_clone_path>\theme-converter\ThemeConverter\ThemeConverter` and build the converter project with `dotnet build ThemeConverter.csproj`. 
4. Go to the `<your_clone_path>\vs-theme-converter\ThemeConverter\ThemeConverter\bin\Debug\netcoreapp3.1`. 
5. Get the theme file with steps described in section [Getting a theme's json file](https://github.com/microsoft/theme-converter#getting-a-themes-json-file)
6. Run `ThemeConverter.exe <theme_json_file_path> <vs_install_dir>`
 (e.g: `./ThemeConverter.exe "C:\Users\foo\Documents\Nord.json" "C:\Program Files\Microsoft Visual Studio\2022\Preview"`)
6. This command will launch a patched VS. You can select your new theme from the Tools -> Options -> General page. 

### Getting a theme's json file
1. Open VS Code. 
2. Install the desired color theme and switch to this theme in VS Code. Please note that this tool will not convert icon themes. 
3. “Ctrl + Shift + P” and run “Developer: Generate Color Theme from current settings.” 
4. In the generated JSON, uncomment all code. When you uncomment, please be careful about missing commas! Make sure the JSON is valid. 
5. Save this as a “JSON” file for the conversion, using the theme's name as the file name. Please ensure that the file’s extension is .json. (The file shouldn’t be saved as a JSONC file.) 

### Creating a VSIX for the new theme
This section describes how you can create a VSIX with the converted theme for publishing and sharing.
1. In VS 2022, create a new "Empty VSIX Project"
2. Select the project node and open the "Add existing Item" window: Use "Shift + Alt + A"; right-click on project node, select Add > Existing Item; or navigate to Project -> Add Existing Item...
3. Set filter to All Files (*.*) and select the .pkgdef file(s) that the converter created under `<vs_install_dir>\Common7\IDE\CommonExtensions\Platform`. (e.g: `C:\Program Files\Microsoft Visual Studio\2022\Preview\Common7\IDE\CommonExtensions\Platform\Nord.pkgdef`)
5. Select the newly added pkgdef file in the solution explorer and open the properties window
6. Set `Copy to Output Directory` to `Copy always`
7. Set `Include in VSIX` to `true`
8. Open the `source.extension.vsixmanifest` file, select Assets, select New.
9. Set `Type` to `Microsoft.VisualStudio.VsPackage`, and `Source` to `File on filesystem`.
10. Select Browse and select the .pkgdef you added. Select OK.
11. Edit other fields in the vsixmanifest as desired (author, version, company, etc)
12. Build solution and you now have a vsix in the output folder.

## Let us know what you think
What do you think about the converted theme? What do you think about the tool? Is there anything you would change? Please let us know!


## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
