//
// System.ComponentModel.EditorBrowsableAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//
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

using System.ComponentModel;

namespace System.ComponentModel 
{

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Delegate |
	AttributeTargets.Enum | AttributeTargets.Event | AttributeTargets.Field |
	AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property |
	AttributeTargets.Struct)]
	public sealed class EditorBrowsableAttribute : Attribute 
	{
		private EditorBrowsableState state;

		public EditorBrowsableAttribute ()
		{
			this.state = EditorBrowsableState.Always;
		}

		public EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState state)
		{
			this.state = state;
		}
			
		public EditorBrowsableState State {
        		get {
        			return state;
        		}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is EditorBrowsableAttribute))
				return false;
			if (obj == this)
				return true;
			return ((EditorBrowsableAttribute) obj).State == state;
		}

		public override int GetHashCode ()
		{
			return state.GetHashCode ();
		}
	}
}
