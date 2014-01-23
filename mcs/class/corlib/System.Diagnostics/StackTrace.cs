//
// System.Diagnostics.StackTrace.cs
//
// Author:
//      Alexander Klyubin (klyubin@aqris.com)
//      Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Diagnostics {

	[Serializable]
	[ComVisible (true)]
	[MonoTODO ("Serialized objects are not compatible with .NET")]
	public class StackTrace {

		public const int METHODS_TO_SKIP = 0;

		private StackFrame[] frames;
		private bool debug_info;

		public StackTrace ()
		{
			init_frames (METHODS_TO_SKIP, false);
		}

		public StackTrace (bool fNeedFileInfo)
		{
			init_frames (METHODS_TO_SKIP, fNeedFileInfo);
		}

		public StackTrace (int skipFrames)
		{
			init_frames (skipFrames, false);
		}

		public StackTrace (int skipFrames, bool fNeedFileInfo)
		{
			init_frames (skipFrames, fNeedFileInfo);
		}

		void init_frames (int skipFrames, bool fNeedFileInfo)
		{
			if (skipFrames < 0)
				throw new ArgumentOutOfRangeException ("< 0", "skipFrames");

			StackFrame sf;
			var l = new List<StackFrame> ();

			skipFrames += 2;
			
			while ((sf = new StackFrame (skipFrames, fNeedFileInfo)) != null &&
			       sf.GetMethod () != null) {
				
				l.Add (sf);
				skipFrames++;
			};

			debug_info = fNeedFileInfo;
			frames = l.ToArray ();
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static StackFrame [] get_trace (Exception e, int skipFrames, bool fNeedFileInfo);

		public StackTrace (Exception e)
			: this (e, METHODS_TO_SKIP, false)
		{
		}

		public StackTrace (Exception e, bool fNeedFileInfo)
			: this (e, METHODS_TO_SKIP, fNeedFileInfo)
		{
		}

		public StackTrace (Exception e, int skipFrames)
			: this (e, skipFrames, false)
		{
		}

		public StackTrace (Exception e, int skipFrames, bool fNeedFileInfo)
			: this (e, skipFrames, fNeedFileInfo, false)
		{
		}

		internal StackTrace (Exception e, int skipFrames, bool fNeedFileInfo, bool returnNativeFrames)
		{
			if (e == null)
				throw new ArgumentNullException ("e");
			if (skipFrames < 0)
				throw new ArgumentOutOfRangeException ("< 0", "skipFrames");

			frames = get_trace (e, skipFrames, fNeedFileInfo);

			if (!returnNativeFrames) {
				bool resize = false;
				for (int i = 0; i < frames.Length; ++i)
					if (frames [i].GetMethod () == null)
						resize = true;

				if (resize) {
					var l = new List<StackFrame> ();

					for (int i = 0; i < frames.Length; ++i)
						if (frames [i].GetMethod () != null)
							l.Add (frames [i]);

					frames = l.ToArray ();
				}
			}
		}

		public StackTrace (StackFrame frame)
		{
			this.frames = new StackFrame [1];
			this.frames [0] = frame;
		}

		[MonoLimitation ("Not possible to create StackTraces from other threads")]
#if NET_4_5
		[Obsolete]
#endif
		public StackTrace (Thread targetThread, bool needFileInfo)
		{
			if (targetThread == Thread.CurrentThread){
				init_frames (METHODS_TO_SKIP, needFileInfo);
				return;
			}
			
			throw new NotImplementedException ();
		}

		public virtual int FrameCount {
			get {
				return (frames == null) ? 0 : frames.Length;
			}
		}

		public virtual StackFrame GetFrame (int index)
		{
			if ((index < 0) || (index >= FrameCount)) {
				return null;
			}

			return frames [index];
		}

		[ComVisibleAttribute (false)]
		public virtual StackFrame[] GetFrames ()
		{
			return frames;
		}

		public override string ToString ()
		{
			string newline = String.Format ("{0}   {1} ", Environment.NewLine, Locale.GetText ("at"));
			string unknown = Locale.GetText ("<unknown method>");
			string debuginfo = Locale.GetText (" in {0}:line {1}");
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < FrameCount; i++) {
				StackFrame frame = GetFrame (i);
				if (i > 0)
					sb.Append (newline);
				else
					sb.AppendFormat ("   {0} ", Locale.GetText ("at"));
				MethodBase method = frame.GetMethod ();
				if (method != null) {
					// Method information available
					sb.AppendFormat ("{0}.{1}", method.DeclaringType.FullName, method.Name);
					/* Append parameter information */
					sb.Append ("(");
					ParameterInfo[] p = method.GetParametersInternal ();
					for (int j = 0; j < p.Length; ++j) {
						if (j > 0)
							sb.Append (", ");
						Type pt = p[j].ParameterType;
						bool byref = pt.IsByRef;
						if (byref)
							pt = pt.GetElementType ();
						if (pt.IsClass && pt.Namespace != String.Empty) {
							sb.Append (pt.Namespace);
							sb.Append (".");
						}
						sb.Append (pt.Name);
						if (byref)
							sb.Append (" ByRef");
						sb.AppendFormat (" {0}", p [j].Name);
					}
					sb.Append (")");
				}
				else {
					// Method information not available
					sb.Append (unknown);
				}

				if (debug_info) {
					// we were asked for debugging informations
					// but that doesn't mean we have the debug information available
					string fname = frame.GetSecureFileName ();
					if (fname != "<filename unknown>")
						sb.AppendFormat (debuginfo, fname, frame.GetFileLineNumber ());
				}
			}
			return sb.ToString ();
		}
	}
}
