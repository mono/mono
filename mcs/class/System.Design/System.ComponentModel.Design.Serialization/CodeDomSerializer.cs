//
// System.ComponentModel.Design.Serialization.CodeDomSerializer
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


using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

using System.CodeDom;

namespace System.ComponentModel.Design.Serialization
{
	public class CodeDomSerializer : CodeDomSerializerBase
	{

		public CodeDomSerializer ()
		{
		}


		public virtual object SerializeAbsolute (IDesignerSerializationManager manager, object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			SerializeAbsoluteContext context = new SerializeAbsoluteContext ();
			manager.Context.Push (context);
			object result = this.Serialize (manager, value);
			manager.Context.Pop ();
			return result;
		}

		public virtual object Serialize (IDesignerSerializationManager manager, object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			object serialized = null;
			bool isComplete = false;
			CodeExpression createExpr = base.SerializeCreationExpression (manager, value, out isComplete);
			if (createExpr != null) {
				if (isComplete) {
					serialized = createExpr;
				} else {
					CodeStatementCollection statements = new CodeStatementCollection ();
					base.SerializeProperties (manager, statements, value, new Attribute[0]);
					base.SerializeEvents (manager, statements, value, new Attribute[0]);
					serialized = statements;
				}
				base.SetExpression (manager, value, createExpr);
			}
			return serialized;
		}

		[Obsolete ("This method has been deprecated. Use SerializeToExpression or GetExpression instead.")] 
		protected CodeExpression SerializeToReferenceExpression (IDesignerSerializationManager manager, object value)
		{
			return base.SerializeToExpression (manager, value);
		}

		// I am not sure what this does, but the only name I can think of this can get is a variable name from 
		// the expression
		public virtual string GetTargetComponentName (CodeStatement statement, CodeExpression expression, Type targetType)
		{
			if (expression is CodeFieldReferenceExpression)
				return ((CodeFieldReferenceExpression) expression).FieldName;
			else if (expression is CodeVariableReferenceExpression)
				return ((CodeVariableReferenceExpression) expression).VariableName;
			return null;
		}

		public virtual CodeStatementCollection SerializeMember (IDesignerSerializationManager manager, 
									object owningObject, MemberDescriptor member)
		{
			if (member == null)
				throw new ArgumentNullException ("member");
			if (owningObject == null)
				throw new ArgumentNullException ("owningObject");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			CodeStatementCollection statements = new CodeStatementCollection ();

			CodeExpression expression = base.GetExpression (manager, owningObject);
			if (expression == null) {
				string name = manager.GetName (owningObject);
				if (name == null)
					name = base.GetUniqueName (manager, owningObject);
				expression = new CodeVariableReferenceExpression (name);
				base.SetExpression (manager, owningObject, expression);
			}

			if (member is PropertyDescriptor)
				base.SerializeProperty (manager, statements, owningObject, (PropertyDescriptor) member);
			if (member is EventDescriptor)
				base.SerializeEvent (manager, statements, owningObject, (EventDescriptor) member);

			return statements;
		}

		public virtual CodeStatementCollection SerializeMemberAbsolute (IDesignerSerializationManager manager, 
										object owningObject, MemberDescriptor member)
		{
			if (member == null)
				throw new ArgumentNullException ("member");
			if (owningObject == null)
				throw new ArgumentNullException ("owningObject");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			SerializeAbsoluteContext context = new SerializeAbsoluteContext (member);
			manager.Context.Push (context);
			CodeStatementCollection result = this.SerializeMember (manager, owningObject, member);
			manager.Context.Pop ();
			return result;
		}


		public virtual object Deserialize (IDesignerSerializationManager manager, object codeObject)
		{
			object deserialized = null;

			CodeExpression expression = codeObject as CodeExpression;
			if (expression != null)
				deserialized = base.DeserializeExpression (manager, null, expression);

			CodeStatement statement = codeObject as CodeStatement;
			if (statement != null)
				deserialized = DeserializeStatementToInstance (manager, statement);

			CodeStatementCollection statements = codeObject as CodeStatementCollection;
			if (statements != null) {
				foreach (CodeStatement s in statements) {
					if (deserialized == null)
						deserialized = DeserializeStatementToInstance (manager, s);
					else
						DeserializeStatement (manager, s);
				}
			}
			return deserialized;
		}

		protected object DeserializeStatementToInstance (IDesignerSerializationManager manager, CodeStatement statement)
		{
			CodeAssignStatement assignment = statement as CodeAssignStatement;
			if (assignment != null) {
				// CodeFieldReferenceExpression
				//
				CodeFieldReferenceExpression fieldRef = assignment.Left as CodeFieldReferenceExpression;
				if (fieldRef != null)
					return base.DeserializeExpression (manager, fieldRef.FieldName, assignment.Right);
			}
			base.DeserializeStatement (manager, statement);
			return null;
		}
	}
}
