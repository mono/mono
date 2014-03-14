// ZipFile.Events.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2008, 2009, 2011 Dino Chiesa .
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
// Time-stamp: <2011-July-09 08:42:35>
//
// ------------------------------------------------------------------
//
// This module defines the methods for issuing events from the ZipFile class.
//
// ------------------------------------------------------------------
//

using System;
using System.IO;

namespace Ionic.Zip
{
    internal partial class ZipFile
    {
        private string ArchiveNameForEvent
        {
            get
            {
                return (_name != null) ? _name : "(stream)";
            }
        }

        #region Save

        /// <summary>
        ///   An event handler invoked when a Save() starts, before and after each
        ///   entry has been written to the archive, when a Save() completes, and
        ///   during other Save events.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   Depending on the particular event, different properties on the <see
        ///   cref="SaveProgressEventArgs"/> parameter are set.  The following
        ///   table summarizes the available EventTypes and the conditions under
        ///   which this event handler is invoked with a
        ///   <c>SaveProgressEventArgs</c> with the given EventType.
        /// </para>
        ///
        /// <list type="table">
        /// <listheader>
        /// <term>value of EntryType</term>
        /// <description>Meaning and conditions</description>
        /// </listheader>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Saving_Started</term>
        /// <description>Fired when ZipFile.Save() begins.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Saving_BeforeSaveEntry</term>
        /// <description>
        ///   Fired within ZipFile.Save(), just before writing data for each
        ///   particular entry.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Saving_AfterSaveEntry</term>
        /// <description>
        ///   Fired within ZipFile.Save(), just after having finished writing data
        ///   for each particular entry.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Saving_Completed</term>
        /// <description>Fired when ZipFile.Save() has completed.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Saving_AfterSaveTempArchive</term>
        /// <description>
        ///   Fired after the temporary file has been created.  This happens only
        ///   when saving to a disk file.  This event will not be invoked when
        ///   saving to a stream.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Saving_BeforeRenameTempArchive</term>
        /// <description>
        ///   Fired just before renaming the temporary file to the permanent
        ///   location.  This happens only when saving to a disk file.  This event
        ///   will not be invoked when saving to a stream.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Saving_AfterRenameTempArchive</term>
        /// <description>
        ///   Fired just after renaming the temporary file to the permanent
        ///   location.  This happens only when saving to a disk file.  This event
        ///   will not be invoked when saving to a stream.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Saving_AfterCompileSelfExtractor</term>
        /// <description>
        ///   Fired after a self-extracting archive has finished compiling.  This
        ///   EventType is used only within SaveSelfExtractor().
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Saving_BytesRead</term>
        /// <description>
        ///   Set during the save of a particular entry, to update progress of the
        ///   Save().  When this EventType is set, the BytesTransferred is the
        ///   number of bytes that have been read from the source stream.  The
        ///   TotalBytesToTransfer is the number of bytes in the uncompressed
        ///   file.
        /// </description>
        /// </item>
        ///
        /// </list>
        /// </remarks>
        ///
        /// <example>
        ///
        ///    This example uses an anonymous method to handle the
        ///    SaveProgress event, by updating a progress bar.
        ///
        /// <code lang="C#">
        /// progressBar1.Value = 0;
        /// progressBar1.Max = listbox1.Items.Count;
        /// using (ZipFile zip = new ZipFile())
        /// {
        ///    // listbox1 contains a list of filenames
        ///    zip.AddFiles(listbox1.Items);
        ///
        ///    // do the progress bar:
        ///    zip.SaveProgress += (sender, e) => {
        ///       if (e.EventType == ZipProgressEventType.Saving_BeforeWriteEntry) {
        ///          progressBar1.PerformStep();
        ///       }
        ///    };
        ///
        ///    zip.Save(fs);
        /// }
        /// </code>
        /// </example>
        ///
        /// <example>
        ///   This example uses a named method as the
        ///   <c>SaveProgress</c> event handler, to update the user, in a
        ///   console-based application.
        ///
        /// <code lang="C#">
        /// static bool justHadByteUpdate= false;
        /// internal static void SaveProgress(object sender, SaveProgressEventArgs e)
        /// {
        ///     if (e.EventType == ZipProgressEventType.Saving_Started)
        ///         Console.WriteLine("Saving: {0}", e.ArchiveName);
        ///
        ///     else if (e.EventType == ZipProgressEventType.Saving_Completed)
        ///     {
        ///         justHadByteUpdate= false;
        ///         Console.WriteLine();
        ///         Console.WriteLine("Done: {0}", e.ArchiveName);
        ///     }
        ///
        ///     else if (e.EventType == ZipProgressEventType.Saving_BeforeWriteEntry)
        ///     {
        ///         if (justHadByteUpdate)
        ///             Console.WriteLine();
        ///         Console.WriteLine("  Writing: {0} ({1}/{2})",
        ///                           e.CurrentEntry.FileName, e.EntriesSaved, e.EntriesTotal);
        ///         justHadByteUpdate= false;
        ///     }
        ///
        ///     else if (e.EventType == ZipProgressEventType.Saving_EntryBytesRead)
        ///     {
        ///         if (justHadByteUpdate)
        ///             Console.SetCursorPosition(0, Console.CursorTop);
        ///          Console.Write("     {0}/{1} ({2:N0}%)", e.BytesTransferred, e.TotalBytesToTransfer,
        ///                       e.BytesTransferred / (0.01 * e.TotalBytesToTransfer ));
        ///         justHadByteUpdate= true;
        ///     }
        /// }
        ///
        /// internal static ZipUp(string targetZip, string directory)
        /// {
        ///   using (var zip = new ZipFile()) {
        ///     zip.SaveProgress += SaveProgress;
        ///     zip.AddDirectory(directory);
        ///     zip.Save(targetZip);
        ///   }
        /// }
        ///
        /// </code>
        ///
        /// <code lang="VB">
        /// Public Sub ZipUp(ByVal targetZip As String, ByVal directory As String)
        ///     Using zip As ZipFile = New ZipFile
        ///         AddHandler zip.SaveProgress, AddressOf MySaveProgress
        ///         zip.AddDirectory(directory)
        ///         zip.Save(targetZip)
        ///     End Using
        /// End Sub
        ///
        /// Private Shared justHadByteUpdate As Boolean = False
        ///
        /// Public Shared Sub MySaveProgress(ByVal sender As Object, ByVal e As SaveProgressEventArgs)
        ///     If (e.EventType Is ZipProgressEventType.Saving_Started) Then
        ///         Console.WriteLine("Saving: {0}", e.ArchiveName)
        ///
        ///     ElseIf (e.EventType Is ZipProgressEventType.Saving_Completed) Then
        ///         justHadByteUpdate = False
        ///         Console.WriteLine
        ///         Console.WriteLine("Done: {0}", e.ArchiveName)
        ///
        ///     ElseIf (e.EventType Is ZipProgressEventType.Saving_BeforeWriteEntry) Then
        ///         If justHadByteUpdate Then
        ///             Console.WriteLine
        ///         End If
        ///         Console.WriteLine("  Writing: {0} ({1}/{2})", e.CurrentEntry.FileName, e.EntriesSaved, e.EntriesTotal)
        ///         justHadByteUpdate = False
        ///
        ///     ElseIf (e.EventType Is ZipProgressEventType.Saving_EntryBytesRead) Then
        ///         If justHadByteUpdate Then
        ///             Console.SetCursorPosition(0, Console.CursorTop)
        ///         End If
        ///         Console.Write("     {0}/{1} ({2:N0}%)", e.BytesTransferred, _
        ///                       e.TotalBytesToTransfer, _
        ///                       (CDbl(e.BytesTransferred) / (0.01 * e.TotalBytesToTransfer)))
        ///         justHadByteUpdate = True
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        ///
        /// <example>
        ///
        /// This is a more complete example of using the SaveProgress
        /// events in a Windows Forms application, with a
        /// Thread object.
        ///
        /// <code lang="C#">
        /// delegate void SaveEntryProgress(SaveProgressEventArgs e);
        /// delegate void ButtonClick(object sender, EventArgs e);
        ///
        /// internal class WorkerOptions
        /// {
        ///     public string ZipName;
        ///     public string Folder;
        ///     public string Encoding;
        ///     public string Comment;
        ///     public int ZipFlavor;
        ///     public Zip64Option Zip64;
        /// }
        ///
        /// private int _progress2MaxFactor;
        /// private bool _saveCanceled;
        /// private long _totalBytesBeforeCompress;
        /// private long _totalBytesAfterCompress;
        /// private Thread _workerThread;
        ///
        ///
        /// private void btnZipup_Click(object sender, EventArgs e)
        /// {
        ///     KickoffZipup();
        /// }
        ///
        /// private void btnCancel_Click(object sender, EventArgs e)
        /// {
        ///     if (this.lblStatus.InvokeRequired)
        ///     {
        ///         this.lblStatus.Invoke(new ButtonClick(this.btnCancel_Click), new object[] { sender, e });
        ///     }
        ///     else
        ///     {
        ///         _saveCanceled = true;
        ///         lblStatus.Text = "Canceled...";
        ///         ResetState();
        ///     }
        /// }
        ///
        /// private void KickoffZipup()
        /// {
        ///     _folderName = tbDirName.Text;
        ///
        ///     if (_folderName == null || _folderName == "") return;
        ///     if (this.tbZipName.Text == null || this.tbZipName.Text == "") return;
        ///
        ///     // check for existence of the zip file:
        ///     if (System.IO.File.Exists(this.tbZipName.Text))
        ///     {
        ///         var dlgResult = MessageBox.Show(String.Format("The file you have specified ({0}) already exists." +
        ///                                                       "  Do you want to overwrite this file?", this.tbZipName.Text),
        ///                                         "Confirmation is Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        ///         if (dlgResult != DialogResult.Yes) return;
        ///         System.IO.File.Delete(this.tbZipName.Text);
        ///     }
        ///
        ///      _saveCanceled = false;
        ///     _nFilesCompleted = 0;
        ///     _totalBytesAfterCompress = 0;
        ///     _totalBytesBeforeCompress = 0;
        ///     this.btnOk.Enabled = false;
        ///     this.btnOk.Text = "Zipping...";
        ///     this.btnCancel.Enabled = true;
        ///     lblStatus.Text = "Zipping...";
        ///
        ///     var options = new WorkerOptions
        ///     {
        ///         ZipName = this.tbZipName.Text,
        ///         Folder = _folderName,
        ///         Encoding = "ibm437"
        ///     };
        ///
        ///     if (this.comboBox1.SelectedIndex != 0)
        ///     {
        ///         options.Encoding = this.comboBox1.SelectedItem.ToString();
        ///     }
        ///
        ///     if (this.radioFlavorSfxCmd.Checked)
        ///         options.ZipFlavor = 2;
        ///     else if (this.radioFlavorSfxGui.Checked)
        ///         options.ZipFlavor = 1;
        ///     else options.ZipFlavor = 0;
        ///
        ///     if (this.radioZip64AsNecessary.Checked)
        ///         options.Zip64 = Zip64Option.AsNecessary;
        ///     else if (this.radioZip64Always.Checked)
        ///         options.Zip64 = Zip64Option.Always;
        ///     else options.Zip64 = Zip64Option.Never;
        ///
        ///     options.Comment = String.Format("Encoding:{0} || Flavor:{1} || ZIP64:{2}\r\nCreated at {3} || {4}\r\n",
        ///                 options.Encoding,
        ///                 FlavorToString(options.ZipFlavor),
        ///                 options.Zip64.ToString(),
        ///                 System.DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss"),
        ///                 this.Text);
        ///
        ///     if (this.tbComment.Text != TB_COMMENT_NOTE)
        ///         options.Comment += this.tbComment.Text;
        ///
        ///     _workerThread = new Thread(this.DoSave);
        ///     _workerThread.Name = "Zip Saver thread";
        ///     _workerThread.Start(options);
        ///     this.Cursor = Cursors.WaitCursor;
        ///  }
        ///
        ///
        /// private void DoSave(Object p)
        /// {
        ///     WorkerOptions options = p as WorkerOptions;
        ///     try
        ///     {
        ///         using (var zip1 = new ZipFile())
        ///         {
        ///             zip1.ProvisionalAlternateEncoding = System.Text.Encoding.GetEncoding(options.Encoding);
        ///             zip1.Comment = options.Comment;
        ///             zip1.AddDirectory(options.Folder);
        ///             _entriesToZip = zip1.EntryFileNames.Count;
        ///             SetProgressBars();
        ///             zip1.SaveProgress += this.zip1_SaveProgress;
        ///
        ///             zip1.UseZip64WhenSaving = options.Zip64;
        ///
        ///             if (options.ZipFlavor == 1)
        ///                 zip1.SaveSelfExtractor(options.ZipName, SelfExtractorFlavor.WinFormsApplication);
        ///             else if (options.ZipFlavor == 2)
        ///                 zip1.SaveSelfExtractor(options.ZipName, SelfExtractorFlavor.ConsoleApplication);
        ///             else
        ///                 zip1.Save(options.ZipName);
        ///         }
        ///     }
        ///     catch (System.Exception exc1)
        ///     {
        ///         MessageBox.Show(String.Format("Exception while zipping: {0}", exc1.Message));
        ///         btnCancel_Click(null, null);
        ///     }
        /// }
        ///
        ///
        ///
        /// void zip1_SaveProgress(object sender, SaveProgressEventArgs e)
        /// {
        ///     switch (e.EventType)
        ///     {
        ///         case ZipProgressEventType.Saving_AfterWriteEntry:
        ///             StepArchiveProgress(e);
        ///             break;
        ///         case ZipProgressEventType.Saving_EntryBytesRead:
        ///             StepEntryProgress(e);
        ///             break;
        ///         case ZipProgressEventType.Saving_Completed:
        ///             SaveCompleted();
        ///             break;
        ///         case ZipProgressEventType.Saving_AfterSaveTempArchive:
        ///             // this event only occurs when saving an SFX file
        ///             TempArchiveSaved();
        ///             break;
        ///     }
        ///     if (_saveCanceled)
        ///         e.Cancel = true;
        /// }
        ///
        ///
        ///
        /// private void StepArchiveProgress(SaveProgressEventArgs e)
        /// {
        ///     if (this.progressBar1.InvokeRequired)
        ///     {
        ///         this.progressBar1.Invoke(new SaveEntryProgress(this.StepArchiveProgress), new object[] { e });
        ///     }
        ///     else
        ///     {
        ///         if (!_saveCanceled)
        ///         {
        ///             _nFilesCompleted++;
        ///             this.progressBar1.PerformStep();
        ///             _totalBytesAfterCompress += e.CurrentEntry.CompressedSize;
        ///             _totalBytesBeforeCompress += e.CurrentEntry.UncompressedSize;
        ///
        ///             // reset the progress bar for the entry:
        ///             this.progressBar2.Value = this.progressBar2.Maximum = 1;
        ///
        ///             this.Update();
        ///         }
        ///     }
        /// }
        ///
        ///
        /// private void StepEntryProgress(SaveProgressEventArgs e)
        /// {
        ///     if (this.progressBar2.InvokeRequired)
        ///     {
        ///         this.progressBar2.Invoke(new SaveEntryProgress(this.StepEntryProgress), new object[] { e });
        ///     }
        ///     else
        ///     {
        ///         if (!_saveCanceled)
        ///         {
        ///             if (this.progressBar2.Maximum == 1)
        ///             {
        ///                 // reset
        ///                 Int64 max = e.TotalBytesToTransfer;
        ///                 _progress2MaxFactor = 0;
        ///                 while (max > System.Int32.MaxValue)
        ///                 {
        ///                     max /= 2;
        ///                     _progress2MaxFactor++;
        ///                 }
        ///                 this.progressBar2.Maximum = (int)max;
        ///                 lblStatus.Text = String.Format("{0} of {1} files...({2})",
        ///                     _nFilesCompleted + 1, _entriesToZip, e.CurrentEntry.FileName);
        ///             }
        ///
        ///              int xferred = e.BytesTransferred >> _progress2MaxFactor;
        ///
        ///              this.progressBar2.Value = (xferred >= this.progressBar2.Maximum)
        ///                 ? this.progressBar2.Maximum
        ///                 : xferred;
        ///
        ///              this.Update();
        ///         }
        ///     }
        /// }
        ///
        /// private void SaveCompleted()
        /// {
        ///     if (this.lblStatus.InvokeRequired)
        ///     {
        ///         this.lblStatus.Invoke(new MethodInvoker(this.SaveCompleted));
        ///     }
        ///     else
        ///     {
        ///         lblStatus.Text = String.Format("Done, Compressed {0} files, {1:N0}% of original.",
        ///             _nFilesCompleted, (100.00 * _totalBytesAfterCompress) / _totalBytesBeforeCompress);
        ///          ResetState();
        ///     }
        /// }
        ///
        /// private void ResetState()
        /// {
        ///     this.btnCancel.Enabled = false;
        ///     this.btnOk.Enabled = true;
        ///     this.btnOk.Text = "Zip it!";
        ///     this.progressBar1.Value = 0;
        ///     this.progressBar2.Value = 0;
        ///     this.Cursor = Cursors.Default;
        ///     if (!_workerThread.IsAlive)
        ///         _workerThread.Join();
        /// }
        /// </code>
        ///
        /// </example>
        ///
        /// <seealso cref="Ionic.Zip.ZipFile.ReadProgress"/>
        /// <seealso cref="Ionic.Zip.ZipFile.AddProgress"/>
        /// <seealso cref="Ionic.Zip.ZipFile.ExtractProgress"/>
        public event EventHandler<SaveProgressEventArgs> SaveProgress;


        internal bool OnSaveBlock(ZipEntry entry, Int64 bytesXferred, Int64 totalBytesToXfer)
        {
            EventHandler<SaveProgressEventArgs> sp = SaveProgress;
            if (sp != null)
            {
                var e = SaveProgressEventArgs.ByteUpdate(ArchiveNameForEvent, entry,
                                                         bytesXferred, totalBytesToXfer);
                sp(this, e);
                if (e.Cancel)
                    _saveOperationCanceled = true;
            }
            return _saveOperationCanceled;
        }

        private void OnSaveEntry(int current, ZipEntry entry, bool before)
        {
            EventHandler<SaveProgressEventArgs> sp = SaveProgress;
            if (sp != null)
            {
                var e = new SaveProgressEventArgs(ArchiveNameForEvent, before, _entries.Count, current, entry);
                sp(this, e);
                if (e.Cancel)
                    _saveOperationCanceled = true;
            }
        }

        private void OnSaveEvent(ZipProgressEventType eventFlavor)
        {
            EventHandler<SaveProgressEventArgs> sp = SaveProgress;
            if (sp != null)
            {
                var e = new SaveProgressEventArgs(ArchiveNameForEvent, eventFlavor);
                sp(this, e);
                if (e.Cancel)
                    _saveOperationCanceled = true;
            }
        }

        private void OnSaveStarted()
        {
            EventHandler<SaveProgressEventArgs> sp = SaveProgress;
            if (sp != null)
            {
                var e = SaveProgressEventArgs.Started(ArchiveNameForEvent);
                sp(this, e);
                if (e.Cancel)
                    _saveOperationCanceled = true;
            }
        }
        private void OnSaveCompleted()
        {
            EventHandler<SaveProgressEventArgs> sp = SaveProgress;
            if (sp != null)
            {
                var e = SaveProgressEventArgs.Completed(ArchiveNameForEvent);
                sp(this, e);
            }
        }
        #endregion


        #region Read
        /// <summary>
        /// An event handler invoked before, during, and after the reading of a zip archive.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// Depending on the particular event being signaled, different properties on the
        /// <see cref="ReadProgressEventArgs"/> parameter are set.  The following table
        /// summarizes the available EventTypes and the conditions under which this
        /// event handler is invoked with a <c>ReadProgressEventArgs</c> with the given EventType.
        /// </para>
        ///
        /// <list type="table">
        /// <listheader>
        /// <term>value of EntryType</term>
        /// <description>Meaning and conditions</description>
        /// </listheader>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Reading_Started</term>
        /// <description>Fired just as ZipFile.Read() begins. Meaningful properties: ArchiveName.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Reading_Completed</term>
        /// <description>Fired when ZipFile.Read() has completed. Meaningful properties: ArchiveName.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Reading_ArchiveBytesRead</term>
        /// <description>Fired while reading, updates the number of bytes read for the entire archive.
        /// Meaningful properties: ArchiveName, CurrentEntry, BytesTransferred, TotalBytesToTransfer.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Reading_BeforeReadEntry</term>
        /// <description>Indicates an entry is about to be read from the archive.
        /// Meaningful properties: ArchiveName, EntriesTotal.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Reading_AfterReadEntry</term>
        /// <description>Indicates an entry has just been read from the archive.
        /// Meaningful properties: ArchiveName, EntriesTotal, CurrentEntry.
        /// </description>
        /// </item>
        ///
        /// </list>
        /// </remarks>
        ///
        /// <seealso cref="Ionic.Zip.ZipFile.SaveProgress"/>
        /// <seealso cref="Ionic.Zip.ZipFile.AddProgress"/>
        /// <seealso cref="Ionic.Zip.ZipFile.ExtractProgress"/>
        public event EventHandler<ReadProgressEventArgs> ReadProgress;

        private void OnReadStarted()
        {
            EventHandler<ReadProgressEventArgs> rp = ReadProgress;
            if (rp != null)
            {
                    var e = ReadProgressEventArgs.Started(ArchiveNameForEvent);
                    rp(this, e);
            }
        }

        private void OnReadCompleted()
        {
            EventHandler<ReadProgressEventArgs> rp = ReadProgress;
            if (rp != null)
            {
                    var e = ReadProgressEventArgs.Completed(ArchiveNameForEvent);
                    rp(this, e);
            }
        }

        internal void OnReadBytes(ZipEntry entry)
        {
            EventHandler<ReadProgressEventArgs> rp = ReadProgress;
            if (rp != null)
            {
                    var e = ReadProgressEventArgs.ByteUpdate(ArchiveNameForEvent,
                                        entry,
                                        ReadStream.Position,
                                        LengthOfReadStream);
                    rp(this, e);
            }
        }

        internal void OnReadEntry(bool before, ZipEntry entry)
        {
            EventHandler<ReadProgressEventArgs> rp = ReadProgress;
            if (rp != null)
            {
                ReadProgressEventArgs e = (before)
                    ? ReadProgressEventArgs.Before(ArchiveNameForEvent, _entries.Count)
                    : ReadProgressEventArgs.After(ArchiveNameForEvent, entry, _entries.Count);
                rp(this, e);
            }
        }

        private Int64 _lengthOfReadStream = -99;
        private Int64 LengthOfReadStream
        {
            get
            {
                if (_lengthOfReadStream == -99)
                {
                    _lengthOfReadStream = (_ReadStreamIsOurs)
                        ? SharedUtilities.GetFileLength(_name)
                        : -1L;
                }
                return _lengthOfReadStream;
            }
        }
        #endregion


        #region Extract
        /// <summary>
        ///   An event handler invoked before, during, and after extraction of
        ///   entries in the zip archive.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   Depending on the particular event, different properties on the <see
        ///   cref="ExtractProgressEventArgs"/> parameter are set.  The following
        ///   table summarizes the available EventTypes and the conditions under
        ///   which this event handler is invoked with a
        ///   <c>ExtractProgressEventArgs</c> with the given EventType.
        /// </para>
        ///
        /// <list type="table">
        /// <listheader>
        /// <term>value of EntryType</term>
        /// <description>Meaning and conditions</description>
        /// </listheader>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Extracting_BeforeExtractAll</term>
        /// <description>
        ///   Set when ExtractAll() begins. The ArchiveName, Overwrite, and
        ///   ExtractLocation properties are meaningful.</description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Extracting_AfterExtractAll</term>
        /// <description>
        ///   Set when ExtractAll() has completed.  The ArchiveName, Overwrite,
        ///   and ExtractLocation properties are meaningful.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Extracting_BeforeExtractEntry</term>
        /// <description>
        ///   Set when an Extract() on an entry in the ZipFile has begun.
        ///   Properties that are meaningful: ArchiveName, EntriesTotal,
        ///   CurrentEntry, Overwrite, ExtractLocation, EntriesExtracted.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Extracting_AfterExtractEntry</term>
        /// <description>
        ///   Set when an Extract() on an entry in the ZipFile has completed.
        ///   Properties that are meaningful: ArchiveName, EntriesTotal,
        ///   CurrentEntry, Overwrite, ExtractLocation, EntriesExtracted.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Extracting_EntryBytesWritten</term>
        /// <description>
        ///   Set within a call to Extract() on an entry in the ZipFile, as data
        ///   is extracted for the entry.  Properties that are meaningful:
        ///   ArchiveName, CurrentEntry, BytesTransferred, TotalBytesToTransfer.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>ZipProgressEventType.Extracting_ExtractEntryWouldOverwrite</term>
        /// <description>
        ///   Set within a call to Extract() on an entry in the ZipFile, when the
        ///   extraction would overwrite an existing file. This event type is used
        ///   only when <c>ExtractExistingFileAction</c> on the <c>ZipFile</c> or
        ///   <c>ZipEntry</c> is set to <c>InvokeExtractProgressEvent</c>.
        /// </description>
        /// </item>
        ///
        /// </list>
        ///
        /// </remarks>
        ///
        /// <example>
        /// <code>
        /// private static bool justHadByteUpdate = false;
        /// internal static void ExtractProgress(object sender, ExtractProgressEventArgs e)
        /// {
        ///   if(e.EventType == ZipProgressEventType.Extracting_EntryBytesWritten)
        ///   {
        ///     if (justHadByteUpdate)
        ///       Console.SetCursorPosition(0, Console.CursorTop);
        ///
        ///     Console.Write("   {0}/{1} ({2:N0}%)", e.BytesTransferred, e.TotalBytesToTransfer,
        ///                   e.BytesTransferred / (0.01 * e.TotalBytesToTransfer ));
        ///     justHadByteUpdate = true;
        ///   }
        ///   else if(e.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
        ///   {
        ///     if (justHadByteUpdate)
        ///       Console.WriteLine();
        ///     Console.WriteLine("Extracting: {0}", e.CurrentEntry.FileName);
        ///     justHadByteUpdate= false;
        ///   }
        /// }
        ///
        /// internal static ExtractZip(string zipToExtract, string directory)
        /// {
        ///   string TargetDirectory= "extract";
        ///   using (var zip = ZipFile.Read(zipToExtract)) {
        ///     zip.ExtractProgress += ExtractProgress;
        ///     foreach (var e in zip1)
        ///     {
        ///       e.Extract(TargetDirectory, true);
        ///     }
        ///   }
        /// }
        ///
        /// </code>
        /// <code lang="VB">
        /// Public Shared Sub Main(ByVal args As String())
        ///     Dim ZipToUnpack As String = "C1P3SML.zip"
        ///     Dim TargetDir As String = "ExtractTest_Extract"
        ///     Console.WriteLine("Extracting file {0} to {1}", ZipToUnpack, TargetDir)
        ///     Using zip1 As ZipFile = ZipFile.Read(ZipToUnpack)
        ///         AddHandler zip1.ExtractProgress, AddressOf MyExtractProgress
        ///         Dim e As ZipEntry
        ///         For Each e In zip1
        ///             e.Extract(TargetDir, True)
        ///         Next
        ///     End Using
        /// End Sub
        ///
        /// Private Shared justHadByteUpdate As Boolean = False
        ///
        /// Public Shared Sub MyExtractProgress(ByVal sender As Object, ByVal e As ExtractProgressEventArgs)
        ///     If (e.EventType = ZipProgressEventType.Extracting_EntryBytesWritten) Then
        ///         If ExtractTest.justHadByteUpdate Then
        ///             Console.SetCursorPosition(0, Console.CursorTop)
        ///         End If
        ///         Console.Write("   {0}/{1} ({2:N0}%)", e.BytesTransferred, e.TotalBytesToTransfer, (CDbl(e.BytesTransferred) / (0.01 * e.TotalBytesToTransfer)))
        ///         ExtractTest.justHadByteUpdate = True
        ///     ElseIf (e.EventType = ZipProgressEventType.Extracting_BeforeExtractEntry) Then
        ///         If ExtractTest.justHadByteUpdate Then
        ///             Console.WriteLine
        ///         End If
        ///         Console.WriteLine("Extracting: {0}", e.CurrentEntry.FileName)
        ///         ExtractTest.justHadByteUpdate = False
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        ///
        /// <seealso cref="Ionic.Zip.ZipFile.SaveProgress"/>
        /// <seealso cref="Ionic.Zip.ZipFile.ReadProgress"/>
        /// <seealso cref="Ionic.Zip.ZipFile.AddProgress"/>
        public event EventHandler<ExtractProgressEventArgs> ExtractProgress;



        private void OnExtractEntry(int current, bool before, ZipEntry currentEntry, string path)
        {
            EventHandler<ExtractProgressEventArgs> ep = ExtractProgress;
            if (ep != null)
            {
                var e = new ExtractProgressEventArgs(ArchiveNameForEvent, before, _entries.Count, current, currentEntry, path);
                ep(this, e);
                if (e.Cancel)
                    _extractOperationCanceled = true;
            }
        }


        // Can be called from within ZipEntry._ExtractOne.
        internal bool OnExtractBlock(ZipEntry entry, Int64 bytesWritten, Int64 totalBytesToWrite)
        {
            EventHandler<ExtractProgressEventArgs> ep = ExtractProgress;
            if (ep != null)
            {
                var e = ExtractProgressEventArgs.ByteUpdate(ArchiveNameForEvent, entry,
                                                            bytesWritten, totalBytesToWrite);
                ep(this, e);
                if (e.Cancel)
                    _extractOperationCanceled = true;
            }
            return _extractOperationCanceled;
        }


        // Can be called from within ZipEntry.InternalExtract.
        internal bool OnSingleEntryExtract(ZipEntry entry, string path, bool before)
        {
            EventHandler<ExtractProgressEventArgs> ep = ExtractProgress;
            if (ep != null)
            {
                var e = (before)
                    ? ExtractProgressEventArgs.BeforeExtractEntry(ArchiveNameForEvent, entry, path)
                    : ExtractProgressEventArgs.AfterExtractEntry(ArchiveNameForEvent, entry, path);
                ep(this, e);
                if (e.Cancel)
                    _extractOperationCanceled = true;
            }
            return _extractOperationCanceled;
        }

        internal bool OnExtractExisting(ZipEntry entry, string path)
        {
            EventHandler<ExtractProgressEventArgs> ep = ExtractProgress;
            if (ep != null)
            {
                var e = ExtractProgressEventArgs.ExtractExisting(ArchiveNameForEvent, entry, path);
                ep(this, e);
                if (e.Cancel)
                    _extractOperationCanceled = true;
            }
            return _extractOperationCanceled;
        }


        private void OnExtractAllCompleted(string path)
        {
            EventHandler<ExtractProgressEventArgs> ep = ExtractProgress;
            if (ep != null)
            {
                var e = ExtractProgressEventArgs.ExtractAllCompleted(ArchiveNameForEvent,
                                                                     path );
                ep(this, e);
            }
        }


        private void OnExtractAllStarted(string path)
        {
            EventHandler<ExtractProgressEventArgs> ep = ExtractProgress;
            if (ep != null)
            {
                var e = ExtractProgressEventArgs.ExtractAllStarted(ArchiveNameForEvent,
                                                                   path );
                ep(this, e);
            }
        }


        #endregion



        #region Add
        /// <summary>
        /// An event handler invoked before, during, and after Adding entries to a zip archive.
        /// </summary>
        ///
        /// <remarks>
        ///     Adding a large number of entries to a zip file can take a long
        ///     time.  For example, when calling <see cref="AddDirectory(string)"/> on a
        ///     directory that contains 50,000 files, it could take 3 minutes or so.
        ///     This event handler allws an application to track the progress of the Add
        ///     operation, and to optionally cancel a lengthy Add operation.
        /// </remarks>
        ///
        /// <example>
        /// <code lang="C#">
        ///
        /// int _numEntriesToAdd= 0;
        /// int _numEntriesAdded= 0;
        /// void AddProgressHandler(object sender, AddProgressEventArgs e)
        /// {
        ///     switch (e.EventType)
        ///     {
        ///         case ZipProgressEventType.Adding_Started:
        ///             Console.WriteLine("Adding files to the zip...");
        ///             break;
        ///         case ZipProgressEventType.Adding_AfterAddEntry:
        ///             _numEntriesAdded++;
        ///             Console.WriteLine(String.Format("Adding file {0}/{1} :: {2}",
        ///                                      _numEntriesAdded, _numEntriesToAdd, e.CurrentEntry.FileName));
        ///             break;
        ///         case ZipProgressEventType.Adding_Completed:
        ///             Console.WriteLine("Added all files");
        ///             break;
        ///     }
        /// }
        ///
        /// void CreateTheZip()
        /// {
        ///     using (ZipFile zip = new ZipFile())
        ///     {
        ///         zip.AddProgress += AddProgressHandler;
        ///         zip.AddDirectory(System.IO.Path.GetFileName(DirToZip));
        ///         zip.Save(ZipFileToCreate);
        ///     }
        /// }
        ///
        /// </code>
        ///
        /// <code lang="VB">
        ///
        /// Private Sub AddProgressHandler(ByVal sender As Object, ByVal e As AddProgressEventArgs)
        ///     Select Case e.EventType
        ///         Case ZipProgressEventType.Adding_Started
        ///             Console.WriteLine("Adding files to the zip...")
        ///             Exit Select
        ///         Case ZipProgressEventType.Adding_AfterAddEntry
        ///             Console.WriteLine(String.Format("Adding file {0}", e.CurrentEntry.FileName))
        ///             Exit Select
        ///         Case ZipProgressEventType.Adding_Completed
        ///             Console.WriteLine("Added all files")
        ///             Exit Select
        ///     End Select
        /// End Sub
        ///
        /// Sub CreateTheZip()
        ///     Using zip as ZipFile = New ZipFile
        ///         AddHandler zip.AddProgress, AddressOf AddProgressHandler
        ///         zip.AddDirectory(System.IO.Path.GetFileName(DirToZip))
        ///         zip.Save(ZipFileToCreate);
        ///     End Using
        /// End Sub
        ///
        /// </code>
        ///
        /// </example>
        ///
        /// <seealso cref="Ionic.Zip.ZipFile.SaveProgress"/>
        /// <seealso cref="Ionic.Zip.ZipFile.ReadProgress"/>
        /// <seealso cref="Ionic.Zip.ZipFile.ExtractProgress"/>
        public event EventHandler<AddProgressEventArgs> AddProgress;

        private void OnAddStarted()
        {
            EventHandler<AddProgressEventArgs> ap = AddProgress;
            if (ap != null)
            {
                var e = AddProgressEventArgs.Started(ArchiveNameForEvent);
                ap(this, e);
                if (e.Cancel) // workitem 13371
                    _addOperationCanceled = true;
            }
        }

        private void OnAddCompleted()
        {
            EventHandler<AddProgressEventArgs> ap = AddProgress;
            if (ap != null)
            {
                var e = AddProgressEventArgs.Completed(ArchiveNameForEvent);
                ap(this, e);
            }
        }

        internal void AfterAddEntry(ZipEntry entry)
        {
            EventHandler<AddProgressEventArgs> ap = AddProgress;
            if (ap != null)
            {
                var e = AddProgressEventArgs.AfterEntry(ArchiveNameForEvent, entry, _entries.Count);
                ap(this, e);
                if (e.Cancel) // workitem 13371
                    _addOperationCanceled = true;
            }
        }

        #endregion



        #region Error
        /// <summary>
        /// An event that is raised when an error occurs during open or read of files
        /// while saving a zip archive.
        /// </summary>
        ///
        /// <remarks>
        ///  <para>
        ///     Errors can occur as a file is being saved to the zip archive.  For
        ///     example, the File.Open may fail, or a File.Read may fail, because of
        ///     lock conflicts or other reasons.  If you add a handler to this event,
        ///     you can handle such errors in your own code.  If you don't add a
        ///     handler, the library will throw an exception if it encounters an I/O
        ///     error during a call to <c>Save()</c>.
        ///  </para>
        ///
        ///  <para>
        ///    Setting a handler implicitly sets <see cref="ZipFile.ZipErrorAction"/> to
        ///    <c>ZipErrorAction.InvokeErrorEvent</c>.
        ///  </para>
        ///
        ///  <para>
        ///    The handler you add applies to all <see cref="ZipEntry"/> items that are
        ///    subsequently added to the <c>ZipFile</c> instance. If you set this
        ///    property after you have added items to the <c>ZipFile</c>, but before you
        ///    have called <c>Save()</c>, errors that occur while saving those items
        ///    will not cause the error handler to be invoked.
        ///  </para>
        ///
        ///  <para>
        ///    If you want to handle any errors that occur with any entry in the zip
        ///    file using the same error handler, then add your error handler once,
        ///    before adding any entries to the zip archive.
        ///  </para>
        ///
        ///  <para>
        ///    In the error handler method, you need to set the <see
        ///    cref="ZipEntry.ZipErrorAction"/> property on the
        ///    <c>ZipErrorEventArgs.CurrentEntry</c>.  This communicates back to
        ///    DotNetZip what you would like to do with this particular error.  Within
        ///    an error handler, if you set the <c>ZipEntry.ZipErrorAction</c> property
        ///    on the <c>ZipEntry</c> to <c>ZipErrorAction.InvokeErrorEvent</c> or if
        ///    you don't set it at all, the library will throw the exception. (It is the
        ///    same as if you had set the <c>ZipEntry.ZipErrorAction</c> property on the
        ///    <c>ZipEntry</c> to <c>ZipErrorAction.Throw</c>.) If you set the
        ///    <c>ZipErrorEventArgs.Cancel</c> to true, the entire <c>Save()</c> will be
        ///    canceled.
        ///  </para>
        ///
        ///  <para>
        ///    In the case that you use <c>ZipErrorAction.Skip</c>, implying that
        ///    you want to skip the entry for which there's been an error, DotNetZip
        ///    tries to seek backwards in the output stream, and truncate all bytes
        ///    written on behalf of that particular entry. This works only if the
        ///    output stream is seekable.  It will not work, for example, when using
        ///    ASPNET's Response.OutputStream.
        ///  </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///
        /// This example shows how to use an event handler to handle
        /// errors during save of the zip file.
        /// <code lang="C#">
        ///
        /// internal static void MyZipError(object sender, ZipErrorEventArgs e)
        /// {
        ///     Console.WriteLine("Error saving {0}...", e.FileName);
        ///     Console.WriteLine("   Exception: {0}", e.exception);
        ///     ZipEntry entry = e.CurrentEntry;
        ///     string response = null;
        ///     // Ask the user whether he wants to skip this error or not
        ///     do
        ///     {
        ///         Console.Write("Retry, Skip, Throw, or Cancel ? (R/S/T/C) ");
        ///         response = Console.ReadLine();
        ///         Console.WriteLine();
        ///
        ///     } while (response != null &amp;&amp;
        ///              response[0]!='S' &amp;&amp; response[0]!='s' &amp;&amp;
        ///              response[0]!='R' &amp;&amp; response[0]!='r' &amp;&amp;
        ///              response[0]!='T' &amp;&amp; response[0]!='t' &amp;&amp;
        ///              response[0]!='C' &amp;&amp; response[0]!='c');
        ///
        ///     e.Cancel = (response[0]=='C' || response[0]=='c');
        ///
        ///     if (response[0]=='S' || response[0]=='s')
        ///         entry.ZipErrorAction = ZipErrorAction.Skip;
        ///     else if (response[0]=='R' || response[0]=='r')
        ///         entry.ZipErrorAction = ZipErrorAction.Retry;
        ///     else if (response[0]=='T' || response[0]=='t')
        ///         entry.ZipErrorAction = ZipErrorAction.Throw;
        /// }
        ///
        /// public void SaveTheFile()
        /// {
        ///   string directoryToZip = "fodder";
        ///   string directoryInArchive = "files";
        ///   string zipFileToCreate = "Archive.zip";
        ///   using (var zip = new ZipFile())
        ///   {
        ///     // set the event handler before adding any entries
        ///     zip.ZipError += MyZipError;
        ///     zip.AddDirectory(directoryToZip, directoryInArchive);
        ///     zip.Save(zipFileToCreate);
        ///   }
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Private Sub MyZipError(ByVal sender As Object, ByVal e As Ionic.Zip.ZipErrorEventArgs)
        ///     ' At this point, the application could prompt the user for an action to take.
        ///     ' But in this case, this application will simply automatically skip the file, in case of error.
        ///     Console.WriteLine("Zip Error,  entry {0}", e.CurrentEntry.FileName)
        ///     Console.WriteLine("   Exception: {0}", e.exception)
        ///     ' set the desired ZipErrorAction on the CurrentEntry to communicate that to DotNetZip
        ///     e.CurrentEntry.ZipErrorAction = Zip.ZipErrorAction.Skip
        /// End Sub
        ///
        /// Public Sub SaveTheFile()
        ///     Dim directoryToZip As String = "fodder"
        ///     Dim directoryInArchive As String = "files"
        ///     Dim zipFileToCreate as String = "Archive.zip"
        ///     Using zipArchive As ZipFile = New ZipFile
        ///         ' set the event handler before adding any entries
        ///         AddHandler zipArchive.ZipError, AddressOf MyZipError
        ///         zipArchive.AddDirectory(directoryToZip, directoryInArchive)
        ///         zipArchive.Save(zipFileToCreate)
        ///     End Using
        /// End Sub
        ///
        /// </code>
        /// </example>
        ///
        /// <seealso cref="Ionic.Zip.ZipFile.ZipErrorAction"/>
        public event EventHandler<ZipErrorEventArgs> ZipError;

        internal bool OnZipErrorSaving(ZipEntry entry, Exception exc)
        {
            if (ZipError != null)
            {
                lock (LOCK)
                {
                    var e = ZipErrorEventArgs.Saving(this.Name, entry, exc);
                    ZipError(this, e);
                    if (e.Cancel)
                        _saveOperationCanceled = true;
                }
            }
            return _saveOperationCanceled;
        }
        #endregion

    }
}
