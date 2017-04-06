using System;

namespace Mono.Debugger.Soft
{
	/*
	 * Represents a valuetype value in the debuggee
	 */
	public class StructMirror : Value, IInvocableMethodOwnerMirror {
	
		TypeMirror type;
		Value[] fields;

		internal StructMirror (VirtualMachine vm, TypeMirror type, Value[] fields) : base (vm, 0) {
			this.type = type;
			this.fields = fields;
		}

		public TypeMirror Type {
			get {
				return type;
			}
		}

		public Value[] Fields {
			get {
				return fields;
			}
		}

		public Value this [String field] {
			get {
				FieldInfoMirror[] field_info = Type.GetFields ();
				int nf = 0;
				for (int i = 0; i < field_info.Length; ++i) {
					if (!field_info [i].IsStatic) {
						if (field_info [i].Name == field)
							return Fields [nf];
						nf++;
					}
				}
				throw new ArgumentException ("Unknown struct field '" + field + "'.", "field");
			}
			set {
				FieldInfoMirror[] field_info = Type.GetFields ();
				int nf = 0;
				for (int i = 0; i < field_info.Length; ++i) {
					if (!field_info [i].IsStatic) {
						if (field_info [i].Name == field) {
							fields [nf] = value;
							return;
						}
						nf++;
					}
				}
				throw new ArgumentException ("Unknown struct field '" + field + "'.", "field");
			}
		}

		internal void SetFields (Value[] fields) {
			this.fields = fields;
		}

		internal void SetField (int index, Value value) {
			fields [index] = value;
		}

		Value IInvocableMethodOwnerMirror.GetThisObject () {
			return this;
		}

		void IInvocableMethodOwnerMirror.ProcessResult (IInvokeResult result)
		{
			var outThis = result.OutThis as StructMirror;
			if (outThis != null) {
				SetFields (outThis.Fields);
			}
		}
	}
}
