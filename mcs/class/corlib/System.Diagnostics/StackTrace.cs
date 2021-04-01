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

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.IO;

namespace System.Diagnostics {

	[Serializable]
	[ComVisible (true)]
#if !NETCORE
	[MonoTODO ("Serialized objects are not compatible with .NET")]
#endif
	public class StackTrace {

        // TraceFormat is Used to specify options for how the 
        // string-representation of a StackTrace should be generated.
        internal enum TraceFormat 
        {
            Normal,
            TrailingNewLine,        // include a trailing new line character
            NoResourceLookup    // to prevent infinite resource recusion
        }

		public const int METHODS_TO_SKIP = 0;
		const string prefix = "  at ";

		private StackFrame[] frames;
		readonly StackTrace[] captured_traces;
#pragma warning disable 414		
		private bool debug_info;
#pragma warning restore

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public StackTrace ()
		{
			init_frames (METHODS_TO_SKIP, false);
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public StackTrace (bool fNeedFileInfo)
		{
			init_frames (METHODS_TO_SKIP, fNeedFileInfo);
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public StackTrace (int skipFrames)
		{
			init_frames (skipFrames, false);
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public StackTrace (int skipFrames, bool fNeedFileInfo)
		{
			init_frames (skipFrames, fNeedFileInfo);
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
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
		{
			if (e == null)
				throw new ArgumentNullException ("e");
			if (skipFrames < 0)
				throw new ArgumentOutOfRangeException ("< 0", "skipFrames");

			frames = get_trace (e, skipFrames, fNeedFileInfo);

			captured_traces = e.captured_traces;
		}

		public StackTrace (StackFrame frame)
		{
			this.frames = new StackFrame [1];
			this.frames [0] = frame;
		}

#if !NETCORE
		[MonoLimitation ("Not possible to create StackTraces from other threads")]
#endif
		[Obsolete]
		public StackTrace (Thread targetThread, bool needFileInfo)
		{
			if (targetThread == Thread.CurrentThread){
				init_frames (METHODS_TO_SKIP, needFileInfo);
				return;
			}
			
			throw new NotImplementedException ();
		}

		internal StackTrace (StackFrame[] frames) {
			this.frames = frames;
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
			if (captured_traces == null)
				return frames;

			var accum = new List<StackFrame> ();
			foreach (var t in captured_traces) {
				for (int i = 0; i < t.FrameCount; i++)
					accum.Add (t.GetFrame (i));
			}

			accum.AddRange (frames);
			return accum.ToArray ();
		}

		static bool isAotidSet;
		static string aotid;
		static string GetAotId ()
		{
			if (!isAotidSet) {
				var arr = RuntimeAssembly.GetAotId ();
				if (arr != null)
					aotid = new Guid (arr).ToString ("N");
				isAotidSet = true;
			}

			return aotid;
		}

		bool AddFrames (StringBuilder sb, bool separator, out bool isAsync)
		{
			isAsync = false;
			bool any_frame = false;

			for (int i = 0; i < FrameCount; i++) {
				StackFrame frame = GetFrame (i);

				if (frame.GetMethod () == null) {
					if (any_frame || separator)
						sb.Append (Environment.NewLine);
					sb.Append (prefix);

					string internal_name = frame.GetInternalMethodName ();
					if (internal_name != null)
						sb.Append (internal_name);
					else
						sb.AppendFormat ("<0x{0:x5} + 0x{1:x5}> <unknown method>", frame.GetMethodAddress (), frame.GetNativeOffset ());
				} else {
					GetFullNameForStackTrace (sb, frame.GetMethod (), any_frame || separator, out var skipped, out isAsync);
					if (skipped)
						continue;

					if (frame.GetILOffset () == -1) {
						sb.AppendFormat (" <0x{0:x5} + 0x{1:x5}>", frame.GetMethodAddress (), frame.GetNativeOffset ());
						if (frame.GetMethodIndex () != 0xffffff)
							sb.AppendFormat (" {0}", frame.GetMethodIndex ());
					} else {
						sb.AppendFormat (" [0x{0:x5}]", frame.GetILOffset ());
					}

					var filename = frame.GetSecureFileName ();
#if !WASM
					if (filename[0] == '<') {
						var mvid = frame.GetMethod ().Module.ModuleVersionId.ToString ("N");
						var aotid = GetAotId ();
						if (frame.GetILOffset () != -1 || aotid == null) {
							filename = string.Format ("<{0}>", mvid);
						} else {
							filename = string.Format ("<{0}#{1}>", mvid, aotid);
						}
					}
#endif

					sb.AppendFormat (" in {0}:{1} ", filename, frame.GetFileLineNumber ());
				}

				any_frame = true;
			}

			return any_frame;
		}

		void GetFullNameForStackTrace (StringBuilder sb, MethodBase mi, bool needsNewLine, out bool skipped, out bool isAsync)
		{
			Type declaringType = mi.DeclaringType;

			// Get generic definition
			if (declaringType.IsGenericType && !declaringType.IsGenericTypeDefinition) {
				declaringType = declaringType.GetGenericTypeDefinition ();

				const BindingFlags bindingflags = BindingFlags.Instance | BindingFlags.Static |
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
				foreach (var m in declaringType.GetMethods (bindingflags)) {
					if (m.MetadataToken == mi.MetadataToken) {
						mi = m;
						break;
					}
				}
			}

			isAsync = typeof (IAsyncStateMachine).IsAssignableFrom (declaringType);
			skipped = mi.IsDefined (typeof (StackTraceHiddenAttribute)) || declaringType.IsDefined (typeof (StackTraceHiddenAttribute));
			if (skipped)
				return;

			if (isAsync) {
				ConvertAsyncStateMachineMethod (ref mi, ref declaringType);
			}

			if (needsNewLine)
				sb.Append (Environment.NewLine);
			sb.Append (prefix);

			sb.Append (declaringType.ToString ());

			sb.Append (".");
			sb.Append (mi.Name);

			if (mi.IsGenericMethod) {
				mi = ((MethodInfo)mi).GetGenericMethodDefinition ();
				Type[] gen_params = mi.GetGenericArguments ();
				sb.Append ("[");
				for (int j = 0; j < gen_params.Length; j++) {
					if (j > 0)
						sb.Append (",");
					sb.Append (gen_params [j].Name);
				}
				sb.Append ("]");
			}

			ParameterInfo[] p = mi.GetParameters ();

			sb.Append (" (");
			for (int i = 0; i < p.Length; ++i) {
				if (i > 0)
					sb.Append (", ");

				Type pt = p[i].ParameterType;
				if (pt.IsGenericType && ! pt.IsGenericTypeDefinition)
					pt = pt.GetGenericTypeDefinition ();

				sb.Append (pt.ToString());

				if (p [i].Name != null) {
					sb.Append (" ");
					sb.Append (p [i].Name);
				}
			}
			sb.Append (")");
		}
        
		static void ConvertAsyncStateMachineMethod (ref MethodBase method, ref Type declaringType)
		{
			Type parentType = declaringType.DeclaringType;
			if (parentType == null)
				return;

			MethodInfo[] methods = parentType.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			if (methods == null)
				return;

			foreach (MethodInfo candidateMethod in methods) {
				var attributes = candidateMethod.GetCustomAttributes<AsyncStateMachineAttribute> ();
				if (attributes == null)
					continue;

				foreach (var attr in attributes) {
					if (attr.StateMachineType == declaringType) {
						method = candidateMethod;
						declaringType = candidateMethod.DeclaringType;
						return;
					}
				}
			}
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			//
			// Add traces captured using ExceptionDispatchInfo
			//
			bool has_frames = false;
			if (captured_traces != null) {
				foreach (var t in captured_traces) {
					has_frames = t.AddFrames (sb, has_frames, out var isAsync);
					if (!has_frames)
						continue;

					if (!isAsync) {
						sb.Append (Environment.NewLine);
						sb.Append ("--- End of stack trace from previous location where exception was thrown ---");
						sb.Append (Environment.NewLine);
					}
				}
			}

			AddFrames (sb, has_frames, out _);

			return sb.ToString ();
		}

		internal String ToString (TraceFormat traceFormat)
		{
			// TODO:
			return ToString ();
		}
	}
}
