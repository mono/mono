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

namespace System.Diagnostics {
        /// <summary>
        ///   Stack frame.
        /// </summary>

	[Serializable]
	[MonoTODO ("Fix serialization compatibility with MS.NET")]
        public class StackFrame {
                /// <value>
                ///   Constant returned when the native or IL offset is unknown.
                /// </value>
                public const int OFFSET_UNKNOWN = -1;
                
                /// <value>
                ///   Offset from the start of the IL code for the method
                ///   being executed.
                /// </value>
                private int ilOffset = OFFSET_UNKNOWN;
                
                /// <value>
                ///   Offset from the start of the native code for the method
                ///   being executed.
                /// </value>
                private int nativeOffset = OFFSET_UNKNOWN;

                /// <value>
                ///   Method associated with this stack frame.
                /// </value>
                private MethodBase methodBase;
                
                /// <value>
                ///   File name.
                /// </value>
                private string fileName;
                
                /// <value>
                ///   Line number.
                /// </value>
                private int lineNumber;
                
                /// <value>
                ///   Column number.
                /// </value>
                private int columnNumber;

			// Description of internal runtime method if methodBase==null
			private string internalMethodName;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool get_frame_info (int skip, bool needFileInfo, out MethodBase method,
						   out int iloffset, out int native_offset,
						   out string file, out int line, out int column);

                /// <summary>
                ///   Initializes a new StackFrame object corresponding to the
                ///   active stack frame.
                /// </summary>
                public StackFrame() {
			get_frame_info (2, false, out methodBase, out ilOffset,
					out nativeOffset, out fileName, out lineNumber,
					out columnNumber);			
                }
                
                /// <summary>
                ///   Initializes a new StackFrame object corresponding to the
                ///   active stack frame.
                /// </summary>
                /// <param name="needFileInfo">
                ///   TODO:
                /// </param>
		public StackFrame (bool needFileInfo)
		{
			get_frame_info (2, needFileInfo, out methodBase, out ilOffset,
					out nativeOffset, out fileName, out lineNumber,
					out columnNumber);			
                }
                
                /// <summary>
                ///   Initializes a new StackFrame object corresponding to the
                ///   active stack frame.
                /// </summary>
                /// <param name="skipFrames">
                ///   The number of frames up the stack to skip.
                /// </param>
                public StackFrame(int skipFrames) {
			get_frame_info (skipFrames + 2, false, out methodBase, out ilOffset,
					out nativeOffset, out fileName, out lineNumber,
					out columnNumber);			
                }
                
                /// <summary>
                ///   Initializes a new StackFrame object corresponding to the
                ///   active stack frame.
                /// </summary>
                /// <param name="skipFrames">
                ///   The number of frames up the stack to skip.
                /// </param>
                /// <param name="needFileInfo">
                ///   TODO:
                /// </param>
                public StackFrame(int skipFrames, bool needFileInfo) {
			get_frame_info (skipFrames + 2, needFileInfo, out methodBase, out ilOffset,
					out nativeOffset, out fileName, out lineNumber,
					out columnNumber);
                }
                
                /// <summary>
                ///   Constructs a fake stack frame that just contains the
                ///   given file name and line number. Use this constructor
                ///   when you do not want to use the debugger's line mapping
                ///   logic.
                /// </summary>
                /// <param name="fileName">
                ///   The given file name.
                /// </param>
                /// <param name="lineNumber">
                ///   The line number in the specified file.
                /// </param>
				// LAMESPEC: According to the MSDN docs, this creates a
				// fake stack frame. But MS fills out the frame info as well
                public StackFrame(string fileName, int lineNumber) {
					get_frame_info (2, false, out methodBase, out ilOffset,
									out nativeOffset, out fileName, out lineNumber,
									out columnNumber);
					this.fileName = fileName;
					this.lineNumber = lineNumber;
					this.columnNumber = 0;
				}
                
                /// <summary>
                ///   Constructs a fake stack frame that just contains the
                ///   given file name and line number. Use this constructor
                ///   when you do not want to use the debugger's line mapping
                ///   logic.
                /// </summary>
                /// <param name="fileName">
                ///   The given file name.
                /// </param>
                /// <param name="lineNumber">
                ///   The line number in the specified file.
                /// </param>
                /// <param name="colNumber">
                ///   The column number in the specified file.
                /// </param>
				// LAMESPEC: According to the MSDN docs, this creates a
				// fake stack frame. But MS fills out the frame info as well
                public StackFrame(string fileName,
                                  int lineNumber,
                                  int colNumber) {
					get_frame_info (2, false, out methodBase, out ilOffset,
									out nativeOffset, out fileName, out lineNumber,
									out columnNumber);
					this.fileName = fileName;
					this.lineNumber = lineNumber;
					this.columnNumber = colNumber;
                }
                                  
                              
                /// <summary>
                ///   Gets the line number in the file containing the code
                ///   being executed. This information is typically extracted
                ///   from the debugging symbols for the executable.
                /// </summary>
                /// <returns>
                ///   The file line number or zero if it cannot be determined.
                /// </returns>
                public virtual int GetFileLineNumber()
                {
                        return lineNumber;
                }
                
                /// <summary>
                ///   Gets the column number in the file containing the code
                ///   being executed. This information is typically extracted
                ///   from the debugging symbols for the executable.
                /// </summary>
                /// <returns>
                ///   The file column number or zero if it cannot be determined.
                /// </returns>
                public virtual int GetFileColumnNumber()
                {
                        return columnNumber;
                }
                
                /// <summary>
                ///   Gets the file name containing the code being executed.
                ///   This information is typically extracted from the
                ///   debugging symbols for the executable.
                /// </summary>
                /// <returns>
                ///   The file name or null if it cannot be determined.
                /// </returns> 
                public virtual string GetFileName()
                {
#if NET_2_0
			if (SecurityManager.SecurityEnabled && (fileName != null) && (fileName.Length > 0)) {
				string fn = Path.GetFullPath (fileName);
				new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fn).Demand ();
			}
#endif
                        return fileName;
                }
                
                /// <summary>
                ///   Gets the offset from the start of the IL code for the
                ///   method being executed. This offset may be approximate
                ///   depending on whether the JIT compiler is generating
                ///   debugging code or not.
                /// </summary>
                /// <returns>
                ///   The offset from the start of the IL code for the method
                ///   being executed.
                /// </returns>
                public virtual int GetILOffset()
                {
                        return ilOffset;
                }
                
                /// <summary>
                ///   Gets the method in which the frame is executing.
                /// </summary>
                /// <returns>
                ///   The method the frame is executing in.
                /// </returns>
#if ONLY_1_1
		[ReflectionPermission (SecurityAction.Demand, TypeInformation = true)]
#endif
		public virtual MethodBase GetMethod ()
                {
                        return methodBase;
                }
                
                /// <summary>
                ///   Gets the offset from the start of the native
                ///   (JIT-compiled) code for the method being executed.
                /// </summary>
                /// <returns>
                ///   The offset from the start of the native (JIT-compiled)
                ///   code or the method being executed.
                /// </returns>
                public virtual int GetNativeOffset()
                {
                        return nativeOffset;                        
                }

		internal string GetInternalMethodName ()
		{
			return internalMethodName;
		}

                /// <summary>
                ///   Builds a readable representation of the stack frame.
                /// </summary>
                /// <returns>
                ///   A readable representation of the stack frame.
                /// </returns>
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

			if (fileName == null) {
				sb.Append (Locale.GetText ("<filename unknown>"));
			} else {
				try {
					// need security check
					sb.Append (GetFileName ());
				}
				catch (SecurityException) {
					sb.Append (Locale.GetText ("<filename unknown>"));
				}
			}

			sb.AppendFormat (":{0}:{1}", lineNumber, columnNumber);
			return sb.ToString ();
                }
                
                /// <summary>
                ///   Checks whether two objects are equal.
                ///   The objects are assumed equal if and only if either
                ///   both of the references are <code>null</code> or they
                ///   equal via <code>Equals</code> method.
                /// </summary>
                /// <param name="obj1">
                ///   First object.
                /// </param>
                /// <param name="obj2">
                ///   Second object.
                /// </param>
                /// <returns>
                ///   <code>true</code> if the two objects are equal,
                ///   </code>false</code> otherwise.
                /// </returns>
                private static bool ObjectsEqual(Object obj1, Object obj2) {
                        if (obj1 == null) {
                                return (obj2 == null);
                        } else {
                                return obj1.Equals(obj2);
                        }
                }
         }
}
