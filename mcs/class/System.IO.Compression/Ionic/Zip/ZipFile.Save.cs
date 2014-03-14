// ZipFile.Save.cs
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
// Time-stamp: <2011-August-05 13:31:23>
//
// ------------------------------------------------------------------
//
// This module defines the methods for Save operations on zip files.
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
        ///   Delete file with retry on UnauthorizedAccessException.
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///     When calling File.Delete() on a file that has been "recently"
        ///     created, the call sometimes fails with
        ///     UnauthorizedAccessException. This method simply retries the Delete 3
        ///     times with a sleep between tries.
        ///   </para>
        /// </remarks>
        ///
        /// <param name='filename'>the name of the file to be deleted</param>
        private void DeleteFileWithRetry(string filename)
        {
            bool done = false;
            int nRetries = 3;
            for (int i=0; i < nRetries && !done; i++)
            {
                try
                {
                    File.Delete(filename);
                    done = true;
                }
                catch (System.UnauthorizedAccessException)
                {
                    Console.WriteLine("************************************************** Retry delete.");
                    System.Threading.Thread.Sleep(200+i*200);
                }
            }
        }


        /// <summary>
        ///   Saves the Zip archive to a file, specified by the Name property of the
        ///   <c>ZipFile</c>.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The <c>ZipFile</c> instance is written to storage, typically a zip file
        ///   in a filesystem, only when the caller calls <c>Save</c>.  In the typical
        ///   case, the Save operation writes the zip content to a temporary file, and
        ///   then renames the temporary file to the desired name. If necessary, this
        ///   method will delete a pre-existing file before the rename.
        /// </para>
        ///
        /// <para>
        ///   The <see cref="ZipFile.Name"/> property is specified either explicitly,
        ///   or implicitly using one of the parameterized ZipFile constructors.  For
        ///   COM Automation clients, the <c>Name</c> property must be set explicitly,
        ///   because COM Automation clients cannot call parameterized constructors.
        /// </para>
        ///
        /// <para>
        ///   When using a filesystem file for the Zip output, it is possible to call
        ///   <c>Save</c> multiple times on the <c>ZipFile</c> instance. With each
        ///   call the zip content is re-written to the same output file.
        /// </para>
        ///
        /// <para>
        ///   Data for entries that have been added to the <c>ZipFile</c> instance is
        ///   written to the output when the <c>Save</c> method is called. This means
        ///   that the input streams for those entries must be available at the time
        ///   the application calls <c>Save</c>.  If, for example, the application
        ///   adds entries with <c>AddEntry</c> using a dynamically-allocated
        ///   <c>MemoryStream</c>, the memory stream must not have been disposed
        ///   before the call to <c>Save</c>. See the <see
        ///   cref="ZipEntry.InputStream"/> property for more discussion of the
        ///   availability requirements of the input stream for an entry, and an
        ///   approach for providing just-in-time stream lifecycle management.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <seealso cref="Ionic.Zip.ZipFile.AddEntry(String, System.IO.Stream)"/>
        ///
        /// <exception cref="Ionic.Zip.BadStateException">
        ///   Thrown if you haven't specified a location or stream for saving the zip,
        ///   either in the constructor or by setting the Name property, or if you try
        ///   to save a regular zip archive to a filename with a .exe extension.
        /// </exception>
        ///
        /// <exception cref="System.OverflowException">
        ///   Thrown if <see cref="MaxOutputSegmentSize"/> is non-zero, and the number
        ///   of segments that would be generated for the spanned zip file during the
        ///   save operation exceeds 99.  If this happens, you need to increase the
        ///   segment size.
        /// </exception>
        ///
        public void Save()
        {
            try
            {
                bool thisSaveUsedZip64 = false;
                _saveOperationCanceled = false;
                _numberOfSegmentsForMostRecentSave = 0;
                OnSaveStarted();

                if (WriteStream == null)
                    throw new BadStateException("You haven't specified where to save the zip.");

                if (_name != null && _name.EndsWith(".exe") && !_SavingSfx)
                    throw new BadStateException("You specified an EXE for a plain zip file.");

                // check if modified, before saving.
                if (!_contentsChanged)
                {
                    OnSaveCompleted();
                    if (Verbose) StatusMessageTextWriter.WriteLine("No save is necessary....");
                    return;
                }

                Reset(true);

                if (Verbose) StatusMessageTextWriter.WriteLine("saving....");

                // validate the number of entries
                if (_entries.Count >= 0xFFFF && _zip64 == Zip64Option.Never)
                    throw new ZipException("The number of entries is 65535 or greater. Consider setting the UseZip64WhenSaving property on the ZipFile instance.");


                // write an entry in the zip for each file
                int n = 0;
                // workitem 9831
                ICollection<ZipEntry> c = (SortEntriesBeforeSaving) ? EntriesSorted : Entries;
                foreach (ZipEntry e in c) // _entries.Values
                {
                    OnSaveEntry(n, e, true);
                    e.Write(WriteStream);
                    if (_saveOperationCanceled)
                        break;

                    n++;
                    OnSaveEntry(n, e, false);
                    if (_saveOperationCanceled)
                        break;

                    // Some entries can be skipped during the save.
                    if (e.IncludedInMostRecentSave)
                        thisSaveUsedZip64 |= e.OutputUsedZip64.Value;
                }



                if (_saveOperationCanceled)
                    return;

                var zss = WriteStream as ZipSegmentedStream;

                _numberOfSegmentsForMostRecentSave = (zss!=null)
                    ? zss.CurrentSegment
                    : 1;

                bool directoryNeededZip64 =
                    ZipOutput.WriteCentralDirectoryStructure
                    (WriteStream,
                     c,
                     _numberOfSegmentsForMostRecentSave,
                     _zip64,
                     Comment,
                     new ZipContainer(this));

                OnSaveEvent(ZipProgressEventType.Saving_AfterSaveTempArchive);

                _hasBeenSaved = true;
                _contentsChanged = false;

                thisSaveUsedZip64 |= directoryNeededZip64;
                _OutputUsesZip64 = new Nullable<bool>(thisSaveUsedZip64);


                // do the rename as necessary
                if (_name != null &&
                    (_temporaryFileName!=null || zss != null))
                {
                    // _temporaryFileName may remain null if we are writing to a stream.
                    // only close the stream if there is a file behind it.
#if NETCF
                    WriteStream.Close();
#else
                    WriteStream.Dispose();
#endif
                    if (_saveOperationCanceled)
                        return;

                    if (_fileAlreadyExists && this._readstream != null)
                    {
                        // This means we opened and read a zip file.
                        // If we are now saving to the same file, we need to close the
                        // orig file, first.
                        this._readstream.Close();
                        this._readstream = null;
                        // the archiveStream for each entry needs to be null
                        foreach (var e in c)
                        {
                            var zss1 = e._archiveStream as ZipSegmentedStream;
                            if (zss1 != null)
#if NETCF
                                zss1.Close();
#else
                                zss1.Dispose();
#endif
                            e._archiveStream = null;
                        }
                    }

                    string tmpName = null;
                    if (File.Exists(_name))
                    {
                        // the steps:
                        //
                        // 1. Delete tmpName
                        // 2. move existing zip to tmpName
                        // 3. rename (File.Move) working file to name of existing zip
                        // 4. delete tmpName
                        //
                        // This series of steps avoids the exception,
                        // System.IO.IOException:
                        //   "Cannot create a file when that file already exists."
                        //
                        // Cannot just call File.Replace() here because
                        // there is a possibility that the TEMP volume is different
                        // that the volume for the final file (c:\ vs d:\).
                        // So we need to do a Delete+Move pair.
                        //
                        // But, when doing the delete, Windows allows a process to
                        // delete the file, even though it is held open by, say, a
                        // virus scanner. It gets internally marked as "delete
                        // pending". The file does not actually get removed from the
                        // file system, it is still there after the File.Delete
                        // call.
                        //
                        // Therefore, we need to move the existing zip, which may be
                        // held open, to some other name. Then rename our working
                        // file to the desired name, then delete (possibly delete
                        // pending) the "other name".
                        //
                        // Ideally this would be transactional. It's possible that the
                        // delete succeeds and the move fails. Lacking transactions, if
                        // this kind of failure happens, we're hosed, and this logic will
                        // throw on the next File.Move().
                        //
                        //File.Delete(_name);
                        // workitem 10447
#if NETCF || SILVERLIGHT
                        tmpName = _name + "." + SharedUtilities.GenerateRandomStringImpl(8,0) + ".tmp";
#else
                        tmpName = _name + "." + Path.GetRandomFileName();
#endif
                        if (File.Exists(tmpName))
                            DeleteFileWithRetry(tmpName);
                        File.Move(_name, tmpName);
                    }

                    OnSaveEvent(ZipProgressEventType.Saving_BeforeRenameTempArchive);
                    File.Move((zss != null) ? zss.CurrentTempName : _temporaryFileName,
                              _name);

                    OnSaveEvent(ZipProgressEventType.Saving_AfterRenameTempArchive);

                    if (tmpName != null)
                    {
                        try
                        {
                            // not critical
                            if (File.Exists(tmpName))
                                File.Delete(tmpName);
                        }
                        catch
                        {
                            // don't care about exceptions here.
                        }

                    }
                    _fileAlreadyExists = true;
                }

                NotifyEntriesSaveComplete(c);
                OnSaveCompleted();
                _JustSaved = true;
            }

            // workitem 5043
            finally
            {
                CleanupAfterSaveOperation();
            }

            return;
        }



        private static void NotifyEntriesSaveComplete(ICollection<ZipEntry> c)
        {
            foreach (ZipEntry e in  c)
            {
                e.NotifySaveComplete();
            }
        }


        private void RemoveTempFile()
        {
            try
            {
                if (File.Exists(_temporaryFileName))
                {
                    File.Delete(_temporaryFileName);
                }
            }
            catch (IOException ex1)
            {
                if (Verbose)
                    StatusMessageTextWriter.WriteLine("ZipFile::Save: could not delete temp file: {0}.", ex1.Message);
            }
        }


        private void CleanupAfterSaveOperation()
        {
            if (_name != null)
            {
                // close the stream if there is a file behind it.
                if (_writestream != null)
                {
                    try
                    {
                        // workitem 7704
#if NETCF
                        _writestream.Close();
#else
                        _writestream.Dispose();
#endif
                    }
                    catch (System.IO.IOException) { }
                }
                _writestream = null;

                if (_temporaryFileName != null)
                {
                    RemoveTempFile();
                    _temporaryFileName = null;
                }
            }
        }


        /// <summary>
        /// Save the file to a new zipfile, with the given name.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// This method allows the application to explicitly specify the name of the zip
        /// file when saving. Use this when creating a new zip file, or when
        /// updating a zip archive.
        /// </para>
        ///
        /// <para>
        /// An application can also save a zip archive in several places by calling this
        /// method multiple times in succession, with different filenames.
        /// </para>
        ///
        /// <para>
        /// The <c>ZipFile</c> instance is written to storage, typically a zip file in a
        /// filesystem, only when the caller calls <c>Save</c>.  The Save operation writes
        /// the zip content to a temporary file, and then renames the temporary file
        /// to the desired name. If necessary, this method will delete a pre-existing file
        /// before the rename.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <exception cref="System.ArgumentException">
        /// Thrown if you specify a directory for the filename.
        /// </exception>
        ///
        /// <param name="fileName">
        /// The name of the zip archive to save to. Existing files will
        /// be overwritten with great prejudice.
        /// </param>
        ///
        /// <example>
        /// This example shows how to create and Save a zip file.
        /// <code>
        /// using (ZipFile zip = new ZipFile())
        /// {
        ///   zip.AddDirectory(@"c:\reports\January");
        ///   zip.Save("January.zip");
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Using zip As New ZipFile()
        ///   zip.AddDirectory("c:\reports\January")
        ///   zip.Save("January.zip")
        /// End Using
        /// </code>
        ///
        /// </example>
        ///
        /// <example>
        /// This example shows how to update a zip file.
        /// <code>
        /// using (ZipFile zip = ZipFile.Read("ExistingArchive.zip"))
        /// {
        ///   zip.AddFile("NewData.csv");
        ///   zip.Save("UpdatedArchive.zip");
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Using zip As ZipFile = ZipFile.Read("ExistingArchive.zip")
        ///   zip.AddFile("NewData.csv")
        ///   zip.Save("UpdatedArchive.zip")
        /// End Using
        /// </code>
        ///
        /// </example>
        public void Save(String fileName)
        {
            // Check for the case where we are re-saving a zip archive
            // that was originally instantiated with a stream.  In that case,
            // the _name will be null. If so, we set _writestream to null,
            // which insures that we'll cons up a new WriteStream (with a filesystem
            // file backing it) in the Save() method.
            if (_name == null)
                _writestream = null;

            else _readName = _name; // workitem 13915

            _name = fileName;
            if (Directory.Exists(_name))
                throw new ZipException("Bad Directory", new System.ArgumentException("That name specifies an existing directory. Please specify a filename.", "fileName"));
            _contentsChanged = true;
            _fileAlreadyExists = File.Exists(_name);
            Save();
        }


        /// <summary>
        ///   Save the zip archive to the specified stream.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The <c>ZipFile</c> instance is written to storage - typically a zip file
        ///   in a filesystem, but using this overload, the storage can be anything
        ///   accessible via a writable stream - only when the caller calls <c>Save</c>.
        /// </para>
        ///
        /// <para>
        ///   Use this method to save the zip content to a stream directly.  A common
        ///   scenario is an ASP.NET application that dynamically generates a zip file
        ///   and allows the browser to download it. The application can call
        ///   <c>Save(Response.OutputStream)</c> to write a zipfile directly to the
        ///   output stream, without creating a zip file on the disk on the ASP.NET
        ///   server.
        /// </para>
        ///
        /// <para>
        ///   Be careful when saving a file to a non-seekable stream, including
        ///   <c>Response.OutputStream</c>. When DotNetZip writes to a non-seekable
        ///   stream, the zip archive is formatted in such a way that may not be
        ///   compatible with all zip tools on all platforms.  It's a perfectly legal
        ///   and compliant zip file, but some people have reported problems opening
        ///   files produced this way using the Mac OS archive utility.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///
        ///   This example saves the zipfile content into a MemoryStream, and
        ///   then gets the array of bytes from that MemoryStream.
        ///
        /// <code lang="C#">
        /// using (var zip = new Ionic.Zip.ZipFile())
        /// {
        ///     zip.CompressionLevel= Ionic.Zlib.CompressionLevel.BestCompression;
        ///     zip.Password = "VerySecret.";
        ///     zip.Encryption = EncryptionAlgorithm.WinZipAes128;
        ///     zip.AddFile(sourceFileName);
        ///     MemoryStream output = new MemoryStream();
        ///     zip.Save(output);
        ///
        ///     byte[] zipbytes = output.ToArray();
        /// }
        /// </code>
        /// </example>
        ///
        /// <example>
        /// <para>
        ///   This example shows a pitfall you should avoid. DO NOT read
        ///   from a stream, then try to save to the same stream.  DO
        ///   NOT DO THIS:
        /// </para>
        ///
        /// <code lang="C#">
        /// using (var fs = new FileSteeam(filename, FileMode.Open))
        /// {
        ///   using (var zip = Ionic.Zip.ZipFile.Read(inputStream))
        ///   {
        ///     zip.AddEntry("Name1.txt", "this is the content");
        ///     zip.Save(inputStream);  // NO NO NO!!
        ///   }
        /// }
        /// </code>
        ///
        /// <para>
        ///   Better like this:
        /// </para>
        ///
        /// <code lang="C#">
        /// using (var zip = Ionic.Zip.ZipFile.Read(filename))
        /// {
        ///     zip.AddEntry("Name1.txt", "this is the content");
        ///     zip.Save();  // YES!
        /// }
        /// </code>
        ///
        /// </example>
        ///
        /// <param name="outputStream">
        ///   The <c>System.IO.Stream</c> to write to. It must be
        ///   writable. If you created the ZipFile instanct by calling
        ///   ZipFile.Read(), this stream must not be the same stream
        ///   you passed to ZipFile.Read().
        /// </param>
        public void Save(Stream outputStream)
        {
            if (outputStream == null)
                throw new ArgumentNullException("outputStream");
            if (!outputStream.CanWrite)
                throw new ArgumentException("Must be a writable stream.", "outputStream");

            // if we had a filename to save to, we are now obliterating it.
            _name = null;

            _writestream = new CountingStream(outputStream);

            _contentsChanged = true;
            _fileAlreadyExists = false;
            Save();
        }


    }



    internal static class ZipOutput
    {
        internal static bool WriteCentralDirectoryStructure(Stream s,
                                                          ICollection<ZipEntry> entries,
                                                          uint numSegments,
                                                          Zip64Option zip64,
                                                          String comment,
                                                          ZipContainer container)
        {
            var zss = s as ZipSegmentedStream;
            if (zss != null)
                zss.ContiguousWrite = true;

            // write to a memory stream in order to keep the
            // CDR contiguous
            Int64 aLength = 0;
            using (var ms = new MemoryStream())
            {
                foreach (ZipEntry e in entries)
                {
                    if (e.IncludedInMostRecentSave)
                    {
                        // this writes a ZipDirEntry corresponding to the ZipEntry
                        e.WriteCentralDirectoryEntry(ms);
                    }
                }
                var a = ms.ToArray();
                s.Write(a, 0, a.Length);
                aLength = a.Length;
            }


            // We need to keep track of the start and
            // Finish of the Central Directory Structure.

            // Cannot always use WriteStream.Length or Position; some streams do
            // not support these. (eg, ASP.NET Response.OutputStream) In those
            // cases we have a CountingStream.

            // Also, we cannot just set Start as s.Position bfore the write, and Finish
            // as s.Position after the write.  In a split zip, the write may actually
            // flip to the next segment.  In that case, Start will be zero.  But we
            // don't know that til after we know the size of the thing to write.  So the
            // answer is to compute the directory, then ask the ZipSegmentedStream which
            // segment that directory would fall in, it it were written.  Then, include
            // that data into the directory, and finally, write the directory to the
            // output stream.

            var output = s as CountingStream;
            long Finish = (output != null) ? output.ComputedPosition : s.Position;  // BytesWritten
            long Start = Finish - aLength;

            // need to know which segment the EOCD record starts in
            UInt32 startSegment = (zss != null)
                ? zss.CurrentSegment
                : 0;

            Int64 SizeOfCentralDirectory = Finish - Start;

            int countOfEntries = CountEntries(entries);

            bool needZip64CentralDirectory =
                zip64 == Zip64Option.Always ||
                countOfEntries >= 0xFFFF ||
                SizeOfCentralDirectory > 0xFFFFFFFF ||
                Start > 0xFFFFFFFF;

            byte[] a2 = null;

            // emit ZIP64 extensions as required
            if (needZip64CentralDirectory)
            {
                if (zip64 == Zip64Option.Never)
                {
#if NETCF
                    throw new ZipException("The archive requires a ZIP64 Central Directory. Consider enabling ZIP64 extensions.");
#else
                    System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(1);
                    if (sf.GetMethod().DeclaringType == typeof(ZipFile))
                        throw new ZipException("The archive requires a ZIP64 Central Directory. Consider setting the ZipFile.UseZip64WhenSaving property.");
                    else
                        throw new ZipException("The archive requires a ZIP64 Central Directory. Consider setting the ZipOutputStream.EnableZip64 property.");
#endif

                }

                var a = GenZip64EndOfCentralDirectory(Start, Finish, countOfEntries, numSegments);
                a2 = GenCentralDirectoryFooter(Start, Finish, zip64, countOfEntries, comment, container);
                if (startSegment != 0)
                {
                    UInt32 thisSegment = zss.ComputeSegment(a.Length + a2.Length);
                    int i = 16;
                    // number of this disk
                    Array.Copy(BitConverter.GetBytes(thisSegment), 0, a, i, 4);
                    i += 4;
                    // number of the disk with the start of the central directory
                    //Array.Copy(BitConverter.GetBytes(startSegment), 0, a, i, 4);
                    Array.Copy(BitConverter.GetBytes(thisSegment), 0, a, i, 4);

                    i = 60;
                    // offset 60
                    // number of the disk with the start of the zip64 eocd
                    Array.Copy(BitConverter.GetBytes(thisSegment), 0, a, i, 4);
                    i += 4;
                    i += 8;

                    // offset 72
                    // total number of disks
                    Array.Copy(BitConverter.GetBytes(thisSegment), 0, a, i, 4);
                }
                s.Write(a, 0, a.Length);
            }
            else
                a2 = GenCentralDirectoryFooter(Start, Finish, zip64, countOfEntries, comment, container);


            // now, the regular footer
            if (startSegment != 0)
            {
                // The assumption is the central directory is never split across
                // segment boundaries.

                UInt16 thisSegment = (UInt16) zss.ComputeSegment(a2.Length);
                int i = 4;
                // number of this disk
                Array.Copy(BitConverter.GetBytes(thisSegment), 0, a2, i, 2);
                i += 2;
                // number of the disk with the start of the central directory
                //Array.Copy(BitConverter.GetBytes((UInt16)startSegment), 0, a2, i, 2);
                Array.Copy(BitConverter.GetBytes(thisSegment), 0, a2, i, 2);
                i += 2;
            }

            s.Write(a2, 0, a2.Length);

            // reset the contiguous write property if necessary
            if (zss != null)
                zss.ContiguousWrite = false;

            return needZip64CentralDirectory;
        }


        private static System.Text.Encoding GetEncoding(ZipContainer container, string t)
        {
            switch (container.AlternateEncodingUsage)
            {
                case ZipOption.Always:
                    return container.AlternateEncoding;
                case ZipOption.Never:
                    return container.DefaultEncoding;
            }

            // AsNecessary is in force
            var e = container.DefaultEncoding;
            if (t == null) return e;

            var bytes = e.GetBytes(t);
            var t2 = e.GetString(bytes,0,bytes.Length);
            if (t2.Equals(t)) return e;
            return container.AlternateEncoding;
        }



        private static byte[] GenCentralDirectoryFooter(long StartOfCentralDirectory,
                                                        long EndOfCentralDirectory,
                                                        Zip64Option zip64,
                                                        int entryCount,
                                                        string comment,
                                                        ZipContainer container)
        {
            System.Text.Encoding encoding = GetEncoding(container, comment);
            int j = 0;
            int bufferLength = 22;
            byte[] block = null;
            Int16 commentLength = 0;
            if ((comment != null) && (comment.Length != 0))
            {
                block = encoding.GetBytes(comment);
                commentLength = (Int16)block.Length;
            }
            bufferLength += commentLength;
            byte[] bytes = new byte[bufferLength];

            int i = 0;
            // signature
            byte[] sig = BitConverter.GetBytes(ZipConstants.EndOfCentralDirectorySignature);
            Array.Copy(sig, 0, bytes, i, 4);
            i+=4;

            // number of this disk
            // (this number may change later)
            bytes[i++] = 0;
            bytes[i++] = 0;

            // number of the disk with the start of the central directory
            // (this number may change later)
            bytes[i++] = 0;
            bytes[i++] = 0;

            // handle ZIP64 extensions for the end-of-central-directory
            if (entryCount >= 0xFFFF || zip64 == Zip64Option.Always)
            {
                // the ZIP64 version.
                for (j = 0; j < 4; j++)
                    bytes[i++] = 0xFF;
            }
            else
            {
                // the standard version.
                // total number of entries in the central dir on this disk
                bytes[i++] = (byte)(entryCount & 0x00FF);
                bytes[i++] = (byte)((entryCount & 0xFF00) >> 8);

                // total number of entries in the central directory
                bytes[i++] = (byte)(entryCount & 0x00FF);
                bytes[i++] = (byte)((entryCount & 0xFF00) >> 8);
            }

            // size of the central directory
            Int64 SizeOfCentralDirectory = EndOfCentralDirectory - StartOfCentralDirectory;

            if (SizeOfCentralDirectory >= 0xFFFFFFFF || StartOfCentralDirectory >= 0xFFFFFFFF)
            {
                // The actual data is in the ZIP64 central directory structure
                for (j = 0; j < 8; j++)
                    bytes[i++] = 0xFF;
            }
            else
            {
                // size of the central directory (we just get the low 4 bytes)
                bytes[i++] = (byte)(SizeOfCentralDirectory & 0x000000FF);
                bytes[i++] = (byte)((SizeOfCentralDirectory & 0x0000FF00) >> 8);
                bytes[i++] = (byte)((SizeOfCentralDirectory & 0x00FF0000) >> 16);
                bytes[i++] = (byte)((SizeOfCentralDirectory & 0xFF000000) >> 24);

                // offset of the start of the central directory (we just get the low 4 bytes)
                bytes[i++] = (byte)(StartOfCentralDirectory & 0x000000FF);
                bytes[i++] = (byte)((StartOfCentralDirectory & 0x0000FF00) >> 8);
                bytes[i++] = (byte)((StartOfCentralDirectory & 0x00FF0000) >> 16);
                bytes[i++] = (byte)((StartOfCentralDirectory & 0xFF000000) >> 24);
            }


            // zip archive comment
            if ((comment == null) || (comment.Length == 0))
            {
                // no comment!
                bytes[i++] = (byte)0;
                bytes[i++] = (byte)0;
            }
            else
            {
                // the size of our buffer defines the max length of the comment we can write
                if (commentLength + i + 2 > bytes.Length) commentLength = (Int16)(bytes.Length - i - 2);
                bytes[i++] = (byte)(commentLength & 0x00FF);
                bytes[i++] = (byte)((commentLength & 0xFF00) >> 8);

                if (commentLength != 0)
                {
                    // now actually write the comment itself into the byte buffer
                    for (j = 0; (j < commentLength) && (i + j < bytes.Length); j++)
                    {
                        bytes[i + j] = block[j];
                    }
                    i += j;
                }
            }

            //   s.Write(bytes, 0, i);
            return bytes;
        }



        private static byte[] GenZip64EndOfCentralDirectory(long StartOfCentralDirectory,
                                                            long EndOfCentralDirectory,
                                                            int entryCount,
                                                            uint numSegments)
        {
            const int bufferLength = 12 + 44 + 20;

            byte[] bytes = new byte[bufferLength];

            int i = 0;
            // signature
            byte[] sig = BitConverter.GetBytes(ZipConstants.Zip64EndOfCentralDirectoryRecordSignature);
            Array.Copy(sig, 0, bytes, i, 4);
            i+=4;

            // There is a possibility to include "Extensible" data in the zip64
            // end-of-central-dir record.  I cannot figure out what it might be used to
            // store, so the size of this record is always fixed.  Maybe it is used for
            // strong encryption data?  That is for another day.
            long DataSize = 44;
            Array.Copy(BitConverter.GetBytes(DataSize), 0, bytes, i, 8);
            i += 8;

            // offset 12
            // VersionMadeBy = 45;
            bytes[i++] = 45;
            bytes[i++] = 0x00;

            // VersionNeededToExtract = 45;
            bytes[i++] = 45;
            bytes[i++] = 0x00;

            // offset 16
            // number of the disk, and the disk with the start of the central dir.
            // (this may change later)
            for (int j = 0; j < 8; j++)
                bytes[i++] = 0x00;

            // offset 24
            long numberOfEntries = entryCount;
            Array.Copy(BitConverter.GetBytes(numberOfEntries), 0, bytes, i, 8);
            i += 8;
            Array.Copy(BitConverter.GetBytes(numberOfEntries), 0, bytes, i, 8);
            i += 8;

            // offset 40
            Int64 SizeofCentraldirectory = EndOfCentralDirectory - StartOfCentralDirectory;
            Array.Copy(BitConverter.GetBytes(SizeofCentraldirectory), 0, bytes, i, 8);
            i += 8;
            Array.Copy(BitConverter.GetBytes(StartOfCentralDirectory), 0, bytes, i, 8);
            i += 8;

            // offset 56
            // now, the locator
            // signature
            sig = BitConverter.GetBytes(ZipConstants.Zip64EndOfCentralDirectoryLocatorSignature);
            Array.Copy(sig, 0, bytes, i, 4);
            i+=4;

            // offset 60
            // number of the disk with the start of the zip64 eocd
            // (this will change later)  (it will?)
            uint x2 = (numSegments==0)?0:(uint)(numSegments-1);
            Array.Copy(BitConverter.GetBytes(x2), 0, bytes, i, 4);
            i+=4;

            // offset 64
            // relative offset of the zip64 eocd
            Array.Copy(BitConverter.GetBytes(EndOfCentralDirectory), 0, bytes, i, 8);
            i += 8;

            // offset 72
            // total number of disks
            // (this will change later)
            Array.Copy(BitConverter.GetBytes(numSegments), 0, bytes, i, 4);
            i+=4;

            return bytes;
        }



        private static int CountEntries(ICollection<ZipEntry> _entries)
        {
            // Cannot just emit _entries.Count, because some of the entries
            // may have been skipped.
            int count = 0;
            foreach (var entry in _entries)
                if (entry.IncludedInMostRecentSave) count++;
            return count;
        }




    }
}
