// ZipFile.x-IEnumerable.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2006, 2007, 2008, 2009 Dino Chiesa and Microsoft Corporation.
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
// Time-stamp: <2009-December-26 15:13:26>
//
// ------------------------------------------------------------------
//
// This module defines smoe methods for IEnumerable support. It is
// particularly important for COM to have these things in a separate module.
//
// ------------------------------------------------------------------


namespace Ionic.Zip
{

    // For some weird reason, the method with the DispId(-4) attribute, which is used as
    // the _NewEnum() method, and which is required to get enumeration to work from COM
    // environments like VBScript and Javascript (etc) must be the LAST MEMBER in the
    // source.  In the event of Partial classes, it needs to be the last member defined
    // in the last source module.  The source modules are ordered alphabetically by
    // filename.  Not sure why this is true. In any case, we put the enumeration stuff
    // here in this oddly-named module, for this reason.
    //



    internal partial class ZipFile
    {




        /// <summary>
        /// Generic IEnumerator support, for use of a ZipFile in an enumeration.
        /// </summary>
        ///
        /// <remarks>
        /// You probably do not want to call <c>GetEnumerator</c> explicitly. Instead
        /// it is implicitly called when you use a <see langword="foreach"/> loop in C#, or a
        /// <c>For Each</c> loop in VB.NET.
        /// </remarks>
        ///
        /// <example>
        /// This example reads a zipfile of a given name, then enumerates the
        /// entries in that zip file, and displays the information about each
        /// entry on the Console.
        /// <code>
        /// using (ZipFile zip = ZipFile.Read(zipfile))
        /// {
        ///   bool header = true;
        ///   foreach (ZipEntry e in zip)
        ///   {
        ///     if (header)
        ///     {
        ///        System.Console.WriteLine("Zipfile: {0}", zip.Name);
        ///        System.Console.WriteLine("Version Needed: 0x{0:X2}", e.VersionNeeded);
        ///        System.Console.WriteLine("BitField: 0x{0:X2}", e.BitField);
        ///        System.Console.WriteLine("Compression Method: 0x{0:X2}", e.CompressionMethod);
        ///        System.Console.WriteLine("\n{1,-22} {2,-6} {3,4}   {4,-8}  {0}",
        ///                     "Filename", "Modified", "Size", "Ratio", "Packed");
        ///        System.Console.WriteLine(new System.String('-', 72));
        ///        header = false;
        ///     }
        ///
        ///     System.Console.WriteLine("{1,-22} {2,-6} {3,4:F0}%   {4,-8}  {0}",
        ///                 e.FileName,
        ///                 e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
        ///                 e.UncompressedSize,
        ///                 e.CompressionRatio,
        ///                 e.CompressedSize);
        ///
        ///     e.Extract();
        ///   }
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        ///   Dim ZipFileToExtract As String = "c:\foo.zip"
        ///   Using zip As ZipFile = ZipFile.Read(ZipFileToExtract)
        ///       Dim header As Boolean = True
        ///       Dim e As ZipEntry
        ///       For Each e In zip
        ///           If header Then
        ///               Console.WriteLine("Zipfile: {0}", zip.Name)
        ///               Console.WriteLine("Version Needed: 0x{0:X2}", e.VersionNeeded)
        ///               Console.WriteLine("BitField: 0x{0:X2}", e.BitField)
        ///               Console.WriteLine("Compression Method: 0x{0:X2}", e.CompressionMethod)
        ///               Console.WriteLine(ChrW(10) &amp; "{1,-22} {2,-6} {3,4}   {4,-8}  {0}", _
        ///                 "Filename", "Modified", "Size", "Ratio", "Packed" )
        ///               Console.WriteLine(New String("-"c, 72))
        ///               header = False
        ///           End If
        ///           Console.WriteLine("{1,-22} {2,-6} {3,4:F0}%   {4,-8}  {0}", _
        ///             e.FileName, _
        ///             e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"), _
        ///             e.UncompressedSize, _
        ///             e.CompressionRatio, _
        ///             e.CompressedSize )
        ///           e.Extract
        ///       Next
        ///   End Using
        /// </code>
        /// </example>
        ///
        /// <returns>A generic enumerator suitable for use  within a foreach loop.</returns>
        public System.Collections.Generic.IEnumerator<ZipEntry> GetEnumerator()
        {
            foreach (ZipEntry e in _entries.Values)
                yield return e;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// An IEnumerator, for use of a ZipFile in a foreach construct.
        /// </summary>
        ///
        /// <remarks>
        /// This method is included for COM support.  An application generally does not call
        /// this method directly.  It is called implicitly by COM clients when enumerating
        /// the entries in the ZipFile instance.  In VBScript, this is done with a <c>For Each</c>
        /// statement.  In Javascript, this is done with <c>new Enumerator(zipfile)</c>.
        /// </remarks>
        ///
        /// <returns>
        /// The IEnumerator over the entries in the ZipFile.
        /// </returns>
        [System.Runtime.InteropServices.DispId(-4)]
        public System.Collections.IEnumerator GetNewEnum()          // the name of this method is not significant
        {
            return GetEnumerator();
        }

    }
}
