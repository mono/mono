// ZipFile.Check.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 Dino Chiesa.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs):
// Time-stamp: <2011-July-31 14:40:50>
//
// ------------------------------------------------------------------
//
// This module defines the methods for doing Checks on zip files.
// These are not necessary to include in the Reduced or CF
// version of the library.
//
// ------------------------------------------------------------------
//


using System;
using System.IO;
using System.Collections.Generic;

namespace Ionic.Zip
{
    internal partial class ZipFile
    {
        /// <summary>
        ///   Checks a zip file to see if its directory is consistent.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   In cases of data error, the directory within a zip file can get out
        ///   of synch with the entries in the zip file.  This method checks the
        ///   given zip file and returns true if this has occurred.
        /// </para>
        ///
        /// <para> This method may take a long time to run for large zip files.  </para>
        ///
        /// <para>
        ///   This method is not supported in the Reduced or Compact Framework
        ///   versions of DotNetZip.
        /// </para>
        ///
        /// <para>
        ///   Developers using COM can use the <see
        ///   cref="ComHelper.CheckZip(String)">ComHelper.CheckZip(String)</see>
        ///   method.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="zipFileName">The filename to of the zip file to check.</param>
        ///
        /// <returns>true if the named zip file checks OK. Otherwise, false. </returns>
        ///
        /// <seealso cref="FixZipDirectory(string)"/>
        /// <seealso cref="CheckZip(string,bool,System.IO.TextWriter)"/>
        internal static bool CheckZip(string zipFileName)
        {
            return CheckZip(zipFileName, false, null);
        }


        /// <summary>
        ///   Checks a zip file to see if its directory is consistent,
        ///   and optionally fixes the directory if necessary.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   In cases of data error, the directory within a zip file can get out of
        ///   synch with the entries in the zip file.  This method checks the given
        ///   zip file, and returns true if this has occurred. It also optionally
        ///   fixes the zipfile, saving the fixed copy in <em>Name</em>_Fixed.zip.
        /// </para>
        ///
        /// <para>
        ///   This method may take a long time to run for large zip files.  It
        ///   will take even longer if the file actually needs to be fixed, and if
        ///   <c>fixIfNecessary</c> is true.
        /// </para>
        ///
        /// <para>
        ///   This method is not supported in the Reduced or Compact
        ///   Framework versions of DotNetZip.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="zipFileName">The filename to of the zip file to check.</param>
        ///
        /// <param name="fixIfNecessary">If true, the method will fix the zip file if
        ///     necessary.</param>
        ///
        /// <param name="writer">
        /// a TextWriter in which messages generated while checking will be written.
        /// </param>
        ///
        /// <returns>true if the named zip is OK; false if the file needs to be fixed.</returns>
        ///
        /// <seealso cref="CheckZip(string)"/>
        /// <seealso cref="FixZipDirectory(string)"/>
        internal static bool CheckZip(string zipFileName, bool fixIfNecessary,
                                    TextWriter writer)

        {
            ZipFile zip1 = null, zip2 = null;
            bool isOk = true;
            try
            {
                zip1 = new ZipFile();
                zip1.FullScan = true;
                zip1.Initialize(zipFileName);

                zip2 = ZipFile.Read(zipFileName);

                foreach (var e1 in zip1)
                {
                    foreach (var e2 in zip2)
                    {
                        if (e1.FileName == e2.FileName)
                        {
                            if (e1._RelativeOffsetOfLocalHeader != e2._RelativeOffsetOfLocalHeader)
                            {
                                isOk = false;
                                if (writer != null)
                                writer.WriteLine("{0}: mismatch in RelativeOffsetOfLocalHeader  (0x{1:X16} != 0x{2:X16})",
                                                        e1.FileName, e1._RelativeOffsetOfLocalHeader,
                                                        e2._RelativeOffsetOfLocalHeader);
                            }
                            if (e1._CompressedSize != e2._CompressedSize)
                            {
                                isOk = false;
                                if (writer != null)
                                writer.WriteLine("{0}: mismatch in CompressedSize  (0x{1:X16} != 0x{2:X16})",
                                                        e1.FileName, e1._CompressedSize,
                                                        e2._CompressedSize);
                            }
                            if (e1._UncompressedSize != e2._UncompressedSize)
                            {
                                isOk = false;
                                if (writer != null)
                                writer.WriteLine("{0}: mismatch in UncompressedSize  (0x{1:X16} != 0x{2:X16})",
                                                        e1.FileName, e1._UncompressedSize,
                                                        e2._UncompressedSize);
                            }
                            if (e1.CompressionMethod != e2.CompressionMethod)
                            {
                                isOk = false;
                                if (writer != null)
                                writer.WriteLine("{0}: mismatch in CompressionMethod  (0x{1:X4} != 0x{2:X4})",
                                                        e1.FileName, e1.CompressionMethod,
                                                        e2.CompressionMethod);
                            }
                            if (e1.Crc != e2.Crc)
                            {
                                isOk = false;
                                if (writer != null)
                                writer.WriteLine("{0}: mismatch in Crc32  (0x{1:X4} != 0x{2:X4})",
                                                        e1.FileName, e1.Crc,
                                                        e2.Crc);
                            }

                            // found a match, so stop the inside loop
                            break;
                        }
                    }
                }

                zip2.Dispose();
                zip2 = null;

                if (!isOk && fixIfNecessary)
                {
                    string newFileName = Path.GetFileNameWithoutExtension(zipFileName);
                    newFileName = System.String.Format("{0}_fixed.zip", newFileName);
                    zip1.Save(newFileName);
                }
            }
            finally
            {
                if (zip1 != null) zip1.Dispose();
                if (zip2 != null) zip2.Dispose();
            }
            return isOk;
        }



        /// <summary>
        ///   Rewrite the directory within a zipfile.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   In cases of data error, the directory in a zip file can get out of
        ///   synch with the entries in the zip file.  This method attempts to fix
        ///   the zip file if this has occurred.
        /// </para>
        ///
        /// <para> This can take a long time for large zip files. </para>
        ///
        /// <para> This won't work if the zip file uses a non-standard
        /// code page - neither IBM437 nor UTF-8. </para>
        ///
        /// <para>
        ///   This method is not supported in the Reduced or Compact Framework
        ///   versions of DotNetZip.
        /// </para>
        ///
        /// <para>
        ///   Developers using COM can use the <see
        ///   cref="ComHelper.FixZipDirectory(String)">ComHelper.FixZipDirectory(String)</see>
        ///   method.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="zipFileName">The filename to of the zip file to fix.</param>
        ///
        /// <seealso cref="CheckZip(string)"/>
        /// <seealso cref="CheckZip(string,bool,System.IO.TextWriter)"/>
        internal static void FixZipDirectory(string zipFileName)
        {
            using (var zip = new ZipFile())
            {
                zip.FullScan = true;
                zip.Initialize(zipFileName);
                zip.Save(zipFileName);
            }
        }



        /// <summary>
        ///   Verify the password on a zip file.
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///     Keep in mind that passwords in zipfiles are applied to
        ///     zip entries, not to the entire zip file. So testing a
        ///     zipfile for a particular password doesn't work in the
        ///     general case. On the other hand, it's often the case
        ///     that a single password will be used on all entries in a
        ///     zip file. This method works for that case.
        ///   </para>
        ///   <para>
        ///     There is no way to check a password without doing the
        ///     decryption. So this code decrypts and extracts the given
        ///     zipfile into <see cref="System.IO.Stream.Null"/>
        ///   </para>
        /// </remarks>
        ///
        /// <param name="zipFileName">The filename to of the zip file to fix.</param>
        ///
        /// <param name="password">The password to check.</param>
        ///
        /// <returns>a bool indicating whether the password matches.</returns>
        internal static bool CheckZipPassword(string zipFileName, string password)
        {
            // workitem 13664
            bool success = false;
            try
            {
                using (ZipFile zip1 = ZipFile.Read(zipFileName))
                {
                    foreach (var e in zip1)
                    {
                        if (!e.IsDirectory && e.UsesEncryption)
                        {
                            e.ExtractWithPassword(System.IO.Stream.Null, password);
                        }
                    }
                }
                success = true;
            }
            catch(Ionic.Zip.BadPasswordException) { }
            return success;
        }


        /// <summary>
        ///   Provides a human-readable string with information about the ZipFile.
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///     The information string contains 10 lines or so, about each ZipEntry,
        ///     describing whether encryption is in use, the compressed and uncompressed
        ///     length of the entry, the offset of the entry, and so on. As a result the
        ///     information string can be very long for zip files that contain many
        ///     entries.
        ///   </para>
        ///   <para>
        ///     This information is mostly useful for diagnostic purposes.
        ///   </para>
        /// </remarks>
        public string Info
        {
            get
            {
                var builder = new System.Text.StringBuilder();
                builder.Append(string.Format("          ZipFile: {0}\n", this.Name));
                if (!string.IsNullOrEmpty(this._Comment))
                {
                    builder.Append(string.Format("          Comment: {0}\n", this._Comment));
                }
                if (this._versionMadeBy != 0)
                {
                    builder.Append(string.Format("  version made by: 0x{0:X4}\n", this._versionMadeBy));
                }
                if (this._versionNeededToExtract != 0)
                {
                    builder.Append(string.Format("needed to extract: 0x{0:X4}\n", this._versionNeededToExtract));
                }

                builder.Append(string.Format("       uses ZIP64: {0}\n", this.InputUsesZip64));

                builder.Append(string.Format("     disk with CD: {0}\n", this._diskNumberWithCd));
                if (this._OffsetOfCentralDirectory == 0xFFFFFFFF)
                    builder.Append(string.Format("      CD64 offset: 0x{0:X16}\n", this._OffsetOfCentralDirectory64));
                else
                    builder.Append(string.Format("        CD offset: 0x{0:X8}\n", this._OffsetOfCentralDirectory));
                builder.Append("\n");
                foreach (ZipEntry entry in this._entries.Values)
                {
                    builder.Append(entry.Info);
                }
                return builder.ToString();
            }
        }


    }

}