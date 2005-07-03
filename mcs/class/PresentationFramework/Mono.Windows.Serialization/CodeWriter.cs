//
// CodeWriter.cs
//
// Author:
//   Iain McCoy (iain@mccoy.id.au)
//
// (C) 2005 Iain McCoy
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
using System.IO;
using System.Collections;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace Mono.Windows.Serialization {
	public class CodeWriter : XamlWriter {
		TextWriter writer;
		ArrayList objects = new ArrayList();
		Hashtable nameClashes = new Hashtable();
		int tempIndex = 0;

		CodeCompileUnit code;
		CodeConstructor constructor;
		
		// pushes: the code writer
		public CodeWriter(TextWriter writer)
		{
			this.writer = writer;
			code = new CodeCompileUnit();
			objects.Add(code);
		}
	
		// pushes: a CodeVariableReferenceExpression to the present
		// 	instance
		public void CreateTopLevel(string parentName, string className)
		{
			Type parent = Type.GetType(parentName);
			int endNamespaceName = className.LastIndexOf(".");
			string clrNamespace;
			if (endNamespaceName < 0)
				clrNamespace = "DefaultNamespace";
			else
				clrNamespace = className.Substring(0,
						endNamespaceName);
			CodeNamespace ns = new CodeNamespace(clrNamespace);
			((CodeCompileUnit)objects[0]).Namespaces.Add(ns);

			CodeTypeDeclaration type = new CodeTypeDeclaration(className);
			type.BaseTypes.Add(new CodeTypeReference(parent));
			constructor = new CodeConstructor();
			type.Members.Add(constructor);
			ns.Types.Add(type);
			
			objects.Add(new CodeThisReferenceExpression());
		}

		// bottom of stack holds CodeVariableReferenceExpression
		// pushes a reference to the new current type
		public void CreateObject(string typeName)
		{
			Type type = Type.GetType(typeName);
			string varName = Char.ToLower(type.Name[0]) + type.Name.Substring(1);
			// make sure something sensible happens when class
			// names start with a lowercase letter
			if (varName == type.Name)
				varName = "_" + varName;

			if (!nameClashes.ContainsKey(varName))
				nameClashes[varName] = 0;
			else {
				nameClashes[varName] = 1 + (int)nameClashes[varName];
				varName += (int)nameClashes[varName];
			}

			CodeVariableDeclarationStatement declaration = 
					new CodeVariableDeclarationStatement(type, varName);
			CodeVariableReferenceExpression varRef = new CodeVariableReferenceExpression(varName);
			CodeMethodInvokeExpression addChild = new CodeMethodInvokeExpression(
					(CodeExpression)objects[objects.Count - 1],
					"AddChild",
					varRef);
			constructor.Statements.Add(declaration);
			constructor.Statements.Add(addChild);
			objects.Add(varRef);
		}

		// top of stack is a reference to an object
		// pushes a reference to the property
		public void CreateProperty(string propertyName)
		{
			CodePropertyReferenceExpression prop = new CodePropertyReferenceExpression(
					(CodeExpression)objects[objects.Count - 1],
					propertyName);
			objects.Add(prop);
		}

		// top of stack is a reference to an object
		// pushes the name of the instance to attach to, the name of 
		//   the property, and a reference to an object
		public void CreateAttachedProperty(string attachedTo, string propertyName, string typeName)
		{
			// need to:
			Type t = Type.GetType(typeName);

			string name = "temp";
			if (tempIndex != 0)
				name += tempIndex;
			CodeVariableDeclarationStatement decl = new CodeVariableDeclarationStatement(t, name);
			constructor.Statements.Add(decl);


			CodeMethodInvokeExpression call = new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression(attachedTo),
					"Set" + propertyName,
					(CodeExpression)objects[objects.Count - 1],
					new CodeVariableReferenceExpression(name));

			objects.Add(call);
			objects.Add(new CodeVariableReferenceExpression(name));
		}

		// pops 2 items: the name of the property, and the object to attach to
		public void EndAttachedProperty()
		{
			objects.RemoveAt(objects.Count - 1);
			CodeExpression call = (CodeExpression)(objects[objects.Count - 1]);
			objects.RemoveAt(objects.Count - 1);
			constructor.Statements.Add(call);
		}

		// top of stack must be an object reference
		public void CreateElementText(string text)
		{
			CodeVariableReferenceExpression var = (CodeVariableReferenceExpression)objects[objects.Count - 1];
			CodeMethodInvokeExpression call = new CodeMethodInvokeExpression(
					var,
					"AddText",
					new CodePrimitiveExpression(text));
			constructor.Statements.Add(call);
		}

		// top of stack is reference to a property
		public void CreatePropertyText(string text, string converter)
		{
			CreateAttachedPropertyText(text, converter);
		}
		public void CreateAttachedPropertyText(string text, string converter)
		{
			CodeExpression expr = new CodePrimitiveExpression(text);
			if (converter != null) {
				Type t = Type.GetType(converter);
				expr = new CodeMethodInvokeExpression(
						new CodeTypeReferenceExpression(t),
						"ConvertFromString",
						expr);
			}
			CodeAssignStatement assignment = new CodeAssignStatement(
					(CodeExpression)objects[objects.Count - 1],
					expr);
			
			constructor.Statements.Add(assignment);
		}
		
		public void EndObject()
		{
			objects.RemoveAt(objects.Count - 1);
		}

		public void EndProperty()
		{
			objects.RemoveAt(objects.Count - 1);
		}

		public void Finish()
		{
			ICodeGenerator generator = (new Microsoft.CSharp.CSharpCodeProvider()).CreateGenerator();
			generator.GenerateCodeFromCompileUnit(code, writer, null);
			writer.Close();
		}
	}
}
