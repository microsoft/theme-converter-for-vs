// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;

namespace ThemeConverter.ColorCompiler
{
    internal class PkgDefFileWriter : IDisposable
    {
        private StreamWriter file;
        private bool isOpen;
        private string lastSectionWritten;
        private bool disposedValue;

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

                switch (item.ValueDataType)
                {
                    //Todo: catch invalid cast exceptions, report, and continue;
                    case PkgDefValueType.PKGDEF_VALUE_STRING:
                        {
                            string line = String.Format("\"{0}\"", (string)item.ValueDataString);
                            file.Write(line);
                            break;
                        }
                    case PkgDefValueType.PKGDEF_VALUE_BINARY:
                        {
                            string line = String.Format("{0}{1}",
                                                        Constants.BinaryPrefix,
                                                        this.DataToHexString((byte[])item.ValueDataBinary, item.ValueDataBinaryLength));
                            file.Write(line);
                            break;
                        }
                }

                file.WriteLine();
            }

            return true;
        }

        private string DataToHexString(byte[] binaryData, int length)
        {
            if (!this.isOpen)
                return null;

            var dataString = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                dataString.Append(binaryData[i].ToString("x2"));
                dataString.Append(",");
            }
            return dataString.ToString().TrimEnd(',');
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    this.file?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
