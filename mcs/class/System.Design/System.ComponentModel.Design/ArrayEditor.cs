//
// System.ComponentModel.Design.ArrayEditor
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2007 Andreas Nahr
// (C) 2003 Martin Willemoes Hansen
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


using System;
using System.Reflection;

namespace System.ComponentModel.Design
{
	public class ArrayEditor : CollectionEditor
	{
		public ArrayEditor (Type type) : base (type)
		{
		}

		protected override Type CreateCollectionItemType()
		{
			return CollectionType.GetElementType ();
		}

		protected override object[] GetItems (object editValue)
		{
			if (editValue == null)
				return null;
			if (!(editValue is Array))
				return new object[0];

			Array editArray = (Array)editValue;
			object[] result = new object[editArray.Length];
			editArray.CopyTo (result, 0);
			return result;
		}

		protected override object SetItems (object editValue, object[] value)
		{
			if (editValue == null)
				return null;

			Array result = Array.CreateInstance (CollectionItemType, value.Length);
			value.CopyTo (result, 0);
			return result;
		}
	}
}
