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

#if CONFIGURATION_DEP

using System;
using System.Collections.Generic;
using System.Configuration;

namespace System.CodeDom.Compiler
{
	[ConfigurationCollection (typeof (Compiler), AddItemName = "compiler", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	internal sealed class CompilerCollection : ConfigurationElementCollection
	{
		static readonly string defaultCompilerVersion = "4.0";
		static ConfigurationPropertyCollection properties;
		static List <CompilerInfo> compiler_infos;
		static Dictionary <string, CompilerInfo> compiler_languages;
		static Dictionary <string, CompilerInfo> compiler_extensions;
		
		static CompilerCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
			compiler_infos = new List <CompilerInfo> ();
			compiler_languages = new Dictionary <string, CompilerInfo> (16, StringComparer.OrdinalIgnoreCase);
			compiler_extensions = new Dictionary <string, CompilerInfo> (4, StringComparer.OrdinalIgnoreCase);
				
			CompilerInfo compiler = new CompilerInfo (null, "Microsoft.CSharp.CSharpCodeProvider, " + Consts.AssemblySystem,
				new [] { "c#", "cs", "csharp" }, new [] { ".cs" });
			compiler.ProviderOptions ["CompilerVersion"] = defaultCompilerVersion;
			AddCompilerInfo (compiler);

			compiler = new CompilerInfo (null, "Microsoft.VisualBasic.VBCodeProvider, " + Consts.AssemblySystem,
				new [] { "vb", "vbs", "visualbasic", "vbscript" }, new [] { ".vb" });
			compiler.ProviderOptions ["CompilerVersion"] = defaultCompilerVersion;
			AddCompilerInfo (compiler);
		}

		public CompilerCollection () { }

		static void AddCompilerInfo (CompilerInfo ci)
		{
			ci.CreateProvider();
			compiler_infos.Add (ci);

			string[] languages = ci.GetLanguages ();
			if (languages != null)
				foreach (string language in languages)
					compiler_languages [language] = ci;
			
			string[] extensions = ci.GetExtensions ();
			if (extensions != null)
				foreach (string extension in extensions)
					compiler_extensions [extension] = ci;
		}

		static void AddCompilerInfo (Compiler compiler)
		{
			CompilerInfo ci = new CompilerInfo (null, compiler.Type, new [] { compiler.Extension }, new [] { compiler.Language });
			ci.CompilerParams.CompilerOptions = compiler.CompilerOptions;
			ci.CompilerParams.WarningLevel = compiler.WarningLevel;
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
			if (compiler_languages.Count == 0)
				return null;
			
			CompilerInfo ci;
			if (compiler_languages.TryGetValue (language, out ci))
				return ci;
			
			return null;
		}

		public CompilerInfo GetCompilerInfoForExtension (string extension)
		{
			if (compiler_extensions.Count == 0)
				return null;
			
			CompilerInfo ci;
			if (compiler_extensions.TryGetValue (extension, out ci))
				return ci;
			
			return null;
		}

		public string GetLanguageFromExtension (string extension)
		{
			CompilerInfo ci = GetCompilerInfoForExtension (extension);
			if (ci == null)
				return null;
			string[] languages = ci.GetLanguages ();
			if (languages != null && languages.Length > 0)
				return languages [0];
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

		public string[] AllKeys {
			get {
				var keys = new string [compiler_infos.Count];
				for (int i = 0; i < Count; i++)
					keys[i] = string.Join(";", compiler_infos[i].GetLanguages ());
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