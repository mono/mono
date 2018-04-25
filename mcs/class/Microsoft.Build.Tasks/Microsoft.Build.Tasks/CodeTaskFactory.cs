//
// CodeTaskFactory.cs
//
// Author:
//   Atsushi Enomoto <atsushi@xamarin.com>
//
// Copyright (C) 2014 Xamarin Inc.
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
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Microsoft.Build.BuildEngine;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using System.Collections.Specialized;

namespace Microsoft.Build.Tasks
{
	public class CodeTaskFactory : ITaskFactory
	{
		public CodeTaskFactory ()
		{
		}

		#region ITaskFactory implementation
		public void CleanupTask (ITask task)
		{
		}
		public ITask CreateTask (IBuildEngine loggingHost)
		{
			return CreateTask (loggingHost, null);
		}
		
		ITask CreateTask (IBuildEngine taskFactoryLoggingHost, IDictionary<string, string> taskIdentityParameters)
		{
			if (assembly == null)
				return null;
			return (ITask) Activator.CreateInstance (assembly.GetType (task_name));
		}

		public TaskPropertyInfo [] GetTaskParameters ()
		{
			return parameter_group != null ? parameter_group.Values.ToArray () : new TaskPropertyInfo [0];
		}
		public bool Initialize (string taskName, IDictionary<string, TaskPropertyInfo> taskParameters, string taskElementContents, IBuildEngine taskFactoryLoggingHost)
		{
			return Initialize (taskName, null, taskParameters, taskElementContents, taskFactoryLoggingHost);
		}
		bool Initialize (string taskName, IDictionary<string, string> factoryIdentityParameters, IDictionary<string, TaskPropertyInfo> parameterGroup, string taskBody, IBuildEngine taskFactoryLoggingHost)
		{
			task_name = taskName;
			if (parameterGroup != null)
				parameter_group = new Dictionary<string, TaskPropertyInfo> (parameterGroup);
			
			List<string> references = new List<string> ();
			List<string> namespace_uses = new List<string> ();
			namespace_uses.Add ("Microsoft.Build.Framework");
			string type = null, language = null, code = null;

			var xml = XmlReader.Create (new StringReader (taskBody), new XmlReaderSettings () { ConformanceLevel = ConformanceLevel.Fragment });
			for (xml.MoveToContent (); !xml.EOF; xml.MoveToContent ()) {
				switch (xml.NodeType) {
				case XmlNodeType.Element:
					switch (xml.LocalName) {
					case "Reference":
						references.Add (xml.GetAttribute ("Include"));
						xml.Skip ();
						break;
					case "Using":
						namespace_uses.Add (xml.GetAttribute ("Namespace"));
						xml.Skip ();
						break;
					case "Code":
						// MSB3757: Multiple Code elements have been found, this is not allowed.
						if (code != null)
							throw new InvalidProjectFileException (null, "Multiple Code elements are not allowed", "MSB", "3757", null);
						type = xml.GetAttribute ("Type");
						language = xml.GetAttribute ("Language");
						code = xml.ReadElementContentAsString ();
						break;
					}
					break;
				default:
					xml.Skip ();
					break;
				}
			}

			if (language != "cs" && language != "vb")
				throw new InvalidProjectFileException (null, string.Format ("{0} is not supported language for inline task", language), "MSB", "4175", null);

			CodeCompileUnit ccu;

			if (type == "Class") {  // 'code' contains the whole class that implements the task
				ccu = new CodeSnippetCompileUnit (code);
			}
			else {  // 'code' contains parts of the class that implements the task
				ccu = new CodeCompileUnit ();
				var nsp = new CodeNamespace ();
				nsp.Imports.AddRange (namespace_uses.Select (x => new CodeNamespaceImport (x)).ToArray ());
				ccu.Namespaces.Add (nsp);

				var taskClass = new CodeTypeDeclaration {
					IsClass = true,
					Name = taskName,
					TypeAttributes = TypeAttributes.Public
				};

				var parameters = new List<CodeMemberProperty> ();
				var parametersBackingFields = new List<CodeMemberField> ();

				// add a public property + backing field for each parameter
				foreach (var param in parameter_group) {
					var prop = new CodeMemberProperty {
						Attributes = MemberAttributes.Public | MemberAttributes.Final,
						Name = param.Value.Name,
						Type = new CodeTypeReference (param.Value.PropertyType)
					};

					var propBf = new CodeMemberField {
						Attributes = MemberAttributes.Private,
						Name = "_" + prop.Name,
						Type = prop.Type
					};

					// add getter and setter to the property
					prop.GetStatements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (new CodeThisReferenceExpression (), propBf.Name)));
					prop.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (new CodeThisReferenceExpression (), propBf.Name), new CodePropertySetValueReferenceExpression ()));

					parameters.Add (prop);
					parametersBackingFields.Add (propBf);
				}

				taskClass.Members.AddRange (parameters.ToArray ());
				taskClass.Members.AddRange (parametersBackingFields.ToArray ());
				taskClass.BaseTypes.Add ("Microsoft.Build.Utilities.Task");  // The documentation says "ITask", but the very first example shows "Log" which is not in ITask! It is likely that the generated code uses Task or TaskExtension.

				if (type == "Method") {  // 'code' contains the 'Execute' method directly
					taskClass.Members.Add (new CodeSnippetTypeMember (code));
				}
				else if (type == "Fragment") {  // 'code' contains the body of the 'Execute' method
					var method = new CodeMemberMethod {
						Attributes = MemberAttributes.Public | MemberAttributes.Override,
						Name = "Execute",
						ReturnType = new CodeTypeReference (typeof (bool))
					};

					// add the code and a 'return true' at the end of the method
					method.Statements.Add (new CodeSnippetStatement (code));
					method.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (true)));

					taskClass.Members.Add (method);
				}
				else {
					throw new ArgumentException ("Invalid type: " + type);
				}

				nsp.Types.Add (taskClass);
			}

			var cscParams = new CompilerParameters ();
			cscParams.ReferencedAssemblies.Add ("Microsoft.Build.Framework.dll");
			cscParams.ReferencedAssemblies.Add ("Microsoft.Build.Utilities.v4.0.dll"); // since we use Task, it depends on this dll.
			cscParams.ReferencedAssemblies.AddRange (GetReferences (references, taskFactoryLoggingHost));
			cscParams.GenerateInMemory = true;
			var results = CodeDomProvider.CreateProvider (language).CompileAssemblyFromDom (cscParams, ccu);
			var errors = new CompilerError [results.Errors.Count];
			results.Errors.CopyTo (errors, 0);
			if (errors.Any (e => !e.IsWarning)) {
				string msg = string.Format ("Invalid '{0}' source code of '{1}' type: {2}", language, type, string.Join (" ", errors.Where (e => !e.IsWarning).Select (e => e.ToString ())));
				throw new InvalidProjectFileException (null, msg, "MSB", "3758", null);
			}
			assembly = results.CompiledAssembly;
			return true;
		}

		static string[] GetReferences (List<string> references, IBuildEngine log)
		{
			var res = new List<string> ();
			foreach (var r in references) {
				if (File.Exists (r)) {
					res.Add (r);
					continue;
				}

				Assembly assembly = null;

				try {
					if (!r.EndsWith (".dll", StringComparison.OrdinalIgnoreCase) || !r.EndsWith (".exe", StringComparison.OrdinalIgnoreCase)) {
						assembly = Assembly.LoadWithPartialName (r);
					}

					if (assembly != null) {
						res.Add (assembly.Location);
						continue;
					}
				} catch {
				}

				log.LogErrorEvent (new BuildErrorEventArgs ("", "", "", 0, 0, 0, 0, "Assembly reference {r} could not be resolved", "", ""));
			}

			return res.ToArray ();
		}

		public string FactoryName {
			get { return "Code Task Factory"; }
		}
		public Type TaskType {
			get { return null; }
		}

		string task_name;
		Assembly assembly;
		Dictionary<string, TaskPropertyInfo> parameter_group;

		#endregion
	}
}

