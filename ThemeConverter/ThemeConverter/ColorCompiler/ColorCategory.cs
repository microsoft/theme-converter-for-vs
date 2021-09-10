// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace ThemeConverter.ColorCompiler
{
    internal class ColorCategory
    {
        private const int HashCombiningMultiplier = 1566083941;

        public ColorCategory(Guid categoryId, string name)
        {
            Id = categoryId;
            Name = name;
        }

        public Guid Id
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public override bool Equals(object obj)
        {
            ColorCategory other = obj as ColorCategory;
            if (other == null)
            {
                return false;
            }

            return Id == other.Id && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return CombineHashes(Name.GetHashCode(), Id.GetHashCode());
        }

        /// <summary>
        /// Combines 2 hashes together.
        /// </summary>
        private static int CombineHashes(int hash1, int hash2)
        {
            unchecked
            {
                return (RotateLeft(hash1, 5) ^ (hash2 * HashCombiningMultiplier));
            }
        }

        /// <summary>
        /// Rotates the bits of an int value to the left
        /// </summary>
        /// <param name="value">The value to rotate</param>
        /// <param name="count">The number of positions to rotate</param>
        /// <returns>The rotated value</returns>
        private static int RotateLeft(int value, int count)
        {
            const int totalBits = 32;
            return unchecked((value << count) | (value >> (totalBits - count)));
        }
    }
}
