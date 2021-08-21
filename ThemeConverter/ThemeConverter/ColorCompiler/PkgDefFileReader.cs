using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ThemeConverter.ColorCompiler
{
    public class PkgDefFileReader
    {
        private StreamReader file;
        private string currentSection;

        private class Constants
        {
            public const string SectionStartChar = @"[";
            public const string SectionEndChar = @"]";
            public const string CommentChars = "//";
            public const string IniCommentChar = ";";
            public const string ExpandSzPrefix = "e:";
            public const string BinaryPrefix = "hex:";
            public const string DwordBinaryPrefix = "hex(b):";
            public const string ExpandSzBinaryPrefix = "hex(2):";
            public const string MultiSzPrefix1 = "hex(7):";
            public const string MultiSzPrefix2 = "hex(m):";
            public const string DwordPrefix = "dword:";
            public const string QwordPrefix = "qword:";

            public const char ContinuationChar = '\\';
        }

        public PkgDefFileReader(string filePath)
        {
            this.file = new StreamReader(filePath);
            currentSection = "";
        }

        public PkgDefItem Read()
        {
            if (file.EndOfStream)
                return new PkgDefItem();

            string currentLine = "";
            PkgDefItem item = new PkgDefItem();
            item.SectionName = null;
            item.ValueName = null;
            item.ValueData = null;

            if (currentSection == "")
            {
                do
                {
                    currentLine = file.ReadLine().TrimStart();
                }
                while ((!this.IsSectionLine(currentLine)) && !file.EndOfStream);

                if (file.EndOfStream && currentLine == "")
                    return item;

                this.SetNewSection(currentLine);
                item.SectionName = this.currentSection;
                return item;
            }
            else
            {
                do
                {
                    do
                    {
                        currentLine = currentLine.TrimEnd(Constants.ContinuationChar);
                        if (file.EndOfStream && !currentLine.Equals(""))
                            return new PkgDefItem();

                        string tempLine = file.ReadLine();
                        currentLine += tempLine;
                    }
                    while (currentLine.EndsWith(@"\"));

                    currentLine = currentLine.TrimStart();

                    if (currentLine == "")
                        continue;

                    if (currentLine.StartsWith(Constants.CommentChars) || currentLine.StartsWith(Constants.IniCommentChar))
                        continue;

                    if (this.IsSectionLine(currentLine))
                    {
                        this.SetNewSection(currentLine);
                        item.SectionName = this.currentSection;
                        return item;
                    }
                    else
                    {
                        item.SectionName = this.currentSection;
                        this.ParseNameValue(currentLine, ref item);
                        return item;
                    }
                }
                while (!file.EndOfStream);

                return item;
            }
        }

        private string ParseSectionName(string line)
        {
            if (line.LastIndexOf(Constants.SectionEndChar) < 0)
                return "";

            return line.Substring(1, line.LastIndexOf(Constants.SectionEndChar) - 1);
        }

        private void ParseNameValue(string line, ref PkgDefItem item)
        {
            if (line == "")
                return;

            int equalsIndex = line.IndexOf("=", (line[0] == '"') ? line.IndexOf('"', 1) + 1 : 1);
            if (equalsIndex < 0)
                return;

            string valueName = line.Substring(0, equalsIndex).Trim();
            string valueDataString = line.Substring(equalsIndex + 1).Trim();

            if ((valueName != "@") && !this.StripQuotes(ref valueName))
                return;

            if (this.StripQuotes(ref valueDataString))
            {
                item.ValueName = valueName;
                item.ValueData = valueDataString;
                item.ValueType = PkgDefItem.PkgDefValueType.PKGDEF_VALUE_STRING;
                return;
            }
            else if (valueDataString.StartsWith(Constants.ExpandSzPrefix))
            {
                valueDataString = valueDataString.Substring(Constants.ExpandSzPrefix.Length);
                valueDataString = valueDataString.Replace("\\\\", "\\");
                this.StripQuotes(ref valueDataString);
                item.ValueName = valueName;
                item.ValueData = valueDataString;
                item.ValueType = PkgDefItem.PkgDefValueType.PKGDEF_VALUE_EXPAND_SZ;
                return;
            }
            else if (valueDataString.StartsWith("hex"))
            {
                int colonIndex = valueDataString.IndexOf(":");
                if (colonIndex < 0)
                    return;

                string binaryDataString = valueDataString.Substring(colonIndex + 1);
                byte[] binaryData = this.TransformHexToBinary(binaryDataString);

                switch (valueDataString.Substring(0, colonIndex + 1))
                {
                    case Constants.DwordBinaryPrefix:
                        {
                            if (binaryData.Length != 8)
                                return;

                            ulong val = 0;
                            for (int i = 0; i < binaryData.Length; i++)
                            {
                                val = (val << 8) | binaryData[i];
                            }
                            item.ValueName = valueName;
                            item.ValueData = val;
                            item.ValueType = PkgDefItem.PkgDefValueType.PKGDEF_VALUE_QWORD;
                            return;
                        }
                    case Constants.ExpandSzBinaryPrefix:
                        {
                            item.ValueName = valueName;
                            item.ValueData = System.Text.Encoding.UTF8.GetString(binaryData);
                            item.ValueType = PkgDefItem.PkgDefValueType.PKGDEF_VALUE_EXPAND_SZ;
                            return;
                        }
                    case Constants.MultiSzPrefix1:
                    case Constants.MultiSzPrefix2:
                        {
                            int binaryDataLength = binaryData.Length;
                            if ((Convert.ToUInt32(binaryData.GetValue(binaryDataLength - 1)) != 0) &&
                                (Convert.ToUInt32(binaryData.GetValue(binaryDataLength - 2)) != 0))
                                return;
                            item.ValueName = valueName;
                            item.ValueData = System.Text.Encoding.UTF8.GetString(binaryData);
                            item.ValueType = PkgDefItem.PkgDefValueType.PKGDEF_VALUE_MULTI_SZ;
                            return;
                        }
                    default:
                        {
                            break;
                        }
                }

                item.ValueName = valueName;
                item.ValueData = binaryData;
                item.ValueType = PkgDefItem.PkgDefValueType.PKGDEF_VALUE_BINARY;
                return;
            }
            else if (valueDataString.StartsWith(Constants.DwordPrefix))
            {
                string numericString = valueDataString.Substring(Constants.DwordPrefix.Length);
                if (numericString.Length > 8)
                    return;

                uint dword = Convert.ToUInt32(numericString, 16);
                item.ValueName = valueName;
                item.ValueData = dword;
                item.ValueType = PkgDefItem.PkgDefValueType.PKGDEF_VALUE_DWORD;
                return;
            }
            else if (valueDataString.StartsWith(Constants.QwordPrefix))
            {
                string numericString = valueDataString.Substring(Constants.QwordPrefix.Length);
                if (numericString.Length > 16)
                    return;

                ulong qword = Convert.ToUInt64(numericString, 16);
                item.ValueName = valueName;
                item.ValueData = qword;
                item.ValueType = PkgDefItem.PkgDefValueType.PKGDEF_VALUE_QWORD;
                return;
            }
            else
            {
                return;
            }

        }

        private void SetNewSection(string line)
        {
            this.currentSection = this.ParseSectionName(line);
        }

        private bool IsSectionLine(string line)
        {
            return line.StartsWith(Constants.SectionStartChar);
        }

        private bool StripQuotes(ref string line)
        {
            if ((line.Length > 1) && line.StartsWith("\"") && line.EndsWith("\""))
            {
                line = line.Substring(1, line.Length - 2);
                return true;
            }
            return false;
        }

        private byte[] TransformHexToBinary(string hexString)
        {
            string normalizedString = "";

            int curPos = 0;
            while (curPos < hexString.Length)
            {
                if (!this.IsValidHexChar(hexString[curPos]))
                {
                    curPos++;
                    continue;
                }

                if ((curPos + 1 >= hexString.Length) ||
                    !this.IsValidHexChar(hexString[curPos + 1]))
                {
                    //expecting a 2nd hex character
                    //throw?
                }

                normalizedString += hexString[curPos].ToString() + hexString[curPos + 1].ToString();
                curPos += 2;
            }

            if (normalizedString.Length % 2 != 0)
            {
                //throw?
            }

            byte[] data = new byte[normalizedString.Length / 2];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)((this.HexCharToByte(normalizedString[(2 * i)]) << 4) | this.HexCharToByte(normalizedString[(2 * i) + 1]));
            }

            return data;
        }

        private bool IsValidHexChar(char ch)
        {
            return (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F') || (ch >= '0' && ch <= '9');
        }

        private byte HexCharToByte(char ch)
        {
            if (ch >= '0' && ch <= '9')
                return (byte)(ch - '0');
            else if (ch >= 'a' && ch <= 'f')
                return (byte)(ch - 'a' + 10);
            else if (ch >= 'A' && ch <= 'F')
                return (byte)(ch - 'A' + 10);
            else
                return (byte)0;
        }
    }
}
