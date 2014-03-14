// Events.cs
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
// Time-stamp: <2011-August-06 12:26:24>
//
// ------------------------------------------------------------------
//
// This module defines events used by the ZipFile class.
//
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Ionic.Zip
{
    /// <summary>
    ///   Delegate in which the application writes the <c>ZipEntry</c> content for the named entry.
    /// </summary>
    ///
    /// <param name="entryName">The name of the entry that must be written.</param>
    /// <param name="stream">The stream to which the entry data should be written.</param>
    ///
    /// <remarks>
    ///   When you add an entry and specify a <c>WriteDelegate</c>, via <see
    ///   cref="Ionic.Zip.ZipFile.AddEntry(string, WriteDelegate)"/>, the application
    ///   code provides the logic that writes the entry data directly into the zip file.
    /// </remarks>
    ///
    /// <example>
    ///
    /// This example shows how to define a WriteDelegate that obtains a DataSet, and then
    /// writes the XML for the DataSet into the zip archive.  There's no need to
    /// save the XML to a disk file first.
    ///
    /// <code lang="C#">
    /// private void WriteEntry (String filename, Stream output)
    /// {
    ///     DataSet ds1 = ObtainDataSet();
    ///     ds1.WriteXml(output);
    /// }
    ///
    /// private void Run()
    /// {
    ///     using (var zip = new ZipFile())
    ///     {
    ///         zip.AddEntry(zipEntryName, WriteEntry);
    ///         zip.Save(zipFileName);
    ///     }
    /// }
    /// </code>
    ///
    /// <code lang="vb">
    /// Private Sub WriteEntry (ByVal filename As String, ByVal output As Stream)
    ///     DataSet ds1 = ObtainDataSet()
    ///     ds1.WriteXml(stream)
    /// End Sub
    ///
    /// Public Sub Run()
    ///     Using zip = New ZipFile
    ///         zip.AddEntry(zipEntryName, New WriteDelegate(AddressOf WriteEntry))
    ///         zip.Save(zipFileName)
    ///     End Using
    /// End Sub
    /// </code>
    /// </example>
    /// <seealso cref="Ionic.Zip.ZipFile.AddEntry(string, WriteDelegate)"/>
    internal delegate void WriteDelegate(string entryName, System.IO.Stream stream);


    /// <summary>
    ///   Delegate in which the application opens the stream, just-in-time, for the named entry.
    /// </summary>
    ///
    /// <param name="entryName">
    /// The name of the ZipEntry that the application should open the stream for.
    /// </param>
    ///
    /// <remarks>
    ///   When you add an entry via <see cref="Ionic.Zip.ZipFile.AddEntry(string,
    ///   OpenDelegate, CloseDelegate)"/>, the application code provides the logic that
    ///   opens and closes the stream for the given ZipEntry.
    /// </remarks>
    ///
    /// <seealso cref="Ionic.Zip.ZipFile.AddEntry(string, OpenDelegate, CloseDelegate)"/>
    internal delegate System.IO.Stream OpenDelegate(string entryName);

    /// <summary>
    ///   Delegate in which the application closes the stream, just-in-time, for the named entry.
    /// </summary>
    ///
    /// <param name="entryName">
    /// The name of the ZipEntry that the application should close the stream for.
    /// </param>
    ///
    /// <param name="stream">The stream to be closed.</param>
    ///
    /// <remarks>
    ///   When you add an entry via <see cref="Ionic.Zip.ZipFile.AddEntry(string,
    ///   OpenDelegate, CloseDelegate)"/>, the application code provides the logic that
    ///   opens and closes the stream for the given ZipEntry.
    /// </remarks>
    ///
    /// <seealso cref="Ionic.Zip.ZipFile.AddEntry(string, OpenDelegate, CloseDelegate)"/>
    internal delegate void CloseDelegate(string entryName, System.IO.Stream stream);

    /// <summary>
    ///   Delegate for the callback by which the application tells the
    ///   library the CompressionLevel to use for a file.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    ///   Using this callback, the application can, for example, specify that
    ///   previously-compressed files (.mp3, .png, .docx, etc) should use a
    ///   <c>CompressionLevel</c> of <c>None</c>, or can set the compression level based
    ///   on any other factor.
    /// </para>
    /// </remarks>
    /// <seealso cref="Ionic.Zip.ZipFile.SetCompression"/>
    internal delegate Ionic.Zlib.CompressionLevel SetCompressionCallback(string localFileName, string fileNameInArchive);

    /// <summary>
    ///   In an EventArgs type, indicates which sort of progress event is being
    ///   reported.
    /// </summary>
    /// <remarks>
    ///   There are events for reading, events for saving, and events for
    ///   extracting. This enumeration allows a single EventArgs type to be sued to
    ///   describe one of multiple subevents. For example, a SaveProgress event is
    ///   invoked before, after, and during the saving of a single entry.  The value
    ///   of an enum with this type, specifies which event is being triggered.  The
    ///   same applies to Extraction, Reading and Adding events.
    /// </remarks>
    internal enum ZipProgressEventType
    {
        /// <summary>
        /// Indicates that a Add() operation has started.
        /// </summary>
        Adding_Started,

        /// <summary>
        /// Indicates that an individual entry in the archive has been added.
        /// </summary>
        Adding_AfterAddEntry,

        /// <summary>
        /// Indicates that a Add() operation has completed.
        /// </summary>
        Adding_Completed,

        /// <summary>
        /// Indicates that a Read() operation has started.
        /// </summary>
        Reading_Started,

        /// <summary>
        /// Indicates that an individual entry in the archive is about to be read.
        /// </summary>
        Reading_BeforeReadEntry,

        /// <summary>
        /// Indicates that an individual entry in the archive has just been read.
        /// </summary>
        Reading_AfterReadEntry,

        /// <summary>
        /// Indicates that a Read() operation has completed.
        /// </summary>
        Reading_Completed,

        /// <summary>
        /// The given event reports the number of bytes read so far
        /// during a Read() operation.
        /// </summary>
        Reading_ArchiveBytesRead,

        /// <summary>
        /// Indicates that a Save() operation has started.
        /// </summary>
        Saving_Started,

        /// <summary>
        /// Indicates that an individual entry in the archive is about to be written.
        /// </summary>
        Saving_BeforeWriteEntry,

        /// <summary>
        /// Indicates that an individual entry in the archive has just been saved.
        /// </summary>
        Saving_AfterWriteEntry,

        /// <summary>
        /// Indicates that a Save() operation has completed.
        /// </summary>
        Saving_Completed,

        /// <summary>
        /// Indicates that the zip archive has been created in a
        /// temporary location during a Save() operation.
        /// </summary>
        Saving_AfterSaveTempArchive,

        /// <summary>
        /// Indicates that the temporary file is about to be renamed to the final archive
        /// name during a Save() operation.
        /// </summary>
        Saving_BeforeRenameTempArchive,

        /// <summary>
        /// Indicates that the temporary file is has just been renamed to the final archive
        /// name during a Save() operation.
        /// </summary>
        Saving_AfterRenameTempArchive,

        /// <summary>
        /// Indicates that the self-extracting archive has been compiled
        /// during a Save() operation.
        /// </summary>
        Saving_AfterCompileSelfExtractor,

        /// <summary>
        /// The given event is reporting the number of source bytes that have run through the compressor so far
        /// during a Save() operation.
        /// </summary>
        Saving_EntryBytesRead,

        /// <summary>
        /// Indicates that an entry is about to be extracted.
        /// </summary>
        Extracting_BeforeExtractEntry,

        /// <summary>
        /// Indicates that an entry has just been extracted.
        /// </summary>
        Extracting_AfterExtractEntry,

        /// <summary>
        ///   Indicates that extraction of an entry would overwrite an existing
        ///   filesystem file. You must use
        ///   <see cref="ExtractExistingFileAction.InvokeExtractProgressEvent">
        ///   ExtractExistingFileAction.InvokeExtractProgressEvent</see> in the call
        ///   to <c>ZipEntry.Extract()</c> in order to receive this event.
        /// </summary>
        Extracting_ExtractEntryWouldOverwrite,

        /// <summary>
        ///   The given event is reporting the number of bytes written so far for
        ///   the current entry during an Extract() operation.
        /// </summary>
        Extracting_EntryBytesWritten,

        /// <summary>
        /// Indicates that an ExtractAll operation is about to begin.
        /// </summary>
        Extracting_BeforeExtractAll,

        /// <summary>
        /// Indicates that an ExtractAll operation has completed.
        /// </summary>
        Extracting_AfterExtractAll,

        /// <summary>
        /// Indicates that an error has occurred while saving a zip file.
        /// This generally means the file cannot be opened, because it has been
        /// removed, or because it is locked by another process.  It can also
        /// mean that the file cannot be Read, because of a range lock conflict.
        /// </summary>
        Error_Saving,
    }


    /// <summary>
    /// Provides information about the progress of a save, read, or extract operation.
    /// This is a base class; you will probably use one of the classes derived from this one.
    /// </summary>
    internal class ZipProgressEventArgs : EventArgs
    {
        private int _entriesTotal;
        private bool _cancel;
        private ZipEntry _latestEntry;
        private ZipProgressEventType _flavor;
        private String _archiveName;
        private Int64 _bytesTransferred;
        private Int64 _totalBytesToTransfer;


        internal ZipProgressEventArgs() { }

        internal ZipProgressEventArgs(string archiveName, ZipProgressEventType flavor)
        {
            this._archiveName = archiveName;
            this._flavor = flavor;
        }

        /// <summary>
        /// The total number of entries to be saved or extracted.
        /// </summary>
        public int EntriesTotal
        {
            get { return _entriesTotal; }
            set { _entriesTotal = value; }
        }

        /// <summary>
        /// The name of the last entry saved or extracted.
        /// </summary>
        public ZipEntry CurrentEntry
        {
            get { return _latestEntry; }
            set { _latestEntry = value; }
        }

        /// <summary>
        /// In an event handler, set this to cancel the save or extract
        /// operation that is in progress.
        /// </summary>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = _cancel || value; }
        }

        /// <summary>
        /// The type of event being reported.
        /// </summary>
        public ZipProgressEventType EventType
        {
            get { return _flavor; }
            set { _flavor = value; }
        }

        /// <summary>
        /// Returns the archive name associated to this event.
        /// </summary>
        public String ArchiveName
        {
            get { return _archiveName; }
            set { _archiveName = value; }
        }


        /// <summary>
        /// The number of bytes read or written so far for this entry.
        /// </summary>
        public Int64 BytesTransferred
        {
            get { return _bytesTransferred; }
            set { _bytesTransferred = value; }
        }



        /// <summary>
        /// Total number of bytes that will be read or written for this entry.
        /// This number will be -1 if the value cannot be determined.
        /// </summary>
        public Int64 TotalBytesToTransfer
        {
            get { return _totalBytesToTransfer; }
            set { _totalBytesToTransfer = value; }
        }
    }



    /// <summary>
    /// Provides information about the progress of a Read operation.
    /// </summary>
    internal class ReadProgressEventArgs : ZipProgressEventArgs
    {

        internal ReadProgressEventArgs() { }

        private ReadProgressEventArgs(string archiveName, ZipProgressEventType flavor)
            : base(archiveName, flavor)
        { }

        internal static ReadProgressEventArgs Before(string archiveName, int entriesTotal)
        {
            var x = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_BeforeReadEntry);
            x.EntriesTotal = entriesTotal;
            return x;
        }

        internal static ReadProgressEventArgs After(string archiveName, ZipEntry entry, int entriesTotal)
        {
            var x = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_AfterReadEntry);
            x.EntriesTotal = entriesTotal;
            x.CurrentEntry = entry;
            return x;
        }

        internal static ReadProgressEventArgs Started(string archiveName)
        {
            var x = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_Started);
            return x;
        }

        internal static ReadProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, Int64 bytesXferred, Int64 totalBytes)
        {
            var x = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_ArchiveBytesRead);
            x.CurrentEntry = entry;
            x.BytesTransferred = bytesXferred;
            x.TotalBytesToTransfer = totalBytes;
            return x;
        }

        internal static ReadProgressEventArgs Completed(string archiveName)
        {
            var x = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_Completed);
            return x;
        }

    }


    /// <summary>
    /// Provides information about the progress of a Add operation.
    /// </summary>
    internal class AddProgressEventArgs : ZipProgressEventArgs
    {
        internal AddProgressEventArgs() { }

        private AddProgressEventArgs(string archiveName, ZipProgressEventType flavor)
            : base(archiveName, flavor)
        { }

        internal static AddProgressEventArgs AfterEntry(string archiveName, ZipEntry entry, int entriesTotal)
        {
            var x = new AddProgressEventArgs(archiveName, ZipProgressEventType.Adding_AfterAddEntry);
            x.EntriesTotal = entriesTotal;
            x.CurrentEntry = entry;
            return x;
        }

        internal static AddProgressEventArgs Started(string archiveName)
        {
            var x = new AddProgressEventArgs(archiveName, ZipProgressEventType.Adding_Started);
            return x;
        }

        internal static AddProgressEventArgs Completed(string archiveName)
        {
            var x = new AddProgressEventArgs(archiveName, ZipProgressEventType.Adding_Completed);
            return x;
        }

    }

    /// <summary>
    /// Provides information about the progress of a save operation.
    /// </summary>
    internal class SaveProgressEventArgs : ZipProgressEventArgs
    {
        private int _entriesSaved;

        /// <summary>
        /// Constructor for the SaveProgressEventArgs.
        /// </summary>
        /// <param name="archiveName">the name of the zip archive.</param>
        /// <param name="before">whether this is before saving the entry, or after</param>
        /// <param name="entriesTotal">The total number of entries in the zip archive.</param>
        /// <param name="entriesSaved">Number of entries that have been saved.</param>
        /// <param name="entry">The entry involved in the event.</param>
        internal SaveProgressEventArgs(string archiveName, bool before, int entriesTotal, int entriesSaved, ZipEntry entry)
            : base(archiveName, (before) ? ZipProgressEventType.Saving_BeforeWriteEntry : ZipProgressEventType.Saving_AfterWriteEntry)
        {
            this.EntriesTotal = entriesTotal;
            this.CurrentEntry = entry;
            this._entriesSaved = entriesSaved;
        }

        internal SaveProgressEventArgs() { }

        internal SaveProgressEventArgs(string archiveName, ZipProgressEventType flavor)
            : base(archiveName, flavor)
        { }


        internal static SaveProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, Int64 bytesXferred, Int64 totalBytes)
        {
            var x = new SaveProgressEventArgs(archiveName, ZipProgressEventType.Saving_EntryBytesRead);
            x.ArchiveName = archiveName;
            x.CurrentEntry = entry;
            x.BytesTransferred = bytesXferred;
            x.TotalBytesToTransfer = totalBytes;
            return x;
        }

        internal static SaveProgressEventArgs Started(string archiveName)
        {
            var x = new SaveProgressEventArgs(archiveName, ZipProgressEventType.Saving_Started);
            return x;
        }

        internal static SaveProgressEventArgs Completed(string archiveName)
        {
            var x = new SaveProgressEventArgs(archiveName, ZipProgressEventType.Saving_Completed);
            return x;
        }

        /// <summary>
        /// Number of entries saved so far.
        /// </summary>
        public int EntriesSaved
        {
            get { return _entriesSaved; }
        }
    }


    /// <summary>
    /// Provides information about the progress of the extract operation.
    /// </summary>
    internal class ExtractProgressEventArgs : ZipProgressEventArgs
    {
        private int _entriesExtracted;
        private string _target;

        /// <summary>
        /// Constructor for the ExtractProgressEventArgs.
        /// </summary>
        /// <param name="archiveName">the name of the zip archive.</param>
        /// <param name="before">whether this is before saving the entry, or after</param>
        /// <param name="entriesTotal">The total number of entries in the zip archive.</param>
        /// <param name="entriesExtracted">Number of entries that have been extracted.</param>
        /// <param name="entry">The entry involved in the event.</param>
        /// <param name="extractLocation">The location to which entries are extracted.</param>
        internal ExtractProgressEventArgs(string archiveName, bool before, int entriesTotal, int entriesExtracted, ZipEntry entry, string extractLocation)
            : base(archiveName, (before) ? ZipProgressEventType.Extracting_BeforeExtractEntry : ZipProgressEventType.Extracting_AfterExtractEntry)
        {
            this.EntriesTotal = entriesTotal;
            this.CurrentEntry = entry;
            this._entriesExtracted = entriesExtracted;
            this._target = extractLocation;
        }

        internal ExtractProgressEventArgs(string archiveName, ZipProgressEventType flavor)
            : base(archiveName, flavor)
        { }

        internal ExtractProgressEventArgs()
        { }


        internal static ExtractProgressEventArgs BeforeExtractEntry(string archiveName, ZipEntry entry, string extractLocation)
        {
            var x = new ExtractProgressEventArgs
                {
                    ArchiveName = archiveName,
                    EventType = ZipProgressEventType.Extracting_BeforeExtractEntry,
                    CurrentEntry = entry,
                    _target = extractLocation,
                };
            return x;
        }

        internal static ExtractProgressEventArgs ExtractExisting(string archiveName, ZipEntry entry, string extractLocation)
        {
            var x = new ExtractProgressEventArgs
                {
                    ArchiveName = archiveName,
                    EventType = ZipProgressEventType.Extracting_ExtractEntryWouldOverwrite,
                    CurrentEntry = entry,
                    _target = extractLocation,
                };
            return x;
        }

        internal static ExtractProgressEventArgs AfterExtractEntry(string archiveName, ZipEntry entry, string extractLocation)
        {
            var x = new ExtractProgressEventArgs
                {
                    ArchiveName = archiveName,
                    EventType = ZipProgressEventType.Extracting_AfterExtractEntry,
                    CurrentEntry = entry,
                    _target = extractLocation,
                };
            return x;
        }

        internal static ExtractProgressEventArgs ExtractAllStarted(string archiveName, string extractLocation)
        {
            var x = new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_BeforeExtractAll);
            x._target = extractLocation;
            return x;
        }

        internal static ExtractProgressEventArgs ExtractAllCompleted(string archiveName, string extractLocation)
        {
            var x = new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_AfterExtractAll);
            x._target = extractLocation;
            return x;
        }


        internal static ExtractProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, Int64 bytesWritten, Int64 totalBytes)
        {
            var x = new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_EntryBytesWritten);
            x.ArchiveName = archiveName;
            x.CurrentEntry = entry;
            x.BytesTransferred = bytesWritten;
            x.TotalBytesToTransfer = totalBytes;
            return x;
        }



        /// <summary>
        /// Number of entries extracted so far.  This is set only if the
        /// EventType is Extracting_BeforeExtractEntry or Extracting_AfterExtractEntry, and
        /// the Extract() is occurring witin the scope of a call to ExtractAll().
        /// </summary>
        public int EntriesExtracted
        {
            get { return _entriesExtracted; }
        }

        /// <summary>
        /// Returns the extraction target location, a filesystem path.
        /// </summary>
        public String ExtractLocation
        {
            get { return _target; }
        }

    }



    /// <summary>
    /// Provides information about the an error that occurred while zipping.
    /// </summary>
    internal class ZipErrorEventArgs : ZipProgressEventArgs
    {
        private Exception _exc;
        private ZipErrorEventArgs() { }
        internal static ZipErrorEventArgs Saving(string archiveName, ZipEntry entry, Exception exception)
        {
            var x = new ZipErrorEventArgs
                {
                    EventType = ZipProgressEventType.Error_Saving,
                    ArchiveName = archiveName,
                    CurrentEntry = entry,
                    _exc = exception
                };
            return x;
        }

        /// <summary>
        /// Returns the exception that occurred, if any.
        /// </summary>
        public Exception @Exception
        {
            get { return _exc; }
        }

        /// <summary>
        /// Returns the name of the file that caused the exception, if any.
        /// </summary>
        public String FileName
        {
            get { return CurrentEntry.LocalFileName; }
        }
    }


}
