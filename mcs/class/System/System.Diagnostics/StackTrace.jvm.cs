//
// System.Diagnostics.StackTrace.cs
//
// Author:
//      Alexander Klyubin (klyubin@aqris.com)
//      Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001
//

using System;
using System.Reflection;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections;

namespace System.Diagnostics {
        /// <summary>
        ///   Stack trace.
        ///   TODO: more information.
        /// </summary>
        [Serializable]
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
			init_frames (METHODS_TO_SKIP, false);
		}
                
                /// <summary>
                ///   Initializes a new instance of the StackTrace class.
                /// </summary>
                /// <param name="needFileInfo">
                ///   TODO:
                /// </param>
                public StackTrace(bool needFileInfo) {
			init_frames (METHODS_TO_SKIP, needFileInfo);
		}

                /// <summary>
                ///   Initializes a new instance of the StackTrace class
                ///   from the current location, in a caller's frame.
                /// </summary>
                /// <param name="skipFrames">
                ///   The number of frames up the stack to start the trace
                ///   from.
                /// </param>
                public StackTrace(int skipFrames) {
			init_frames (skipFrames, false);
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
                public StackTrace(int skipFrames, bool needFileInfo) {
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
#if TARGET_JVM
		static StackFrame [] get_trace (Exception e, int skipFrames, bool needFileInfo)
		{
			return null;
		}
#else
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static StackFrame [] get_trace (Exception e, int skipFrames, bool needFileInfo);
#endif
		/// <summary>
                ///   Initializes a new instance of the StackTrace class.
                /// </summary>
                /// <param name="e">
                ///   TODO:
                /// </param>
                public StackTrace(Exception e) 
				{
                        frames = get_trace (e, METHODS_TO_SKIP, false);
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
                public StackTrace(Exception e, bool needFileInfo) {
                        frames = get_trace (e, METHODS_TO_SKIP, needFileInfo);
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
                public StackTrace(Exception e, int skipFrames) {
                        frames = get_trace (e, skipFrames, false);
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
                public StackTrace(Exception e, int skipFrames, bool needFileInfo) {
                        frames = get_trace (e, skipFrames, needFileInfo);
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
			MethodBase method = frame.GetMethod();
                        if (method != null) {
                                // Method information available
                                return  method.DeclaringType.FullName
                                        + "." + method.Name + "()";
                        } else {
                                // Method information not available
                                return "<unknown method>";
                        }
                }
        }
}
