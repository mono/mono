//
// System.ComponentModel.Design.Serialization.ExpressionContext
//
// Authors:	 
//	  Ivan N. Zlatev (contact@i-nZ.net)
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
using System.CodeDom;

namespace System.ComponentModel.Design.Serialization
{
	public sealed class ExpressionContext
	{

		private object _owner;
		private Type _expressionType;
		private CodeExpression _expression;
		private object _presetValue;

		public ExpressionContext (CodeExpression expression, Type expressionType, object owner)
		{
			_expression = expression;
			_expressionType = expressionType;
			_owner = owner;
			_presetValue = null;
		}

		public ExpressionContext (CodeExpression expression, Type expressionType, object owner, object presetValue)
		{
			_expression = expression;
			_expressionType = expressionType;
			_owner = owner;
			_presetValue = presetValue;
		}

		public object PresetValue {
			get { return _presetValue; }
		}

		public CodeExpression Expression {
			get { return _expression; }
		}

		public Type ExpressionType {
			get { return _expressionType; }
		}

		public object Owner {
			get { return _owner; }
		}
	}
}
#endif
