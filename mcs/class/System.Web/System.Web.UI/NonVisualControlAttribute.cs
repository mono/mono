//
// System.Web.UI.WebControls.NonVisualControlAttribute.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;

namespace System.Web.UI
{
	public sealed class NonVisualControlAttribute: Attribute
	{
		public static readonly NonVisualControlAttribute Visual = new NonVisualControlAttribute (false);
		public static readonly NonVisualControlAttribute NonVisual = new NonVisualControlAttribute (true);
		public static readonly NonVisualControlAttribute Default = Visual;
		
		bool nonVisual;
		
		public NonVisualControlAttribute (): this (true)
		{
		}
		
		public NonVisualControlAttribute (bool nonVisual)
		{
			this.nonVisual = nonVisual; 
		}
		
		public override bool Equals (object obj)
		{
			NonVisualControlAttribute ot = obj as NonVisualControlAttribute;
			return ot != null && ot.nonVisual == nonVisual;
		}
		
		public override int GetHashCode ()
		{
			return GetType().GetHashCode () + nonVisual.GetHashCode ();
		}
		
		public override bool IsDefaultAttribute ()
		{
			return Equals (Default);
		}
		
		public bool IsNonVisual {
			get { return nonVisual; }
		}
	}
}

#endif
