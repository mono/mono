
//
// System.Reflection.Emit/ILGenerator.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Diagnostics.SymbolStore;

namespace System.Reflection.Emit {
	public class ILGenerator: Object {

		public virtual void BeginCatchBlock (Type exceptionType) {}
		public virtual void BeginExceptFilterBlock () {}
		public virtual void BeginExceptionBlock () {}
		public virtual void BeginFaultBlock() {}
		public virtual void BeginFinallyBlock() {}
		public virtual void BeginScope () {}
		public virtual void DeclareLocal (Type localType) {}
		public virtual Label DefineLabel () {return new Label ();}
		public virtual void Emit (OpCode opcode) {}
		public virtual void Emit (OpCode opcode, Byte val) {}
		public virtual void Emit (OpCode opcode, ConstructorInfo contructor) {}
		public virtual void Emit (OpCode opcode, Double val) {}
		public virtual void Emit (OpCode opcode, FieldInfo field) {}
		public virtual void Emit (OpCode opcode, Int16 val) {}
		public virtual void Emit (OpCode opcode, Int32 val) {}
		public virtual void Emit (OpCode opcode, Int64 val) {}
		public virtual void Emit (OpCode opcode, Label label) {}
		public virtual void Emit (OpCode opcode, Label[] labels) {}
		public virtual void Emit (OpCode opcode, LocalBuilder lbuilder) {}
		public virtual void Emit (OpCode opcode, MethodInfo method) {}
		public virtual void Emit (OpCode opcode, sbyte val) {}
		public virtual void Emit (OpCode opcode, SignatureHelper shelper) {}
		public virtual void Emit (OpCode opcode, float val) {}
		public virtual void Emit (OpCode opcode, string val) {}
		public virtual void Emit (OpCode opcode, Type type) {}

		public void EmitCall (OpCode opcode, MethodInfo methodinfo, Type[] optionalParamTypes) {}
		public void EmitCalli (OpCode opcode, CallingConventions call_conv, Type returnType, Type[] paramTypes, Type[] optionalParamTypes) {}

		public virtual void EmitWriteLine (FieldInfo field) {}
		public virtual void EmitWriteLine (LocalBuilder lbuilder) {}
		public virtual void EmitWriteLine (string val) {}

		public virtual void EndExceptionBlock () {}
		public virtual void EndScope () {}
		public virtual void MarkLabel (Label loc) {}
		public virtual void MarkSequencePoint (ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int EndColumn) {}
		public virtual void ThrowException (Type exceptionType) {}
		public virtual void UsingNamespace (String usingNamespace) {}
	}
}
