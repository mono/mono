//
// System.ComponentModel.Design.Serialization.TypeCodeDomSerializer
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
	public class TypeCodeDomSerializer : CodeDomSerializerBase
	{

		public TypeCodeDomSerializer ()
		{
		}

		public virtual CodeTypeDeclaration Serialize (IDesignerSerializationManager manager, object root, ICollection members)
		{
			if (root == null)
				throw new ArgumentNullException ("root");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			RootContext rootContext = new RootContext (new CodeThisReferenceExpression (), root);
			StatementContext statementContext = new StatementContext ();
			if (members != null)
				statementContext.StatementCollection.Populate (members);
			statementContext.StatementCollection.Populate (root);
			CodeTypeDeclaration declaration = new CodeTypeDeclaration (manager.GetName (root));

			manager.Context.Push (rootContext);
			manager.Context.Push (statementContext);
			manager.Context.Push (declaration);

			if (members != null) {
				foreach (object member in members)
					base.SerializeToExpression (manager, member);
			}
			base.SerializeToExpression (manager, root);

			manager.Context.Pop ();
			manager.Context.Pop ();
			manager.Context.Pop ();

			return declaration;
		}

		// TODO - http://msdn2.microsoft.com/en-us/library/system.componentmodel.design.serialization.typecodedomserializer.deserialize.aspx
		//
		public virtual object Deserialize (IDesignerSerializationManager manager, CodeTypeDeclaration declaration)
		{
			throw new NotImplementedException ();
		}

		protected virtual CodeMemberMethod GetInitializeMethod (IDesignerSerializationManager manager, CodeTypeDeclaration declaration, object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (declaration == null)
				throw new ArgumentNullException ("declaration");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			return new CodeConstructor ();
		}

		protected virtual CodeMemberMethod[] GetInitializeMethods (IDesignerSerializationManager manager, CodeTypeDeclaration declaration)
		{
			if (manager == null)
				throw new ArgumentNullException ("manager");
			if (declaration == null)
				throw new ArgumentNullException ("declaration");

			return (new CodeMemberMethod[] { new CodeConstructor () });
		}
	}
}
#endif
