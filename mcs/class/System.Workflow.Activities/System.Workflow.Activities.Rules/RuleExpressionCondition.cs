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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//
//
// See: http://windowssdk.msdn.microsoft.com/en-us/library/ms733601.aspx
//

using System.Collections.Generic;
using System.CodeDom;
using System.Reflection;

namespace System.Workflow.Activities.Rules
{
	[Serializable]
	public sealed class RuleExpressionCondition : RuleCondition
	{
		private string name;
		private CodeExpression expression;

		public RuleExpressionCondition ()
		{

		}

		public RuleExpressionCondition (CodeExpression expression)
		{
			this.expression = expression;
		}

		public RuleExpressionCondition (string conditionName)
		{
			name = conditionName;
		}

		public RuleExpressionCondition (string conditionName, CodeExpression expression)
		{
			name = conditionName;
			this.expression = expression;
		}

		// Properties
		public CodeExpression Expression {
			get { return expression; }
			set { expression = value; }
		}
		public override string Name {
			get { return name; }
			set { name = value; }
		}


		// Methods
		[MonoTODO]
		public override RuleCondition Clone ()
		{
			return null;
		}

		public override bool Equals (object obj)
		{
			RuleExpressionCondition target = (obj as RuleExpressionCondition);

			if (target== null) {
				return false;
			}

			if (Name == target.Name && Expression == target.Expression) {
				return true;
			}

			return false;

		}

		public override bool Evaluate (RuleExecution execution)
		{
	            	Type type = expression.GetType ();
	            	Console.WriteLine ("RuleExpressionCondition.RuleExpressionCondition {0}", type);

	            	if (type == typeof (CodeBinaryOperatorExpression))
	            		return RuleExpressionBinaryOperatorResolver.Evaluate (execution, expression);

	            	if (type == typeof (CodePropertyReferenceExpression)) {
	            		return (bool) CodePropertyReferenceValue (execution, expression);
	            	}

	            	if (type == typeof (CodeFieldReferenceExpression)) {
	            		return (bool) CodePropertyReferenceValue (execution, expression);
	            	}

	            	throw new InvalidOperationException ();
		}

		public override ICollection <string> GetDependencies (RuleValidation validation)
		{
			return null;
		}

		public override int GetHashCode ()
		{
			return name.GetHashCode () ^ expression.GetHashCode ();
		}

		public override void OnRuntimeInitialized ()
		{

		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}

		public override bool Validate (RuleValidation validation)
		{
			return true;
		}
		
		// Private Methods
		internal static object CodePropertyReferenceValue (RuleExecution execution, CodeExpression expression)
		{
			CodePropertyReferenceExpression property = (CodePropertyReferenceExpression) expression;
			Type type;

			// Assumes CodeThisReferenceExpression
			type = execution.ThisObject.GetType ();

			PropertyInfo info = type.GetProperty (property.PropertyName, BindingFlags.FlattenHierarchy  |
				BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);

			return info.GetValue (execution.ThisObject, null);
		}

		internal static object CodeFieldReferenceValue (RuleExecution execution, CodeExpression expression)
		{
			CodeFieldReferenceExpression field = (CodeFieldReferenceExpression) expression;
			Type type;

			// Assumes CodeThisReferenceExpression
			type = execution.ThisObject.GetType ();

			FieldInfo info = type.GetField(field.FieldName, BindingFlags.FlattenHierarchy  |
				BindingFlags.Public | BindingFlags.GetField | BindingFlags.Instance);

			return info.GetValue (execution.ThisObject);
		}

	}
}

