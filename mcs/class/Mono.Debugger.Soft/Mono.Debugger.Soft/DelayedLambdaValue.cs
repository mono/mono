using System;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	public class DelayedLambdaValue : Value
	{
		DelayedLambdaType delayedType;

		public DelayedLambdaValue (VirtualMachine vm, Tuple<string, Value>[] locals, string expression) : base (vm, 0)
		{
			this.delayedType = new DelayedLambdaType (vm, locals, expression);
		}

		public string Expression {
			get { return DelayedType.Expression; }
		}

		public DelayedLambdaType DelayedType {
			get { return delayedType; }
		}

		public string Name {
			get { return DelayedType.Name; }
		}

		public Tuple<string, Value>[] Locals {
			get { return DelayedType.Locals; }
		}

		public Value[] GetLocalValues ()
		{
			return DelayedType.GetLocalValues ();
		}

		public string GetLiteralType (TypeMirror t)
		{
			return delayedType.GetLiteralType (t);
		}

		public bool IsAcceptableType (TypeMirror toType)
		{
			return delayedType.IsAcceptableType (toType);
		}

		public override string ToString ()
		{
			return string.Format ("LambdaValue for ({0})", Expression);
		}
	}
}
