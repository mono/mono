//
// Plus.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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

namespace Microsoft.JScript {

	public sealed class Plus : BinaryOp {

		public Plus ()
			: base (null, null, JSToken.Plus)
		{
		}

		public  object EvaluatePlus (object v1, object v2)
		{
			IConvertible ic1 = v1 as IConvertible;
			IConvertible ic2 = v2 as IConvertible;

			TypeCode tc1 = Convert.GetTypeCode (v1, ic1);
			TypeCode tc2 = Convert.GetTypeCode (v2, ic2);
			
			switch (tc1) {
			case TypeCode.Double:
				switch (tc2) {
				case TypeCode.Boolean:
				case TypeCode.Double:
					return ic1.ToDouble (null) + ic2.ToDouble (null);

				case TypeCode.String:
					return ic1.ToString (null) + ic2.ToString (null);
				}
				break;

			case TypeCode.String:
				switch (tc2) {
				case TypeCode.Boolean:
					return ic1.ToString (null) + Convert.ToString (ic2.ToBoolean (null));

				case TypeCode.Double:
				case TypeCode.String:
					return ic1.ToString (null) + ic2.ToString (null);
				}
				break;
				
			case TypeCode.Boolean:
				switch (tc2) {
				case TypeCode.Double:
					return ic1.ToDouble (null) + ic2.ToDouble (null);

				case TypeCode.String:
					return Convert.ToString (ic1.ToBoolean (null)) + ic2.ToString (null);

				case TypeCode.Boolean:
					return ic1.ToInt32 (null) + ic2.ToInt32 (null);
				}
				break;
			default:
				return EvaluatePlus (v2, v1);
			}
			throw new NotImplementedException ();
		}

		public static object DoOp (object v1, object v2)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
