//
// FileSystemWatcher.cs
//
// Authors:
//  Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.IO
{
    public class FileSystemWatcher : IDisposable
    {
        public FileSystemWatcher () { throw new NotImplementedException (); }
        public FileSystemWatcher (string path) { throw new NotImplementedException (); }
        public FileSystemWatcher (string path, string filter) { throw new NotImplementedException (); }
        public bool EnableRaisingEvents { get { throw new NotImplementedException (); } set { throw new NotImplementedException (); } }
        public string Filter { get { throw new NotImplementedException (); } set { } }
        public bool IncludeSubdirectories { get { throw new NotImplementedException (); } set { } }
        public int InternalBufferSize { get { throw new NotImplementedException (); } set { } }
        public NotifyFilters NotifyFilter { get { throw new NotImplementedException (); } set { } }
        public string Path { get { throw new NotImplementedException (); } set { } }
        public event FileSystemEventHandler Changed;
        public event FileSystemEventHandler Created;
        public event FileSystemEventHandler Deleted;
        public event ErrorEventHandler Error;
        public event RenamedEventHandler Renamed;
        protected void OnChanged (FileSystemEventArgs e) { throw new NotImplementedException (); }
        protected void OnCreated (FileSystemEventArgs e) { throw new NotImplementedException (); }
        protected void OnDeleted (System.IO.FileSystemEventArgs e) { throw new NotImplementedException (); }
        protected void OnError (ErrorEventArgs e) { throw new NotImplementedException (); }
        protected void OnRenamed (RenamedEventArgs e) { throw new NotImplementedException (); }
        public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType) { throw new NotImplementedException (); }
        public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType, int timeout) { throw new NotImplementedException (); }

        public virtual void Dispose ()
        {
        }

        protected virtual void Dispose (bool disposing)
        {
        }
    }
}