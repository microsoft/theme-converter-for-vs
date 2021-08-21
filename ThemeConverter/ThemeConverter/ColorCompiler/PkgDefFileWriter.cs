using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ThemeConverter.ColorCompiler
{
    class PkgDefFileWriter
    {
        private StreamWriter file;
        private bool isOpen;
        private string lastSectionWritten;

        private class Constants
        {
            public const string SectionStartChar = @"[";
            public const string SectionEndChar = @"]";
            public const string BinaryPrefix = "hex:";
        }

        public PkgDefFileWriter(string filePath, bool overwriteExisting)
        {
            this.file = new StreamWriter(filePath, !overwriteExisting, System.Text.Encoding.UTF8);
            this.isOpen = true;
            this.lastSectionWritten = "";
        }

        public bool Write(PkgDefItem item)
        {
            if (!this.isOpen)
                return false;

            if ((item.SectionName == String.Empty) ||
                (item.SectionName == null))
                return false;

            if (item.SectionName != this.lastSectionWritten)
            {
                if (this.lastSectionWritten != String.Empty)
                {
                    file.WriteLine();
                }
                string line = String.Format("{0}{1}{2}",
                                            Constants.SectionStartChar,
                                            item.SectionName,
                                            Constants.SectionEndChar);
                file.WriteLine(line);
                this.lastSectionWritten = item.SectionName;
            }

            if ((item.ValueName != null) &&
                (item.ValueName != String.Empty))
            {
                if (item.ValueName == "@")
                {
                    file.Write(item.ValueName);
                }
                else
                {
                    string line = String.Format("\"{0}\"", item.ValueName);
                    file.Write(line);
                }

                file.Write("=");

                switch (item.ValueType)
                {
                    //Todo: catch invalid cast exceptions, report, and continue;
                    case PkgDefItem.PkgDefValueType.PKGDEF_VALUE_STRING:
                        {
                            string line = String.Format("\"{0}\"", (string)item.ValueData);
                            file.Write(line);
                            break;
                        }
                    case PkgDefItem.PkgDefValueType.PKGDEF_VALUE_BINARY:
                        {
                            string line = String.Format("{0}{1}",
                                                        Constants.BinaryPrefix,
                                                        this.DataToHexString((byte[])item.ValueData));
                            file.Write(line);
                            break;
                        }
                }

                file.WriteLine();
            }

            return true;
        }

        private string DataToHexString(byte[] binaryData)
        {
            if (!this.isOpen)
                return null;

            string dataString = "";
            foreach (byte b in binaryData)
            {
                dataString = dataString + b.ToString("x2") + ",";
            }
            return dataString.TrimEnd(',');
        }
    }
}
