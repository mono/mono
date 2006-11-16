//
// System.Web.Configuration.CompilerCollection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0 && CONFIGURATION_DEP

using System;
using System.Collections.Generic;
using System.Configuration;

namespace System.CodeDom.Compiler
{
	[ConfigurationCollection (typeof (Compiler), AddItemName = "compiler", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	internal sealed class CompilerCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;
		static SortedDictionary<string, CompilerInfo> compiler_infos_by_language;
		static SortedDictionary<string, CompilerInfo> compiler_infos_by_extension;
		
		static CompilerCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
			compiler_infos_by_language = new SortedDictionary <string, CompilerInfo> ();
			compiler_infos_by_extension = new SortedDictionary <string, CompilerInfo> ();
			
			CompilerInfo compiler = new CompilerInfo ();
                        compiler.Languages = "c#;cs;csharp";
                        compiler.Extensions = ".cs";
                        compiler.TypeName = "Microsoft.CSharp.CSharpCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
			AddCompilerInfo (compiler);

			compiler = new CompilerInfo ();
			compiler.Languages = "vb;vbs;visualbasic;vbscript";
                        compiler.Extensions = ".vb";
                        compiler.TypeName = "Microsoft.VisualBasic.VBCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
			AddCompilerInfo (compiler);

			compiler = new CompilerInfo ();
                        compiler.Languages = "js;jscript;javascript";
                        compiler.Extensions = ".js";
                        compiler.TypeName = "Microsoft.JScript.JScriptCodeProvider, Microsoft.JScript, Version=8.0.1100.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
			AddCompilerInfo (compiler);

			compiler = new CompilerInfo ();
                        compiler.Languages = "vj#;vjs;vjsharp";
                        compiler.Extensions = ".jsl;.java";
                        compiler.TypeName = "Microsoft.VJSharp.VJSharpCodeProvider, VJSharpCodeProvider, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
			AddCompilerInfo (compiler);

			compiler = new CompilerInfo ();
                        compiler.Languages = "c++;mc;cpp";
                        compiler.Extensions = ".h";
                        compiler.TypeName = "Microsoft.VisualC.CppCodeProvider, CppCodeProvider, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
			AddCompilerInfo (compiler);
		}

		public CompilerCollection ()
		{
		}

		static void AddCompilerInfo (CompilerInfo ci)
		{
			ci.Init ();
			foreach (string l in ci.Languages.Split (';'))
				compiler_infos_by_language.Add (l, ci);
			foreach (string e in ci.Extensions.Split (';'))
				compiler_infos_by_extension.Add (e, ci);
		}

		static void AddCompilerInfo (Compiler compiler)
		{
			CompilerInfo ci = new CompilerInfo ();
			ci.Languages = compiler.Language;
			ci.Extensions = compiler.Extension;
			ci.TypeName = compiler.Type;
			AddCompilerInfo (ci);
		}
		
		protected override void BaseAdd (ConfigurationElement element)
		{
			Compiler compiler = element as Compiler;
			if (compiler != null)
				AddCompilerInfo (compiler);
			base.BaseAdd (element);
		}
		
		protected override bool ThrowOnDuplicate {
                        get { return false; }
                }
		
		protected override ConfigurationElement CreateNewElement ()
		{
			return new Compiler ();
		}

		public CompilerInfo GetCompilerInfoForLanguage (string language)
		{
			return compiler_infos_by_language [language];
		}

		public CompilerInfo GetCompilerInfoForExtension (string extension)
		{
			return compiler_infos_by_extension [extension];
		}

		public string GetLanguageFromExtension (string extension)
		{
			CompilerInfo ci = GetCompilerInfoForExtension (extension);
			if (ci == null)
				return null;
			foreach (KeyValuePair <string, CompilerInfo> kvp in compiler_infos_by_language)
				if (ci.Equals (kvp.Value))
					return kvp.Key;
			return null;
		}
		
		public Compiler Get (int index)
		{
			return (Compiler) BaseGet (index);
		}

		public Compiler Get (string language)
		{
			return (Compiler) BaseGet (language);
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((Compiler)element).Language;
		}

		public string GetKey (int index)
		{
			return (string)BaseGetKey (index);
		}

		public string[ ] AllKeys {
			get {
				string[] ret = new string [compiler_infos_by_language.Keys.Count];
				compiler_infos_by_language.Keys.CopyTo (ret, 0);
				return ret;
			}
		}
		
		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName {
			get { return "compiler"; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public Compiler this[int index] {
			get { return (Compiler) BaseGet (index); }
		}

		public new CompilerInfo this[string language] {
			get {
				return compiler_infos_by_language [language];
			}
		}

		public CompilerInfo[] CompilerInfos {
			get {
				CompilerInfo[] ret = new CompilerInfo [compiler_infos_by_language.Values.Count];
				compiler_infos_by_language.Values.CopyTo (ret, 0);
				return ret;
			}
		}
	}
}
#endif
