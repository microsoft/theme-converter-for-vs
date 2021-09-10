// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ThemeConverter.ColorCompiler
{
    /// <summary>
    /// Reads or writes a sequence of category records from a binary stream.
    /// A CategoryCollectionRecord contains a sequence of CategoryRecords.
    /// Each CategoryRecord contains a sequence of ColorRecords, and each
    /// ColorRecord specifies a name and values for a single color.
    /// CategoryCollectionRecords are merged together by a ColorTheme to
    /// form the full theme.
    /// </summary>
    internal sealed class CategoryCollectionRecord
    {
        List<CategoryRecord> _categories;

        public IList<CategoryRecord> Categories
        {
            get
            {
                return _categories;
            }
        }

        public CategoryCollectionRecord()
        {
            _categories = new List<CategoryRecord>();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_categories.Count);

            foreach (CategoryRecord theme in _categories)
            {
                theme.Write(writer);
            }
        }
    }
}
