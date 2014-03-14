// ZipOutputStream.cs
//
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa.
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
// Time-stamp: <2011-July-28 06:34:30>
//
// ------------------------------------------------------------------
//
// This module defines the ZipOutputStream class, which is a stream metaphor for
// generating zip files.  This class does not depend on Ionic.Zip.ZipFile, but rather
// stands alongside it as an alternative "container" for ZipEntry.  It replicates a
// subset of the properties, including these:
//
//  - Comment
//  - Encryption
//  - Password
//  - CodecBufferSize
//  - CompressionLevel
//  - CompressionMethod
//  - EnableZip64 (UseZip64WhenSaving)
//  - IgnoreCase (!CaseSensitiveRetrieval)
//
// It adds these novel methods:
//
//  - PutNextEntry
//
//
// ------------------------------------------------------------------
//

using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

namespace Ionic.Zip
{
    /// <summary>
    ///   Provides a stream metaphor for generating zip files.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    ///   This class writes zip files, as defined in the <see
    ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">specification
    ///   for zip files described by PKWare</see>.  The compression for this
    ///   implementation is provided by a managed-code version of Zlib, included with
    ///   DotNetZip in the classes in the Ionic.Zlib namespace.
    /// </para>
    ///
    /// <para>
    ///   This class provides an alternative programming model to the one enabled by the
    ///   <see cref="ZipFile"/> class. Use this when creating zip files, as an
    ///   alternative to the <see cref="ZipFile"/> class, when you would like to use a
    ///   <c>Stream</c> type to write the zip file.
    /// </para>
    ///
    /// <para>
    ///   Both the <c>ZipOutputStream</c> class and the <c>ZipFile</c> class can be used
    ///   to create zip files. Both of them support many of the common zip features,
    ///   including Unicode, different compression levels, and ZIP64.   They provide
    ///   very similar performance when creating zip files.
    /// </para>
    ///
    /// <para>
    ///   The <c>ZipFile</c> class is generally easier to use than
    ///   <c>ZipOutputStream</c> and should be considered a higher-level interface.  For
    ///   example, when creating a zip file via calls to the <c>PutNextEntry()</c> and
    ///   <c>Write()</c> methods on the <c>ZipOutputStream</c> class, the caller is
    ///   responsible for opening the file, reading the bytes from the file, writing
    ///   those bytes into the <c>ZipOutputStream</c>, setting the attributes on the
    ///   <c>ZipEntry</c>, and setting the created, last modified, and last accessed
    ///   timestamps on the zip entry. All of these things are done automatically by a
    ///   call to <see cref="ZipFile.AddFile(string,string)">ZipFile.AddFile()</see>.
    ///   For this reason, the <c>ZipOutputStream</c> is generally recommended for use
    ///   only when your application emits arbitrary data, not necessarily data from a
    ///   filesystem file, directly into a zip file, and does so using a <c>Stream</c>
    ///   metaphor.
    /// </para>
    ///
    /// <para>
    ///   Aside from the differences in programming model, there are other
    ///   differences in capability between the two classes.
    /// </para>
    ///
    /// <list type="bullet">
    ///   <item>
    ///     <c>ZipFile</c> can be used to read and extract zip files, in addition to
    ///     creating zip files. <c>ZipOutputStream</c> cannot read zip files. If you want
    ///     to use a stream to read zip files, check out the <see cref="ZipInputStream"/> class.
    ///   </item>
    ///
    ///   <item>
    ///     <c>ZipOutputStream</c> does not support the creation of segmented or spanned
    ///     zip files.
    ///   </item>
    ///
    ///   <item>
    ///     <c>ZipOutputStream</c> cannot produce a self-extracting archive.
    ///   </item>
    /// </list>
    ///
    /// <para>
    ///   Be aware that the <c>ZipOutputStream</c> class implements the <see
    ///   cref="System.IDisposable"/> interface.  In order for
    ///   <c>ZipOutputStream</c> to produce a valid zip file, you use use it within
    ///   a using clause (<c>Using</c> in VB), or call the <c>Dispose()</c> method
    ///   explicitly.  See the examples for how to employ a using clause.
    /// </para>
    ///
    /// <para>
    ///   Also, a note regarding compression performance: On the desktop .NET
    ///   Framework, DotNetZip can use a multi-threaded compression implementation
    ///   that provides significant speed increases on large files, over 300k or so,
    ///   at the cost of increased memory use at runtime.  (The output of the
    ///   compression is almost exactly the same size).  But, the multi-threaded
    ///   approach incurs a performance hit on smaller files. There's no way for the
    ///   ZipOutputStream to know whether parallel compression will be beneficial,
    ///   because the ZipOutputStream does not know how much data you will write
    ///   through the stream.  You may wish to set the <see
    ///   cref="ParallelDeflateThreshold"/> property to zero, if you are compressing
    ///   large files through <c>ZipOutputStream</c>.  This will cause parallel
    ///   compression to be used, always.
    /// </para>
    /// </remarks>
    internal class ZipOutputStream : Stream
    {
        /// <summary>
        ///   Create a ZipOutputStream, wrapping an existing stream.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The <see cref="ZipFile"/> class is generally easier to use when creating
        ///   zip files. The ZipOutputStream offers a different metaphor for creating a
        ///   zip file, based on the <see cref="System.IO.Stream"/> class.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="stream">
        /// The stream to wrap. It must be writable. This stream will be closed at
        /// the time the ZipOutputStream is closed.
        /// </param>
        ///
        /// <example>
        ///
        ///   This example shows how to create a zip file, using the
        ///   ZipOutputStream class.
        ///
        /// <code lang="C#">
        /// private void Zipup()
        /// {
        ///     if (filesToZip.Count == 0)
        ///     {
        ///         System.Console.WriteLine("Nothing to do.");
        ///         return;
        ///     }
        ///
        ///     using (var raw = File.Open(_outputFileName, FileMode.Create, FileAccess.ReadWrite ))
        ///     {
        ///         using (var output= new ZipOutputStream(raw))
        ///         {
        ///             output.Password = "VerySecret!";
        ///             output.Encryption = EncryptionAlgorithm.WinZipAes256;
        ///
        ///             foreach (string inputFileName in filesToZip)
        ///             {
        ///                 System.Console.WriteLine("file: {0}", inputFileName);
        ///
        ///                 output.PutNextEntry(inputFileName);
        ///                 using (var input = File.Open(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Write ))
        ///                 {
        ///                     byte[] buffer= new byte[2048];
        ///                     int n;
        ///                     while ((n= input.Read(buffer,0,buffer.Length)) > 0)
        ///                     {
        ///                         output.Write(buffer,0,n);
        ///                     }
        ///                 }
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Private Sub Zipup()
        ///     Dim outputFileName As String = "XmlData.zip"
        ///     Dim filesToZip As String() = Directory.GetFiles(".", "*.xml")
        ///     If (filesToZip.Length = 0) Then
        ///         Console.WriteLine("Nothing to do.")
        ///     Else
        ///         Using raw As FileStream = File.Open(outputFileName, FileMode.Create, FileAccess.ReadWrite)
        ///             Using output As ZipOutputStream = New ZipOutputStream(raw)
        ///                 output.Password = "VerySecret!"
        ///                 output.Encryption = EncryptionAlgorithm.WinZipAes256
        ///                 Dim inputFileName As String
        ///                 For Each inputFileName In filesToZip
        ///                     Console.WriteLine("file: {0}", inputFileName)
        ///                     output.PutNextEntry(inputFileName)
        ///                     Using input As FileStream = File.Open(inputFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        ///                         Dim n As Integer
        ///                         Dim buffer As Byte() = New Byte(2048) {}
        ///                         Do While (n = input.Read(buffer, 0, buffer.Length) > 0)
        ///                             output.Write(buffer, 0, n)
        ///                         Loop
        ///                     End Using
        ///                 Next
        ///             End Using
        ///         End Using
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public ZipOutputStream(Stream stream) : this(stream, false) { }


        /// <summary>
        ///   Create a ZipOutputStream that writes to a filesystem file.
        /// </summary>
        ///
        /// <remarks>
        ///   The <see cref="ZipFile"/> class is generally easier to use when creating
        ///   zip files. The ZipOutputStream offers a different metaphor for creating a
        ///   zip file, based on the <see cref="System.IO.Stream"/> class.
        /// </remarks>
        ///
        /// <param name="fileName">
        ///   The name of the zip file to create.
        /// </param>
        ///
        /// <example>
        ///
        ///   This example shows how to create a zip file, using the
        ///   ZipOutputStream class.
        ///
        /// <code lang="C#">
        /// private void Zipup()
        /// {
        ///     if (filesToZip.Count == 0)
        ///     {
        ///         System.Console.WriteLine("Nothing to do.");
        ///         return;
        ///     }
        ///
        ///     using (var output= new ZipOutputStream(outputFileName))
        ///     {
        ///         output.Password = "VerySecret!";
        ///         output.Encryption = EncryptionAlgorithm.WinZipAes256;
        ///
        ///         foreach (string inputFileName in filesToZip)
        ///         {
        ///             System.Console.WriteLine("file: {0}", inputFileName);
        ///
        ///             output.PutNextEntry(inputFileName);
        ///             using (var input = File.Open(inputFileName, FileMode.Open, FileAccess.Read,
        ///                                          FileShare.Read | FileShare.Write ))
        ///             {
        ///                 byte[] buffer= new byte[2048];
        ///                 int n;
        ///                 while ((n= input.Read(buffer,0,buffer.Length)) > 0)
        ///                 {
        ///                     output.Write(buffer,0,n);
        ///                 }
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Private Sub Zipup()
        ///     Dim outputFileName As String = "XmlData.zip"
        ///     Dim filesToZip As String() = Directory.GetFiles(".", "*.xml")
        ///     If (filesToZip.Length = 0) Then
        ///         Console.WriteLine("Nothing to do.")
        ///     Else
        ///         Using output As ZipOutputStream = New ZipOutputStream(outputFileName)
        ///             output.Password = "VerySecret!"
        ///             output.Encryption = EncryptionAlgorithm.WinZipAes256
        ///             Dim inputFileName As String
        ///             For Each inputFileName In filesToZip
        ///                 Console.WriteLine("file: {0}", inputFileName)
        ///                 output.PutNextEntry(inputFileName)
        ///                 Using input As FileStream = File.Open(inputFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        ///                     Dim n As Integer
        ///                     Dim buffer As Byte() = New Byte(2048) {}
        ///                     Do While (n = input.Read(buffer, 0, buffer.Length) > 0)
        ///                         output.Write(buffer, 0, n)
        ///                     Loop
        ///                 End Using
        ///             Next
        ///         End Using
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public ZipOutputStream(String fileName)
        {
            Stream stream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            _Init(stream, false, fileName);
        }


        /// <summary>
        ///   Create a ZipOutputStream.
        /// </summary>
        ///
        /// <remarks>
        ///   See the documentation for the <see
        ///   cref="ZipOutputStream(Stream)">ZipOutputStream(Stream)</see>
        ///   constructor for an example.
        /// </remarks>
        ///
        /// <param name="stream">
        ///   The stream to wrap. It must be writable.
        /// </param>
        ///
        /// <param name="leaveOpen">
        ///   true if the application would like the stream
        ///   to remain open after the <c>ZipOutputStream</c> has been closed.
        /// </param>
        public ZipOutputStream(Stream stream, bool leaveOpen)
        {
            _Init(stream, leaveOpen, null);
        }

        private void _Init(Stream stream, bool leaveOpen, string name)
        {
            // workitem 9307
            _outputStream = stream.CanRead ? stream : new CountingStream(stream);
            CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
            CompressionMethod = Ionic.Zip.CompressionMethod.Deflate;
            _encryption = EncryptionAlgorithm.None;
            _entriesWritten = new Dictionary<String, ZipEntry>(StringComparer.Ordinal);
            _zip64 = Zip64Option.Never;
            _leaveUnderlyingStreamOpen = leaveOpen;
            Strategy = Ionic.Zlib.CompressionStrategy.Default;
            _name = name ?? "(stream)";
#if !NETCF
            ParallelDeflateThreshold = -1L;
#endif
        }


        /// <summary>Provides a string representation of the instance.</summary>
        /// <remarks>
        ///   <para>
        ///     This can be useful for debugging purposes.
        ///   </para>
        /// </remarks>
        /// <returns>a string representation of the instance.</returns>
        public override String ToString()
        {
            return String.Format ("ZipOutputStream::{0}(leaveOpen({1})))", _name, _leaveUnderlyingStreamOpen);
        }


        /// <summary>
        ///   Sets the password to be used on the <c>ZipOutputStream</c> instance.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   When writing a zip archive, this password is applied to the entries, not
        ///   to the zip archive itself. It applies to any <c>ZipEntry</c> subsequently
        ///   written to the <c>ZipOutputStream</c>.
        /// </para>
        ///
        /// <para>
        ///   Using a password does not encrypt or protect the "directory" of the
        ///   archive - the list of entries contained in the archive.  If you set the
        ///   <c>Password</c> property, the password actually applies to individual
        ///   entries that are added to the archive, subsequent to the setting of this
        ///   property.  The list of filenames in the archive that is eventually created
        ///   will appear in clear text, but the contents of the individual files are
        ///   encrypted.  This is how Zip encryption works.
        /// </para>
        ///
        /// <para>
        ///   If you set this property, and then add a set of entries to the archive via
        ///   calls to <c>PutNextEntry</c>, then each entry is encrypted with that
        ///   password.  You may also want to change the password between adding
        ///   different entries. If you set the password, add an entry, then set the
        ///   password to <c>null</c> (<c>Nothing</c> in VB), and add another entry, the
        ///   first entry is encrypted and the second is not.
        /// </para>
        ///
        /// <para>
        ///   When setting the <c>Password</c>, you may also want to explicitly set the <see
        ///   cref="Encryption"/> property, to specify how to encrypt the entries added
        ///   to the ZipFile.  If you set the <c>Password</c> to a non-null value and do not
        ///   set <see cref="Encryption"/>, then PKZip 2.0 ("Weak") encryption is used.
        ///   This encryption is relatively weak but is very interoperable. If
        ///   you set the password to a <c>null</c> value (<c>Nothing</c> in VB),
        ///   <c>Encryption</c> is reset to None.
        /// </para>
        ///
        /// <para>
        ///   Special case: if you wrap a ZipOutputStream around a non-seekable stream,
        ///   and use encryption, and emit an entry of zero bytes, the <c>Close()</c> or
        ///   <c>PutNextEntry()</c> following the entry will throw an exception.
        /// </para>
        ///
        /// </remarks>
        public String Password
        {
            set
            {
                if (_disposed)
                {
                    _exceptionPending = true;
                    throw new System.InvalidOperationException("The stream has been closed.");
                }

                _password = value;
                if (_password == null)
                {
                    _encryption = EncryptionAlgorithm.None;
                }
                else if (_encryption == EncryptionAlgorithm.None)
                {
                    _encryption = EncryptionAlgorithm.PkzipWeak;
                }
            }
        }


        /// <summary>
        ///   The Encryption to use for entries added to the <c>ZipOutputStream</c>.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The specified Encryption is applied to the entries subsequently
        ///   written to the <c>ZipOutputStream</c> instance.
        /// </para>
        ///
        /// <para>
        ///   If you set this to something other than
        ///   EncryptionAlgorithm.None, you will also need to set the
        ///   <see cref="Password"/> to a non-null, non-empty value in
        ///   order to actually get encryption on the entry.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <seealso cref="Password">ZipOutputStream.Password</seealso>
        /// <seealso cref="Ionic.Zip.ZipEntry.Encryption">ZipEntry.Encryption</seealso>
        public EncryptionAlgorithm Encryption
        {
            get
            {
                return _encryption;
            }
            set
            {
                if (_disposed)
                {
                    _exceptionPending = true;
                    throw new System.InvalidOperationException("The stream has been closed.");
                }
                if (value == EncryptionAlgorithm.Unsupported)
                {
                    _exceptionPending = true;
                    throw new InvalidOperationException("You may not set Encryption to that value.");
                }
                _encryption = value;
            }
        }


        /// <summary>
        ///   Size of the work buffer to use for the ZLIB codec during compression.
        /// </summary>
        ///
        /// <remarks>
        ///   Setting this may affect performance.  For larger files, setting this to a
        ///   larger size may improve performance, but I'm not sure.  Sorry, I don't
        ///   currently have good recommendations on how to set it.  You can test it if
        ///   you like.
        /// </remarks>
        public int CodecBufferSize
        {
            get;
            set;
        }


        /// <summary>
        ///   The compression strategy to use for all entries.
        /// </summary>
        ///
        /// <remarks>
        ///   Set the Strategy used by the ZLIB-compatible compressor, when compressing
        ///   data for the entries in the zip archive. Different compression strategies
        ///   work better on different sorts of data. The strategy parameter can affect
        ///   the compression ratio and the speed of compression but not the correctness
        ///   of the compresssion.  For more information see <see
        ///   cref="Ionic.Zlib.CompressionStrategy "/>.
        /// </remarks>
        public Ionic.Zlib.CompressionStrategy Strategy
        {
            get;
            set;
        }


        /// <summary>
        ///   The type of timestamp attached to the ZipEntry.
        /// </summary>
        ///
        /// <remarks>
        ///   Set this in order to specify the kind of timestamp that should be emitted
        ///   into the zip file for each entry.
        /// </remarks>
        public ZipEntryTimestamp Timestamp
        {
            get
            {
                return _timestamp;
            }
            set
            {
                if (_disposed)
                {
                    _exceptionPending = true;
                    throw new System.InvalidOperationException("The stream has been closed.");
                }
                _timestamp = value;
            }
        }


        /// <summary>
        ///   Sets the compression level to be used for entries subsequently added to
        ///   the zip archive.
        /// </summary>
        ///
        /// <remarks>
        ///  <para>
        ///    Varying the compression level used on entries can affect the
        ///    size-vs-speed tradeoff when compression and decompressing data streams
        ///    or files.
        ///  </para>
        ///
        ///  <para>
        ///    As with some other properties on the <c>ZipOutputStream</c> class, like <see
        ///    cref="Password"/>, and <see cref="Encryption"/>,
        ///    setting this property on a <c>ZipOutputStream</c>
        ///    instance will cause the specified <c>CompressionLevel</c> to be used on all
        ///    <see cref="ZipEntry"/> items that are subsequently added to the
        ///    <c>ZipOutputStream</c> instance.
        ///  </para>
        ///
        ///  <para>
        ///    If you do not set this property, the default compression level is used,
        ///    which normally gives a good balance of compression efficiency and
        ///    compression speed.  In some tests, using <c>BestCompression</c> can
        ///    double the time it takes to compress, while delivering just a small
        ///    increase in compression efficiency.  This behavior will vary with the
        ///    type of data you compress.  If you are in doubt, just leave this setting
        ///    alone, and accept the default.
        ///  </para>
        /// </remarks>
        public Ionic.Zlib.CompressionLevel CompressionLevel
        {
            get;
            set;
        }

        /// <summary>
        ///   The compression method used on each entry added to the ZipOutputStream.
        /// </summary>
        public Ionic.Zip.CompressionMethod CompressionMethod
        {
            get;
            set;
        }


        /// <summary>
        ///   A comment attached to the zip archive.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   The application sets this property to specify a comment to be embedded
        ///   into the generated zip archive.
        /// </para>
        ///
        /// <para>
        ///   According to <see
        ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">PKWARE's
        ///   zip specification</see>, the comment is not encrypted, even if there is a
        ///   password set on the zip file.
        /// </para>
        ///
        /// <para>
        ///   The specification does not describe how to indicate the encoding used
        ///   on a comment string. Many "compliant" zip tools and libraries use
        ///   IBM437 as the code page for comments; DotNetZip, too, follows that
        ///   practice.  On the other hand, there are situations where you want a
        ///   Comment to be encoded with something else, for example using code page
        ///   950 "Big-5 Chinese". To fill that need, DotNetZip will encode the
        ///   comment following the same procedure it follows for encoding
        ///   filenames: (a) if <see cref="AlternateEncodingUsage"/> is
        ///   <c>Never</c>, it uses the default encoding (IBM437). (b) if <see
        ///   cref="AlternateEncodingUsage"/> is <c>Always</c>, it always uses the
        ///   alternate encoding (<see cref="AlternateEncoding"/>). (c) if <see
        ///   cref="AlternateEncodingUsage"/> is <c>AsNecessary</c>, it uses the
        ///   alternate encoding only if the default encoding is not sufficient for
        ///   encoding the comment - in other words if decoding the result does not
        ///   produce the original string.  This decision is taken at the time of
        ///   the call to <c>ZipFile.Save()</c>.
        /// </para>
        ///
        /// </remarks>
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_disposed)
                {
                    _exceptionPending = true;
                    throw new System.InvalidOperationException("The stream has been closed.");
                }
                _comment = value;
            }
        }



        /// <summary>
        ///   Specify whether to use ZIP64 extensions when saving a zip archive.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The default value for the property is <see
        ///   cref="Zip64Option.Never"/>. <see cref="Zip64Option.AsNecessary"/> is
        ///   safest, in the sense that you will not get an Exception if a
        ///   pre-ZIP64 limit is exceeded.
        /// </para>
        ///
        /// <para>
        ///   You must set this property before calling <c>Write()</c>.
        /// </para>
        ///
        /// </remarks>
        public Zip64Option EnableZip64
        {
            get
            {
                return _zip64;
            }
            set
            {
                if (_disposed)
                {
                    _exceptionPending = true;
                    throw new System.InvalidOperationException("The stream has been closed.");
                }
                _zip64 = value;
            }
        }


        /// <summary>
        ///   Indicates whether ZIP64 extensions were used when saving the zip archive.
        /// </summary>
        ///
        /// <remarks>
        ///   The value is defined only after the <c>ZipOutputStream</c> has been closed.
        /// </remarks>
        public bool OutputUsedZip64
        {
            get
            {
                return _anyEntriesUsedZip64 || _directoryNeededZip64;
            }
        }


        /// <summary>
        ///   Whether the ZipOutputStream should use case-insensitive comparisons when
        ///   checking for uniqueness of zip entries.
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///   Though the zip specification doesn't prohibit zipfiles with duplicate
        ///   entries, Sane zip files have no duplicates, and the DotNetZip library
        ///   cannot create zip files with duplicate entries. If an application attempts
        ///   to call <see cref="PutNextEntry(String)"/> with a name that duplicates one
        ///   already used within the archive, the library will throw an Exception.
        ///   </para>
        ///   <para>
        ///   This property allows the application to specify whether the
        ///   ZipOutputStream instance considers ordinal case when checking for
        ///   uniqueness of zip entries.
        ///   </para>
        /// </remarks>
        public bool IgnoreCase
        {
          get
          {
              return !_DontIgnoreCase;
          }

          set
          {
              _DontIgnoreCase = !value;
          }

        }


        /// <summary>
        ///   Indicates whether to encode entry filenames and entry comments using
        ///   Unicode (UTF-8).
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   <see href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">The
        ///   PKWare zip specification</see> provides for encoding file names and file
        ///   comments in either the IBM437 code page, or in UTF-8.  This flag selects
        ///   the encoding according to that specification.  By default, this flag is
        ///   false, and filenames and comments are encoded into the zip file in the
        ///   IBM437 codepage.  Setting this flag to true will specify that filenames
        ///   and comments that cannot be encoded with IBM437 will be encoded with
        ///   UTF-8.
        /// </para>
        ///
        /// <para>
        ///   Zip files created with strict adherence to the PKWare specification with
        ///   respect to UTF-8 encoding can contain entries with filenames containing
        ///   any combination of Unicode characters, including the full range of
        ///   characters from Chinese, Latin, Hebrew, Greek, Cyrillic, and many other
        ///   alphabets.  However, because at this time, the UTF-8 portion of the PKWare
        ///   specification is not broadly supported by other zip libraries and
        ///   utilities, such zip files may not be readable by your favorite zip tool or
        ///   archiver. In other words, interoperability will decrease if you set this
        ///   flag to true.
        /// </para>
        ///
        /// <para>
        ///   In particular, Zip files created with strict adherence to the PKWare
        ///   specification with respect to UTF-8 encoding will not work well with
        ///   Explorer in Windows XP or Windows Vista, because Windows compressed
        ///   folders, as far as I know, do not support UTF-8 in zip files.  Vista can
        ///   read the zip files, but shows the filenames incorrectly. Unpacking from
        ///   Windows Vista Explorer will result in filenames that have rubbish
        ///   characters in place of the high-order UTF-8 bytes.
        /// </para>
        ///
        /// <para>
        ///   Also, zip files that use UTF-8 encoding will not work well with Java
        ///   applications that use the java.util.zip classes, as of v5.0 of the Java
        ///   runtime. The Java runtime does not correctly implement the PKWare
        ///   specification in this regard.
        /// </para>
        ///
        /// <para>
        ///   As a result, we have the unfortunate situation that "correct" behavior by
        ///   the DotNetZip library with regard to Unicode encoding of filenames during
        ///   zip creation will result in zip files that are readable by strictly
        ///   compliant and current tools (for example the most recent release of the
        ///   commercial WinZip tool); but these zip files will not be readable by
        ///   various other tools or libraries, including Windows Explorer.
        /// </para>
        ///
        /// <para>
        ///   The DotNetZip library can read and write zip files with UTF8-encoded
        ///   entries, according to the PKware spec.  If you use DotNetZip for both
        ///   creating and reading the zip file, and you use UTF-8, there will be no
        ///   loss of information in the filenames. For example, using a self-extractor
        ///   created by this library will allow you to unpack files correctly with no
        ///   loss of information in the filenames.
        /// </para>
        ///
        /// <para>
        ///   If you do not set this flag, it will remain false.  If this flag is false,
        ///   the <c>ZipOutputStream</c> will encode all filenames and comments using
        ///   the IBM437 codepage.  This can cause "loss of information" on some
        ///   filenames, but the resulting zipfile will be more interoperable with other
        ///   utilities. As an example of the loss of information, diacritics can be
        ///   lost.  The o-tilde character will be down-coded to plain o.  The c with a
        ///   cedilla (Unicode 0xE7) used in Portugese will be downcoded to a c.
        ///   Likewise, the O-stroke character (Unicode 248), used in Danish and
        ///   Norwegian, will be down-coded to plain o. Chinese characters cannot be
        ///   represented in codepage IBM437; when using the default encoding, Chinese
        ///   characters in filenames will be represented as ?. These are all examples
        ///   of "information loss".
        /// </para>
        ///
        /// <para>
        ///   The loss of information associated to the use of the IBM437 encoding is
        ///   inconvenient, and can also lead to runtime errors. For example, using
        ///   IBM437, any sequence of 4 Chinese characters will be encoded as ????.  If
        ///   your application creates a <c>ZipOutputStream</c>, does not set the
        ///   encoding, then adds two files, each with names of four Chinese characters
        ///   each, this will result in a duplicate filename exception.  In the case
        ///   where you add a single file with a name containing four Chinese
        ///   characters, the zipfile will save properly, but extracting that file
        ///   later, with any zip tool, will result in an error, because the question
        ///   mark is not legal for use within filenames on Windows.  These are just a
        ///   few examples of the problems associated to loss of information.
        /// </para>
        ///
        /// <para>
        ///   This flag is independent of the encoding of the content within the entries
        ///   in the zip file. Think of the zip file as a container - it supports an
        ///   encoding.  Within the container are other "containers" - the file entries
        ///   themselves.  The encoding within those entries is independent of the
        ///   encoding of the zip archive container for those entries.
        /// </para>
        ///
        /// <para>
        ///   Rather than specify the encoding in a binary fashion using this flag, an
        ///   application can specify an arbitrary encoding via the <see
        ///   cref="ProvisionalAlternateEncoding"/> property.  Setting the encoding
        ///   explicitly when creating zip archives will result in non-compliant zip
        ///   files that, curiously, are fairly interoperable.  The challenge is, the
        ///   PKWare specification does not provide for a way to specify that an entry
        ///   in a zip archive uses a code page that is neither IBM437 nor UTF-8.
        ///   Therefore if you set the encoding explicitly when creating a zip archive,
        ///   you must take care upon reading the zip archive to use the same code page.
        ///   If you get it wrong, the behavior is undefined and may result in incorrect
        ///   filenames, exceptions, stomach upset, hair loss, and acne.
        /// </para>
        /// </remarks>
        /// <seealso cref="ProvisionalAlternateEncoding"/>
        [Obsolete("Beginning with v1.9.1.6 of DotNetZip, this property is obsolete. It will be removed in a future version of the library. Use AlternateEncoding and AlternateEncodingUsage instead.")]
        public bool UseUnicodeAsNecessary
        {
            get
            {
                return (_alternateEncoding == System.Text.Encoding.UTF8) &&
                    (AlternateEncodingUsage == ZipOption.AsNecessary);
            }
            set
            {
                if (value)
                {
                    _alternateEncoding = System.Text.Encoding.UTF8;
                    _alternateEncodingUsage = ZipOption.AsNecessary;

                }
                else
                {
                    _alternateEncoding = Ionic.Zip.ZipOutputStream.DefaultEncoding;
                    _alternateEncodingUsage = ZipOption.Never;
                }
            }
        }


        /// <summary>
        ///   The text encoding to use when emitting entries into the zip archive, for
        ///   those entries whose filenames or comments cannot be encoded with the
        ///   default (IBM437) encoding.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   In <see href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">its
        ///   zip specification</see>, PKWare describes two options for encoding
        ///   filenames and comments: using IBM437 or UTF-8.  But, some archiving tools
        ///   or libraries do not follow the specification, and instead encode
        ///   characters using the system default code page.  For example, WinRAR when
        ///   run on a machine in Shanghai may encode filenames with the Big-5 Chinese
        ///   (950) code page.  This behavior is contrary to the Zip specification, but
        ///   it occurs anyway.
        /// </para>
        ///
        /// <para>
        ///   When using DotNetZip to write zip archives that will be read by one of
        ///   these other archivers, set this property to specify the code page to use
        ///   when encoding the <see cref="ZipEntry.FileName"/> and <see
        ///   cref="ZipEntry.Comment"/> for each <c>ZipEntry</c> in the zip file, for
        ///   values that cannot be encoded with the default codepage for zip files,
        ///   IBM437.  This is why this property is "provisional".  In all cases, IBM437
        ///   is used where possible, in other words, where no loss of data would
        ///   result. It is possible, therefore, to have a given entry with a
        ///   <c>Comment</c> encoded in IBM437 and a <c>FileName</c> encoded with the
        ///   specified "provisional" codepage.
        /// </para>
        ///
        /// <para>
        ///   Be aware that a zip file created after you've explicitly set the
        ///   <c>ProvisionalAlternateEncoding</c> property to a value other than
        ///   IBM437 may not be compliant to the PKWare specification, and may not be
        ///   readable by compliant archivers.  On the other hand, many (most?)
        ///   archivers are non-compliant and can read zip files created in arbitrary
        ///   code pages.  The trick is to use or specify the proper codepage when
        ///   reading the zip.
        /// </para>
        ///
        /// <para>
        ///   When creating a zip archive using this library, it is possible to change
        ///   the value of <c>ProvisionalAlternateEncoding</c> between each entry you
        ///   add, and between adding entries and the call to <c>Close()</c>. Don't do
        ///   this. It will likely result in a zipfile that is not readable.  For best
        ///   interoperability, either leave <c>ProvisionalAlternateEncoding</c>
        ///   alone, or specify it only once, before adding any entries to the
        ///   <c>ZipOutputStream</c> instance.  There is one exception to this
        ///   recommendation, described later.
        /// </para>
        ///
        /// <para>
        ///   When using an arbitrary, non-UTF8 code page for encoding, there is no
        ///   standard way for the creator application - whether DotNetZip, WinZip,
        ///   WinRar, or something else - to formally specify in the zip file which
        ///   codepage has been used for the entries. As a result, readers of zip files
        ///   are not able to inspect the zip file and determine the codepage that was
        ///   used for the entries contained within it.  It is left to the application
        ///   or user to determine the necessary codepage when reading zip files encoded
        ///   this way.  If you use an incorrect codepage when reading a zipfile, you
        ///   will get entries with filenames that are incorrect, and the incorrect
        ///   filenames may even contain characters that are not legal for use within
        ///   filenames in Windows. Extracting entries with illegal characters in the
        ///   filenames will lead to exceptions. It's too bad, but this is just the way
        ///   things are with code pages in zip files. Caveat Emptor.
        /// </para>
        ///
        /// <para>
        ///   One possible approach for specifying the code page for a given zip file is
        ///   to describe the code page in a human-readable form in the Zip comment. For
        ///   example, the comment may read "Entries in this archive are encoded in the
        ///   Big5 code page".  For maximum interoperability, the zip comment in this
        ///   case should be encoded in the default, IBM437 code page.  In this case,
        ///   the zip comment is encoded using a different page than the filenames.  To
        ///   do this, Specify <c>ProvisionalAlternateEncoding</c> to your desired
        ///   region-specific code page, once before adding any entries, and then set
        ///   the <see cref="Comment"/> property and reset
        ///   <c>ProvisionalAlternateEncoding</c> to IBM437 before calling <c>Close()</c>.
        /// </para>
        /// </remarks>
        [Obsolete("use AlternateEncoding and AlternateEncodingUsage instead.")]
        public System.Text.Encoding ProvisionalAlternateEncoding
        {
            get
            {
                if (_alternateEncodingUsage == ZipOption.AsNecessary)
                    return _alternateEncoding;
                return null;
            }
            set
            {
                _alternateEncoding = value;
                _alternateEncodingUsage = ZipOption.AsNecessary;
            }
        }

        /// <summary>
        ///   A Text Encoding to use when encoding the filenames and comments for
        ///   all the ZipEntry items, during a ZipFile.Save() operation.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Whether the encoding specified here is used during the save depends
        ///     on <see cref="AlternateEncodingUsage"/>.
        ///   </para>
        /// </remarks>
        public System.Text.Encoding AlternateEncoding
        {
            get
            {
                return _alternateEncoding;
            }
            set
            {
                _alternateEncoding = value;
            }
        }

        /// <summary>
        ///   A flag that tells if and when this instance should apply
        ///   AlternateEncoding to encode the filenames and comments associated to
        ///   of ZipEntry objects contained within this instance.
        /// </summary>
        public ZipOption AlternateEncodingUsage
        {
            get
            {
                return _alternateEncodingUsage;
            }
            set
            {
                _alternateEncodingUsage = value;
            }
        }

        /// <summary>
        /// The default text encoding used in zip archives.  It is numeric 437, also
        /// known as IBM437.
        /// </summary>
        /// <seealso cref="Ionic.Zip.ZipFile.ProvisionalAlternateEncoding"/>
        internal static System.Text.Encoding DefaultEncoding
        {
            get
            {
                return System.Text.Encoding.GetEncoding("IBM437");
            }
        }


#if !NETCF
        /// <summary>
        ///   The size threshold for an entry, above which a parallel deflate is used.
        /// </summary>
        ///
        /// <remarks>
        ///
        ///   <para>
        ///     DotNetZip will use multiple threads to compress any ZipEntry, when
        ///     the <c>CompressionMethod</c> is Deflate, and if the entry is
        ///     larger than the given size.  Zero means "always use parallel
        ///     deflate", while -1 means "never use parallel deflate".
        ///   </para>
        ///
        ///   <para>
        ///     If the entry size cannot be known before compression, as with any entry
        ///     added via a ZipOutputStream, then Parallel deflate will never be
        ///     performed, unless the value of this property is zero.
        ///   </para>
        ///
        ///   <para>
        ///     A parallel deflate operations will speed up the compression of
        ///     large files, on computers with multiple CPUs or multiple CPU
        ///     cores.  For files above 1mb, on a dual core or dual-cpu (2p)
        ///     machine, the time required to compress the file can be 70% of the
        ///     single-threaded deflate.  For very large files on 4p machines the
        ///     compression can be done in 30% of the normal time.  The downside
        ///     is that parallel deflate consumes extra memory during the deflate,
        ///     and the deflation is slightly less effective.
        ///   </para>
        ///
        ///   <para>
        ///     Parallel deflate tends to not be as effective as single-threaded deflate
        ///     because the original data stream is split into multiple independent
        ///     buffers, each of which is compressed in parallel.  But because they are
        ///     treated independently, there is no opportunity to share compression
        ///     dictionaries, and additional framing bytes must be added to the output
        ///     stream.  For that reason, a deflated stream may be slightly larger when
        ///     compressed using parallel deflate, as compared to a traditional
        ///     single-threaded deflate. For files of about 512k, the increase over the
        ///     normal deflate is as much as 5% of the total compressed size. For larger
        ///     files, the difference can be as small as 0.1%.
        ///   </para>
        ///
        ///   <para>
        ///     Multi-threaded compression does not give as much an advantage when using
        ///     Encryption. This is primarily because encryption tends to slow down
        ///     the entire pipeline. Also, multi-threaded compression gives less of an
        ///     advantage when using lower compression levels, for example <see
        ///     cref="Ionic.Zlib.CompressionLevel.BestSpeed"/>.  You may have to perform
        ///     some tests to determine the best approach for your situation.
        ///   </para>
        ///
        ///   <para>
        ///     The default value for this property is -1, which means parallel
        ///     compression will not be performed unless you set it to zero.
        ///   </para>
        ///
        /// </remarks>
        public long ParallelDeflateThreshold
        {
            set
            {
                if ((value != 0) && (value != -1) && (value < 64 * 1024))
                    throw new ArgumentOutOfRangeException("value must be greater than 64k, or 0, or -1");
                _ParallelDeflateThreshold = value;
            }
            get
            {
                return _ParallelDeflateThreshold;
            }
        }


        /// <summary>
        ///   The maximum number of buffer pairs to use when performing
        ///   parallel compression.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This property sets an upper limit on the number of memory
        ///   buffer pairs to create when performing parallel
        ///   compression.  The implementation of the parallel
        ///   compression stream allocates multiple buffers to
        ///   facilitate parallel compression.  As each buffer fills up,
        ///   the stream uses <see
        ///   cref="System.Threading.ThreadPool.QueueUserWorkItem(WaitCallback)">
        ///   ThreadPool.QueueUserWorkItem()</see> to compress those
        ///   buffers in a background threadpool thread. After a buffer
        ///   is compressed, it is re-ordered and written to the output
        ///   stream.
        /// </para>
        ///
        /// <para>
        ///   A higher number of buffer pairs enables a higher degree of
        ///   parallelism, which tends to increase the speed of compression on
        ///   multi-cpu computers.  On the other hand, a higher number of buffer
        ///   pairs also implies a larger memory consumption, more active worker
        ///   threads, and a higher cpu utilization for any compression. This
        ///   property enables the application to limit its memory consumption and
        ///   CPU utilization behavior depending on requirements.
        /// </para>
        ///
        /// <para>
        ///   For each compression "task" that occurs in parallel, there are 2
        ///   buffers allocated: one for input and one for output.  This property
        ///   sets a limit for the number of pairs.  The total amount of storage
        ///   space allocated for buffering will then be (N*S*2), where N is the
        ///   number of buffer pairs, S is the size of each buffer (<see
        ///   cref="CodecBufferSize"/>).  By default, DotNetZip allocates 4 buffer
        ///   pairs per CPU core, so if your machine has 4 cores, and you retain
        ///   the default buffer size of 128k, then the
        ///   ParallelDeflateOutputStream will use 4 * 4 * 2 * 128kb of buffer
        ///   memory in total, or 4mb, in blocks of 128kb.  If you then set this
        ///   property to 8, then the number will be 8 * 2 * 128kb of buffer
        ///   memory, or 2mb.
        /// </para>
        ///
        /// <para>
        ///   CPU utilization will also go up with additional buffers, because a
        ///   larger number of buffer pairs allows a larger number of background
        ///   threads to compress in parallel. If you find that parallel
        ///   compression is consuming too much memory or CPU, you can adjust this
        ///   value downward.
        /// </para>
        ///
        /// <para>
        ///   The default value is 16. Different values may deliver better or
        ///   worse results, depending on your priorities and the dynamic
        ///   performance characteristics of your storage and compute resources.
        /// </para>
        ///
        /// <para>
        ///   This property is not the number of buffer pairs to use; it is an
        ///   upper limit. An illustration: Suppose you have an application that
        ///   uses the default value of this property (which is 16), and it runs
        ///   on a machine with 2 CPU cores. In that case, DotNetZip will allocate
        ///   4 buffer pairs per CPU core, for a total of 8 pairs.  The upper
        ///   limit specified by this property has no effect.
        /// </para>
        ///
        /// <para>
        ///   The application can set this value at any time, but it is
        ///   effective only if set before calling
        ///   <c>ZipOutputStream.Write()</c> for the first time.
        /// </para>
        /// </remarks>
        ///
        /// <seealso cref="ParallelDeflateThreshold"/>
        ///
        public int ParallelDeflateMaxBufferPairs
        {
            get
            {
                return _maxBufferPairs;
            }
            set
            {
                if (value < 4)
                    throw new ArgumentOutOfRangeException("ParallelDeflateMaxBufferPairs",
                                                "Value must be 4 or greater.");
                _maxBufferPairs = value;
            }
        }
#endif


        private void InsureUniqueEntry(ZipEntry ze1)
        {
            if (_entriesWritten.ContainsKey(ze1.FileName))
            {
                _exceptionPending = true;
                throw new ArgumentException(String.Format("The entry '{0}' already exists in the zip archive.", ze1.FileName));
            }
        }


        internal Stream OutputStream
        {
            get
            {
                return _outputStream;
            }
        }

        internal String Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        ///   Returns true if an entry by the given name has already been written
        ///   to the ZipOutputStream.
        /// </summary>
        ///
        /// <param name="name">
        ///   The name of the entry to scan for.
        /// </param>
        ///
        /// <returns>
        /// true if an entry by the given name has already been written.
        /// </returns>
        public bool ContainsEntry(string name)
        {
            return _entriesWritten.ContainsKey(SharedUtilities.NormalizePathForUseInZipFile(name));
        }


        /// <summary>
        ///   Write the data from the buffer to the stream.
        /// </summary>
        ///
        /// <remarks>
        ///   As the application writes data into this stream, the data may be
        ///   compressed and encrypted before being written out to the underlying
        ///   stream, depending on the settings of the <see cref="CompressionLevel"/>
        ///   and the <see cref="Encryption"/> properties.
        /// </remarks>
        ///
        /// <param name="buffer">The buffer holding data to write to the stream.</param>
        /// <param name="offset">the offset within that data array to find the first byte to write.</param>
        /// <param name="count">the number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed)
            {
                _exceptionPending = true;
                throw new System.InvalidOperationException("The stream has been closed.");
            }

            if (buffer==null)
            {
                _exceptionPending = true;
                throw new System.ArgumentNullException("buffer");
            }

            if (_currentEntry == null)
            {
                _exceptionPending = true;
                throw new System.InvalidOperationException("You must call PutNextEntry() before calling Write().");
            }

            if (_currentEntry.IsDirectory)
            {
                _exceptionPending = true;
                throw new System.InvalidOperationException("You cannot Write() data for an entry that is a directory.");
            }

            if (_needToWriteEntryHeader)
                _InitiateCurrentEntry(false);

            if (count != 0)
                _entryOutputStream.Write(buffer, offset, count);
        }



        /// <summary>
        ///   Specify the name of the next entry that will be written to the zip file.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   Call this method just before calling <see cref="Write(byte[], int, int)"/>, to
        ///   specify the name of the entry that the next set of bytes written to
        ///   the <c>ZipOutputStream</c> belongs to. All subsequent calls to <c>Write</c>,
        ///   until the next call to <c>PutNextEntry</c>,
        ///   will be inserted into the named entry in the zip file.
        /// </para>
        ///
        /// <para>
        ///   If the <paramref name="entryName"/> used in <c>PutNextEntry()</c> ends in
        ///   a slash, then the entry added is marked as a directory. Because directory
        ///   entries do not contain data, a call to <c>Write()</c>, before an
        ///   intervening additional call to <c>PutNextEntry()</c>, will throw an
        ///   exception.
        /// </para>
        ///
        /// <para>
        ///   If you don't call <c>Write()</c> between two calls to
        ///   <c>PutNextEntry()</c>, the first entry is inserted into the zip file as a
        ///   file of zero size.  This may be what you want.
        /// </para>
        ///
        /// <para>
        ///   Because <c>PutNextEntry()</c> closes out the prior entry, if any, this
        ///   method may throw if there is a problem with the prior entry.
        /// </para>
        ///
        /// <para>
        ///   This method returns the <c>ZipEntry</c>.  You can modify public properties
        ///   on the <c>ZipEntry</c>, such as <see cref="ZipEntry.Encryption"/>, <see
        ///   cref="ZipEntry.Password"/>, and so on, until the first call to
        ///   <c>ZipOutputStream.Write()</c>, or until the next call to
        ///   <c>PutNextEntry()</c>.  If you modify the <c>ZipEntry</c> <em>after</em>
        ///   having called <c>Write()</c>, you may get a runtime exception, or you may
        ///   silently get an invalid zip archive.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///
        ///   This example shows how to create a zip file, using the
        ///   <c>ZipOutputStream</c> class.
        ///
        /// <code>
        /// private void Zipup()
        /// {
        ///     using (FileStream fs raw = File.Open(_outputFileName, FileMode.Create, FileAccess.ReadWrite ))
        ///     {
        ///         using (var output= new ZipOutputStream(fs))
        ///         {
        ///             output.Password = "VerySecret!";
        ///             output.Encryption = EncryptionAlgorithm.WinZipAes256;
        ///             output.PutNextEntry("entry1.txt");
        ///             byte[] buffer= System.Text.Encoding.ASCII.GetBytes("This is the content for entry #1.");
        ///             output.Write(buffer,0,buffer.Length);
        ///             output.PutNextEntry("entry2.txt");  // this will be zero length
        ///             output.PutNextEntry("entry3.txt");
        ///             buffer= System.Text.Encoding.ASCII.GetBytes("This is the content for entry #3.");
        ///             output.Write(buffer,0,buffer.Length);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        ///
        /// <param name="entryName">
        ///   The name of the entry to be added, including any path to be used
        ///   within the zip file.
        /// </param>
        ///
        /// <returns>
        ///   The ZipEntry created.
        /// </returns>
        ///
        public ZipEntry PutNextEntry(String entryName)
        {
            if (String.IsNullOrEmpty(entryName))
                throw new ArgumentNullException("entryName");

            if (_disposed)
            {
                _exceptionPending = true;
                throw new System.InvalidOperationException("The stream has been closed.");
            }

            _FinishCurrentEntry();
            _currentEntry = ZipEntry.CreateForZipOutputStream(entryName);
            _currentEntry._container = new ZipContainer(this);
            _currentEntry._BitField |= 0x0008;  // workitem 8932
            _currentEntry.SetEntryTimes(DateTime.Now, DateTime.Now, DateTime.Now);
            _currentEntry.CompressionLevel = this.CompressionLevel;
            _currentEntry.CompressionMethod = this.CompressionMethod;
            _currentEntry.Password = _password; // workitem 13909
            _currentEntry.Encryption = this.Encryption;
            // workitem 12634
            _currentEntry.AlternateEncoding = this.AlternateEncoding;
            _currentEntry.AlternateEncodingUsage = this.AlternateEncodingUsage;

            if (entryName.EndsWith("/"))  _currentEntry.MarkAsDirectory();

            _currentEntry.EmitTimesInWindowsFormatWhenSaving = ((_timestamp & ZipEntryTimestamp.Windows) != 0);
            _currentEntry.EmitTimesInUnixFormatWhenSaving = ((_timestamp & ZipEntryTimestamp.Unix) != 0);
            InsureUniqueEntry(_currentEntry);
            _needToWriteEntryHeader = true;

            return _currentEntry;
        }



        private void _InitiateCurrentEntry(bool finishing)
        {
            // If finishing==true, this means we're initiating the entry at the time of
            // Close() or PutNextEntry().  If this happens, it means no data was written
            // for the entry - Write() was never called.  (The usual case us to call
            // _InitiateCurrentEntry(bool) from within Write().)  If finishing==true,
            // the entry could be either a zero-byte file or a directory.

            _entriesWritten.Add(_currentEntry.FileName,_currentEntry);
            _entryCount++; // could use _entriesWritten.Count, but I don't want to incur
            // the cost.

            if (_entryCount > 65534 && _zip64 == Zip64Option.Never)
            {
                _exceptionPending = true;
                throw new System.InvalidOperationException("Too many entries. Consider setting ZipOutputStream.EnableZip64.");
            }

            // Write out the header.
            //
            // If finishing, and encryption is in use, then we don't want to emit the
            // normal encryption header.  Signal that with a cycle=99 to turn off
            // encryption for zero-byte entries or directories.
            //
            // If finishing, then we know the stream length is zero.  Else, unknown
            // stream length.  Passing stream length == 0 allows an optimization so as
            // not to setup an encryption or deflation stream, when stream length is
            // zero.

            _currentEntry.WriteHeader(_outputStream, finishing ? 99 : 0);
            _currentEntry.StoreRelativeOffset();

            if (!_currentEntry.IsDirectory)
            {
                _currentEntry.WriteSecurityMetadata(_outputStream);
                _currentEntry.PrepOutputStream(_outputStream,
                                               finishing ? 0 : -1,
                                               out _outputCounter,
                                               out _encryptor,
                                               out _deflater,
                                               out _entryOutputStream);
            }
            _needToWriteEntryHeader = false;
        }



        private void _FinishCurrentEntry()
        {
            if (_currentEntry != null)
            {
                if (_needToWriteEntryHeader)
                    _InitiateCurrentEntry(true); // an empty entry - no writes

                _currentEntry.FinishOutputStream(_outputStream, _outputCounter, _encryptor, _deflater, _entryOutputStream);
                _currentEntry.PostProcessOutput(_outputStream);
                // workitem 12964
                if (_currentEntry.OutputUsedZip64!=null)
                    _anyEntriesUsedZip64 |= _currentEntry.OutputUsedZip64.Value;

                // reset all the streams
                _outputCounter = null; _encryptor = _deflater = null; _entryOutputStream = null;
            }
        }



        /// <summary>
        /// Dispose the stream
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This method writes the Zip Central directory, then closes the stream.  The
        ///   application must call Dispose() (or Close) in order to produce a valid zip file.
        /// </para>
        ///
        /// <para>
        ///   Typically the application will call <c>Dispose()</c> implicitly, via a <c>using</c>
        ///   statement in C#, or a <c>Using</c> statement in VB.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="disposing">set this to true, always.</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) // not called from finalizer
            {
                // handle pending exceptions
                if (!_exceptionPending)
                {
                    _FinishCurrentEntry();
                    _directoryNeededZip64 = ZipOutput.WriteCentralDirectoryStructure(_outputStream,
                                                                                     _entriesWritten.Values,
                                                                                     1, // _numberOfSegmentsForMostRecentSave,
                                                                                     _zip64,
                                                                                     Comment,
                                                                                     new ZipContainer(this));
                    Stream wrappedStream = null;
                    CountingStream cs = _outputStream as CountingStream;
                    if (cs != null)
                    {
                        wrappedStream = cs.WrappedStream;
#if NETCF
                    cs.Close();
#else
                        cs.Dispose();
#endif
                    }
                    else
                    {
                        wrappedStream = _outputStream;
                    }

                    if (!_leaveUnderlyingStreamOpen)
                    {
#if NETCF
                    wrappedStream.Close();
#else
                        wrappedStream.Dispose();
#endif
                    }
                    _outputStream = null;
                }
            }
            _disposed = true;
        }



        /// <summary>
        /// Always returns false.
        /// </summary>
        public override bool CanRead { get { return false; } }

        /// <summary>
        /// Always returns false.
        /// </summary>
        public override bool CanSeek { get { return false; } }

        /// <summary>
        /// Always returns true.
        /// </summary>
        public override bool CanWrite { get { return true; } }

        /// <summary>
        /// Always returns a NotSupportedException.
        /// </summary>
        public override long Length { get { throw new NotSupportedException(); } }

        /// <summary>
        /// Setting this property always returns a NotSupportedException. Getting it
        /// returns the value of the Position on the underlying stream.
        /// </summary>
        public override long Position
        {
            get { return _outputStream.Position; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// This is a no-op.
        /// </summary>
        public override void Flush() { }

        /// <summary>
        /// This method always throws a NotSupportedException.
        /// </summary>
        /// <param name="buffer">ignored</param>
        /// <param name="offset">ignored</param>
        /// <param name="count">ignored</param>
        /// <returns>nothing</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Read");
        }

        /// <summary>
        /// This method always throws a NotSupportedException.
        /// </summary>
        /// <param name="offset">ignored</param>
        /// <param name="origin">ignored</param>
        /// <returns>nothing</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seek");
        }

        /// <summary>
        /// This method always throws a NotSupportedException.
        /// </summary>
        /// <param name="value">ignored</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }


        private EncryptionAlgorithm _encryption;
        private ZipEntryTimestamp _timestamp;
        internal String _password;
        private String _comment;
        private Stream _outputStream;
        private ZipEntry _currentEntry;
        internal Zip64Option _zip64;
        private Dictionary<String, ZipEntry> _entriesWritten;
        private int _entryCount;
        private ZipOption _alternateEncodingUsage = ZipOption.Never;
        private System.Text.Encoding _alternateEncoding
            = System.Text.Encoding.GetEncoding("IBM437"); // default = IBM437

        private bool _leaveUnderlyingStreamOpen;
        private bool _disposed;
        private bool _exceptionPending; // **see note below
        private bool _anyEntriesUsedZip64, _directoryNeededZip64;
        private CountingStream _outputCounter;
        private Stream _encryptor;
        private Stream _deflater;
        private Ionic.Crc.CrcCalculatorStream _entryOutputStream;
        private bool _needToWriteEntryHeader;
        private string _name;
        private bool _DontIgnoreCase;
#if !NETCF
        internal Ionic.Zlib.ParallelDeflateOutputStream ParallelDeflater;
        private long _ParallelDeflateThreshold;
        private int _maxBufferPairs = 16;
#endif

        // **Note regarding exceptions:

        // When ZipOutputStream is employed within a using clause, which
        // is the typical scenario, and an exception is thrown within
        // the scope of the using, Close()/Dispose() is invoked
        // implicitly before processing the initial exception.  In that
        // case, _exceptionPending is true, and we don't want to try to
        // write anything in the Close/Dispose logic.  Doing so can
        // cause additional exceptions that mask the original one. So,
        // the _exceptionPending flag is used to track that, and to
        // allow the original exception to be propagated to the
        // application without extra "noise."

    }



    internal class ZipContainer
    {
        private ZipFile _zf;
        private ZipOutputStream _zos;
        private ZipInputStream _zis;

        public ZipContainer(Object o)
        {
            _zf = (o as ZipFile);
            _zos = (o as ZipOutputStream);
            _zis = (o as ZipInputStream);
        }

        public ZipFile ZipFile
        {
            get { return _zf; }
        }

        public ZipOutputStream ZipOutputStream
        {
            get { return _zos; }
        }

        public string Name
        {
            get
            {
                if (_zf != null) return _zf.Name;
                if (_zis != null) throw new NotSupportedException();
                return _zos.Name;
            }
        }

        public string Password
        {
            get
            {
                if (_zf != null) return _zf._Password;
                if (_zis != null) return _zis._Password;
                return _zos._password;
            }
        }

        public Zip64Option Zip64
        {
            get
            {
                if (_zf != null) return _zf._zip64;
                if (_zis != null) throw new NotSupportedException();
                return _zos._zip64;
            }
        }

        public int BufferSize
        {
            get
            {
                if (_zf != null) return _zf.BufferSize;
                if (_zis != null) throw new NotSupportedException();
                return 0;
            }
        }

#if !NETCF
        public Ionic.Zlib.ParallelDeflateOutputStream ParallelDeflater
        {
            get
            {
                if (_zf != null) return _zf.ParallelDeflater;
                if (_zis != null) return null;
                return _zos.ParallelDeflater;
            }
            set
            {
                if (_zf != null) _zf.ParallelDeflater = value;
                else if (_zos != null) _zos.ParallelDeflater = value;
            }
        }

        public long ParallelDeflateThreshold
        {
            get
            {
                if (_zf != null) return _zf.ParallelDeflateThreshold;
                return _zos.ParallelDeflateThreshold;
            }
        }
        public int ParallelDeflateMaxBufferPairs
        {
            get
            {
                if (_zf != null) return _zf.ParallelDeflateMaxBufferPairs;
                return _zos.ParallelDeflateMaxBufferPairs;
            }
        }
#endif

        public int CodecBufferSize
        {
            get
            {
                if (_zf != null) return _zf.CodecBufferSize;
                if (_zis != null) return _zis.CodecBufferSize;
                return _zos.CodecBufferSize;
            }
        }

        public Ionic.Zlib.CompressionStrategy Strategy
        {
            get
            {
                if (_zf != null) return _zf.Strategy;
                return _zos.Strategy;
            }
        }

        public Zip64Option UseZip64WhenSaving
        {
            get
            {
                if (_zf != null) return _zf.UseZip64WhenSaving;
                return _zos.EnableZip64;
            }
        }

        public System.Text.Encoding AlternateEncoding
        {
            get
            {
                if (_zf != null) return _zf.AlternateEncoding;
                if (_zos!=null) return _zos.AlternateEncoding;
                return null;
            }
        }
        public System.Text.Encoding DefaultEncoding
        {
            get
            {
                if (_zf != null) return ZipFile.DefaultEncoding;
                if (_zos!=null) return ZipOutputStream.DefaultEncoding;
                return null;
            }
        }
        public ZipOption AlternateEncodingUsage
        {
            get
            {
                if (_zf != null) return _zf.AlternateEncodingUsage;
                if (_zos!=null) return _zos.AlternateEncodingUsage;
                return ZipOption.Never; // n/a
            }
        }

        public Stream ReadStream
        {
            get
            {
                if (_zf != null) return _zf.ReadStream;
                return _zis.ReadStream;
            }
        }
    }

}