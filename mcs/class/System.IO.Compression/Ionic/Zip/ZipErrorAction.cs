// ZipErrorAction.cs
// ------------------------------------------------------------------
//
// Copyright (c)  2009 Dino Chiesa
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
// Time-stamp: <2009-September-01 18:43:20>
//
// ------------------------------------------------------------------
//
// This module defines the ZipErrorAction enum, which provides
// an action to take when errors occur when opening or reading
// files to be added to a zip file. 
// 
// ------------------------------------------------------------------


namespace Ionic.Zip
{
    /// <summary>
    /// An enum providing the options when an error occurs during opening or reading
    /// of a file or directory that is being saved to a zip file. 
    /// </summary>
    ///
    /// <remarks>
    ///  <para>
    ///    This enum describes the actions that the library can take when an error occurs
    ///    opening or reading a file, as it is being saved into a Zip archive. 
    ///  </para>
    ///
    ///  <para>
    ///     In some cases an error will occur when DotNetZip tries to open a file to be
    ///     added to the zip archive.  In other cases, an error might occur after the
    ///     file has been successfully opened, while DotNetZip is reading the file.
    ///  </para>
    /// 
    ///  <para>
    ///    The first problem might occur when calling AddDirectory() on a directory
    ///    that contains a Clipper .dbf file; the file is locked by Clipper and
    ///    cannot be opened by another process. An example of the second problem is
    ///    the ERROR_LOCK_VIOLATION that results when a file is opened by another
    ///    process, but not locked, and a range lock has been taken on the file.
    ///    Microsoft Outlook takes range locks on .PST files.
    ///  </para>
    /// </remarks>
    internal enum ZipErrorAction
    {
        /// <summary>
        /// Throw an exception when an error occurs while zipping.  This is the default
        /// behavior.  (For COM clients, this is a 0 (zero).)
        /// </summary>
        Throw,

        /// <summary>
        /// When an error occurs during zipping, for example a file cannot be opened,
        /// skip the file causing the error, and continue zipping.  (For COM clients,
        /// this is a 1.)
        /// </summary>
        Skip,
        
        /// <summary>
        /// When an error occurs during zipping, for example a file cannot be opened,
        /// retry the operation that caused the error. Be careful with this option. If
        /// the error is not temporary, the library will retry forever.  (For COM
        /// clients, this is a 2.)
        /// </summary>
        Retry,

        /// <summary>
        /// When an error occurs, invoke the zipError event.  The event type used is
        /// <see cref="ZipProgressEventType.Error_Saving"/>.  A typical use of this option:
        /// a GUI application may wish to pop up a dialog to allow the user to view the
        /// error that occurred, and choose an appropriate action.  After your
        /// processing in the error event, if you want to skip the file, set <see
        /// cref="ZipEntry.ZipErrorAction"/> on the
        /// <c>ZipProgressEventArgs.CurrentEntry</c> to <c>Skip</c>.  If you want the
        /// exception to be thrown, set <c>ZipErrorAction</c> on the <c>CurrentEntry</c>
        /// to <c>Throw</c>.  If you want to cancel the zip, set
        /// <c>ZipProgressEventArgs.Cancel</c> to true.  Cancelling differs from using
        /// Skip in that a cancel will not save any further entries, if there are any.
        /// (For COM clients, the value of this enum is a 3.)
        /// </summary>
        InvokeErrorEvent,
    }

}
