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
			else if (!property.Attributes.Contains (DesignerSerializationVisibilityAttribute.Hidden))
				SerializeNormalProperty (manager, value, property, statements);
		}


		private void SerializeNormalProperty (IDesignerSerializationManager manager, 
											  object component, PropertyDescriptor descriptor, CodeStatementCollection statements)
		{
			CodeAssignStatement assignment = new CodeAssignStatement ();

			CodeExpression leftSide = null;
			CodePropertyReferenceExpression propRef = null;
			ExpressionContext expression = manager.Context[typeof (ExpressionContext)] as ExpressionContext;
			RootContext root = manager.Context[typeof (RootContext)] as RootContext;

			if (expression != null && expression.PresetValue == component && expression.Expression != null) {
				leftSide = new CodePropertyReferenceExpression (expression.Expression, descriptor.Name);
			} else if (root != null && root.Value == component) {
				leftSide = new CodePropertyReferenceExpression (root.Expression, descriptor.Name);
			} else {
				propRef = new CodePropertyReferenceExpression ();
				propRef.PropertyName =  descriptor.Name;
				propRef.TargetObject = TryGetCachedExpression (manager, component, propRef);
				leftSide = propRef;
			}

			CodeExpression rightSide = null;

			MemberRelationship relationship = GetRelationship (manager, component, descriptor);
			if (!relationship.IsEmpty) {
				propRef = new CodePropertyReferenceExpression ();
				propRef.PropertyName = relationship.Member.Name;
				propRef.TargetObject = TryGetCachedExpression (manager, relationship.Owner, propRef);
				rightSide = propRef;
			} else {
				object rightSideValue = descriptor.GetValue (component);
				rightSide = TryGetCachedExpression (manager, rightSideValue, null, component);
			}

			if (rightSide == null) {
				Console.WriteLine ("SerializeNormalProperty: <" + component.GetType().Name + "." +
								   descriptor.Name + "> - unable to serialize the right side of the assignment to expression");
			} else if (leftSide == null) {
				Console.WriteLine ("SerializeNormalProperty: <" + component.GetType().Name + "." +
								   descriptor.Name + "> - unable to serialize the left side of the assignment to expression");
			} else  {
				assignment.Left = leftSide;
				assignment.Right = rightSide;
				statements.Add (assignment);
			}
		}

		private CodeExpression TryGetCachedExpression (IDesignerSerializationManager manager, object value)
		{
			return TryGetCachedExpression (manager, value, null);
		}

		private CodeExpression TryGetCachedExpression (IDesignerSerializationManager manager, object value, 
													   CodeExpression parentExpression)
		{
			return TryGetCachedExpression (manager, value, parentExpression, null);
		}

		private CodeExpression TryGetCachedExpression (IDesignerSerializationManager manager, object value, 
													   CodeExpression parentExpression, object presetValue)
		{
			CodeExpression expression = null;
			if (value != null) // in order to support null value serialization
				expression = base.GetExpression (manager, value);
			if (expression == null) {
				if (parentExpression == null)
					manager.Context.Push (new ExpressionContext (null, null, value, presetValue));
				else
					manager.Context.Push (new ExpressionContext (parentExpression, parentExpression.GetType (), value, presetValue));
				expression = base.SerializeToExpression (manager, value);
				manager.Context.Pop ();
			}
			return expression;
		}

		private void SerializeContentProperty (IDesignerSerializationManager manager, object component, 
											   PropertyDescriptor descriptor, CodeStatementCollection statements)
		{
			CodePropertyReferenceExpression propRef = new CodePropertyReferenceExpression ();
			propRef.PropertyName = descriptor.Name;
			object value = descriptor.GetValue (component);

			ExpressionContext expressionCtx = manager.Context[typeof (ExpressionContext)] as ExpressionContext;
			if (expressionCtx != null && expressionCtx.PresetValue == component) {
				propRef.TargetObject = expressionCtx.Expression;
			} else {
				manager.Context.Push (new CodeStatementCollection ());
				propRef.TargetObject = TryGetCachedExpression (manager, component, propRef, value);
				manager.Context.Pop ();
			}

			CodeDomSerializer serializer = manager.GetSerializer (value.GetType (), typeof (CodeDomSerializer)) as CodeDomSerializer;

			if (propRef.TargetObject != null && serializer != null) {
				// request full serialization (presetvalue == instance)
				//
				manager.Context.Push (new ExpressionContext (propRef, propRef.GetType (), component, value));
				object serialized = serializer.Serialize (manager, value);
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

			if (property.Attributes.Contains (DesignerSerializationVisibilityAttribute.Hidden))
				return false;
			else if (property.Attributes.Contains (DesignOnlyAttribute.Yes))
				return false;

			bool result = property.ShouldSerializeValue (value);

			if (!result) {
				if (!GetRelationship (manager, value, descriptor).IsEmpty)
					result = true;
			}

			if (!result) {
				SerializeAbsoluteContext absolute = manager.Context[typeof (SerializeAbsoluteContext)] as SerializeAbsoluteContext;
				if (absolute != null && absolute.ShouldSerialize (descriptor))
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
