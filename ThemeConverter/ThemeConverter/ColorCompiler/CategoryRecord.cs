// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ThemeConverter.ColorCompiler
{
    /// <summary>
    /// Reads or writes a category of colors from a binary stream.  Each
    /// category record contains the GUID identifier for the category
    /// and the sequence of color names and values contained in the category.
    /// </summary>
    internal sealed class CategoryRecord
    {
        Guid _category;
        List<ColorRecord> _colors;

        public IList<ColorRecord> Colors
        {
            get
            {
                return _colors;
            }
        }

        public CategoryRecord(Guid category)
        {
            _category = category;
            _colors = new List<ColorRecord>();
        }

        public void Write(BinaryWriter writer)
        {
            WriteGuid(writer, _category);
            writer.Write(_colors.Count);
            foreach (ColorRecord entry in _colors)
            {
                entry.Write(writer);
            }
        }

        public static void WriteGuid(BinaryWriter writer, Guid guid)
        {
            writer.Write(guid.ToByteArray());
        }
    }
}
