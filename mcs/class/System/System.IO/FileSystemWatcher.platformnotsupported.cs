// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Enumeration;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    public partial class FileSystemWatcher : Component, ISupportInitialize
    {
        internal const string EXCEPTION_MESSAGE = "System.IO.FileSystemWatcher is not supported on the current platform.";

        public FileSystemWatcher ()
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public FileSystemWatcher (string path)
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public FileSystemWatcher (string path, string filter)
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public NotifyFilters NotifyFilter
        {
            get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public Collection<string> Filters => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

        public bool EnableRaisingEvents
        {
            get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public string Filter
        {
            get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public bool IncludeSubdirectories
        {
            get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public int InternalBufferSize
        {
            get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public string Path
        {
            get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public event FileSystemEventHandler Changed
        {
            add { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            remove { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public event FileSystemEventHandler Created
        {
            add { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            remove { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public event FileSystemEventHandler Deleted
        {
            add { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            remove { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public event ErrorEventHandler Error
        {
            add { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            remove { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public event RenamedEventHandler Renamed
        {
            add { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            remove { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType) => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

        public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType, int timeout)
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public override ISite Site
        {
            get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public ISynchronizeInvoke SynchronizingObject
        {
            get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
            set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
        }

        public void BeginInit ()
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public void EndInit ()
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        [SuppressMessage ("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#", Justification = "Changing from protected to private would be a breaking change")]
        protected void OnChanged (FileSystemEventArgs e)
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        [SuppressMessage ("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#", Justification = "Changing from protected to private would be a breaking change")]
        protected void OnCreated (FileSystemEventArgs e)
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        [SuppressMessage ("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#", Justification = "Changing from protected to private would be a breaking change")]
        protected void OnDeleted(FileSystemEventArgs e)
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        [SuppressMessage ("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#", Justification = "Changing from protected to private would be a breaking change")]
        protected void OnError (ErrorEventArgs e)
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        [SuppressMessage ("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#", Justification = "Changing from protected to private would be a breaking change")]
        protected void OnRenamed (RenamedEventArgs e)
        {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }
    }
}