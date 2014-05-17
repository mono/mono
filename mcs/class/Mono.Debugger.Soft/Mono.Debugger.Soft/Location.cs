using System;

namespace Mono.Debugger.Soft
{
	public class Location : Mirror
	{
		MethodMirror method;
		//long native_addr;
		int il_offset;
		string source_file;
		int line_number;
		byte[] hash;
		int column_number;
		int end_line_number;
		int end_column_number;
		
		internal Location (VirtualMachine vm, MethodMirror method, long native_addr, int il_offset, string source_file, int line_number, int column_number, int end_line_number, int end_column_number, byte[] hash) : base (vm, 0) {
			this.method = method;
			//this.native_addr = native_addr;
			this.il_offset = il_offset;
			this.source_file = source_file;
			this.line_number = line_number;
			this.hash = hash;
			this.column_number = column_number;
			this.end_line_number = end_line_number;
			this.end_column_number = end_column_number;
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

		// Since protocol version 2.19, 0 in earlier protocol versions
		public int ColumnNumber {
			get {
				return column_number;
			}
	    }

		// Since protocol version 2.32, -1 in earlier protocol versions, or if not available
		public int EndLineNumber {
			get {
				return end_line_number;
			}
	    }

		// Since protocol version 2.32, -1 in earlier protocol versions, or if not available
		public int EndColumnNumber {
			get {
				return end_column_number;
			}
	    }

		// MD5 hash of source file
		// Since protocol version 2.14, null in earlier protocol versions
		public byte[] SourceFileHash {
			get {
				return hash;
			}
		}

		public override string ToString () {
			if (EndLineNumber != -1 && EndColumnNumber != -1)
				return String.Format ("{0}+0x{1:x} at {2}:[{3}:{4}-{5}:{6}]", Method.FullName, ILOffset, SourceFile, LineNumber, ColumnNumber, EndLineNumber, EndColumnNumber);
			else
				return String.Format ("{0}+0x{1:x} at {2}:{3}", Method.FullName, ILOffset, SourceFile, LineNumber);
		}
    }
}
