//
// System.ComponentModel.Design.Serialization.RootCodeDomSerializer
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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;

using System.CodeDom;

namespace System.ComponentModel.Design.Serialization
{
	internal class RootCodeDomSerializer : CodeDomSerializer
	{

		internal class CodeMap
		{

			private string _className;
			private Type _classType;
			private List<CodeMemberField> _fields;
			private CodeStatementCollection _preInit;
			private CodeStatementCollection _init;
			private CodeStatementCollection _postInit;

			public CodeMap (Type classType, string className)
			{
				if (classType == null)
					throw new ArgumentNullException ("classType");
				if (className == null)
					throw new ArgumentNullException ("className");

				_classType = classType;
				_className = className;
				_fields = new List<CodeMemberField> ();
				_preInit = new CodeStatementCollection ();
				_init = new CodeStatementCollection ();
				_init = new CodeStatementCollection ();
				_postInit = new CodeStatementCollection ();
			}

			public void AddField (CodeMemberField field)
			{
				_fields.Add (field);
			}

			public void AddPreInitStatement (CodeStatement statement)
			{
				_preInit.Add (statement);
			}

			public void AddInitStatement (CodeStatement statement)
			{
				_init.Add (statement);
			}

			public void AddInitStatements (CodeStatementCollection statements)
			{
				_init.AddRange (statements);
			}

			public void AddPostInitStatement (CodeStatement statement)
			{
				_postInit.Add (statement);
			}

			/*
				class Type : BaseType
				{
					#region Windows Form Designer generated code

					private void InitializeComponent ()
					{
						preInit;
						init;
						postInit;
					}

					private field1;
					private field2;

					#endregion
				}
            */

			public CodeTypeDeclaration GenerateClass ()
			{
				CodeTypeDeclaration clas = new CodeTypeDeclaration (_className);
				clas.BaseTypes.Add (_classType);

				clas.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Windows Form Designer generated code"));

				CodeMemberMethod initialize = new CodeMemberMethod ();
				initialize.Name = "InitializeComponent";
				initialize.ReturnType = new CodeTypeReference (typeof (void));
				initialize.Attributes = MemberAttributes.Private;

				initialize.Statements.AddRange (_preInit);
				initialize.Statements.AddRange (_init);
				initialize.Statements.AddRange (_postInit);

				clas.Members.Add (initialize);

				foreach (CodeMemberField field in _fields)
					clas.Members.Add (field);

				clas.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, null));

				return clas;
			}

			public void Clear ()
			{
				_preInit.Clear ();
				_init.Clear ();
				_postInit.Clear ();
				_fields.Clear ();
			}
		}


		private CodeMap _codeMap;

		public RootCodeDomSerializer ()
		{
		}

		public override object Serialize (IDesignerSerializationManager manager, object value)
		{
			if (manager == null)
				throw new ArgumentNullException ("manager");
			if (value == null)
				throw new ArgumentNullException ("value");

			if (_codeMap == null)
				_codeMap = new CodeMap (value.GetType (), manager.GetName (value));
			_codeMap.Clear ();

			RootContext rootContext = new RootContext (new CodeThisReferenceExpression (), value);
			manager.Context.Push (rootContext);

			this.SerializeComponents (manager, ((IComponent) value).Site.Container.Components, (IComponent) value);

			// Serialize root component
			// 
			CodeStatementCollection statements = new CodeStatementCollection ();
			statements.Add (new CodeCommentStatement (String.Empty));
			statements.Add (new CodeCommentStatement (manager.GetName (value)));
			statements.Add (new CodeCommentStatement (String.Empty));
			// Note that during the serialization process below ComponentCodeDomSerializer
			// will be invoked to serialize the rootcomponent during expression serialization.
			// It will check for RootContext and return that.
			base.SerializeProperties (manager, statements, value, new Attribute[0]);
			base.SerializeEvents (manager, statements, value, new Attribute[0]);
			_codeMap.AddInitStatements (statements);

			manager.Context.Pop ();
			return _codeMap.GenerateClass ();
		}

		private void SerializeComponents (IDesignerSerializationManager manager, ICollection components, IComponent rootComponent)
		{
			foreach (IComponent component in components) {
				if (!Object.ReferenceEquals (component, rootComponent)) {
					manager.Context.Push (new ExpressionContext (null, null, rootComponent, component));
					SerializeComponent (manager, component);
					manager.Context.Pop ();
				}
			}
		}

		private void SerializeComponent (IDesignerSerializationManager manager, IComponent component)
		{
			CodeDomSerializer serializer = base.GetSerializer (manager, component) as CodeDomSerializer; // ComponentCodeDomSerializer
			if (serializer != null) {
				this.Code.AddField (new CodeMemberField (component.GetType (), manager.GetName (component)));

				CodeStatementCollection statements = (CodeStatementCollection) serializer.Serialize (manager, component);

				CodeStatement ctorStatement = ExtractCtorStatement (manager, statements, component);
				if (ctorStatement != null)
					Code.AddPreInitStatement (ctorStatement);
				Code.AddInitStatements (statements);
			}
		}

		internal CodeMap Code {
			get { return _codeMap; }
		}

		// Used to remove the ctor from the statement colletion in order for the ctor statement to be moved.
		//
		private CodeStatement ExtractCtorStatement (IDesignerSerializationManager manager, CodeStatementCollection statements, 
													object component)
		{
			CodeStatement result = null;
			CodeAssignStatement assignment = null;
			CodeObjectCreateExpression ctor = null;
			int toRemove = -1;

			for (int i=0; i < statements.Count; i++) {
				assignment = statements[i] as CodeAssignStatement;
				if (assignment != null) {
					ctor = assignment.Right as CodeObjectCreateExpression;
					if (ctor != null && manager.GetType (ctor.CreateType.BaseType) == component.GetType ()) {
						result = assignment;
						toRemove = i;
					}
				}
			}

			if (toRemove != -1)
				statements.RemoveAt (toRemove);

			return result;
		}

		public override object Deserialize (IDesignerSerializationManager manager, object codeObject)
		{
			CodeTypeDeclaration declaration = (CodeTypeDeclaration) codeObject;
			Type rootType = manager.GetType (declaration.BaseTypes[0].BaseType);
			object root = manager.CreateInstance (rootType, null, declaration.Name, true);

			CodeMemberMethod initComponentMethod = GetInitializeMethod (declaration);
			if (initComponentMethod == null)
				throw new InvalidOperationException ("InitializeComponent method is missing in: " + declaration.Name);

			foreach (CodeStatement statement in initComponentMethod.Statements)
				base.DeserializeStatement (manager, statement);

			return root;
		}

		private CodeMemberMethod GetInitializeMethod (CodeTypeDeclaration declaration)
		{
			CodeMemberMethod method = null;
			foreach (CodeTypeMember member in declaration.Members) {
				method = member as CodeMemberMethod;
				if (method != null && method.Name == "InitializeComponent")
					break;
			}
			return method;
		}
	}
}
#endif
