//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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

using System;
using System.Web.Util;
using System.IO;
using vmw.@internal.io;
using vmw.common;
using System.ComponentModel;
using System.Threading;
using javax.servlet;
using System.Diagnostics;

namespace System.Web.J2EE
{
	internal static class J2EEUtils
	{
		#region InputStreamWrapper

		public sealed class InputStreamWrapper : Stream
		{
			readonly java.io.InputStream _ins;

			public InputStreamWrapper (java.io.InputStream ins) {
				_ins = ins;
			}

			public override bool CanRead {
				get { return true; }
			}

			public override bool CanSeek {
				get { return _ins.markSupported (); }
			}

			public override bool CanWrite {
				get { return false; }
			}

			public override void Flush () {
			}

			public override long Length {
				get { return _ins.available (); }
			}

			public override long Position {
				get {
					throw new NotSupportedException ();
				}
				set {
					throw new NotSupportedException ();
				}
			}

			public override int Read (byte [] buffer, int offset, int count) {
				int r = _ins.read (TypeUtils.ToSByteArray (buffer), offset, count);
				return r < 0 ? 0 : r;
			}

			public override long Seek (long offset, SeekOrigin origin) {
				throw new NotImplementedException ();
			}

			public override void SetLength (long value) {
				throw new NotSupportedException ();
			}

			public override void Write (byte [] buffer, int offset, int count) {
				throw new NotSupportedException ();
			}

			public override void Close () {
				_ins.close ();
			}
		}

		#endregion

		public static int RunProc(string[] cmd)
		{	
			java.lang.Runtime rt = java.lang.Runtime.getRuntime();
			java.lang.Process proc = rt.exec(cmd);
			
			StreamGobbler errorGobbler = new 
				StreamGobbler(proc.getErrorStream(), "ERROR");            
          
			StreamGobbler outputGobbler = new 
				StreamGobbler(proc.getInputStream(), "OUTPUT");
                
			errorGobbler.start();
			outputGobbler.start();
                             
			int exitVal = proc.waitFor();
			return exitVal;	
		}
	}

	public class StreamGobbler : java.lang.Thread
	{
		java.io.InputStream _is;
		String _type;
    
		public StreamGobbler(java.io.InputStream ins, String type)
		{
			this._is = ins;
			this._type = type;
		}
    
		public override void run()
		{
			try
			{
				java.io.InputStreamReader isr = new java.io.InputStreamReader(_is);
				java.io.BufferedReader br = new java.io.BufferedReader(isr);
				String line=null;
				while ( (line = br.readLine()) != null)
				{
					Debug.WriteLine(_type + ">" + line); 
				}
			} 
			catch (Exception ex)
			{
				Debug.WriteLine (ex);
			}
		}
	}
}

#region FileSystemWatcher Stub

namespace System.IO
{
	[DefaultEvent ("Changed")]
#if NET_2_0
	[IODescription ("")]
#endif
	public class FileSystemWatcher : Component, ISupportInitialize
	{
		public FileSystemWatcher ()
			: this (String.Empty) {
		}

		public FileSystemWatcher (string path)
			: this (path, "*.*") {
		}

		public FileSystemWatcher (string path, string filter) {
		}

		#region Properties

		[DefaultValue (false)]
		[IODescription ("Flag to indicate if this instance is active")]
		public bool EnableRaisingEvents {
			get { return false; }
			set { }
		}

		[DefaultValue ("*.*")]
		[IODescription ("File name filter pattern")]
		[RecommendedAsConfigurable (true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string Filter {
			get { return "*.*"; }
			set { }
		}

		[DefaultValue (false)]
		[IODescription ("Flag to indicate we want to watch subdirectories")]
		public bool IncludeSubdirectories {
			get { return false; }
			set { }
		}

		[Browsable (false)]
		[DefaultValue (8192)]
		public int InternalBufferSize {
			get { return 8192; }
			set { }
		}

		[DefaultValue (NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite)]
		[IODescription ("Flag to indicate which change event we want to monitor")]
		public NotifyFilters NotifyFilter {
			get { return NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite; }
			set { }
		}

		[DefaultValue ("")]
		[IODescription ("The directory to monitor")]
		[RecommendedAsConfigurable (true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[Editor ("System.Diagnostics.Design.FSWPathEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string Path {
			get { return String.Empty; }
			set { }
		}

		[DefaultValue (null)]
		[IODescription ("The object used to marshal the event handler calls resulting from a directory change")]
#if NET_2_0
		[Browsable (false)]
#endif
		public ISynchronizeInvoke SynchronizingObject {
			get { return null; }
			set { }
		}

		#endregion // Properties

		#region Methods

		protected override void Dispose (bool disposing) {
			base.Dispose (disposing);
		}

		enum EventType
		{
			FileSystemEvent,
			ErrorEvent,
			RenameEvent
		}

		void RaiseEvent (Delegate ev, EventArgs arg, EventType evtype) {
			if (ev == null)
				return;

			if (SynchronizingObject == null) {
				Delegate [] delegates = ev.GetInvocationList ();
				if (evtype == EventType.RenameEvent) {
					foreach (RenamedEventHandler d in delegates) {
						d.BeginInvoke (this, (RenamedEventArgs) arg, null, null);
					}
				}
				else if (evtype == EventType.ErrorEvent) {
					foreach (ErrorEventHandler d in delegates) {
						d.BeginInvoke (this, (ErrorEventArgs) arg, null, null);
					}
				}
				else {
					foreach (FileSystemEventHandler d in delegates) {
						d.BeginInvoke (this, (FileSystemEventArgs) arg, null, null);
					}
				}
				return;
			}

			SynchronizingObject.BeginInvoke (ev, new object [] { this, arg });
		}

		protected void OnChanged (FileSystemEventArgs e) {
			RaiseEvent (Changed, e, EventType.FileSystemEvent);
		}

		protected void OnCreated (FileSystemEventArgs e) {
			RaiseEvent (Created, e, EventType.FileSystemEvent);
		}

		protected void OnDeleted (FileSystemEventArgs e) {
			RaiseEvent (Deleted, e, EventType.FileSystemEvent);
		}

		protected void OnError (ErrorEventArgs e) {
			RaiseEvent (Error, e, EventType.ErrorEvent);
		}

		protected void OnRenamed (RenamedEventArgs e) {
			RaiseEvent (Renamed, e, EventType.RenameEvent);
		}

		public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType) {
			return WaitForChanged (changeType, Timeout.Infinite);
		}

		public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType, int timeout) {
			return new WaitForChangedResult ();
		}

		#endregion

		#region Events and Delegates

		[IODescription ("Occurs when a file/directory change matches the filter")]
		public event FileSystemEventHandler Changed;

		[IODescription ("Occurs when a file/directory creation matches the filter")]
		public event FileSystemEventHandler Created;

		[IODescription ("Occurs when a file/directory deletion matches the filter")]
		public event FileSystemEventHandler Deleted;

		[Browsable (false)]
		public event ErrorEventHandler Error;

		[IODescription ("Occurs when a file/directory rename matches the filter")]
		public event RenamedEventHandler Renamed;

		#endregion // Events and Delegates

		#region ISupportInitialize Members

		public void BeginInit () {
		}

		public void EndInit () {
		}

		#endregion
	}
}
#endregion
