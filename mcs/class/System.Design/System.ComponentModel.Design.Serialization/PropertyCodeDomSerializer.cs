//
// System.ComponentModel.Design.Serialization.PropertyCodeDomSerializer
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
	internal class PropertyCodeDomSerializer : MemberCodeDomSerializer
	{

		public PropertyCodeDomSerializer ()
		{
		}
	
		public override void Serialize (IDesignerSerializationManager manager, object value, MemberDescriptor descriptor, CodeStatementCollection statements)
		{
			if (manager == null)
				throw new ArgumentNullException ("manager");
			if (value == null)
				throw new ArgumentNullException ("value");
			if (descriptor == null)
				throw new ArgumentNullException ("descriptor");
			if (statements == null)
				throw new ArgumentNullException ("statements");

			PropertyDescriptor property = (PropertyDescriptor) descriptor;

			if (property.Attributes.Contains (DesignerSerializationVisibilityAttribute.Content))
				SerializeContentProperty (manager, value, property, statements);
			else
				SerializeNormalProperty (manager, value, property, statements);
		}


		private void SerializeNormalProperty (IDesignerSerializationManager manager, 
						      object instance, PropertyDescriptor descriptor, CodeStatementCollection statements)
		{
			CodeAssignStatement assignment = new CodeAssignStatement ();

			CodeExpression leftSide = null;
			CodePropertyReferenceExpression propRef = null;
			ExpressionContext expression = manager.Context[typeof (ExpressionContext)] as ExpressionContext;
			RootContext root = manager.Context[typeof (RootContext)] as RootContext;

			if (expression != null && expression.PresetValue == instance && expression.Expression != null) {
				leftSide = new CodePropertyReferenceExpression (expression.Expression, descriptor.Name);
			} else if (root != null && root.Value == instance) {
				leftSide = new CodePropertyReferenceExpression (root.Expression, descriptor.Name);
			} else {
				propRef = new CodePropertyReferenceExpression ();
				propRef.PropertyName =  descriptor.Name;
				propRef.TargetObject = base.SerializeToExpression (manager, instance);
				leftSide = propRef;
			}

			CodeExpression rightSide = null;

			MemberRelationship relationship = GetRelationship (manager, instance, descriptor);
			if (!relationship.IsEmpty) {
				propRef = new CodePropertyReferenceExpression ();
				propRef.PropertyName = relationship.Member.Name;
				propRef.TargetObject = base.SerializeToExpression (manager, relationship.Owner);
				rightSide = propRef;
			} else {
				rightSide = base.SerializeToExpression (manager, descriptor.GetValue (instance));
			}

			if (rightSide == null || leftSide == null) {
				base.ReportError (manager, "Cannot serialize " + ((IComponent)instance).Site.Name + "." + descriptor.Name,
						  "Property Name: " + descriptor.Name + System.Environment.NewLine +
						  "Property Type: " + descriptor.PropertyType.Name + System.Environment.NewLine);
			} else  {
				assignment.Left = leftSide;
				assignment.Right = rightSide;
				statements.Add (assignment);
			}
		}

		private void SerializeContentProperty (IDesignerSerializationManager manager, object instance, 
						       PropertyDescriptor descriptor, CodeStatementCollection statements)
		{
			CodePropertyReferenceExpression propRef = new CodePropertyReferenceExpression ();
			propRef.PropertyName = descriptor.Name;
			object propertyValue = descriptor.GetValue (instance);

			ExpressionContext expressionCtx = manager.Context[typeof (ExpressionContext)] as ExpressionContext;
			if (expressionCtx != null && expressionCtx.PresetValue == instance)
				propRef.TargetObject = expressionCtx.Expression;
			else
				propRef.TargetObject = base.SerializeToExpression (manager, instance);

			CodeDomSerializer serializer = manager.GetSerializer (propertyValue.GetType (), typeof (CodeDomSerializer)) as CodeDomSerializer;
			if (propRef.TargetObject != null && serializer != null) {
				manager.Context.Push (new ExpressionContext (propRef, propRef.GetType (), null, propertyValue));
				object serialized = serializer.Serialize (manager, propertyValue);
				manager.Context.Pop ();

				CodeStatementCollection serializedStatements = serialized as CodeStatementCollection;
				if (serializedStatements != null)
					statements.AddRange (serializedStatements);

				CodeStatement serializedStatement = serialized as CodeStatement;
				if (serializedStatement != null)
					statements.Add (serializedStatement);

				CodeExpression serializedExpr = serialized as CodeExpression;
				if (serializedExpr != null)
					statements.Add (new CodeAssignStatement (propRef, serializedExpr));
			}
		}

		public override bool ShouldSerialize (IDesignerSerializationManager manager, object value, MemberDescriptor descriptor)
		{
			if (manager == null)
				throw new ArgumentNullException ("manager");
			if (value == null)
				throw new ArgumentNullException ("value");
			if (descriptor == null)
				throw new ArgumentNullException ("descriptor");

			PropertyDescriptor property = (PropertyDescriptor) descriptor;

			if (property.Attributes.Contains (DesignOnlyAttribute.Yes))
				return false;

			SerializeAbsoluteContext absolute = manager.Context[typeof (SerializeAbsoluteContext)] as SerializeAbsoluteContext;
			if (absolute != null && absolute.ShouldSerialize (descriptor))
				return true;

			bool result = property.ShouldSerializeValue (value);

			if (!result) {
				if (!GetRelationship (manager, value, descriptor).IsEmpty)
					result = true;
			}

			return result;
		}

		private MemberRelationship GetRelationship (IDesignerSerializationManager manager, object value, MemberDescriptor descriptor)
		{
			MemberRelationshipService service = manager.GetService (typeof (MemberRelationshipService)) as MemberRelationshipService;
			if (service != null)
				return service[value, descriptor];
			else
				return MemberRelationship.Empty;
		}
	}
}
#endif
