// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;

namespace ThemeConverter.ColorCompiler
{
    /// <summary>
    /// Specialization of BinaryWriter that writes a versioned byte stream.
    /// </summary>
    internal class VersionedBinaryWriter : BinaryWriter
    {
        public VersionedBinaryWriter(Stream stream)
            : base(stream)
        { }

        /// <summary>
        /// Delegate that will write the body of the stream.
        /// </summary>
        /// <param name="reader">The VersionedBinaryWriter</param>
        /// <param name="version">The version of the stream.  It is for
        /// reference only; the delegate does not have to write it to
        /// the stream.</param>
        public delegate void WriteCallback(VersionedBinaryWriter writer, int version);

        /// <summary>
        /// Writes versioning header to a stream, the calls a delegate to write
        /// the meat of the data.
        /// </summary>
        /// <param name="version">Version number to write</param>
        /// <param name="callback">The delegate that will write the body of the stream</param>
        public void WriteVersioned(int version, WriteCallback callback)
        {
            // remember the starting stream seek position
            Stream stream = BaseStream;
            long startingPosition = stream.Position;

            // write a placeholder for the integer number of bytes we write; when we're
            // done we'll seek back and overwrite this with the correct value
            Write((int)-1);

            // now write the version
            Write(version);

            try
            {
                // let the delegate write the real data
                callback(this, version);
            }
            catch (Exception)
            {
                // rewind the stream to the starting position
                stream.Position = startingPosition;

                throw;
            }

            // go back and update the number of bytes we wrote
            long endingPosition = stream.Position;
            stream.Position = startingPosition;
            Write((int)(endingPosition - startingPosition));

            // return to the end of the stream
            stream.Position = endingPosition;
        }
    }
}
