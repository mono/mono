//
// System.Web.Compilation.ExpressionBuilder
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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

using System.CodeDom;
using System.Web.UI;

namespace System.Web.Compilation
{
	public abstract class ExpressionBuilder {
		protected ExpressionBuilder ()
		{
		}

		public abstract CodeExpression GetCodeExpression (BoundPropertyEntry entry, object parsedData,
								  ExpressionBuilderContext context);

		public virtual object EvaluateExpression (object target, BoundPropertyEntry entry, object parsedData,
							  ExpressionBuilderContext context)
		{
			return null;
		}

		public virtual object ParseExpression (string expression, Type propertyType, ExpressionBuilderContext context)
		{
			return null;
		}

		public virtual bool SupportsEvaluate {
			get { return false; }
		}
	}
	
}


