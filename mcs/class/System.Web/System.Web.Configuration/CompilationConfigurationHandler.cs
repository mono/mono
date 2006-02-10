//
// System.Web.Configuration.CompilationConfigurationHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

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
#if !NET_2_0
using System;
using System.Collections;
using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	class CompilationConfigurationHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			CompilationConfiguration config = new CompilationConfiguration (parent);

			string tmp = AttValue ("tempDirectory", section, true);
			if (tmp != null && tmp != "")
				config.TempDirectory = tmp;
			config.DefaultLanguage = AttValue ("defaultLanguage", section);
			if (config.DefaultLanguage == null)
				config.DefaultLanguage = "c#";

			config.Debug = AttBoolValue ("debug", section, false);
			config.Batch = AttBoolValue ("batch", section, false);
			config.Explicit = AttBoolValue ("explicit", section, true);
			config.Strict = AttBoolValue ("strict", section, false);
			config.BatchTimeout = AttUIntValue ("batchTimeout", section, 0);
			config.MaxBatchSize = AttUIntValue ("maxBatchSize", section, 0);
			config.MaxBatchFileSize = AttUIntValue ("maxBatchFileSize", section, 0);
			config.NumRecompilesBeforeAppRestart =
					AttUIntValue ("numRecompilesBeforeAppRestart", section, 15);

			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute.", section);

			XmlNodeList authNodes = section.ChildNodes;
			foreach (XmlNode child in authNodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype != XmlNodeType.Element)
					continue;
				
				if (child.Name == "compilers") {
					ReadCompilers (child.ChildNodes, config);
					continue;
				}

				if (child.Name == "assemblies") {
					ReadAssemblies (child.ChildNodes, config);
					continue;
				}

				ThrowException ("Unexpected element", child);
			}

			return config;
		}

		static void ReadCompilers (XmlNodeList nodes, CompilationConfiguration config)
		{
			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype != XmlNodeType.Element)
					continue;

				if (child.Name != "compiler")
					ThrowException ("Unexpected element", child);

				Compiler compiler = new Compiler ();
				compiler.Language  = AttValue ("language", child);
				compiler.Extension  = AttValue ("extension", child);
				compiler.Type = AttValue ("type", child);
				compiler.CompilerOptions = AttValue ("compilerOptions", child, true, true);
				compiler.WarningLevel = AttUIntValue ("warningLevel", child, 0);
#if NET_2_0
				config.Compilers.Add (compiler);
#else
				config.Compilers [compiler.Language] = compiler;
#endif
			}
		}

		static void ReadAssemblies (XmlNodeList nodes, CompilationConfiguration config)
		{
			ArrayList assemblies = config.Assemblies;

			foreach (XmlNode child in nodes) {
				XmlNodeType ntype = child.NodeType;
				if (ntype != XmlNodeType.Element)
					continue;

				if (child.Name == "clear") {
					assemblies.Clear ();
					config.AssembliesInBin = false;
					continue;
				}

				string aname = AttValue ("assembly", child);
				if (child.Name == "add") {
					if (aname == "*") {
						config.AssembliesInBin = true;
						continue;
					}

					if (!assemblies.Contains (aname))
						assemblies.Add (aname);

					continue;
				}

				if (child.Name == "remove") {
					if (aname == "*") {
						config.AssembliesInBin = false;
						continue;
					}
					assemblies.Remove (aname);
					continue;
				}

				ThrowException ("Unexpected element " + child.Name, child);
			}
		}

		static string AttValue (string name, XmlNode node, bool optional)
		{
			return AttValue (name, node, optional, false);
		}
		
		static string AttValue (string name, XmlNode node, bool optional, bool allowEmpty)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, optional, allowEmpty);
		}

		static bool AttBoolValue (string name, XmlNode node, bool _default)
		{
			string v = AttValue (name, node, true);
			if (v == null)
				return _default;

			bool result = (v == "true");
			if (!result && v != "false")
				ThrowException ("Invalid boolean value in " + name, node);

			return result;
		}

		static int AttUIntValue (string name, XmlNode node, int _default)
		{
			string v = AttValue (name, node, true);
			if (v == null)
				return _default;

			int result = 0;
			try {
				result = (int) UInt32.Parse (v);
			} catch {
				ThrowException ("Invalid number in " + name, node);
			}

			return result;
		}

		static string AttValue (string name, XmlNode node)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
	}
}
#endif

