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

#if NET_2_0
		[MonoNotSupported ("")]
		public void Append (object context)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
