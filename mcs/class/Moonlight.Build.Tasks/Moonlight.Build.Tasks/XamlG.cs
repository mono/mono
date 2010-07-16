//
// XamlG.cs
//
// Author:
//	Michael Hutchinson <mhutchinson@novell.com>
//	Ankit Jain <jankit@novell.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Moonlight.Build.Tasks {
	public class XamlG : Task {

		public override bool Execute ()
		{
			if (Sources.Length == 0)
				return true;

			if (OutputFiles == null || Sources.Length != OutputFiles.Length) {
				Log.LogError ("Number of OutputFiles must match the number of Source files");
				return false;
			}

			var codedom_provider = GetCodeDomProviderForLanguage (Language);
			if (codedom_provider == null) {
				Log.LogError ("Language {0} not supported for code generation.", Language);
				return false;
			}

			for (int i = 0; i < Sources.Length; i ++) {
				ITaskItem source_item = Sources [i];
				ITaskItem dest_item = OutputFiles [i];
				if (!File.Exists (dest_item.ItemSpec) ||
					File.GetLastWriteTime (dest_item.ItemSpec) < File.GetLastWriteTime (source_item.ItemSpec)) {
					Log.LogMessage (MessageImportance.Low, "Generating codebehind accessors for {0}...", source_item.ItemSpec);

					string full_source_path = source_item.GetMetadata ("FullPath");
					try {
						if (!XamlGCompiler.GenerateFile (codedom_provider, AssemblyName, full_source_path,
										Path.Combine (source_item.GetMetadata ("RelativeDir"),
											Path.GetFileName (source_item.ItemSpec)),
										dest_item.ItemSpec, Log)) {
							Log.LogError ("Error generating {0} from {1}", full_source_path, dest_item.ItemSpec);
							return false;
						}
					} catch (Exception e) {
						Log.LogError ("Error generating {0} from {1}: {2}", full_source_path, dest_item.ItemSpec, e.Message);
						Log.LogMessage (MessageImportance.Low, "Error generating {0} from {1}: {2}",
								full_source_path, dest_item.ItemSpec, e.ToString ());
						return false;
					}
				}
			}

			return true;
		}

		CodeDomProvider GetCodeDomProviderForLanguage (string lang)
		{
			switch (lang.ToLower ()) {
			case "c#": return new Microsoft.CSharp.CSharpCodeProvider ();
			case "vb": return new Microsoft.VisualBasic.VBCodeProvider ();
			}

			return null;
		}

		[Required]
		public ITaskItem [] Sources {
			get; set;
		}

		[Required]
		public string Language {
			get; set;
		}

		[Required]
		public string AssemblyName {
			get; set;
		}

		[Output]
		public ITaskItem [] OutputFiles {
			get; set;
		}

		bool HasFileChanged (string source, string dest)
		{
			if (!File.Exists (dest))
				return true;

			FileInfo sourceInfo = new FileInfo (source);
			FileInfo destinationInfo = new FileInfo (dest);

			return !(sourceInfo.Length == destinationInfo.Length &&
					File.GetLastWriteTime(source) <= File.GetLastWriteTime (dest));
		}

	}

	static class XamlGCompiler
	{
		private static bool sl2 = true;

		public static bool GenerateFile (CodeDomProvider provider, string app_name,
		                                       string xaml_file, string xaml_path_in_project, string out_file, TaskLoggingHelper log)
		{
			XmlDocument xmldoc = new XmlDocument ();
			xmldoc.Load (xaml_file);

			XmlNamespaceManager nsmgr = new XmlNamespaceManager (xmldoc.NameTable);
			nsmgr.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");

			XmlNode root = xmldoc.SelectSingleNode ("/*", nsmgr);
			if (root == null) {
				log.LogError ("{0}: No root node found.", xaml_file);
				return false;
			}

			XmlAttribute root_class = root.Attributes ["x:Class"];
			if (root_class == null) {
				File.WriteAllText (out_file, "");
				return true;
			}

			bool is_application = root.LocalName == "Application";
			string root_ns;
			string root_type;
			string root_asm;

			ParseXmlns (root_class.Value, out root_type, out root_ns, out root_asm);

			Hashtable names_and_types = GetNamesAndTypes (root, nsmgr);
//			Hashtable keys_and_types = GetKeysAndTypes (root, nsmgr);

			CodeCompileUnit ccu = new CodeCompileUnit ();
			CodeNamespace decl_ns = new CodeNamespace (root_ns);
			ccu.Namespaces.Add (decl_ns);

			decl_ns.Imports.Add (new CodeNamespaceImport ("System"));
			decl_ns.Imports.Add (new CodeNamespaceImport ("System.Windows"));
			decl_ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Controls"));
			decl_ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Documents"));
			decl_ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Input"));
			decl_ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Media"));
			decl_ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Media.Animation"));
			decl_ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Shapes"));
			decl_ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Controls.Primitives"));

			CodeTypeDeclaration decl_type = new CodeTypeDeclaration (root_type);
			decl_type.IsPartial = true;

			decl_ns.Types.Add (decl_type);

			CodeMemberMethod initcomp = new CodeMemberMethod ();
			initcomp.Name = "InitializeComponent";
			decl_type.Members.Add (initcomp);

			if (sl2) {
				CodeMemberField field = new CodeMemberField ();
				field.Name = "_contentLoaded";
				field.Type = new CodeTypeReference (typeof (bool));

				decl_type.Members.Add (field);

				CodeConditionStatement is_content_loaded = new CodeConditionStatement (new CodeVariableReferenceExpression ("_contentLoaded"),
						new CodeStatement [] { new CodeMethodReturnStatement () });
				initcomp.Statements.Add (is_content_loaded);

				CodeAssignStatement set_content_loaded = new CodeAssignStatement (new CodeVariableReferenceExpression ("_contentLoaded"),
						new CodePrimitiveExpression (true));

				initcomp.Statements.Add (set_content_loaded);

				string component_path = String.Format ("/{0};component/{1}", app_name, xaml_path_in_project);
				CodeMethodInvokeExpression load_component = new CodeMethodInvokeExpression (
					new CodeTypeReferenceExpression ("System.Windows.Application"), "LoadComponent",
					new CodeExpression [] { new CodeThisReferenceExpression (),
								new CodeObjectCreateExpression (new CodeTypeReference ("System.Uri"), new CodeExpression [] {
									new CodePrimitiveExpression (component_path),
									new CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("System.UriKind"), "Relative") })
					});
				initcomp.Statements.Add (load_component);
			}

			if (!is_application) {
				foreach (DictionaryEntry entry  in names_and_types) {
					string name = (string) entry.Key;
					CodeTypeReference type = (CodeTypeReference) entry.Value;

					CodeMemberField field = new CodeMemberField ();

					if (sl2)
						field.Attributes = MemberAttributes.Assembly;

					field.Name = name;
					field.Type = type;

					decl_type.Members.Add (field);

					CodeMethodInvokeExpression find_invoke = new CodeMethodInvokeExpression (
						new CodeThisReferenceExpression(), "FindName",
						new CodeExpression[] { new CodePrimitiveExpression (name) } );

					CodeCastExpression cast = new CodeCastExpression (type, find_invoke);

					CodeAssignStatement assign = new CodeAssignStatement (
						new CodeVariableReferenceExpression (name), cast);

					initcomp.Statements.Add (assign);
				}
			}


			using (StreamWriter writer = new StreamWriter (out_file)) {
				provider.GenerateCodeFromCompileUnit (ccu, writer, new CodeGeneratorOptions ());
			}

			return true;
		}

		private static Hashtable GetNamesAndTypes (XmlNode root, XmlNamespaceManager nsmgr)
		{
			Hashtable res = new Hashtable ();

			XmlNodeList names = root.SelectNodes ("//*[@x:Name]", nsmgr);
			foreach (XmlNode node in names)	{

				// Don't take the root canvas
				if (node == root)
					continue;

				XmlAttribute attr = node.Attributes ["x:Name"];
				string name = attr.Value;
				string ns = GetNamespace (node);
				string member_type = node.LocalName;

				if (ns != null)
					member_type = String.Concat (ns, ".", member_type);

				CodeTypeReference type = new CodeTypeReference (member_type);
				if (ns != null)
					type.Options |= CodeTypeReferenceOptions.GlobalReference;

				res [name] = type;
			}

			return res;
		}

		/*
		private static Hashtable GetKeysAndTypes (XmlNode root, XmlNamespaceManager nsmgr)
		{
			Hashtable res = new Hashtable ();

			XmlNodeList keys = root.SelectNodes ("//*[@x:Key]", nsmgr);
			foreach (XmlNode node in keys)	{

				// Don't take the root canvas
				if (node == root)
					continue;

				XmlAttribute attr = node.Attributes ["x:Key"];
				string key = attr.Value;
				string ns = GetNamespace (node);
				string member_type = node.LocalName;

				if (ns != null)
					member_type = String.Concat (ns, ".", member_type);

				res [key] = member_type;
			}

			return res;
		}
		*/

		internal static string GetNamespace (XmlNode node)
		{
			if (!IsCustom (node.NamespaceURI))
				return null;

			return ParseNamespaceFromXmlns (node.NamespaceURI);
		}

		private static bool IsCustom (string ns)
		{
			switch (ns) {
			case "http://schemas.microsoft.com/winfx/2006/xaml":
			case "http://schemas.microsoft.com/winfx/2006/xaml/presentation":
			case "http://schemas.microsoft.com/client/2007":
				return false;
			}

			return true;
		}

		private static string ParseNamespaceFromXmlns (string xmlns)
		{
			string type_name = null;
			string ns = null;
			string asm = null;

			ParseXmlns (xmlns, out type_name, out ns, out asm);

			return ns;
		}

//		private static string ParseTypeFromXmlns (string xmlns)
//		{
//			string type_name = null;
//			string ns = null;
//			string asm = null;
//
//			ParseXmlns (xmlns, out type_name, out ns, out asm);
//
//			return type_name;
//		}

		internal static void ParseXmlns (string xmlns, out string type_name, out string ns, out string asm)
		{
			type_name = null;
			ns = null;
			asm = null;

			string [] decls = xmlns.Split (';');
			foreach (string decl in decls) {
				if (decl.StartsWith ("clr-namespace:")) {
					ns = decl.Substring (14, decl.Length - 14);
					continue;
				}
				if (decl.StartsWith ("assembly=")) {
					asm = decl.Substring (9, decl.Length - 9);
					continue;
				}
				int nsind = decl.LastIndexOf (".");
				if (nsind > 0) {
					ns = decl.Substring (0, nsind);
					type_name = decl.Substring (nsind + 1, decl.Length - nsind - 1);
				} else {
					type_name = decl;
				}
			}
		}
	}

}
