//
// System.ComponentModel.EventDescriptor.cs
//
// Authors:
//  Rodrigo Moya (rodrigo@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc. 2002
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

using System.Runtime.InteropServices;

namespace System.ComponentModel
{
	[ComVisible (true)]
	public abstract class EventDescriptor : MemberDescriptor
	{

		protected EventDescriptor (MemberDescriptor desc) : base (desc)
		{
		}

		protected EventDescriptor (MemberDescriptor desc, Attribute[] attrs) : base (desc, attrs)
		{
		}

		protected EventDescriptor (string str, Attribute[] attrs) : base (str, attrs)
		{
		}

		public abstract void AddEventHandler (object component, System.Delegate value);

		public abstract void RemoveEventHandler(object component, System.Delegate value);

		public abstract System.Type ComponentType { get; }

		public abstract System.Type EventType { get; }

		public abstract bool IsMulticast { get; }
	}
}
