//
// System.Diagnostics.StackFrame.cs
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

using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Diagnostics {

	[Serializable]
	[ComVisible (true)]
	[MonoTODO ("Serialized objects are not compatible with MS.NET")]
	[StructLayout (LayoutKind.Sequential)]
        public class StackFrame {

                public const int OFFSET_UNKNOWN = -1;

		#region Keep in sync with object-internals.h
		private int ilOffset = OFFSET_UNKNOWN;
		private int nativeOffset = OFFSET_UNKNOWN;
		private MethodBase methodBase;
		private string fileName;
		private int lineNumber;
		private int columnNumber;
        #pragma warning disable 649
		private string internalMethodName;
		#pragma warning restore 649
		#endregion

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool get_frame_info (int skip, bool needFileInfo, out MethodBase method,
						   out int iloffset, out int native_offset,
						   out string file, out int line, out int column);

                public StackFrame ()
		{
			get_frame_info (2, false, out methodBase, out ilOffset,
					out nativeOffset, out fileName, out lineNumber,
					out columnNumber);			
                }
                
		public StackFrame (bool fNeedFileInfo)
		{
			get_frame_info (2, fNeedFileInfo, out methodBase, out ilOffset,
					out nativeOffset, out fileName, out lineNumber,
					out columnNumber);			
                }
                
                public StackFrame (int skipFrames)
		{
			get_frame_info (skipFrames + 2, false, out methodBase, out ilOffset,
					out nativeOffset, out fileName, out lineNumber,
					out columnNumber);			
                }
                
                public StackFrame (int skipFrames, bool fNeedFileInfo) 
		{
			get_frame_info (skipFrames + 2, fNeedFileInfo, out methodBase, out ilOffset,
					out nativeOffset, out fileName, out lineNumber,
					out columnNumber);
                }
                
		// LAMESPEC: According to the MSDN docs, this creates a frame with _only_
		// the filename and lineNumber, but MS fills out the frame info as well.
                public StackFrame (string fileName, int lineNumber)
		{
			get_frame_info (2, false, out methodBase, out ilOffset,
					out nativeOffset, out fileName, out lineNumber,
					out columnNumber);
			this.fileName = fileName;
			this.lineNumber = lineNumber;
			this.columnNumber = 0;
		}
                
		// LAMESPEC: According to the MSDN docs, this creates a frame with _only_
		// the filename, lineNumber and colNumber, but MS fills out the frame info as well.
                public StackFrame (string fileName, int lineNumber, int colNumber)
		{
			get_frame_info (2, false, out methodBase, out ilOffset,
					out nativeOffset, out fileName, out lineNumber,
					out columnNumber);
			this.fileName = fileName;
			this.lineNumber = lineNumber;
			this.columnNumber = colNumber;
                }
                                  
                public virtual int GetFileLineNumber()
                {
                        return lineNumber;
                }
                
                public virtual int GetFileColumnNumber()
                {
                        return columnNumber;
                }
                
                public virtual string GetFileName()
                {
#if !NET_2_1
			if (SecurityManager.SecurityEnabled && (fileName != null) && (fileName.Length > 0)) {
				string fn = Path.GetFullPath (fileName);
				new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fn).Demand ();
			}
#endif
                        return fileName;
                }

		internal string GetSecureFileName ()
		{
			string filename = "<filename unknown>";
			if (fileName == null)
				return filename;
			try {
				filename = GetFileName ();
			}
			catch (SecurityException) {
				// CAS check failure
			}
			return filename;
		}
                
                public virtual int GetILOffset()
                {
                        return ilOffset;
                }
                
		public virtual MethodBase GetMethod ()
                {
                        return methodBase;
                }
                
                public virtual int GetNativeOffset()
                {
                        return nativeOffset;                        
                }

		internal string GetInternalMethodName ()
		{
			return internalMethodName;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			if (methodBase == null) {
				sb.Append (Locale.GetText ("<unknown method>"));
			} else {
				sb.Append (methodBase.Name);
			}

			sb.Append (Locale.GetText (" at "));

			if (ilOffset == OFFSET_UNKNOWN) {
				sb.Append (Locale.GetText ("<unknown offset>"));
			} else {
				sb.Append (Locale.GetText ("offset "));
				sb.Append (ilOffset);
			}

			sb.Append (Locale.GetText (" in file:line:column "));
			sb.Append (GetSecureFileName ());
			sb.AppendFormat (":{0}:{1}", lineNumber, columnNumber);
			return sb.ToString ();
		}
	}
}
