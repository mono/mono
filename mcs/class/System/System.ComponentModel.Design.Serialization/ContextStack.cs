//
// System.ComponentModel.Design.Serialization.ContextStack.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@gnome.org)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
//

using System.Collections;

namespace System.ComponentModel.Design.Serialization
{
	public sealed class ContextStack
	{
		private Stack stack;

		public ContextStack () 
		{
			stack = new Stack ();
		}

		public object Current {
			get { 
				try {
					return stack.Peek ();
				}
				catch {
					return null;
				}
			}
		}

		public object this[Type type] {
			get {
				foreach (object o in stack.ToArray())
					if (o.GetType () == type)
 						return o;
				return null;
			}
		}

		public object this[int level] {
			get {
				if (level < 0)
					throw new ArgumentException ("level has to be >= 0","level");
				Array A = stack.ToArray();
				if (level > (A.Length - 1))
					return null;
				return A.GetValue(level);
			}
		}

		public object Pop ()
		{
			return stack.Pop ();
		}

		public void Push (object context)
		{
			stack.Push (context);
		}
	}
}
