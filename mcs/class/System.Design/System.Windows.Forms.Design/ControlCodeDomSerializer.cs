//
// System.Windows.Forms.Design.ControlCodeDomSerializaer
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
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.CodeDom;
using System.Windows.Forms;

namespace System.Windows.Forms.Design
{
	internal class ControlCodeDomSerializer : ComponentCodeDomSerializer
	{

		public ControlCodeDomSerializer ()
		{
		}

		public override object Serialize (IDesignerSerializationManager manager, object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			if (!(value is Control))
				throw new InvalidOperationException ("value is not a Control");

			object serialized = base.Serialize (manager, value);
			CodeStatementCollection statements = serialized as CodeStatementCollection;
			if (statements != null) { // the root control is serialized to CodeExpression
				ICollection childControls = TypeDescriptor.GetProperties (value)["Controls"].GetValue (value) as ICollection;
				if (childControls.Count > 0) {
					CodeExpression componentRef = base.GetExpression (manager, value);

					CodeStatement statement = new CodeExpressionStatement (
						new CodeMethodInvokeExpression (componentRef, "SuspendLayout"));
					statement.UserData["statement-order"] = "begin";
					statements.Add (statement);
					statement = new CodeExpressionStatement (
						new CodeMethodInvokeExpression (componentRef, "ResumeLayout", 
										new CodeExpression[] { 
											new CodePrimitiveExpression (false) }));
					statement.UserData["statement-order"] = "end";
					statements.Add (statement);
					serialized = statements;
				}
			}
			return serialized;
		}
	}
}
#endif
