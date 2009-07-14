//
// CSharpConvertBinder.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CSharp.RuntimeBinder
{
	public class CSharpConvertBinder : ConvertBinder
	{
		bool is_checked;

		public CSharpConvertBinder (Type type, CSharpConversionKind conversionKind, bool isChecked)
			: base (type, conversionKind == CSharpConversionKind.ExplicitConversion)
		{
			this.is_checked = isChecked;
		}
		
		public CSharpConversionKind ConversionKind {
			get {
				return Explicit ? CSharpConversionKind.ExplicitConversion : CSharpConversionKind.ImplicitConversion;
			}
		}		
		
		public override bool Equals (object obj)
		{
			var other = obj as CSharpConvertBinder;
			return other != null && other.Type == Type && other.Explicit == Explicit && other.is_checked == is_checked;
		}

		public bool IsChecked {
			get {
				return is_checked;
			}
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		[MonoTODO]
		public override DynamicMetaObject FallbackConvert (DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			throw new NotImplementedException ();
		}
	}
}
