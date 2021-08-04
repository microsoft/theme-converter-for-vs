# VSThemeConverter 
 This is a CLI tool that takes in a VS Code theme’s json file and converts it into a VS theme. 

 ## Using the tool 

1. Clone the repo and build the solution. 
2. Open command line in Admin mode. 
3. Go to the `vs-theme-converter\VSCodeThemeImporter\VSCodeThemeImporter\bin\Debug\netcoreapp3.1`. 
4. Run VSCodeThemeImporter.exe <theme_json_file_path> <vs_install_dir> 
5. For example, my command would be:  
`./VSCodeThemeImporter.exe "C:\Users\foo\Documents\Nord.json" "C:\Program Files\Microsoft Visual Studio\2022\Preview" `
6. This command will launch a patched VS. You can select your new theme from the Tools -> Options -> General page. 
7. Play around with your theme and let us know what you think! 

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
