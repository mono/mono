//
// System.Diagnostics.StackTrace.cs
//
// Author:
//      Alexander Klyubin (klyubin@aqris.com)
//      Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections;

namespace System.Diagnostics {

	[Serializable]
	[MonoTODO ("Fix serialization compatibility with MS.NET")]
	public class StackTrace {

		public const int METHODS_TO_SKIP = 0;

		private StackFrame[] frames;

		public StackTrace ()
		{
			init_frames (METHODS_TO_SKIP, false);
		}

		public StackTrace (bool needFileInfo)
		{
			init_frames (METHODS_TO_SKIP, needFileInfo);
		}

		public StackTrace (int skipFrames)
		{
			init_frames (skipFrames, false);
		}

		public StackTrace (int skipFrames, bool needFileInfo)
		{
			init_frames (skipFrames, needFileInfo);
		}

		void init_frames (int skipFrames, bool needFileInfo)
		{
			StackFrame sf;
			ArrayList al = new ArrayList ();

			skipFrames += 2;
			
			while ((sf = new StackFrame (skipFrames, needFileInfo)) != null &&
			       sf.GetMethod () != null) {
				
				al.Add (sf);
				skipFrames++;
			};

			frames = (StackFrame [])al.ToArray (typeof (StackFrame));	
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static StackFrame [] get_trace (Exception e, int skipFrames, bool needFileInfo);

		public StackTrace (Exception e)
		{
			frames = get_trace (e, METHODS_TO_SKIP, false);
		}

		public StackTrace (Exception e, bool needFileInfo)
		{
			frames = get_trace (e, METHODS_TO_SKIP, needFileInfo);
		}

		public StackTrace (Exception e, int skipFrames)
		{
			frames = get_trace (e, skipFrames, false);
		}

		public StackTrace (Exception e, int skipFrames, bool needFileInfo)
		{
			frames = get_trace (e, skipFrames, needFileInfo);
		}

		public StackTrace (StackFrame frame)
		{
			this.frames = new StackFrame [1];
			this.frames [0] = frame;
		}

		[MonoTODO]
		public StackTrace (Thread targetThread, bool needFileInfo)
		{
			throw new NotImplementedException();
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

#if NET_2_0
		[ComVisibleAttribute (false)]
		public virtual
#else
		// used for CAS implementation
		internal
#endif
		StackFrame[] GetFrames ()
		{
			return frames;
		}

		public override string ToString ()
		{
			string result = "";
			for (int i = 0; i < FrameCount; i++) {
				StackFrame frame = GetFrame (i);
				result += "\n\tat " + FrameToString (frame);
			}

			return result;
		}

		//
		// These are not on the Framework
		//
#if false
		public override bool Equals (Object obj)
		{
			if ((obj == null) || (!(obj is StackTrace))) {
				return false;
			}

			StackTrace rhs = (StackTrace) obj;

			if (FrameCount != rhs.FrameCount) {
				return false;
			}

			for (int i = 0; i < FrameCount; i++) {
				if (!GetFrame (i).Equals (rhs.GetFrame (i))) {
					return false;
				}
			}

			return true;
		}

		public override int GetHashCode ()
		{
			return FrameCount;
		}
#endif

		private static string FrameToString (StackFrame frame)
		{
			MethodBase method = frame.GetMethod ();
			if (method != null) {
				// Method information available
				return  method.DeclaringType.FullName
					+ "." + method.Name + "()";
			}
			else {
				// Method information not available
				return "<unknown method>";
			}
		}
	}
}
