using System;
using System.Collections.Generic;
using System.Text;

namespace ThemeConverter.ColorCompiler
{
    public class ColorCompiler
    {
        public ColorCompiler()
        {
        }

        public void Compile(string pathToFile)
        {
            var fileName = Path.ChangeExtension(pathToFile, ".pkgdef");
            ColorManager manager = OpenXML(fileName);
            SaveColorManagerToFile(manager, fileName);
        }

        private ColorManager OpenXML(string XmlPath)
        {
            XmlFileValidator validator = new XmlFileValidator(Resources.ThemeValidation);
            XmlFileReader reader = new XmlFileReader(XmlPath, validator);

            return reader.ColorManager;
        }

        public void SaveColorManagerToFile(ColorManager manager, string fileName)
        {
                PkgDefWriter writer = new PkgDefWriter(manager);
                writer.SaveThemeRegistration(fileName);
                writer.SaveToFile(fileName);
        }

    }
}
