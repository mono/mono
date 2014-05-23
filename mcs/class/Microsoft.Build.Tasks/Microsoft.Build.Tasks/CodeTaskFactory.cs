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
#if NET_4_0
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
	public class CodeTaskFactory : ITaskFactory2
	{
		public CodeTaskFactory ()
		{
		}

		#region ITaskFactory implementation
		public void CleanupTask (ITask task)
		{
		}
		public ITask CreateTask (IBuildEngine taskFactoryLoggingHost)
		{
			return CreateTask (taskFactoryLoggingHost, null);
		}
		public ITask CreateTask (IBuildEngine taskFactoryLoggingHost, IDictionary<string, string> taskIdentityParameters)
		{
			if (assembly == null)
				return null;
			return (ITask) Activator.CreateInstance (assembly.GetType (task_name));
		}

		public TaskPropertyInfo [] GetTaskParameters ()
		{
			return parameter_group != null ? parameter_group.Values.ToArray () : new TaskPropertyInfo [0];
		}
		public bool Initialize (string taskName, IDictionary<string, TaskPropertyInfo> parameterGroup, string taskBody, IBuildEngine taskFactoryLoggingHost)
		{
			return Initialize (taskName, null, parameterGroup, taskBody, taskFactoryLoggingHost);
		}
		public bool Initialize (string taskName, IDictionary<string, string> factoryIdentityParameters, IDictionary<string, TaskPropertyInfo> parameterGroup, string taskBody, IBuildEngine taskFactoryLoggingHost)
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
			if (language == "vb")
				throw new NotImplementedException (string.Format ("{0} is not supported language for inline task", language));
			if (language != "cs")
				throw new InvalidProjectFileException (null, string.Format ("{0} is not supported language for inline task", language), "MSB", "4175", null);
			string gen = null;
			// The documentation says "ITask", but the very first example shows "Log" which is not in ITask! It is likely that the generated code uses Task or TaskExtension.
			string classTemplate = @"public class " + taskName + " : Microsoft.Build.Utilities.Task { @@EXECUTE@@ }";
			foreach (var ns in namespace_uses)
				gen += "using " + ns + ";\n";
			switch (type) {
			case "Class":
				gen += code;
				break;
			case "Method":
				gen += classTemplate.Replace ("@@EXECUTE@@", code);
				break;
			case "Fragment":
				gen += classTemplate.Replace ("@@EXECUTE@@", "public override bool Execute () { " + code + " return true; }");
				break;
			}

			var cscParams = new CompilerParameters ();
			cscParams.ReferencedAssemblies.Add ("Microsoft.Build.Framework.dll");
			cscParams.ReferencedAssemblies.Add ("Microsoft.Build.Utilities.v4.0.dll"); // since we use Task, it depends on this dll.
			var results = new CSharpCodeProvider ().CompileAssemblyFromSource (cscParams, gen);
			var errors = new CompilerError [results.Errors.Count];
			results.Errors.CopyTo (errors, 0);
			if (errors.Any (e => !e.IsWarning)) {
				string msg = string.Format ("Invalid '{0}' source code of '{1}' type: {2}", language, type, string.Join (" ", errors.Where (e => !e.IsWarning).Select (e => e.ToString ())));
				throw new InvalidProjectFileException (null, msg, "MSB", "3758", null);
			}
			assembly = results.CompiledAssembly;
			return true;
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

#endif
