// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace ThemeConverter.ColorCompiler
{
    /// <summary>
    /// Represents an item in a pkgdef file.
    /// </summary>
    internal struct PkgDefItem
    {
        public string SectionName { get; set; }

        public string ValueName { get; set; }

        public PkgDefValueType ValueDataType { get; set; }
        public string ValueDataString { get; set; }

        public string[] ValueDataStringArray
        {
            get; set;
        }


        public byte[] ValueDataBinary
        {
            get; set;
        }

        public int ValueDataBinaryLength
        {
            get; set;
        }

        public uint ValueDataDWORD
        {
            get; set;
        }

        public ulong ValueDataQWORD
        {
            get; set;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.Append(SectionName);
            stringBuilder.Append("]");
            if (!string.IsNullOrEmpty(ValueName))
            {
                stringBuilder.Append(" ");
                stringBuilder.Append(ValueName);
                stringBuilder.Append("=");
                stringBuilder.Append("???");
            }

            return stringBuilder.ToString();
        }

        //public sealed override bool Equals(object obj)
        //{
        //    if (obj == null)
        //    {
        //        return false;
        //    }



        //    if (obj.GetType() != GetType())
        //    {
        //        return false;
        //    }



        //    ValueType valueType = (PkgDefItem)obj;
        //    if (SectionName != ((PkgDefItem)valueType).SectionName)
        //    {
        //        return false;
        //    }



        //    if (ValueName != ((PkgDefItem)valueType).ValueName)
        //    {
        //        return false;
        //    }



        //    PkgDefValueType valueDataType = ValueDataType;
        //    if (valueDataType != ((PkgDefItem)valueType).ValueDataType)
        //    {
        //        return false;
        //    }



        //    switch (valueDataType)
        //    {
        //        case PkgDefValueType.PKGDEF_VALUE_STRING:
        //        case PkgDefValueType.PKGDEF_VALUE_EXPAND_SZ:
        //            if (ValueDataString != ((PkgDefItem)valueType).ValueDataString)
        //            {
        //                return false;
        //            }



        //            break;
        //        case PkgDefValueType.PKGDEF_VALUE_MULTI_SZ:
        //            {
        //                string[] valueDataStringArray = ValueDataStringArray;
        //                int num2 = (valueDataStringArray != null) ? 1 : 0;
        //                if ((((((PkgDefItem)valueType).ValueDataStringArray != null) ? 1 : 0) ^ num2) != 0)
        //                {
        //                    return false;
        //                }



        //                if (valueDataStringArray == null)
        //                {
        //                    break;
        //                }



        //                if (ValueDataStringArray.GetLength(0) != ((PkgDefItem)valueType).ValueDataStringArray.GetLength(0))
        //                {
        //                    return false;
        //                }



        //                int length = ValueDataStringArray.GetLength(0);
        //                int num3 = 0;
        //                if (0 >= length)
        //                {
        //                    break;
        //                }



        //                do
        //                {
        //                    if (!(ValueDataStringArray[num3] != ((PkgDefItem)valueType).ValueDataStringArray[num3]))
        //                    {
        //                        num3++;
        //                        continue;
        //                    }



        //                    return false;
        //                }
        //                while (num3 < length);
        //                break;
        //            }
        //        case PkgDefValueType.PKGDEF_VALUE_DWORD:
        //            if (ValueDataDWORD != ((PkgDefItem)valueType).ValueDataDWORD)
        //            {
        //                return false;
        //            }



        //            break;
        //        case PkgDefValueType.PKGDEF_VALUE_QWORD:
        //            if (ValueDataQWORD != ((PkgDefItem)valueType).ValueDataQWORD)
        //            {
        //                return false;
        //            }



        //            break;
        //        case PkgDefValueType.PKGDEF_VALUE_BINARY:
        //            {
        //                int valueDataBinaryLength = ValueDataBinaryLength;
        //                if (valueDataBinaryLength != ((PkgDefItem)valueType).ValueDataBinaryLength)
        //                {
        //                    return false;
        //                }



        //                if (valueDataBinaryLength <= 0)
        //                {
        //                    break;
        //                }



        //                byte[] valueDataBinary = ValueDataBinary;
        //                if (valueDataBinary != null && ((PkgDefItem)valueType).ValueDataBinary != null)
        //                {
        //                    if (ValueDataBinaryLength <= valueDataBinary.GetLength(0) && ((PkgDefItem)valueType).ValueDataBinaryLength <= ((PkgDefItem)valueType).ValueDataBinary.GetLength(0))
        //                    {
        //                        int num = 0;
        //                        int valueDataBinaryLength2 = ValueDataBinaryLength;
        //                        if (0 >= valueDataBinaryLength2)
        //                        {
        //                            break;
        //                        }



        //                        byte[] valueDataBinary2 = ValueDataBinary;
        //                        do
        //                        {
        //                            if (valueDataBinary2[num] == ((PkgDefItem)valueType).ValueDataBinary[num])
        //                            {
        //                                num++;
        //                                continue;
        //                            }



        //                            return false;
        //                        }
        //                        while (num < valueDataBinaryLength2);
        //                        break;
        //                    }



        //                    throw new IndexOutOfRangeException();
        //                }



        //                return false;
        //            }
        //    }



        //    return true;
        //}
    }
}
