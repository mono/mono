//
// System.ComponentModel.Design.Serialization.EnumCodeDomSerializer
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

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
using System.ComponentModel;
using System.ComponentModel.Design;

using System.CodeDom;

namespace System.ComponentModel.Design.Serialization
{
	internal class EnumCodeDomSerializer : CodeDomSerializer
	{

		public EnumCodeDomSerializer ()
		{
		}

		public override object Serialize (IDesignerSerializationManager manager, object value)
		{
			if (manager == null)
				throw new ArgumentNullException ("manager");
			if (value == null)
				throw new ArgumentNullException ("value");

			Enum[] enums = null;
			TypeConverter converter = TypeDescriptor.GetConverter (value);
			if (converter.CanConvertTo (typeof (Enum[])))
				enums = (Enum[]) converter.ConvertTo (value, typeof (Enum[]));
			else
				enums = new Enum[] { (Enum) value };
			CodeExpression left = null;
			CodeExpression right = null;
			foreach (Enum e in enums) {
				right = GetEnumExpression (e);
				if (left == null) // just the first time
					left = right;
				else
					left = new CodeBinaryOperatorExpression (left, CodeBinaryOperatorType.BitwiseOr, right);
			}

			return left;
		}

		private CodeExpression GetEnumExpression (Enum e)
		{
			TypeConverter converter = TypeDescriptor.GetConverter (e);
			if (converter != null && converter.CanConvertTo (typeof (string)))
				return new CodeFieldReferenceExpression (new CodeTypeReferenceExpression (e.GetType().FullName), 
														 (string) converter.ConvertTo (e, typeof (string)));
			else
				return null;
		}
	}
}
#endif
