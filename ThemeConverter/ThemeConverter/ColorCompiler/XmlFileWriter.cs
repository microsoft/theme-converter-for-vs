// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ThemeConverter.ColorCompiler
{
    /// <summary>
    /// Writes a color manager out to an xml file.
    /// </summary>
    internal class XmlFileWriter : FileWriter
    {
        private const string EmptyThemesXml = "<Themes />";

        private XmlDocument _document = new XmlDocument() { XmlResolver = null };

        public XmlFileWriter(ColorManager colorManager)
            : base(colorManager)
        {
        }

        /// <summary>
        /// Writes the data for the XmlFileWriter to an xml file.
        /// </summary>
        public override void SaveToFile(string fileName)
        {
            _document = new XmlDocument() { XmlResolver = null };
            WriteThemes(_document);
            _document.Save(fileName);
        }

        private void WriteThemes(XmlDocument document)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(EmptyThemesXml)))
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                document.Load(reader);
                foreach (ColorTheme theme in ColorManager.Themes)
                {
                    AddTheme(theme, document.DocumentElement);
                }
            }
        }

        private void AddTheme(ColorTheme theme, XmlElement parent)
        {
            XmlElement themeElement = AddNewElement("Theme", parent);
            AddNameAttribute(theme.Name, themeElement);
            AddGuidAttribute(theme.ThemeId, themeElement);

            if (theme.FallbackId != Guid.Empty)
            {
                AddFallbackIdAttribute(theme.FallbackId, themeElement);
            }

            foreach (ColorCategory category in ColorManager.Categories)
            {
                AddCategory(category, themeElement, theme);
            }

            if (themeElement.ChildNodes.Count == 0)
            {
                parent.RemoveChild(themeElement);
            }
        }

        private void AddCategory(ColorCategory category, XmlElement themeElement, ColorTheme theme)
        {
            XmlElement categoryElement = AddNewElement("Category", themeElement);
            string categoryName = category.Name;
            AddNameAttribute(categoryName, categoryElement);
            AddGuidAttribute(category.Id, categoryElement);

            foreach (ColorRow colorRow in ColorManager.Colors.Where(color => color.Name.Category.Name == categoryName))
            {
                AddColor(colorRow, categoryElement, theme);
            }

            if (categoryElement.ChildNodes.Count == 0)
            {
                themeElement.RemoveChild(categoryElement);
            }
        }

        private void AddColor(ColorRow colorRow, XmlElement category, ColorTheme theme)
        {
            ColorEntry entry = colorRow.GetOrCreateEntry(theme);
            if (entry.IsEmpty)
                return;

            XmlElement colorElement = AddNewElement("Color", category);
            AddNameAttribute(colorRow.Name.Name, colorElement);
            AddColorEntry(entry, colorElement);
        }

        private void AddColorEntry(ColorEntry entry, XmlElement colorRow)
        {
            if (entry.BackgroundType != __VSCOLORTYPE.CT_INVALID)
            {
                XmlElement colorEntryElement = AddNewElement("Background", colorRow);
                AddTypeAttribute(entry.BackgroundType, colorEntryElement);
                AddSourceAttribute(entry.BackgroundSource, entry.BackgroundType, colorEntryElement);
            }

            if (entry.ForegroundType != __VSCOLORTYPE.CT_INVALID)
            {
                XmlElement colorEntryElement = AddNewElement("Foreground", colorRow);
                AddTypeAttribute(entry.ForegroundType, colorEntryElement);
                AddSourceAttribute(entry.ForegroundSource, entry.ForegroundType, colorEntryElement);
            }
        }

        private void AddSourceAttribute(uint source, __VSCOLORTYPE type, XmlElement element)
        {
            AddAttribute("Source", ConvertColorSourceHexToString(source, type), element);
        }

        private string ConvertColorSourceHexToString(uint source, __VSCOLORTYPE type)
        {
            return SwapARGBandABGR(source, type).ToString("X8", CultureInfo.InvariantCulture);
        }

        private void AddTypeAttribute(__VSCOLORTYPE type, XmlElement element)
        {
            AddAttribute("Type", type.ToString(), element);
        }

        private XmlElement AddNewElement(string newElementName, XmlElement parent)
        {
            XmlElement newElement = _document.CreateElement(newElementName);
            parent.AppendChild(newElement);
            return newElement;
        }

        private void AddNameAttribute(string name, XmlElement element)
        {
            AddAttribute("Name", name, element);
        }

        private void AddGuidAttribute(Guid guid, XmlElement element)
        {
            // format for guid is '{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}'
            AddAttribute("GUID", guid.ToString("B"), element);
        }

        private void AddFallbackIdAttribute(Guid fallbackId, XmlElement element)
        {
            AddAttribute("FallbackId", fallbackId.ToString("B"), element);
        }

        private void AddAttribute(string name, string value, XmlElement element)
        {
            XmlAttribute attribute = _document.CreateAttribute(name);
            attribute.Value = value;
            element.Attributes.Append(attribute);
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
