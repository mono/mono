//
// System.Windows.Forms.Design.StringArrayEditor
// 
// Author:
//   Ivan N. Zlatev <contact@i-nz.net>
// 
// (C) 2007 Ivan N. Zlatev
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
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace System.Windows.Forms.Design
{
	internal class StringArrayEditor : StringCollectionEditor
	{

		public StringArrayEditor (Type type) : base (type)
		{
		}

		protected override object[] GetItems (object editValue)
		{
			Array array = editValue as Array;
			if (array == null)
				return new object[0];

			object[] objectArray = new object[array.GetLength (0)];
			Array.Copy (array, objectArray, objectArray.Length);
			return objectArray;
		}

		protected override object SetItems (object editValue, object[] value)
		{
			if (!(editValue is Array))
				return editValue;

			Array typeArray = Array.CreateInstance (base.CollectionItemType, value.Length);
			Array.Copy (value, typeArray, value.Length);
			return typeArray;
		}

		protected override Type CreateCollectionItemType ()
		{
			return base.CollectionType.GetElementType ();
		}
	}
}
