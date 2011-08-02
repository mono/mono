//
// System.Web.Compilation.TemplateBuildProvider
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.Compilation
{
	abstract class TemplateBuildProvider : GenericBuildProvider <TemplateParser>
	{
		delegate void ExtractDirectiveDependencies (string baseDirectory, CaptureCollection names, CaptureCollection values, TemplateBuildProvider bp);
		
		static SortedDictionary <string, ExtractDirectiveDependencies> directiveAttributes;
		static char[] directiveValueTrimChars = {' ', '\t', '\r', '\n', '"', '\''};

		SortedDictionary <string, bool> dependencies;
		string compilationLanguage;
		
		internal override string LanguageName {
			get {
				if (String.IsNullOrEmpty (compilationLanguage)) {
					ExtractDependencies ();
					if (String.IsNullOrEmpty (compilationLanguage))
						compilationLanguage = base.LanguageName;
				}
				
				return compilationLanguage;
			}
		}
		
		static TemplateBuildProvider ()
		{
			directiveAttributes = new SortedDictionary <string, ExtractDirectiveDependencies> (StringComparer.InvariantCultureIgnoreCase);
			directiveAttributes.Add ("Control", ExtractControlDependencies);
			directiveAttributes.Add ("Master", ExtractPageOrMasterDependencies);
			directiveAttributes.Add ("MasterType", ExtractPreviousPageTypeOrMasterTypeDependencies);
			directiveAttributes.Add ("Page", ExtractPageOrMasterDependencies);
			directiveAttributes.Add ("PreviousPageType", ExtractPreviousPageTypeOrMasterTypeDependencies);
			directiveAttributes.Add ("Reference", ExtractReferenceDependencies);
			directiveAttributes.Add ("Register", ExtractRegisterDependencies);
			directiveAttributes.Add ("WebHandler", ExtractLanguage);
			directiveAttributes.Add ("WebService", ExtractLanguage);
		}

		static string ExtractDirectiveAttribute (string baseDirectory, string name, CaptureCollection names, CaptureCollection values)
		{
			return ExtractDirectiveAttribute (baseDirectory, name, names, values, true);
		}
		
		static string ExtractDirectiveAttribute (string baseDirectory, string name, CaptureCollection names, CaptureCollection values, bool isPath)
		{
			if (names.Count == 0)
				return String.Empty;

			int index = 0;
			int valuesCount = values.Count;
			foreach (Capture c in names) {
				if (String.Compare (c.Value, name, StringComparison.OrdinalIgnoreCase) != 0) {
					index++;
					continue;
				}
				
				if (index > valuesCount)
					return String.Empty;

				if (isPath) {
					string value = values [index].Value.Trim (directiveValueTrimChars);
					if (String.IsNullOrEmpty (value))
						return String.Empty;
					
					return new VirtualPath (value, baseDirectory).Absolute;
				} else
					return values [index].Value.Trim ();
			}

			return String.Empty;
		}

		static void ExtractControlDependencies (string baseDirectory, CaptureCollection names, CaptureCollection values, TemplateBuildProvider bp)
		{
			ExtractLanguage (baseDirectory, names, values, bp);
			ExtractCodeBehind (baseDirectory, names, values, bp);
		}
		
		static void ExtractLanguage (string baseDirectory, CaptureCollection names, CaptureCollection values, TemplateBuildProvider bp)
		{
			string value = ExtractDirectiveAttribute (baseDirectory, "Language", names, values, false);
			if (String.IsNullOrEmpty (value))
				return;

			bp.compilationLanguage = value;

			ExtractCodeBehind (baseDirectory, names, values, bp);
		}
		
		static void ExtractPageOrMasterDependencies (string baseDirectory, CaptureCollection names, CaptureCollection values, TemplateBuildProvider bp)
		{
			ExtractLanguage (baseDirectory, names, values, bp);
			string value = ExtractDirectiveAttribute (baseDirectory, "MasterPageFile", names, values);
			if (!String.IsNullOrEmpty (value)) {
				if (!bp.dependencies.ContainsKey (value))
					bp.dependencies.Add (value, true);
			}

			ExtractCodeBehind (baseDirectory, names, values, bp);
		}

		static void ExtractCodeBehind (string baseDirectory, CaptureCollection names, CaptureCollection values, TemplateBuildProvider bp)
		{
			string[] varray = new string [2];

			varray [0] = ExtractDirectiveAttribute (baseDirectory, "CodeFile", names, values);
			varray [1] = ExtractDirectiveAttribute (baseDirectory, "Src", names, values);
			foreach (string value in varray) {
				if (!String.IsNullOrEmpty (value)) {
					if (!bp.dependencies.ContainsKey (value))
						bp.dependencies.Add (value, true);
				}
			}
		}
		
		static void ExtractRegisterDependencies (string baseDirectory, CaptureCollection names, CaptureCollection values, TemplateBuildProvider bp)
		{
			string src = ExtractDirectiveAttribute (baseDirectory, "Src", names, values);
			if (String.IsNullOrEmpty (src))
				return;
			
			string value = ExtractDirectiveAttribute (baseDirectory, "TagName", names, values);
			if (String.IsNullOrEmpty (value))
				return;

			value = ExtractDirectiveAttribute (baseDirectory, "TagPrefix", names, values);
			if (String.IsNullOrEmpty (value))
				return;

			if (bp.dependencies.ContainsKey (src))
				return;

			bp.dependencies.Add (src, true);
		}

		static void ExtractPreviousPageTypeOrMasterTypeDependencies (string baseDirectory, CaptureCollection names, CaptureCollection values, TemplateBuildProvider bp)
		{
			string value = ExtractDirectiveAttribute (baseDirectory, "VirtualPath", names, values);
			if (String.IsNullOrEmpty (value))
				return;

			if (bp.dependencies.ContainsKey (value))
				return;

			bp.dependencies.Add (value, true);
		}

		static void ExtractReferenceDependencies (string baseDirectory, CaptureCollection names, CaptureCollection values, TemplateBuildProvider bp)
		{
			string control = ExtractDirectiveAttribute (baseDirectory, "Control", names, values);
			string virtualPath = ExtractDirectiveAttribute (baseDirectory, "VirtualPath", names, values);
			string page = ExtractDirectiveAttribute (baseDirectory, "Page", names, values);
			bool controlEmpty = String.IsNullOrEmpty (control);
			bool virtualPathEmpty = String.IsNullOrEmpty (virtualPath);
			bool pageEmpty = String.IsNullOrEmpty (page);
			
			if (controlEmpty && virtualPathEmpty && pageEmpty)
				return;

			if ((controlEmpty ? 1 : 0) + (virtualPathEmpty ? 1 : 0) + (pageEmpty ? 1 : 0) != 2)
				return;
			
			string value;
			if (!controlEmpty)
				value = control;
			else if (!virtualPathEmpty)
				value = virtualPath;
			else
				value = page;

			if (bp.dependencies.ContainsKey (value))
				return;

			bp.dependencies.Add (value, true);
		}

		IDictionary <string, bool> AddParsedDependencies (IDictionary <string, bool> dict)
		{
			if (Parsed) {
				List <string> deps = Parser.Dependencies;
				if (deps == null || deps.Count > 0)
					return dict;
				
				if (dict == null) {
					dict = dependencies;
					if (dict == null)
						dict = dependencies = new SortedDictionary <string, bool> (StringComparer.OrdinalIgnoreCase);
				}
				
				string s;
				foreach (object o in deps) {
					s = o as string;
					if (s == null || dict.ContainsKey (s))
						continue;
					dict.Add (s, true);
				}
			}

			if (dict == null || dict.Count == 0)
				return null;
			
			return dict;
		}
		
		internal override IDictionary <string, bool> ExtractDependencies ()
		{
			if (dependencies != null)
				return AddParsedDependencies (dependencies);

			string vpath = VirtualPath;
			if (String.IsNullOrEmpty (vpath))
				return AddParsedDependencies (null);

			VirtualPathProvider vpp = HostingEnvironment.VirtualPathProvider;
			if (!vpp.FileExists (vpath))
				return AddParsedDependencies (null);
			
			VirtualFile vf = vpp.GetFile (vpath);
			if (vf == null)
				return AddParsedDependencies (null);

			string input;
			using (Stream st = vf.Open ()) {
				if (st == null || !st.CanRead)
					return AddParsedDependencies (null);
				
				using (StreamReader sr = new StreamReader (st, WebEncoding.FileEncoding)) {
					input = sr.ReadToEnd ();
				}
			}
					
			if (String.IsNullOrEmpty (input))
				return AddParsedDependencies (null);

			MatchCollection matches = AspGenerator.DirectiveRegex.Matches (input);
			if (matches == null || matches.Count == 0)
				return AddParsedDependencies (null);
			
			dependencies = new SortedDictionary <string, bool> (StringComparer.InvariantCultureIgnoreCase);
			CaptureCollection ccNames;
			GroupCollection groups;
			string directiveName;
			ExtractDirectiveDependencies edd;
			string baseDirectory = VirtualPathUtility.GetDirectory (vpath);
			
			foreach (Match match in matches) {
				groups = match.Groups;
				if (groups.Count < 6)
					continue;
				
				ccNames = groups [3].Captures;
				directiveName = ccNames [0].Value;
				if (!directiveAttributes.TryGetValue (directiveName, out edd))
					continue;
				edd (baseDirectory, ccNames, groups [5].Captures, this);
			}

			return AddParsedDependencies (dependencies);
		}
		
		protected override string GetClassType (BaseCompiler compiler, TemplateParser parser)
		{
			if (compiler != null)
				return compiler.MainClassType;

			return null;
		}
		
		protected override ICollection GetParserDependencies (TemplateParser parser)
		{
			if (parser != null)
				return parser.Dependencies;
			
			return null;
		}
		
		protected override string GetParserLanguage (TemplateParser parser)
		{
			if (parser != null)
				return parser.Language;

			return null;
		}
		
		protected override string GetCodeBehindSource (TemplateParser parser)
		{
			if (parser != null) {
				string codeBehind = parser.CodeBehindSource;
				if (String.IsNullOrEmpty (codeBehind))
					return null;				

				return parser.CodeBehindSource;
			}
			
			return null;
		}
		
		protected override AspGenerator CreateAspGenerator (TemplateParser parser)
		{
			if (parser != null)
				return new AspGenerator (parser);

			return null;
		}

		protected override List <string> GetReferencedAssemblies (TemplateParser parser)
		{
			if (parser == null)
				return null;
			
			List <string> asms = parser.Assemblies;
			if (asms == null || asms.Count == 0)
				return null;

			List <string> ret = new List <string> ();			
			foreach (string loc in asms) {
				if (String.IsNullOrEmpty (loc))
					continue;

				if (ret.Contains (loc))
					continue;

				ret.Add (loc);
			}

			return ret;
		}
	}
}
