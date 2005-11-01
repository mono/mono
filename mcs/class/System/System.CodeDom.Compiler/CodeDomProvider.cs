//
// System.CodeDom.Compiler.CodeDomProvider.cs
//
// Authors:
//   Daniel Stodden (stodden@in.tum.de)
//   Marek Safar (marek.safar@seznam.cz)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2002,2003,2004,2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.CodeDom.Compiler {

#if NET_2_0
	[ComVisible (true)]
#endif
	[ToolboxItem ("")]
	public abstract class CodeDomProvider : Component
	{
		//
		// Constructors
		//
		protected CodeDomProvider()
		{
		}

		//
		// Properties
		//
		public virtual string FileExtension {
			get {
				return String.Empty;
			}
		}

		public virtual LanguageOptions LanguageOptions {
			get {
				return LanguageOptions.None;
			}
		}

		//
		// Methods
		//
#if NET_2_0
		[Obsolete ("ICodeCompiler is obsolete")]
#endif
		public abstract ICodeCompiler CreateCompiler();

#if NET_2_0
		[Obsolete ("ICodeGenerator is obsolete")]
#endif
		public abstract ICodeGenerator CreateGenerator();
		
		public virtual ICodeGenerator CreateGenerator (string fileName)
		{
			return CreateGenerator();
		}

		public virtual ICodeGenerator CreateGenerator (TextWriter output)
		{
			return CreateGenerator();
		}

#if NET_2_0
		[Obsolete ("ICodeParser is obsolete")]
#endif
		public virtual ICodeParser CreateParser()
		{
			return null;
		}

		public virtual TypeConverter GetConverter (Type type)
		{
			return TypeDescriptor.GetConverter (type);
		}

#if NET_2_0
		public virtual CompilerResults CompileAssemblyFromDom (CompilerParameters options, params CodeCompileUnit[] compilationUnits)
		{
			ICodeCompiler cc = CreateCompiler ();
			if (cc == null)
				throw new NotImplementedException ();
			return cc.CompileAssemblyFromDomBatch (options, compilationUnits);
		}

		public virtual CompilerResults CompileAssemblyFromFile (CompilerParameters options, params string[] fileNames)
		{
			ICodeCompiler cc = CreateCompiler ();
			if (cc == null)
				throw new NotImplementedException ();
			return cc.CompileAssemblyFromFileBatch (options, fileNames);
		}

		public virtual CompilerResults CompileAssemblyFromSource (CompilerParameters options, params string[] fileNames)
		{
			ICodeCompiler cc = CreateCompiler ();
			if (cc == null)
				throw new NotImplementedException ();
			return cc.CompileAssemblyFromSourceBatch (options, fileNames);
		}

		public virtual string CreateEscapedIdentifier (string value)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw new NotImplementedException ();
			return cg.CreateEscapedIdentifier (value);
		}

		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static CodeDomProvider CreateProvider (string language)
		{
			CompilerInfo ci = GetCompilerInfo (language);
			return (ci == null) ? null : ci.CreateProvider ();
		}

		public virtual string CreateValidIdentifier (string value)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw new NotImplementedException ();
			return cg.CreateValidIdentifier (value);
		}

		public virtual void GenerateCodeFromCompileUnit (CodeCompileUnit compileUnit, 
			TextWriter writer, CodeGeneratorOptions options)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw new NotImplementedException ();
			cg.GenerateCodeFromCompileUnit (compileUnit, writer, options);
		}

		public virtual void GenerateCodeFromExpression (CodeExpression expression, TextWriter writer, CodeGeneratorOptions options)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw new NotImplementedException ();
			cg.GenerateCodeFromExpression (expression, writer, options);
		}

		public virtual void GenerateCodeFromMember (CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
		{
			// Documented to always throw an exception (if not overriden)
			throw new NotImplementedException ();
			// Note: the pattern is different from other GenerateCodeFrom* because 
			// ICodeGenerator doesn't have a GenerateCodeFromMember member
		}

		public virtual void GenerateCodeFromNamespace (CodeNamespace codeNamespace, TextWriter writer, CodeGeneratorOptions options)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw new NotImplementedException ();
			cg.GenerateCodeFromNamespace (codeNamespace, writer, options);
		}

		public virtual void GenerateCodeFromStatement (CodeStatement statement, TextWriter writer, CodeGeneratorOptions options)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw new NotImplementedException ();
			cg.GenerateCodeFromStatement (statement, writer, options);
		}

		public virtual void GenerateCodeFromType (CodeTypeDeclaration codeType, TextWriter writer, CodeGeneratorOptions options)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw new NotImplementedException ();
			cg.GenerateCodeFromType (codeType, writer, options);
		}

		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static CompilerInfo[] GetAllCompilerInfo ()
		{
			int n = 0;
			if ((Config != null) && (Config.Compilers != null)) 
				n = Config.Compilers.Hash.Count;
			CompilerInfo[] ci = new CompilerInfo [n];
			if (n > 0)
				Config.Compilers.Hash.Values.CopyTo (ci, 0);
			return ci;
		}

		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static CompilerInfo GetCompilerInfo (string language)
		{
			if (language == null)
				throw new ArgumentNullException ("language");

			return (Config == null) ? null : Config.GetCompilerInfo (language);
		}

		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static string GetLanguageFromExtension (string extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");

			if (Config != null) {
				foreach (DictionaryEntry de in Config.Compilers.Hash) {
					CompilerInfo c = (CompilerInfo) de.Value;
					if (Array.IndexOf (c.GetExtensions (), extension) != -1)
						return (string) de.Key;
				}
			}
			return null;
		}

		public virtual string GetTypeOutput (CodeTypeReference type)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw new NotImplementedException ();
			return cg.GetTypeOutput (type);
		}

		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static bool IsDefinedExtension (string extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");

			if (Config != null) {
				foreach (CompilerInfo c in Config.Compilers.Hash.Values) {
					if (Array.IndexOf (c.GetExtensions (), extension) != -1)
						return true;
				}
			}
			return false;
		}

		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static bool IsDefinedLanguage (string language)
		{
			if (language == null)
				throw new ArgumentNullException ("language");

			if (Config == null)
				return false;
			return (Config.GetCompilerInfo (language) == null);
		}

		public virtual bool IsValidIdentifier (string value)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw new NotImplementedException ();
			return cg.IsValidIdentifier (value);
		}

		public virtual CodeCompileUnit Parse (TextReader codeStream)
		{
			ICodeParser cp = CreateParser ();
			if (cp == null)
				throw new NotImplementedException ();
			return cp.Parse (codeStream);
		}

		public virtual bool Supports (GeneratorSupport supports)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw new NotImplementedException ();
			return cg.Supports (supports);
		}

		static CompilationConfiguration Config {
			get { return ConfigurationSettings.GetConfig ("system.codedom") as CompilationConfiguration; }
		}
#endif
	}
}
