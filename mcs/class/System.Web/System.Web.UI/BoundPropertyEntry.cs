//
// System.Web.UI.BoundPropertyEntry
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	public class BoundPropertyEntry : PropertyEntry
	{
		internal BoundPropertyEntry ()
		{
		}
		
		public string ControlID {
			get; set;
		}

		public Type ControlType {
			get; set;
		}
		
		public string Expression {
			get; set;
		}

		public ExpressionBuilder ExpressionBuilder {
			get; set;
		}
		
		public string ExpressionPrefix {
			get; set;
		}
		
		public string FieldName {
			get; set;
		}
		
		public string FormatString {
			get; set;
		}
		
		public bool Generated {
			get; set;
		}
		
		public object ParsedExpressionData {
			get; set;
		}

		public bool ReadOnlyProperty {
			get; set;
		}
		
		public bool TwoWayBound {
			get; set;
		}
		
		public bool UseSetAttribute {
			get; set;
		}
	}
}

