using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ThemeConverter.ColorCompiler
{
    /// <summary>
    /// Reads an XML file, verifies that the XML is valid, and that it has the proper elements to 
    /// make a valid ColorManager.
    /// </summary>
    public class XmlFileValidator
    {
        private readonly string _themeValidation;

        public XmlFileValidator(string themeValidation)
        {
            Validate.IsNotNullOrWhiteSpace(themeValidation, "themeValidation");

            _themeValidation = themeValidation;
        }

        /// <summary>
        /// Validates the file at fileName conforms to the correct validation schema.
        /// </summary>
        /// <param name="fileName">File to validate.</param>
        /// <returns>
        /// The result that contains information about whether the file is valid, and if it's invalid, 
        /// what the error is.
        /// </returns>
        public XmlValidationResult ValidateFile(string fileName)
        {
            using (FileStream fi = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Assuming an empty file means someone is going to make a new theme with the editor
                if (fi.Length == 0)
                {
                    return XmlValidationResult.ValidResult;
                }

                XmlReader schema = GetValidationSchema();
                XmlReaderSettings settings = new XmlReaderSettings()
                {
                    ValidationType = ValidationType.Schema,
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                };

                settings.Schemas.Add(null, schema);

                using (XmlReader reader = XmlReader.Create(fi, settings))
                {
                    XmlDocument document = new XmlDocument() { XmlResolver = null };
                    document.Load(reader);

                    return XmlValidationResult.ValidResult;
                }
            }
        }

        private XmlReader GetValidationSchema()
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
            };

            Stream xmlStream = ConvertStringToStream(_themeValidation);
            return XmlReader.Create(xmlStream, settings);
        }

        private Stream ConvertStringToStream(string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }
    }

}
