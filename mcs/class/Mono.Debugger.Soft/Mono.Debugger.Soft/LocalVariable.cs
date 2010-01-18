using System;

namespace Mono.Debugger.Soft
{
	public class LocalVariable : Mirror {

		MethodMirror method;
		string name;
		int index;
		long type_id;
		TypeMirror t;
		bool is_arg;
		int live_range_start, live_range_end;

	    internal LocalVariable (VirtualMachine vm, MethodMirror method, int index, long type_id, string name, int live_range_start, int live_range_end, bool is_arg) : base (vm, 0) {
			this.method = method;
			this.index = index;
			this.name = name;
			this.type_id = type_id;
			this.is_arg = is_arg;
			this.live_range_start = live_range_start;
			this.live_range_end = live_range_end;
		}

		public string Name {
			get {
				return name;
			}
		}

		public int Index {
			get {
				return index;
			}
		}

		public TypeMirror Type {
			get {
				if (t == null)
					t = vm.GetType (type_id);
				return t;
			}
		}

		public bool IsArg {
			get {
				return is_arg;
			}
		}

		public MethodMirror Method {
			get {
				return method;
			}
		}

		internal int LiveRangeStart {
			get {
				return live_range_start;
			}
		}

		internal int LiveRangeEnd {
			get {
				return live_range_end;
			}
		}

		internal int GetValueIndex {
			get {
				if (IsArg)
					return (-Index) - 1;
				else
					return Index;
			}
		}
	}
}

