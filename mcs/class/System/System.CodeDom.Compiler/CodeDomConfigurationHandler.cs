//
// System.Configuration.CodeDomConfigurationHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0 && XML_DEP
using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Xml;

namespace System.CodeDom.Compiler
{
	class CodeDomConfigurationHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			return new CompilationConfigurationHandler ().Create (parent, context, section);
		}
	}

	class CompilationConfigurationHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			CompilationConfiguration config = new CompilationConfiguration (parent);

			config.TempDirectory = AttValue ("tempDirectory", section, true);
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

				CompilerInfo compiler = new CompilerInfo ();
				compiler.Languages = AttValue ("language", child);
				compiler.Extensions = AttValue ("extension", child);
				compiler.TypeName = AttValue ("type", child);
				compiler.CompilerOptions = AttValue ("compilerOptions", child, true, true);
				compiler.WarningLevel = AttUIntValue ("warningLevel", child, 0);
				config.Compilers [compiler.Languages] = compiler;
			}
		}

		static string AttValue (string name, XmlNode node, bool optional)
		{
			return AttValue (name, node, optional, false);
		}
		
		static string AttValue (string name, XmlNode node, bool optional, bool allowEmpty)
		{
			return ExtractAttributeValue (name, node, optional, allowEmpty);
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
			return ExtractAttributeValue (name, node, true);
		}

		static string ShortAsmName (string long_name)
		{
			int i = long_name.IndexOf (',');
			if (i < 0)
				return long_name + ".dll";
			return long_name.Substring (0, i) + ".dll";
		}
		
		static void ThrowException (string message, XmlNode node)
		{
			ThrowException (message, node);
		}

		static internal string ExtractAttributeValue (string attKey, XmlNode node)
		{
			return ExtractAttributeValue (attKey, node, false);
		}
			
		static internal string ExtractAttributeValue (string attKey, XmlNode node, bool optional)
		{
			return ExtractAttributeValue (attKey, node, optional, false);
		}
		
		static internal string ExtractAttributeValue (string attKey, XmlNode node, bool optional,
							      bool allowEmpty)
		{
			if (node.Attributes == null) {
				if (optional)
					return null;

				ThrowException ("Required attribute not found: " + attKey, node);
			}

			XmlNode att = node.Attributes.RemoveNamedItem (attKey);
			if (att == null) {
				if (optional)
					return null;
				ThrowException ("Required attribute not found: " + attKey, node);
			}

			string value = att.Value;
			if (!allowEmpty && value == String.Empty) {
				string opt = optional ? "Optional" : "Required";
				ThrowException (opt + " attribute is empty: " + attKey, node);
			}

			return value;
		}
	}

	sealed class CompilationConfiguration
	{
		bool debug;
		bool batch;
		int batch_timeout;
		string default_language = "c#";
		bool _explicit = true;
		int max_batch_size = 30;
		int max_batch_file_size = 3000;
		int num_recompiles_before_app_restart = 15;
		bool strict;
		string temp_directory;
		CompilerCollection compilers;

		/* Only the config. handler should create instances of this. Use GetInstance (context) */
		public CompilationConfiguration (object p)
		{
			CompilationConfiguration parent = p as CompilationConfiguration;
			if (parent != null)
				Init (parent);

			if (compilers == null)
				compilers = new CompilerCollection ();

			if (temp_directory == null)
				temp_directory = Path.GetTempPath ();
		}

		public CompilerInfo GetCompilerInfo (string language)
		{
			return Compilers [language];
		}

		void Init (CompilationConfiguration parent)
		{
			debug = parent.debug;
			batch = parent.batch;
			batch_timeout = parent.batch_timeout;
			default_language = parent.default_language;
			_explicit = parent._explicit;
			max_batch_size = parent.max_batch_size;
			max_batch_file_size = parent.max_batch_file_size;
			num_recompiles_before_app_restart = parent.num_recompiles_before_app_restart;
			strict = parent.strict;
			temp_directory = parent.temp_directory;
			compilers = new CompilerCollection (parent.compilers);
		}

		public bool Debug {
			get { return debug; }
			set { debug = value; }
		}

		public bool Batch {
			get { return batch; }
			set { batch = value; }
		}

		public int BatchTimeout {
			get { return batch_timeout; }
			set { batch_timeout = value; }
		}

		public string DefaultLanguage {
			get { return default_language; }
			set { default_language = value; }
		}

		public bool Explicit {
			get { return _explicit; }
			set { _explicit = value; }
		}

		public int MaxBatchSize {
			get { return max_batch_size; }
			set { max_batch_size = value; }
		}

		public int MaxBatchFileSize {
			get { return max_batch_file_size; }
			set { max_batch_file_size = value; }
		}

		public int NumRecompilesBeforeAppRestart {
			get { return num_recompiles_before_app_restart; }
			set { num_recompiles_before_app_restart = value; }
		}

		public bool Strict {
			get { return strict; }
			set { strict = value; }
		}

		public string TempDirectory {
			get { return temp_directory; }
			set {
				if (value != null && !Directory.Exists (value))
					throw new ArgumentException ("Directory does not exist");

				temp_directory = value;
			}
		}

		public CompilerCollection Compilers {
			get { return compilers; }
		}
	}

	sealed class CompilerCollection
	{
		Hashtable compilers;

		public CompilerCollection () : this (null) {}

		public CompilerCollection (CompilerCollection parent)
		{
			compilers = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
						   CaseInsensitiveComparer.Default);

			if (parent != null && parent.compilers != null) {
				foreach (DictionaryEntry entry in parent.compilers)
					compilers [entry.Key] = entry.Value;
			}
		}

		public CompilerInfo this [string language] {
			get { return compilers [language] as CompilerInfo; }
			set {
				compilers [language] = value;
				string [] langs = language.Split (';');
				foreach (string s in langs) {
					string x = s.Trim ();
					if (x != "")
						compilers [x] = value;
				}
			}
		}

		internal Hashtable Hash {
			get { return compilers; }
		}
	}
}

#endif // NET_2_0 && XML_DEP

