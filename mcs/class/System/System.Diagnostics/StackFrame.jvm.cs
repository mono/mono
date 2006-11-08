//
// System.Diagnostics.StackFrame.cs
//
// Author:
//      Alexander Klyubin (klyubin@aqris.com)
//      Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001
//

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Diagnostics {
        /// <summary>
        ///   Stack frame.
        /// </summary>

	[Serializable]
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
#if TARGET_JVM
		static bool get_frame_info (int skip, bool needFileInfo, out MethodBase method,
			out int iloffset, out int native_offset,
			out string file, out int line, out int column)
		{
			native_offset = 0;
			line = 0;
			column = 0;
			file = "";
			iloffset = 0;
			method = null;
			return false;
		}
#else
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool get_frame_info (int skip, bool needFileInfo, out MethodBase method,
						   out int iloffset, out int native_offset,
						   out string file, out int line, out int column);
#endif
		/// <summary>
                ///   Initializes a new StackFrame object corresponding to the
                ///   active stack frame.
                /// </summary>
                public StackFrame() 
				{
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
                public StackFrame(bool needFileInfo) : this() {
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
                public virtual MethodBase GetMethod()
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
                
                /// <summary>
                ///   Builds a readable representation of the stack frame.
                /// </summary>
                /// <returns>
                ///   A readable representation of the stack frame.
                /// </returns>
                public override string ToString() {
                        string methodNameString =
                                (GetMethod() == null)
                                        ? "<unknown method>"
                                          : GetMethod().Name;
                        string offsetString =
                                (GetILOffset() == OFFSET_UNKNOWN)
                                        ? "<unknown offset>"
                                          : "offset " + GetILOffset();
                        string fileNameString =
                                (GetFileName() == null)
                                        ? "<filename unknown>" : GetFileName();
                        return methodNameString + " at " + offsetString
                                + " in file:line:column " + fileNameString
                                + ":" + GetFileLineNumber()
                                + ":" + GetFileColumnNumber();
                }
                
                public override bool Equals(Object obj) {
                        if ((obj == null) || (!(obj is StackFrame))) {
                                return false;
                        }
                        
                        StackFrame rhs = (StackFrame) obj;
                        
                        if (!ObjectsEqual(GetMethod(), rhs.GetMethod())) {
                                return false;
                        }
                        
                        if (!ObjectsEqual(GetFileName(), rhs.GetFileName())) {
                                return false;
                        }
                        
                        if (GetFileLineNumber() != rhs.GetFileLineNumber()) {
                                return false;
                        }
                        
                        if (GetFileColumnNumber() != rhs.GetFileColumnNumber()) {
                                return false;
                        }
                        
                        if (GetILOffset() != rhs.GetILOffset()) {
                                return false;
                        }
                        
                        if (GetNativeOffset() != rhs.GetNativeOffset()) {
                                return false;
                        }
                        
                        return true;
                        
                }
                
                public override int GetHashCode() {
                        return GetFileLineNumber();
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
