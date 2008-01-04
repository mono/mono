//
// System.ComponentModel.Design.Serialization.ComponentCodeDomSerializer
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
using System.ComponentModel;
using System.ComponentModel.Design;

using System.CodeDom;

namespace System.ComponentModel.Design.Serialization
{
	// A serializer for the IComponent Type, supplied by the CodeDomSerializationProvider,
	// added as a provider by the RootComponentCodeDomSerializer
	//
	internal class ComponentCodeDomSerializer : CodeDomSerializer
	{

		public ComponentCodeDomSerializer ()
		{
		}

		public override object Serialize (IDesignerSerializationManager manager, object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			RootContext rootContext = manager.Context[typeof (RootContext)] as RootContext;
			if (rootContext != null && rootContext.Value == value)
				return rootContext.Expression;

			CodeStatementCollection statements = new CodeStatementCollection ();

			if (((IComponent)value).Site == null) {
				ReportError (manager, "Component of type '" + value.GetType().Name + "' not sited");
				return statements;
			}

			// the trick with the nested components is that GetName will return the full name
			// e.g splitter1.Panel1 and thus the code below will create a reference to that.
			// 
			string name = manager.GetName (value);

			CodeExpression componentRef = null;
			if (rootContext != null)
				componentRef = new CodeFieldReferenceExpression (rootContext.Expression , name);
			else
				componentRef = new CodeFieldReferenceExpression (new CodeThisReferenceExpression () , name);

			base.SetExpression (manager, value, componentRef);

			ExpressionContext context = manager.Context[typeof (ExpressionContext)] as ExpressionContext;
			// Perform some heuristics here. 
			// 
			// If there is an ExpressionContext of PropertyReference where PresetValue == this
			// partial serialization doesn't make sense, so perform full. E.g in the case of:
			// 
			// PropertyCodeDomSerializer.SerializeContentProperty and splitContainer1.*Panel1* 
			//
			if (context == null || context.PresetValue != value ||
			    (context.PresetValue == value && (context.Expression is CodeFieldReferenceExpression ||
							      context.Expression is CodePropertyReferenceExpression))) {
				bool isComplete = true;
				statements.Add (new CodeCommentStatement (String.Empty));
				statements.Add (new CodeCommentStatement (name));
				statements.Add (new CodeCommentStatement (String.Empty));
	
				// Do not serialize a creation expression for Nested components
				//
				if (! (((IComponent)value).Site is INestedSite)) {
					CodeStatement assignment = new CodeAssignStatement (componentRef, 
									base.SerializeCreationExpression (manager, value, 
													  out isComplete));
					assignment.UserData["statement-order"] = "initializer";
					statements.Add (assignment);
				}
	
				base.SerializeProperties (manager, statements, value, new Attribute[0]);
				base.SerializeEvents (manager, statements, value);
			}

			return statements;
		}
	}
}
#endif
