using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Debugger.Soft
{
	public class StackFrame : Mirror
	{
		ThreadMirror thread;
		AppDomainMirror domain;
		MethodMirror method;
		int il_offset;
		Location location;
		StackFrameFlags flags;

		/*
		 * FIXME: Decide on the way to request/handle debugging information:
		 * - request the info in bulk for all frames/on demand for individual frames
		 * - request the info from the runtime/request only the il offset, and compute
		 *   everything else based on this info using the method debug info.
		 */

		internal StackFrame (VirtualMachine vm, long id, ThreadMirror thread, MethodMirror method, int il_offset, StackFrameFlags flags) : base (vm, id) {
			this.thread = thread;
			this.method = method;
			this.il_offset = il_offset;
			this.flags = flags;
		}

		public ThreadMirror Thread {
			get {
				return thread;
			}
		}

		public AppDomainMirror Domain {
			get {
				if (domain == null) {
					if (vm.Version.AtLeast (2, 38)) {
						try {
							domain = vm.GetDomain (vm.conn.StackFrame_GetDomain (thread.Id, Id));
						} catch (InvalidStackFrameException) {
							domain = Thread.Domain;
						} catch (AbsentInformationException) {
							domain = Thread.Domain;
						}
					} else {
						domain = Thread.Domain;
					}
				}

				return domain;
			}
		}

		public MethodMirror Method {
			get {
				return method;
			}
		}

		public Location Location {
			get {
				if (location == null) {
					int line_number;
					string src_file = null;
					byte[] hash = null;
					int column_number = 0;
					int end_line_number = -1;
					int end_column_number = -1;

					if (il_offset == -1)
						line_number = -1;
					else
						line_number = method.il_offset_to_line_number (il_offset, out src_file, out hash, out column_number, out end_line_number, out end_column_number);

					location = new Location (vm, Method, 0, il_offset, src_file != null ? src_file : method.SourceFile, line_number, column_number, end_line_number, end_column_number, hash);
				}
				return location;
			}
		}

		public string FileName {
			get {
				return Location.SourceFile;
			}
		}

		public int ILOffset {
			get {
				return Location.ILOffset;
			}
		}

		public int LineNumber {
			get {
				return Location.LineNumber;
			}
		}

		public int ColumnNumber {
			get {
				return Location.ColumnNumber;
			}
		}

		public int EndLineNumber {
			get {
				return Location.EndLineNumber;
			}
		}

		public int EndColumnNumber {
			get {
				return Location.EndColumnNumber;
			}
		}

		public bool IsDebuggerInvoke {
			get {
				return (flags & StackFrameFlags.DEBUGGER_INVOKE) != 0;
			}
		}

		/*
		 * Whenever this frame transitions to native code. The method associated
		 * with the frame is either an InternalCall or a pinvoke method.
		 */
		public bool IsNativeTransition {
			get {
				return (flags & StackFrameFlags.NATIVE_TRANSITION) != 0;
			}
		}

		public Value GetValue (ParameterInfoMirror param) {
			if (param == null)
				throw new ArgumentNullException ("param");
			if (param.Method != Method)
				throw new ArgumentException ("Parameter doesn't belong to this frame's method.");
			if (param.IsRetval)
				throw new ArgumentException ("Parameter represents the method return value.");

			// FIXME: Liveness
			// FIXME: Allow returning the frame return value if possible
			return vm.DecodeValue (vm.conn.StackFrame_GetValues (thread.Id, Id, new int [] { (- param.Position) - 1 })[0]);
		}

		public Value GetValue (LocalVariable var) {
			if (var == null)
				throw new ArgumentNullException ("var");
			if (var.Method != Method)
				throw new ArgumentException ("Local variable doesn't belong to this frame's method.");

			// FIXME: Liveness
			// FIXME: Check for return value
			// FIXME: Allow returning the frame return value if possible
			return vm.DecodeValue (vm.conn.StackFrame_GetValues (thread.Id, Id, new int [] { var.GetValueIndex } )[0]);
		}

		public Value[] GetValues (LocalVariable[] vars) {
			if (vars == null)
				throw new ArgumentNullException ("vars");
			for (int i = 0; i < vars.Length; ++i) {
				if (vars [i] == null)
					throw new ArgumentNullException ("vars");
				if (vars [i].Method != Method)
					throw new ArgumentException ("Local variable doesn't belong to this frame's method.");
			}
			int[] pos = new int [vars.Length];
			for (int i = 0; i < vars.Length; ++i)
				pos [i] = vars [i].GetValueIndex;
			return vm.DecodeValues (vm.conn.StackFrame_GetValues (thread.Id, Id, pos));
		}

		public Value GetArgument (int pos) {
			return GetValue (Method.GetParameters () [pos]);
		}

		public Value GetThis () {
			return vm.DecodeValue (vm.conn.StackFrame_GetThis (thread.Id, Id));
		}

		// Since protocol version 2.44
		public void SetThis (Value value) {
			if (value == null)
				throw new ArgumentNullException ("value");
			if (Method.IsStatic || !Method.DeclaringType.IsValueType)
				throw new InvalidOperationException ("The frame's method needs to be a valuetype instance method.");
			vm.conn.StackFrame_SetThis (thread.Id, Id, vm.EncodeValue (value));
		}

		public void SetValue (LocalVariable var, Value value) {
			if (var == null)
				throw new ArgumentNullException ("var");
			if (var.Method != Method)
				throw new ArgumentException ("Local variable doesn't belong to this frame's method.");
			if (value == null)
				throw new ArgumentNullException ("value");
			CheckMirror (value);
			// FIXME: Liveness
			// FIXME: Check for return value
			try {
				vm.conn.StackFrame_SetValues (thread.Id, Id, new int [] { var.GetValueIndex }, new ValueImpl [] { vm.EncodeValue (value) });
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_ARGUMENT)
					throw new ArgumentException ("Value does not match the type of the local variable.");
				else
					throw;
			}
		}

		public void SetValue (ParameterInfoMirror param, Value value) {
			if (param == null)
				throw new ArgumentNullException ("param");
			if (param.Method != Method)
				throw new ArgumentException ("Parameter doesn't belong to this frame's method.");
			if (param.IsRetval)
				throw new ArgumentException ("Parameter represents the method return value.");
			if (value == null)
				throw new ArgumentNullException ("value");
			CheckMirror (value);

			// FIXME: Liveness
			// FIXME: Allow setting the frame return value if possible
			try {
				vm.conn.StackFrame_SetValues (thread.Id, Id, new int [] { (- param.Position) - 1 }, new ValueImpl [] { vm.EncodeValue (value) });
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_ARGUMENT)
					throw new ArgumentException ("Value does not match the type of the variable.");
				else
					throw;
			}
		}

		public IList<LocalVariable> GetVisibleVariables () {
			if (Location.ILOffset == -1)
				throw new AbsentInformationException ();

			return Method.GetLocals ().Where (l => l.LiveRangeStart <= location.ILOffset && l.LiveRangeEnd >= location.ILOffset).ToList ();
		}

		public LocalVariable GetVisibleVariableByName (string name) {
			if (name == null)
				throw new ArgumentNullException ("name");

			if (Location.ILOffset == -1)
				throw new AbsentInformationException ();

			return Method.GetLocals ().Where (l => l.LiveRangeStart <= location.ILOffset && l.LiveRangeEnd >= location.ILOffset && l.Name == name).FirstOrDefault ();
		}
    }
}
