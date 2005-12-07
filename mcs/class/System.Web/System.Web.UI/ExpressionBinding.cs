//
// System.Web.UI.ExpressionBinding.cs
//
// Authors:
// 	Sanjay Gupta gsanjay@novell.com)
//
// (C) 2004 Novell, Inc. (http://www.novell.com)
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

namespace System.Web.UI {
	public sealed class ExpressionBinding
	{
		string propertyName;
		Type propertyType;
		string expression;
        	string prefix;
        	bool generated;

        	public ExpressionBinding (string propertyName, Type propertyType, 
					string expressionPrefix, string expression)
		{
			this.propertyName = propertyName;
			this.propertyType = propertyType;
            		this.prefix = expressionPrefix;
			this.expression = expression;            
            		this.generated = false;
		}

		public string Expression {
			get { return expression; }
			set { expression = value; }
		}

        	public string ExpressionPrefix {
			get { return prefix; }
			set { prefix = value; }
		}
        
        	public bool Generated {
            		get { return generated; }
        	}

		public string PropertyName {
			get { return propertyName; }
		}

		public Type PropertyType {
			get { return propertyType; }
		}

		public override bool Equals (object obj)
		{
            		if (!(obj is ExpressionBinding))
                		return false;

            		ExpressionBinding o = (ExpressionBinding)obj;
            		return (o.Expression == expression &&
                		o.ExpressionPrefix == prefix &&
				o.PropertyName == propertyName &&
				o.PropertyType == propertyType);
		}

		public override int GetHashCode ()
		{
			return propertyName.GetHashCode () +
			       (propertyType.GetHashCode () << 1) +
			       (prefix.GetHashCode () << 2) +
                   		(expression.GetHashCode () << 3);
		}
	}
}
#endif
