using System;
using System.Collections.Generic;
using System.Text;

namespace ThemeConverter.ColorCompiler
{
    /// <summary>
    /// Represents an item in a pkgdef file.
    /// </summary>
    public class PkgDefItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PkgDefItem"/> class.
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="valueName">Name of the item.</param>
        /// <param name="valueData">Value of the item.</param>
        /// <param name="valueType">Type of the value.</param>
        public PkgDefItem(string sectionName, string valueName, object valueData, PkgDefValueType valueType)
        {
            this.SectionName = sectionName;
            this.ValueName = valueName;
            this.ValueData = valueData;
            this.ValueType = valueType;
        }

        /// <summary>
        /// Gets the name of the section.
        /// </summary>
        public string SectionName { get; private set; }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        public string ValueName { get; private set; }

        /// <summary>
        /// Gets the value of the item.
        /// </summary>
        public object ValueData { get; private set; }

        /// <summary>
        /// Gets the type of the item.
        /// </summary>
        public PkgDefValueType ValueType { get; private set; }

        /// <summary>
        /// Pkgdef value type.
        /// </summary>
        public enum PkgDefValueType
        {
            String,
            ExpandSz,
            MultiSz,
            Binary,
            DWord,
            QWord,
        }
    }
}
