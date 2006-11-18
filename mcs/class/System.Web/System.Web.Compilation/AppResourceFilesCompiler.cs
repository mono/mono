//
// System.Web.Compilation.AppResourceFilesCompiler: A compiler for application resource files
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
#if NET_2_0
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Web.Util;

using Microsoft.CSharp;
//using Microsoft.VisualBasic;
//using Microsoft.JScript;

namespace System.Web.Compilation
{
	class LengthComparer<T>: Comparer<T>
	{
		private int CompareStrings (string a, string b)
		{
			if (a == null || b == null)
				return 0;
			return (int)b.Length - (int)a.Length;
		}

		public override int Compare (T _a, T _b) 
		{
			string a = null, b = null;
			if (_a is string && _b is string) {
				a = _a as string;
				b = _b as string;
			} else if (_a is List<string> && _b is List<string>) {
				List<string> tmp = _a as List<string>;
				a = tmp [0];
				tmp = _b as List<string>;
				b = tmp [0];
			} else
				return 0;
			return CompareStrings (a, b);
		}
	}
	
	internal class AppResourceFilesCompiler
	{
		public enum FcCodeGenerator
		{
			Typed,
			CSharp,
			VBasic,
			JScript
		};
		
		protected string []	  filePaths = null;
		protected FcCodeGenerator codeGenerator = FcCodeGenerator.CSharp;
		protected string          codeGenType = null;
		protected List<string> [] resxFiles = null;
		protected List<string>    resourceFiles = null;
		protected Dictionary<string,bool?> assemblies = null;
		protected string          tempdirectory = null;
		protected Random          rnd = null;
		
		public FcCodeGenerator CodeGen {
			get { return codeGenerator; }
			set { codeGenerator = value; }
		}

		virtual public string TempDir {
			get {
				if (tempdirectory != null)
					return tempdirectory;
				return (tempdirectory = GenTempDir (Path.GetTempPath ()));
			}
			set { tempdirectory = value; }
		}
		
		public string Source {
			get { return FilesToString (); }
		}

		public CodeCompileUnit CompileUnit {
			get { return FilesToDom (); }
		}

		protected AppResourceFilesCompiler ()
		{
			filePaths = new string [] {};
		}
		
		public AppResourceFilesCompiler (string filePath)
		{
			DoInit (filePath, FcCodeGenerator.CSharp);
		}
		
		public AppResourceFilesCompiler (string filePath, FcCodeGenerator cg)
		{
			DoInit (filePath, cg);
		}

		public AppResourceFilesCompiler (string[] filePaths)
		{
			DoInit (filePaths, FcCodeGenerator.CSharp);
		}
		
		public AppResourceFilesCompiler (string[] filePaths, FcCodeGenerator cg)
		{
			DoInit (filePaths, cg);
		}

		public AppResourceFilesCompiler (string filePath, string genType)
		{
			DoInit (filePath, genType);
		}

		public AppResourceFilesCompiler (string[] filePaths, string genType)
		{
			DoInit (filePaths, genType);
		}

		private void DoInit (string filePath, string genType)
		{
			this.codeGenType = genType;
			DoInit (filePath, FcCodeGenerator.Typed);
		}

		private void DoInit (string [] filePaths, string genType)
		{
			this.codeGenType = genType;
			DoInit (filePaths, FcCodeGenerator.Typed);
		}
		
		private void DoInit (string filePath, FcCodeGenerator cg)
		{
			this.codeGenerator = cg;
			if (filePath != null)
				this.filePaths = new string [] { filePath };
		}

		private void DoInit (string [] filePaths, FcCodeGenerator cg)
		{
			this.codeGenerator = cg;
			if (filePaths != null)
				this.filePaths = (string [])filePaths.Clone ();
		}

		protected CodeDomProvider GetCodeProvider ()
		{
			switch (codeGenerator) {
			default:
				goto case FcCodeGenerator.CSharp;
					
			case FcCodeGenerator.CSharp:
				return new CSharpCodeProvider ();

//                         case FcCodeGenerator.VBasic:
//                                 return new VBCodeProvider ();

//                         case FcCodeGenerator.JScript:
//                                 return new JScriptCodeProvider ();

			case FcCodeGenerator.Typed:
				return null;
			}
		}
		
		protected string FilesToString ()
		{
			CodeCompileUnit unit = FilesToDom ();
			CodeDomProvider provider = GetCodeProvider ();
			StringWriter writer = new StringWriter ();
			CodeGeneratorOptions opts = new CodeGeneratorOptions ();

			opts.BlankLinesBetweenMembers = false;
			provider.GenerateCodeFromCompileUnit (unit, writer, opts);
			
			string ret = writer.ToString ();
			writer.Close ();
			return ret;
		}
		
		protected CodeCompileUnit FilesToDom ()
		{
			CollectFiles ();
			if (resxFiles.Length == 0)
				return null;
			
			string destdir = TempDir;

			string s, resfile;
			resourceFiles = new List<string> ();
			CodeCompileUnit ret = new CodeCompileUnit ();
			foreach (List<string> al in resxFiles) {
				if (al == null)
					continue;
				for (int i = 0; i < al.Count; i++) {
					s = al [i];
					if (s == null)
						continue;
					resfile = CompileFile (destdir, s);
					resourceFiles.Add (resfile);				 
					if (i > 0 || resfile == null)
						continue;
					
					// Default file. Generate the class
					DomFromResource (resfile, ret);
				}
			}
			if (assemblies != null)
				foreach (KeyValuePair<string,bool?> de in assemblies)
					ret.ReferencedAssemblies.Add (de.Key);
			
			return ret;
		}

		private void DomFromResource (string resfile, CodeCompileUnit unit)
		{
			string fname, nsname, classname;

			fname = Path.GetFileNameWithoutExtension (resfile);
			nsname = Path.GetFileNameWithoutExtension (fname);
			classname = Path.GetExtension (fname);
			if (classname == null || classname.Length == 0) {
				classname = nsname;
				nsname = "Resources";
			} else {
				nsname = String.Format ("Resources.{0}", nsname);
				classname = classname.Substring(1);
			}		 

			Dictionary<string,bool> imports = new Dictionary<string,bool> ();
			if (assemblies == null)
				assemblies = new Dictionary<string,bool?> ();
			CodeNamespace ns = new CodeNamespace (nsname);
			imports ["System"] = true;
			imports ["System.Globalization"] = true;
			imports ["System.Reflection"] = true;
			imports ["System.Resources"] = true;

			CodeTypeDeclaration cls = new CodeTypeDeclaration (classname);
			cls.IsClass = true;
			cls.TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;

			CodeMemberField cmf = new CodeMemberField (typeof(CultureInfo), "culture");
			cmf.InitExpression = new CodePrimitiveExpression (null);
			cmf.Attributes = MemberAttributes.Private | MemberAttributes.Final | MemberAttributes.Static;
			cls.Members.Add (cmf);

			cmf = new CodeMemberField (typeof(ResourceManager), "resourceManager");
			cmf.InitExpression = new CodePrimitiveExpression (null);
			cmf.Attributes = MemberAttributes.Private | MemberAttributes.Final | MemberAttributes.Static;
			cls.Members.Add (cmf);
			
			// Property: ResourceManager
			CodeMemberProperty cmp = new CodeMemberProperty ();
			cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;
			cmp.Name = "ResourceManager";
			cmp.HasGet = true;
			cmp.Type = new CodeTypeReference (typeof(ResourceManager));
			CodePropertyResourceManagerGet (cmp.GetStatements, resfile, classname);
			cls.Members.Add (cmp);

			// Property: Culture
			cmp = new CodeMemberProperty ();
			cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;
			cmp.Name = "Culture";
			cmp.HasGet = true;
			cmp.HasSet = true;
			cmp.Type = new CodeTypeReference (typeof(CultureInfo));
			CodePropertyGenericGet (cmp.GetStatements, "culture", classname);
			CodePropertyGenericSet (cmp.SetStatements, "culture", classname);
			cls.Members.Add (cmp);

			// Add the resource properties
			try {
				ResourceReader res = new ResourceReader (resfile);
				foreach (DictionaryEntry de in res) {
					Type type = de.Value.GetType ();

					if (!imports.ContainsKey (type.Namespace))
						imports [type.Namespace] = true;

					string asname = new AssemblyName (type.Assembly.FullName).Name;
					if (!assemblies.ContainsKey (asname))
						assemblies [asname] = true;
					
					cmp = new CodeMemberProperty ();
					cmp.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;
					cmp.Name = SanitizeResourceName ((string)de.Key);
					cmp.HasGet = true;
					CodePropertyResourceGet (cmp.GetStatements, (string)de.Key, type, classname);
					cmp.Type = new CodeTypeReference (type);
					cls.Members.Add (cmp);
				}
			} catch (Exception ex) {
			}
			foreach (KeyValuePair<string,bool> de in imports)
				ns.Imports.Add (new CodeNamespaceImport(de.Key));
			
			ns.Types.Add (cls);
			unit.Namespaces.Add (ns);
		}

		private string SanitizeResourceName (string name)
		{
			return name.Replace (' ', '_').Replace ('-', '_').Replace ('.', '_');
		}
		
		private CodeObjectCreateExpression NewResourceManager (string name, string typename)
		{
			CodeExpression resname = new CodePrimitiveExpression (name);
			CodePropertyReferenceExpression asm = new CodePropertyReferenceExpression (
				new CodeTypeOfExpression (new CodeTypeReference (typename)),
				"Assembly");
			
			return new CodeObjectCreateExpression ("System.Resources.ResourceManager",
							       new CodeExpression [] {resname, asm});
		}
		
		private void CodePropertyResourceManagerGet (CodeStatementCollection csc, string resfile, string typename)
		{
			string name = Path.GetFileNameWithoutExtension (resfile);
			CodeStatement st;
			CodeExpression exp;

			exp = new CodeFieldReferenceExpression (new CodeTypeReferenceExpression (typename), "resourceManager");
			st = new CodeConditionStatement (
				new CodeBinaryOperatorExpression (
					exp,
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression (null)),
				new CodeStatement [] { new CodeMethodReturnStatement (exp) });
			csc.Add (st);

			st = new CodeAssignStatement (exp, NewResourceManager (name, typename));
			csc.Add (st);
			csc.Add (new CodeMethodReturnStatement (exp));
		}

		private void CodePropertyResourceGet (CodeStatementCollection csc, string resname, Type restype, string typename)
		{
			CodeStatement st = new CodeVariableDeclarationStatement (
				typeof (ResourceManager),
				"rm",
				new CodePropertyReferenceExpression (
					new CodeTypeReferenceExpression (typename), "ResourceManager"));
			csc.Add (st);

			st = new CodeConditionStatement (
				new CodeBinaryOperatorExpression (
					new CodeVariableReferenceExpression ("rm"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression (null)),
				new CodeStatement [] { new CodeMethodReturnStatement (new CodePrimitiveExpression (null)) });
			csc.Add (st);

			bool gotstr = (restype == typeof (string));
			CodeExpression exp = new CodeMethodInvokeExpression (
				new CodeVariableReferenceExpression ("rm"),
				gotstr ? "GetString" : "GetObject",
				new CodeExpression [] { new CodePrimitiveExpression (resname),
							new CodeFieldReferenceExpression (
								new CodeTypeReferenceExpression (typename), "culture") });
			st = new CodeVariableDeclarationStatement (
				restype,
				"obj",
				gotstr ? exp : new CodeCastExpression (restype, exp));
			csc.Add (st);
			csc.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("obj")));
		}
		
		private void CodePropertyGenericGet (CodeStatementCollection csc, string field, string typename)
		{
			csc.Add(new CodeMethodReturnStatement (
					new CodeFieldReferenceExpression (
						new CodeTypeReferenceExpression (typename), field)));
		}

		private void CodePropertyGenericSet (CodeStatementCollection csc, string field, string typename)
		{
			csc.Add(new CodeAssignStatement (
					new CodeFieldReferenceExpression (new CodeTypeReferenceExpression (typename), field),
					new CodeVariableReferenceExpression ("value")));
		}
		
		private uint CountChars (char c, string s)
		{
			uint ret = 0;
			foreach (char ch in s) {
				if (ch == c)
					ret++;
			}
			return ret;
		}
		
		private void CollectFiles ()
		{
			List<string> files = new List<string> (filePaths);
			List<List<string>> groups = new List<List<string>> ();
			LengthComparer<string> lcString = new LengthComparer<string> ();
			LengthComparer<List<string>> lcList = new LengthComparer<List<string>> ();
			
			files.Sort (lcString);
			Array.Sort (filePaths, lcString);

			string tmp;
			foreach (string s in filePaths) {
				tmp = Path.GetExtension (s);
				if (tmp == null)
					continue;
				tmp = tmp.ToLower ();
				if (tmp != ".resx")
					continue;

				string basename = Path.GetFileNameWithoutExtension (s);
				uint basedots = CountChars ('.', basename);
				uint filedots;
				bool gotdefault = false;

				// If there are any files that start with this baseName, we have a default file
				for (int i = 0; i < files.Count; i++) {
					string s2 = files [i];
					if (s2 == null || s == s2)
						continue;
					tmp = Path.GetFileNameWithoutExtension (s2);
					filedots = CountChars ('.', tmp);

					if (filedots == basedots + 1 && tmp.StartsWith (basename)) {
						gotdefault = true;
						break;
					}
				}
				if (gotdefault) {
					List<string> al = new List<string> ();
					al.Add (s);
					int	idx = files.IndexOf (s);
					if (idx != -1)
						files [idx] = null;
					groups.Add (al);
				}
			}
			groups.Sort (lcList);

			string tmp2;
			// Now find their translated counterparts
			foreach (List<string> al in groups) {
				string s = al [0];
				tmp = Path.GetFileNameWithoutExtension (s);
				for (int i = 0; i < files.Count; i++) {
					s = files [i];
					if (s == null)
						continue;
					tmp2 = Path.GetFileName (s);
					if (tmp2.StartsWith (tmp)) {
						al.Add (s);
						files [i] = null;
					}
				}
			}

			// Anything that's left here might be orphans or lone default files.
			// For those files we check the part following the last dot
			// before the .resx extension and test whether it's a registered
			// culture or not. If it is not a culture, then we have a
			// default file that doesn't have any translations. Otherwise,
			// the file is ignored (it's the same thing MS.NET does)
			CultureInfo ci;
			foreach (string s in files) {
				if (s == null)
					continue;
				
				tmp = Path.GetFileNameWithoutExtension (s);
				tmp = Path.GetExtension (tmp);
				if (tmp == null || tmp.Length == 0)
					continue;
				tmp = tmp.Substring (1);
				try {
					ci = CultureInfo.GetCultureInfo (tmp);
					continue; // Culture found, we reject the file
				} catch {
				}

				// A single default file, create a group
				List<string> al = new List<string> ();
				al.Add (s);
				groups.Add (al);
			}
			groups.Sort (lcList);
			resxFiles = groups.ToArray ();
		}		 

		private IResourceReader GetReader (Stream stream, string path)
		{
			string ext = Path.GetExtension (path);
			if (ext == null)
				throw new Exception ("Unknown resource type.");
			switch (ext.ToLower ()) {
			case ".resx":
				return new ResXResourceReader (stream);

			default:
				throw new Exception ("Unknown resource type.");
			}
		}
		
		private string CompileFile (string destdir, string path)
		{
			string resfile = String.Format ("{1}{0}{2}.resources",
							Path.DirectorySeparatorChar,
							destdir,
							Path.GetFileNameWithoutExtension (path));

			FileStream source = null, dest = null;
			IResourceReader reader = null;
			ResourceWriter writer = null;
			
			try {
				source = new FileStream (path, FileMode.Open, FileAccess.Read);
				dest = new FileStream (resfile, FileMode.Create, FileAccess.Write);
				reader = GetReader (source, path);
				writer = new ResourceWriter (dest);
				foreach (DictionaryEntry de in reader) {
					object val = de.Value;
					if (val is string)
						writer.AddResource ((string)de.Key, (string)val);
					else
						writer.AddResource ((string)de.Key, val);
				}
			} catch (Exception ex) {
				Console.WriteLine ("Resource compiler error: {0}", ex.Message);
				Exception inner = ex.InnerException;
				if (inner != null)
					Console.WriteLine ("Inner exception: {0}", inner.Message);
				return null;
			} finally {
				if (reader != null)
					reader.Close ();
				else if (source != null)
					source.Close ();
				if (writer != null)
					writer.Close ();
				else if (dest != null)
					dest.Close ();
			}
			return resfile;
		}

		object OnCreateRandomFile (string path)
		{
			FileStream f = new FileStream (path, FileMode.CreateNew);
			f.Close ();
			return path;
		}
		
		protected string GenRandomFileName (string basepath)
		{
			return GenRandomFileName (basepath, null);
		}
		
		protected string GenRandomFileName (string basepath, string ext)
		{
			return (string)FileUtils.CreateTemporaryFile (basepath, ext, OnCreateRandomFile);
		}

		object OnCreateRandomDir (string path)
		{
			DirectoryInfo di = Directory.CreateDirectory (path);
			return di.FullName;
		}
		
		protected string GenTempDir (string basepath)
		{
			return (string)FileUtils.CreateTemporaryFile (basepath, OnCreateRandomDir);
		}
	}
}
#endif
