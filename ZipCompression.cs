/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Licensed under the Apache License, Version 2.0; you may not use this
 * file except in compliance with the License.  You may obtain a copy of the
 * License at https://opensource.org/licenses/Apache-2.0
 * -----------------------------------------------------*/

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Cyclops
{
    public static class ZipCompression
    {
        #region Methods
        /// <summary>
        /// Compresses the files in the nominated folder, and creates a zip file on disk named as outPathname.
        /// </summary>
        /// <param name="outputZipFilePath">Name of output zip file</param>
        /// <param name="password">Password to protect zip file</param>
        /// <param name="folderName">Directory to compress</param>
        public static void CompressFolder(string outputZipFilePath, string password, string folderName)
        {

            var fsOut = File.Create(outputZipFilePath);
            var zipStream = new ZipOutputStream(fsOut);

            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

            zipStream.Password = password;	// optional. Null is the same as not setting.

            // This setting will strip the leading part of the folder path in the entries, to
            // make the entries relative to the starting folder.
            // To include the full path for each entry up to the drive root, assign folderOffset = 0.
            var folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);

            ZipUpTheFolder(folderName, zipStream, folderOffset);

            zipStream.IsStreamOwner = true;	// Makes the Close also Close the underlying stream
            zipStream.Close();
        }

        /// <summary>
        /// Recurses down the folder structure
        /// </summary>
        /// <param name="path">Directory</param>
        /// <param name="zipStream">Path to zip file</param>
        /// <param name="folderOffset">Folder Offset</param>
        private static void ZipUpTheFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            var files = Directory.GetFiles(path);

            foreach (var filename in files)
            {

                var fi = new FileInfo(filename);

                var entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                var newEntry = new ZipEntry(entryName)
                {
                    // Note the zip format stores 2 second granularity
                    DateTime = fi.LastWriteTime,
                    Size = fi.Length
                };


                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                var buffer = new byte[4096];
                using (var streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            var folders = Directory.GetDirectories(path);
            foreach (var folder in folders)
            {
                ZipUpTheFolder(folder, zipStream, folderOffset);
            }
        }

        /// <summary>
        /// Extracts a zip file to a directory
        /// </summary>
        /// <param name="archiveFilenameIn">Zip file to extract</param>
        /// <param name="password">Password used to protect zip file</param>
        /// <param name="outFolder">Output directory to extract results to</param>
        public static void ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;		// AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;			// Ignore directories
                    }
                    var entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    var buffer = new byte[4096];		// 4K is optimum
                    var zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    var fullZipToPath = Path.Combine(outFolder, entryFileName);
                    var directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (!string.IsNullOrWhiteSpace(directoryName))
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }
        #endregion
    }
}
