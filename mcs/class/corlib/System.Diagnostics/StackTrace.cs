//
// System.Diagnostics.StackTrace.cs
//
// Author:
//      Alexander Klyubin (klyubin@aqris.com)
//
// (C) 2001
//

using System;
using System.Reflection;
using System.Threading;

namespace System.Diagnostics {
        /// <summary>
        ///   Stack trace.
        ///   TODO: more information.
        /// </summary>
        public class StackTrace {
                /// <value>
                ///   Uses a constant to define the number of methods that are
                ///   to be omitted from the stack trace.
                /// </value>
                public const int METHODS_TO_SKIP = 0;
                
                /// <value>
                ///   Frames. First frame is the last stack frame pushed.
                /// </value>
                private StackFrame[] frames;


                /// <summary>
                ///   Initializes a new instance of the StackTrace class.
                /// </summary>
		[MonoTODO]
                public StackTrace() {
                        throw new NotImplementedException();
                }
                
                /// <summary>
                ///   Initializes a new instance of the StackTrace class.
                /// </summary>
                /// <param name="needFileInfo">
                ///   TODO:
                /// </param>
                public StackTrace(bool needFileInfo) : this() {}
                
                /// <summary>
                ///   Initializes a new instance of the StackTrace class.
                /// </summary>
                /// <param name="e">
                ///   TODO:
                /// </param>
		[MonoTODO]
                public StackTrace(Exception e) {
                        throw new NotImplementedException();
                }
                
                
                /// <summary>
                ///   Initializes a new instance of the StackTrace class,
                ///   using the provided exception object. The resulting stack
                ///   trace describes the stack at the time of the exception.
                /// </summary>
                /// <param name="e">
                ///   TODO:
                /// </param>
                /// <param name="needFileInfo">
                ///   TODO:
                /// </param>
                public StackTrace(Exception e, bool needFileInfo) : this(e) {}
                
                /// <summary>
                ///   Initializes a new instance of the StackTrace class,
                ///   using the provided exception object. The resulting stack
                ///   trace describes the stack at the time of the exception.
                /// </summary>
                /// <param name="e">
                ///   Exception.
                /// </param>
                /// <param name="skipFrames">
                ///   The number of frames up the stack to start the trace
                ///   from.
                /// </param>
		[MonoTODO]
                public StackTrace(Exception e, int skipFrames) {
                        throw new NotImplementedException();
                }
                
                /// <summary>
                ///   Initializes a new instance of the StackTrace class,
                ///   using the provided exception object. The resulting stack
                ///   trace describes the stack at the time of the exception.
                /// </summary>
                /// <param name="e">
                ///   Exception.
                /// </param>
                /// <param name="skipFrames">
                ///   The number of frames up the stack to start the trace
                ///   from.
                /// </param>
                /// <param name="needFileInfo">
                ///   TODO:
                /// </param>
                public StackTrace(Exception e, int skipFrames, bool needFileInfo)
                : this(e, skipFrames) {}
                
                /// <summary>
                ///   Initializes a new instance of the StackTrace class
                ///   from the current location, in a caller's frame.
                /// </summary>
                /// <param name="skipFrames">
                ///   The number of frames up the stack to start the trace
                ///   from.
                /// </param>
		[MonoTODO]
                public StackTrace(int skipFrames) {
                        throw new NotImplementedException();
                }
                
                /// <summary>
                ///   Initializes a new instance of the StackTrace class
                ///   containing a single frame.
                /// </summary>
                /// <param name="frame">
                ///   The frame that the StackTrace object should contain.
                /// </param>
                public StackTrace(StackFrame frame) {
                        this.frames = new StackFrame[1];
                        this.frames[0] = frame;
                }
                
                
                /// <summary>
                ///   Initializes a new instance of the StackTrace class
                ///   from the current location, in a caller's frame.
                /// </summary>
                /// <param name="skipFrames">
                ///   The number of frames up the stack to start the trace
                ///   from.
                /// </param>
                /// <param name="needFileInfo">
                ///   TODO:
                /// </param>
                public StackTrace(int skipFrames, bool needFileInfo)
                : this(skipFrames) {}
                
                /// <summary>
                ///   Initializes a new instance of the StackTrace class.
                /// </summary>
                /// <param name="targetThread">
                ///   TODO:
                /// </param>
                /// <param name="needFileInfo">
                ///   TODO:
                /// </param>
		[MonoTODO]
                public StackTrace(Thread targetThread, bool needFileInfo) {
                        throw new NotImplementedException();
                }
                
                
                                  
                /// <summary>
                ///   Holds the number of frames in the stack trace.
                /// </summary>
                public virtual int FrameCount {
                        get {
                                return (frames == null) ? 0 : frames.Length;
                        }
                }             
                              
                /// <summary>
                ///   Gets the specified stack frame.
                /// </summary>
                /// <param name="index">
                ///   The index of the stack frame requested.
                /// </param>
                /// <returns>
                ///   The specified stack frame. Returns <code>null</code> if
                ///   frame with specified index does not exist in this stack
                ///   trace.
                /// </returns>
                /// <remarks>
                ///   Stack frames are numbered starting at zero, which is the
                ///   last stack frame pushed.
                /// </remarks>
                public virtual StackFrame GetFrame(int index) {
                        if ((index < 0) || (index >= FrameCount)) {
                                return null;
                        }
                        
                        return frames[index];
                }              
                
                /// <summary>
                ///   Builds a readable representation of the stack trace.
                /// </summary>
                /// <returns>
                ///   A readable representation of the stack trace.
                /// </returns>
                public override string ToString() {
                        string result = "";
                        for (int i = 0; i < FrameCount; i++) {
                                StackFrame frame = GetFrame(i);
                                result += "\n\tat " + FrameToString(frame);
                        }
                        
                        return result;
                }
                
                public override bool Equals(Object obj) {
                        if ((obj == null) || (!(obj is StackTrace))) {
                                return false;
                        }
                        
                        StackTrace rhs = (StackTrace) obj;
                        
                        if (FrameCount != rhs.FrameCount) {
                                return false;
                        }
                        
                        for (int i = 0; i < FrameCount; i++) {
                                if (!GetFrame(i).Equals(rhs.GetFrame(i))) {
                                        return false;
                                }
                        }
                        
                        return true;
                }
                
                public override int GetHashCode() {
                        return FrameCount;
                }
                
                /// <summary>
                ///   Converts single stack frame to string to be used in
                ///   ToString method.
                /// </summary>
                /// <param name="frame">
                ///   Frame to convert.
                /// </param>
                /// <returns>
                ///   A readable representation of stack frame for using
                ///   ToString.
                /// </returns>
                private static String FrameToString(StackFrame frame) {
                        string locationInfo;
                        
                        if (frame.GetFileName() == null) {
                                // File name not available
                                locationInfo = "";
                        } else {
                                // File name available
                                locationInfo = frame.GetFileName();
                                if (frame.GetFileLineNumber() != 0) {
                                        // Line number information available
                                        locationInfo += ":line "
                                                + frame.GetFileLineNumber();
                                        if (frame.GetFileColumnNumber() != 0) {
                                                // Column number information available
                                                locationInfo += ":column"
                                                        + frame.GetFileColumnNumber();
                                        }
                                }
                                
                        }
                        
                        MethodBase method = frame.GetMethod();
                        if (method != null) {
                                // Method information available
                                return  method.DeclaringType.Name
                                        + "." + method.Name + "()"
                                        + ((locationInfo != null)
                                                ? " at " + locationInfo
                                                  : "");
                        } else {
                                // Method information not available
                                string methodInfo = "<unknown method>";
                                if ("".Equals(locationInfo)) {
                                        // No location information available
                                        return methodInfo;
                                }
                                
                                // Location information available
                                return methodInfo + " at " + locationInfo;
                        }
                }
        }
}
