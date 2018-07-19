using System;

namespace Mono.Debugger.Soft
{
	public class LocalScope : Mirror {

		MethodMirror method;
		int live_range_start, live_range_end;

	    internal LocalScope (VirtualMachine vm, MethodMirror method, int live_range_start, int live_range_end) : base (vm, 0) {
			this.method = method;
			this.live_range_start = live_range_start;
			this.live_range_end = live_range_end;
		}

		public MethodMirror Method {
			get {
				return method;
			}
		}

		public int LiveRangeStart {
			get {
				return live_range_start;
			}
		}

		public int LiveRangeEnd {
			get {
				return live_range_end;
			}
		}
	}
}

