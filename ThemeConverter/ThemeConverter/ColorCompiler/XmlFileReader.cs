// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace ThemeConverter.ColorCompiler
{
    /// <summary>
    /// This class reads Visual Studio XML theme files. It does no verification, so the
    /// file should be verified before being passed to this class.
    /// </summary>
    internal class XmlFileReader
    {
        private ColorTheme _currentTheme = null;
        private ColorCategory _currentCategory = null;
        private ColorName _currentColor = null;
        private ColorEntry _currentEntry = null;
        private XmlReader _reader;
        private ColorManager _colorManager;
        protected string _fileName;

        public XmlFileReader(string fileName)
        {
            _fileName = fileName;
        }

        public ColorManager ColorManager
        {
            get
            {
                if (_colorManager == null)
                {
                    _colorManager = new ColorManager();
                    if (!FileIsEmptyOrNonExistent())
                    {
                        LoadColorManagerFromFile();
                    }
                }
                return _colorManager;
            }
        }

        protected void LoadColorManagerFromFile()
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
            };

            _reader = XmlReader.Create(_fileName, settings);

            while (_reader.Read())
            {
                if (_reader.NodeType == XmlNodeType.Element)
                {
                    switch (_reader.Name)
                    {
                        case "Theme":
                            ReadThemeElement();
                            break;
                        case "Category":
                            ReadCategoryElement();
                            break;
                        case "Color":
                            ReadColorElement();
                            break;
                        case "Background":
                            ReadBackgroundElement();
                            break;
                        case "Foreground":
                            ReadForegroundElement();
                            break;
                        default: break;
                    }
                }
            }

            _reader.Close();
        }

        private bool FileIsEmptyOrNonExistent()
        {
            FileInfo info = new FileInfo(_fileName);
            return !info.Exists || info.Length == 0;
        }

        private void ReadThemeElement()
        {
            Guid guid;
            if (Guid.TryParse(_reader.GetAttribute("GUID"), out guid))
            {
                _currentTheme = ColorManager.GetOrCreateTheme(guid);
                _currentTheme.Name = _reader.GetAttribute("Name");

                if (Guid.TryParse(_reader.GetAttribute("FallbackId"), out Guid fallBackguid))
                {
                    _currentTheme.FallbackId = fallBackguid;
                }
            }
        }

        private void ReadCategoryElement()
        {
            Guid guid;
            if (Guid.TryParse(_reader.GetAttribute("GUID"), out guid))
            {
                _currentCategory = ColorManager.RegisterCategory(guid, _reader.GetAttribute("Name"));
            }
        }

        private void ReadColorElement()
        {
            _currentColor = new ColorName(_currentCategory, _reader.GetAttribute("Name"));
            _currentEntry = ColorManager.GetOrCreateEntry(_currentTheme.ThemeId, _currentColor);
        }

        private void ReadBackgroundElement()
        {
            __VSCOLORTYPE colorType;
            uint source;

            if (Enum.TryParse<__VSCOLORTYPE>(_reader.GetAttribute("Type"), out colorType))
            {
                _currentEntry.BackgroundType = colorType;
            }

            if (UInt32.TryParse(_reader.GetAttribute("Source"), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out source))
            {
                _currentEntry.BackgroundSource = SwapARGBandABGR(source, colorType);
            }
        }

        private void ReadForegroundElement()
        {
            __VSCOLORTYPE colorType;
            uint source;

            if (Enum.TryParse<__VSCOLORTYPE>(_reader.GetAttribute("Type"), out colorType))
            {
                _currentEntry.ForegroundType = colorType;
            }

            if (UInt32.TryParse(_reader.GetAttribute("Source"), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out source))
            {
                _currentEntry.ForegroundSource = SwapARGBandABGR(source, colorType);
            }
        }

        private uint SwapARGBandABGR(uint argb, __VSCOLORTYPE type)
        {
            if (type == __VSCOLORTYPE.CT_RAW)
            {
                byte alpha = (byte)(argb >> 24);
                byte blue = (byte)(argb >> 16);
                byte green = (byte)(argb >> 8);
                byte red = (byte)argb;

                return (uint)(alpha << 24 | red << 16 | green << 8 | blue);
            }
            else return argb;
        }
    }
}
