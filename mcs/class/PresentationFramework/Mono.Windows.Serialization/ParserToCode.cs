//
// ParserToCode.cs
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
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Collections;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Windows.Serialization;
using System.Windows;

namespace Mono.Windows.Serialization {
	public class ParserToCode {
		private string result;

		private static readonly Type xamlParserType = typeof(XamlParser);

		public static string Parse(XmlTextReader reader, ICodeGenerator generator,  bool isPartial)
		{
			ParserToCode cw = new ParserToCode(reader, generator, isPartial);
			return cw.result;
		}

		private ParserToCode(XmlTextReader reader, ICodeGenerator generator, bool isPartial)
		{
			Consumer consumer = new Consumer(generator, isPartial);
			consumer.crunch(reader);
			result = ((StringWriter)consumer.writer).ToString();
		}

		private class Consumer : ParserConsumerBase {
			public TextWriter writer;
			ICodeGenerator generator;
			bool isPartial;
		
			ArrayList objects = new ArrayList();
			Hashtable nameClashes = new Hashtable();
			int tempIndex = 0;
			
			Hashtable keys = new Hashtable();

			CodeCompileUnit code;
			CodeTypeDeclaration type;
			CodeConstructor constructor;

			public Consumer(ICodeGenerator generator, bool isPartial)
			{
				this.generator = generator;
				this.isPartial = isPartial;
				this.writer = new StringWriter();
			}
			// pushes: a CodeVariableReferenceExpression to the present
			// 	instance
			public override void CreateTopLevel(Type parent, string className)
			{
				debug();
				code = new CodeCompileUnit();
				push(code);
				if (className == null) {
					className = "derived" + parent.Name;
				}
				int endNamespaceName = className.LastIndexOf(".");
				string clrNamespace;
				if (endNamespaceName < 0) {
					clrNamespace = "DefaultNamespace";
				} else {
					clrNamespace = className.Substring(0,
							endNamespaceName);
					className = className.Substring(endNamespaceName+1);
				}
				CodeNamespace ns = new CodeNamespace(clrNamespace);
				((CodeCompileUnit)objects[0]).Namespaces.Add(ns);

				type = new CodeTypeDeclaration(className);
				if (isPartial) {
	#if NET_2_0
					type.IsPartial = isPartial;
	#else
					throw new Exception("Cannot create partial class");
	#endif
				}
				type.BaseTypes.Add(new CodeTypeReference(parent));
				constructor = new CodeConstructor();
				type.Members.Add(constructor);
				ns.Types.Add(type);
				
				push(new CodeThisReferenceExpression());
			}

			// bottom of stack holds CodeVariableReferenceExpression
			// pushes a reference to the new current type
			public override void CreateObject(Type type, string varName, string key)
			{
				debug();
				bool isDefaultName;
				if (varName == null) {
					isDefaultName = true;
					varName = Char.ToLower(type.Name[0]) + type.Name.Substring(1);
					// make sure something sensible happens when class
					// names start with a lowercase letter
					if (varName == type.Name)
						varName = "_" + varName;
				} else {
					isDefaultName = false;
				}
	
				if (isDefaultName) {
					if (!nameClashes.ContainsKey(varName))
						nameClashes[varName] = 0;

					nameClashes[varName] = 1 + (int)nameClashes[varName];
					varName += (int)nameClashes[varName];
				}

				if (key != null)
					keys[key] = varName;

				if (isDefaultName) {
					CodeVariableDeclarationStatement declaration = 
							new CodeVariableDeclarationStatement(type, 
									varName,
									new CodeObjectCreateExpression(type));
					constructor.Statements.Add(declaration);
				} else {
					CodeMemberField declaration = new CodeMemberField(type, varName);
					declaration.InitExpression = new CodeObjectCreateExpression(type);
					this.type.Members.Add(declaration);
				}
				CodeVariableReferenceExpression varRef = new CodeVariableReferenceExpression(varName);
				CodeMethodInvokeExpression addChild = new CodeMethodInvokeExpression(
						(CodeExpression)peek(),
						"AddChild",
						varRef);
				constructor.Statements.Add(addChild);
				push(varRef);
			}

			// top of stack is a reference to an object
			// pushes a reference to the property
			public override void CreateProperty(PropertyInfo property)
			{
				debug();
				CodePropertyReferenceExpression prop = new CodePropertyReferenceExpression(
						(CodeExpression)peek(),
						property.Name);
				push(prop);
			}

			// top of stack is a reference to an object
			// pushes a reference to the event
			public override void CreateEvent(EventInfo evt)
			{
				debug();
				CodeEventReferenceExpression expr = new CodeEventReferenceExpression(
						(CodeExpression)peek(),
						evt.Name);
				push(expr);
			}

			// top of stack is a reference to an object
			// pushes a reference to the expression that
			// will set the property and a reference to
			// the name of the temp variable to hold the
			// property
			public override void CreateDependencyProperty(Type attachedTo, string propertyName, Type propertyType)
			{
				debug();
				string varName = "temp";
				varName += tempIndex;
				tempIndex += 1;
				CodeVariableDeclarationStatement decl = new CodeVariableDeclarationStatement(propertyType, varName);
				constructor.Statements.Add(decl);


				CodeMethodInvokeExpression call = new CodeMethodInvokeExpression(
						new CodeTypeReferenceExpression(attachedTo),
						"Set" + propertyName,
						(CodeExpression)peek(),
						new CodeVariableReferenceExpression(varName));

				push(call);
				push(new CodeVariableReferenceExpression(varName));
			}

			// pops 2 items: the name of the property, and the object to attach to
			public override void EndDependencyProperty()
			{
				debug();
				pop(); // pop the variable name - we don't need it since it's already baked into the call
				CodeExpression call = (CodeExpression)pop();
				constructor.Statements.Add(call);
			}

			// top of stack must be an object reference
			public override void CreateObjectText(string text)
			{
				debug();
				CodeVariableReferenceExpression var = (CodeVariableReferenceExpression)peek();
				CodeMethodInvokeExpression call = new CodeMethodInvokeExpression(
						var,
						"AddText",
						new CodePrimitiveExpression(text));
				constructor.Statements.Add(call);
			}

			// top of stack is reference to an event
			public override void CreateEventDelegate(string functionName, Type eventDelegateType)
			{
				debug();
				CodeExpression expr = buildDelegate(eventDelegateType, functionName);
				CodeAttachEventStatement attach = new CodeAttachEventStatement(
						(CodeEventReferenceExpression)peek(),
						expr);
				constructor.Statements.Add(attach);

			}
			// top of stack is reference to a property
			public override void CreatePropertyDelegate(string functionName, Type propertyType)
			{
				debug();
				CodeExpression expr = buildDelegate(propertyType, functionName);
				CodeAssignStatement assignment = new CodeAssignStatement(
						(CodeExpression)peek(),
						expr);
				constructor.Statements.Add(assignment);
			}
			private CodeExpression buildDelegate(Type propertyType, string functionName)
			{
				return new CodeObjectCreateExpression(
						propertyType,
						new CodeMethodReferenceExpression(
								new CodeThisReferenceExpression(),
								functionName));

			}

			private CodeExpression fetchConverter(Type propertyType)
			{
				return new CodeMethodInvokeExpression(
						new CodeMethodReferenceExpression(
								new CodeTypeReferenceExpression(typeof(System.ComponentModel.TypeDescriptor)),
								"GetConverter"),
						new CodeTypeOfExpression(propertyType));
			}

			// top of stack is reference to a property
			public override void CreatePropertyText(string text, Type propertyType)
			{
				debug();
				CreateDependencyPropertyText(text, propertyType);
			}
			// top of stack is reference to an attached property
			public override void CreateDependencyPropertyText(string text, Type propertyType)
			{
				debug();
				CodeExpression expr = new CodePrimitiveExpression(text);
				if (propertyType != typeof(string)) {
					expr = new CodeCastExpression(
							new CodeTypeReference(propertyType),
							new CodeMethodInvokeExpression(
									fetchConverter(propertyType),
									"ConvertFromString",
									expr));
				}
				CodeAssignStatement assignment = new CodeAssignStatement(
						(CodeExpression)peek(),
						expr);
				
				constructor.Statements.Add(assignment);
			}


			public override void CreatePropertyReference(string key)
			{
				CodeAssignStatement assignment = new CodeAssignStatement(
						(CodeExpression)peek(),
						new CodeVariableReferenceExpression((string)keys[key]));
				
				constructor.Statements.Add(assignment);
			}
			public override void CreateDependencyPropertyReference(string key)
			{
				CreatePropertyReference(key);
			}
			
			public override void CreateDependencyPropertyObject(Type type, string varName, string key)
			{
				CreatePropertyObject(type, varName, key);
			}

			public override void CreatePropertyObject(Type type, string varName, string key)
			{
				debug();
				bool isDefaultName;
				if (varName == null) {
					isDefaultName = true;
					varName = Char.ToLower(type.Name[0]) + type.Name.Substring(1);
					// make sure something sensible happens when class
					// names start with a lowercase letter
					if (varName == type.Name)
						varName = "_" + varName;
				} else {
					isDefaultName = false;
				}

				if (isDefaultName) {
					if (!nameClashes.ContainsKey(varName))
						nameClashes[varName] = 0;
					nameClashes[varName] = 1 + (int)nameClashes[varName];
					varName += (int)nameClashes[varName];
				}

				if (key != null)
					keys[key] = varName;

				if (isDefaultName) {
					CodeVariableDeclarationStatement declaration = 
							new CodeVariableDeclarationStatement(type, 
									varName,
									new CodeObjectCreateExpression(type));
					constructor.Statements.Add(declaration);
				} else {
					CodeMemberField declaration = new CodeMemberField(type, varName);
					declaration.InitExpression = new CodeObjectCreateExpression(type);
					this.type.Members.Add(declaration);
				}
				CodeVariableReferenceExpression varRef = new CodeVariableReferenceExpression(varName);

				push(type);
				push(varRef);
			
			}

			public override void EndDependencyPropertyObject(Type destType)
			{
				EndPropertyObject(destType);
			}
			public override void EndPropertyObject(Type destType)
			{
				debug();
				CodeExpression varRef = (CodeExpression)pop();
				Type sourceType = (Type)pop();

				Debug.WriteLine("ParserToCode: " + destType + "->" + sourceType);

				
				CodeExpression expr;
				if (sourceType == destType || sourceType.IsSubclassOf(destType))
					expr = varRef;
				else
					expr = new CodeCastExpression(
							new CodeTypeReference(destType),
							new CodeMethodInvokeExpression(
									fetchConverter(sourceType),
									"ConvertTo",
									varRef,
									new CodeTypeOfExpression(destType)));
				CodeAssignStatement assignment = new CodeAssignStatement(
						(CodeExpression)peek(),
						expr);
				constructor.Statements.Add(assignment);
			}
			
			public override void EndObject()
			{
				debug();
				pop();
			}

			public override void EndProperty()
			{
				debug();
				pop();
			}
			
			public override void EndEvent()
			{
				debug();
				pop();
			}

			public override void Finish()
			{
				debug();
				generator.GenerateCodeFromCompileUnit(code, writer, null);
				writer.Close();
			}

			public override void CreateCode(string code)
			{
				debug();
				type.Members.Add(new CodeSnippetTypeMember(code));
			}

			private void debug()
			{
				Debug.WriteLine("ParserToCode: " + new System.Diagnostics.StackTrace());
			}
			
			private object pop()
			{
				object v = objects[objects.Count - 1];
				objects.RemoveAt(objects.Count - 1);
				Debug.WriteLine("ParserToCode: POPPING");
				return v;
			}
			private void push(object v)
			{
				Debug.WriteLine("ParserToCode: PUSHING " + v);
				objects.Add(v);
			}
			private object peek()
			{
				return peek(0);
			}
			private object peek(int i)
			{
				return objects[objects.Count - 1 - i];
			}
		}
	}
}
