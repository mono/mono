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
using System.Reflection;
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
		CodeTypeDeclaration type;
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
		public void CreateTopLevel(Type parent, string className)
		{
			int endNamespaceName = className.LastIndexOf(".");
			string clrNamespace;
			if (endNamespaceName < 0)
				clrNamespace = "DefaultNamespace";
			else
				clrNamespace = className.Substring(0,
						endNamespaceName);
			CodeNamespace ns = new CodeNamespace(clrNamespace);
			((CodeCompileUnit)objects[0]).Namespaces.Add(ns);

			type = new CodeTypeDeclaration(className);
			type.BaseTypes.Add(new CodeTypeReference(parent));
			constructor = new CodeConstructor();
			type.Members.Add(constructor);
			ns.Types.Add(type);
			
			objects.Add(new CodeThisReferenceExpression());
		}

		// bottom of stack holds CodeVariableReferenceExpression
		// pushes a reference to the new current type
		public void CreateObject(Type type)
		{
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
					new CodeVariableDeclarationStatement(type, 
							varName,
							new CodeObjectCreateExpression(type));
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
		public void CreateProperty(PropertyInfo property)
		{
			CodePropertyReferenceExpression prop = new CodePropertyReferenceExpression(
					(CodeExpression)objects[objects.Count - 1],
					property.Name);
			objects.Add(prop);
		}

		// top of stack is a reference to an object
		// pushes a reference to the event
		public void CreateEvent(EventInfo evt)
		{
			CodeEventReferenceExpression expr = new CodeEventReferenceExpression(
					(CodeExpression)objects[objects.Count - 1],
					evt.Name);
			objects.Add(expr);
		}

		// top of stack is a reference to an object
		// pushes a reference to the expression that
		// will set the property and a reference to
		// the name of the temp variable to hold the
		// property
		public void CreateDependencyProperty(Type attachedTo, string propertyName, Type propertyType)
		{
			string varName = "temp";
			varName += tempIndex;
			tempIndex += 1;
			CodeVariableDeclarationStatement decl = new CodeVariableDeclarationStatement(propertyType, varName);
			constructor.Statements.Add(decl);


			CodeMethodInvokeExpression call = new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(attachedTo),
					"Set" + propertyName,
					(CodeExpression)objects[objects.Count - 1],
					new CodeVariableReferenceExpression(varName));

			objects.Add(call);
			objects.Add(new CodeVariableReferenceExpression(varName));
		}

		// pops 2 items: the name of the property, and the object to attach to
		public void EndDependencyProperty()
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

		// top of stack is reference to an event
		public void CreateEventDelegate(string functionName, Type eventDelegateType)
		{
			CodeExpression expr = new CodeObjectCreateExpression(
					eventDelegateType,
					new CodeMethodReferenceExpression(
							new CodeThisReferenceExpression(),
							functionName));
			CodeAttachEventStatement attach = new CodeAttachEventStatement(
					(CodeEventReferenceExpression)objects[objects.Count - 1],
					expr);
			constructor.Statements.Add(attach);

		}
		// top of stack is reference to a property
		public void CreatePropertyDelegate(string functionName, Type propertyType)
		{
			CodeExpression expr = new CodeObjectCreateExpression(
					propertyType,
					new CodeMethodReferenceExpression(
							new CodeThisReferenceExpression(),
							functionName));
			CodeAssignStatement assignment = new CodeAssignStatement(
					(CodeExpression)objects[objects.Count - 1],
					expr);
			constructor.Statements.Add(assignment);
		}
		// top of stack is reference to a property
		public void CreatePropertyText(string text, Type propertyType, Type converterType)
		{
			CreateDependencyPropertyText(text, propertyType, converterType);
		}
		// top of stack is reference to an attached property
		public void CreateDependencyPropertyText(string text, Type propertyType, Type converterType)
		{
			CodeExpression expr = new CodePrimitiveExpression(text);
			if (converterType != null) {
				expr = new CodeCastExpression(
						new CodeTypeReference(propertyType),
						new CodeMethodInvokeExpression(
								new CodeObjectCreateExpression(converterType),
								"ConvertFromString",
								expr));
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
		
		public void EndEvent()
		{
			objects.RemoveAt(objects.Count - 1);
		}

		public void Finish()
		{
			ICodeGenerator generator = (new Microsoft.CSharp.CSharpCodeProvider()).CreateGenerator();
			generator.GenerateCodeFromCompileUnit(code, writer, null);
			writer.Close();
		}

		public void CreateCode(string code)
		{
			type.Members.Add(new CodeSnippetTypeMember(code));
		}
	}
}
