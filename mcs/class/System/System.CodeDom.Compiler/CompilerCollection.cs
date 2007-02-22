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
		static List<CompilerInfo> compiler_infos;
		
		static CompilerCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
			compiler_infos = new List <CompilerInfo> ();
			
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
			compiler_infos.Add (ci);
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
			foreach (CompilerInfo ci in compiler_infos)
				if (ci.Languages.IndexOf (language, StringComparison.InvariantCultureIgnoreCase) != -1)
					return ci;
			return null;
		}

		public CompilerInfo GetCompilerInfoForExtension (string extension)
		{
			foreach (CompilerInfo ci in compiler_infos)
				if (ci.Extensions.IndexOf (extension, StringComparison.InvariantCultureIgnoreCase) != -1)
					return ci;
			return null;
		}

		public string GetLanguageFromExtension (string extension)
		{
			CompilerInfo ci = GetCompilerInfoForExtension (extension);
			if (ci == null)
				return null;
			string[] languages = ci.Languages.Split (';');
			if (languages != null && languages.Length > 0)
				return languages[0];
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
				string[] keys = new string[compiler_infos.Count];
				for (int i = 0; i < Count; i++)
					keys[i] = compiler_infos[i].Languages;
				return keys;
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
				return GetCompilerInfoForLanguage (language);
			}
		}

		public CompilerInfo[] CompilerInfos {
			get {
				return compiler_infos.ToArray ();
			}
		}
	}
}
#endif
