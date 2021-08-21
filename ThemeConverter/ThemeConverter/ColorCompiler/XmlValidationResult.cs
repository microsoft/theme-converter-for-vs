using System;
using System.Collections.Generic;
using System.Text;

namespace ThemeConverter.ColorCompiler
{
    /// <summary>
    /// The result of the validation of some XML. ErrorMessage should be empty string if
    /// the XML was valid.
    /// </summary>
    public class XmlValidationResult
    {
        private readonly bool _isValid;
        private readonly string _errorMessage;

        private XmlValidationResult(bool isValid, string errorMessage)
        {
            _isValid = isValid;
            _errorMessage = errorMessage;
        }
        /// <summary>
        /// Whether the XML is valid.
        /// </summary>
        public bool IsValid { get { return _isValid; } }

        /// <summary>
        /// The error message in the case of invalid XML. Empty string if it's valid.
        /// </summary>
        public string ErrorMessage { get { return _errorMessage; } }

        public static XmlValidationResult InvalidResult(string error)
        {
            return new XmlValidationResult(false, error);
        }

        private static readonly XmlValidationResult _validResult = new XmlValidationResult(true, "");

        public static XmlValidationResult ValidResult { get { return _validResult; } }
    }
}
