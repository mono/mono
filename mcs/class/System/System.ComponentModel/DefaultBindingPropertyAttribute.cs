//
// System.ComponentModel.DefaultBindingPropertyAttribute
//
// Authors:
//	Rolf Bjarne Kvinge  (RKvinge@novell.com)
//
// Copyright (c) 2006 Novell, Inc.
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

namespace System.ComponentModel 
{
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class DefaultBindingPropertyAttribute : Attribute
	{
		public static readonly DefaultBindingPropertyAttribute Default;
		
		private string name;
		
		static DefaultBindingPropertyAttribute ()
		{
			Default = new DefaultBindingPropertyAttribute ();
		}
		
		public DefaultBindingPropertyAttribute ()
		{
		}
		
		public DefaultBindingPropertyAttribute (string name)
		{
			this.name = name;
		}
		
		public override bool Equals (object obj)
		{
			DefaultBindingPropertyAttribute other = obj as DefaultBindingPropertyAttribute;
			
			if (obj == null)
				return false;
				
			return name == other.Name;
		}
		
		public override int GetHashCode ()
		{
			return name == null ? 0 : name.GetHashCode ();
		}
		
		public string Name {
			get {
				return name;
			}
		}
		
		
	}
}
#endif
