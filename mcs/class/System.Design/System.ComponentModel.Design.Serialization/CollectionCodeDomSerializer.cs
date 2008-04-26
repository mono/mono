//
// System.ComponentModel.Design.Serialization.CollectionCodeDomSerializer
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

using System.CodeDom;

namespace System.ComponentModel.Design.Serialization
{
	public class CollectionCodeDomSerializer : CodeDomSerializer
	{

		public CollectionCodeDomSerializer ()
		{
		}

		// FIXME: What is this supposed to do?
		protected bool MethodSupportsSerialization (MethodInfo method)
		{
			return true;
		}

		public override object Serialize (IDesignerSerializationManager manager, object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			ICollection originalCollection = value as ICollection;
			if (originalCollection == null)
				throw new ArgumentException ("originalCollection is not an ICollection");

			CodeExpression targetExpression = null;

			ExpressionContext exprContext = manager.Context[typeof (ExpressionContext)] as ExpressionContext;
			RootContext root = manager.Context[typeof (RootContext)] as RootContext;

			if (exprContext != null && exprContext.PresetValue == value)
				targetExpression = exprContext.Expression;
			else if (root != null)
				targetExpression = root.Expression;

			ArrayList valuesToSerialize = new ArrayList ();
			foreach (object o in originalCollection)
				valuesToSerialize.Add (o);

			return this.SerializeCollection (manager, targetExpression, value.GetType (), originalCollection, valuesToSerialize);
		}

		protected virtual object SerializeCollection (IDesignerSerializationManager manager, CodeExpression targetExpression, 
							      Type targetType, ICollection originalCollection, ICollection valuesToSerialize)
		{
			if (valuesToSerialize == null)
				throw new ArgumentNullException ("valuesToSerialize");
			if (originalCollection == null)
				throw new ArgumentNullException ("originalCollection");
			if (targetType == null)
				throw new ArgumentNullException ("targetType");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			if (valuesToSerialize.Count == 0)
				return null;

			MethodInfo method = null;
			try {
				object sampleParam = null;
				IEnumerator e = valuesToSerialize.GetEnumerator ();
				e.MoveNext ();		
				sampleParam = e.Current;
				// try to find a method matching the type of the sample parameter.
				// Assuming objects in the collection are from the same base type
				method = GetExactMethod (targetType, "Add", new object [] { sampleParam });
			} catch {
				base.ReportError (manager, "A compatible Add/AddRange method is missing in the collection type '" 
						  + targetType.Name + "'");
			}

			if (method == null)
				return null;

			CodeStatementCollection statements = new CodeStatementCollection ();

			foreach (object value in valuesToSerialize) {

				CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression ();
				methodInvoke.Method = new CodeMethodReferenceExpression (targetExpression, "Add");

				CodeExpression expression = base.SerializeToExpression (manager, value);
				if (expression != null) {
					methodInvoke.Parameters.AddRange (new CodeExpression[] { expression });
					statements.Add (methodInvoke);
				}
			}

			return statements;
		}

		// Searches for a method on type that matches argument types
		//
		private MethodInfo GetExactMethod (Type type, string methodName, ICollection argsCollection)
		{
			object[] arguments = null;
			Type[] types = Type.EmptyTypes;

			if (argsCollection != null) {
				arguments = new object[argsCollection.Count];
				types = new Type[argsCollection.Count];
				argsCollection.CopyTo (arguments, 0);

				for (int i=0; i < arguments.Length; i++) {
					if (arguments[i] == null)
						types[i] = null;
					else
						types[i] = arguments[i].GetType ();
				}
			}

			return type.GetMethod (methodName, types);
		}
	}
}
#endif
