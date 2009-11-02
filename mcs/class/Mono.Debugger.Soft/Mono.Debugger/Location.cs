using System;

namespace Mono.Debugger
{
	public class Location : Mirror
	{
		MethodMirror method;
		//long native_addr;
		int il_offset;
		string source_file;
		int line_number;
		//int column_number;
		
		internal Location (VirtualMachine vm, MethodMirror method, long native_addr, int il_offset, string source_file, int line_number, int column_number) : base (vm, 0) {
			this.method = method;
			//this.native_addr = native_addr;
			this.il_offset = il_offset;
			this.source_file = source_file;
			this.line_number = line_number;
			//this.column_number = column_number;
		}

		public MethodMirror Method {
			get {
				return method;
			}
		}

		public int ILOffset {
			get {
				return il_offset;
			}
		}

		public string SourceFile {
			get {
				return source_file;
			}
	    }

		public int LineNumber {
			get {
				return line_number;
			}
	    }
    }
}
