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
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.CodeDom.Compiler {

#if NET_2_0
	[ComVisible (true)]
#endif
	[ToolboxItem (false)]
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
				throw GetNotImplemented ();
			return cc.CompileAssemblyFromDomBatch (options, compilationUnits);
		}

		public virtual CompilerResults CompileAssemblyFromFile (CompilerParameters options, params string[] fileNames)
		{
			ICodeCompiler cc = CreateCompiler ();
			if (cc == null)
				throw GetNotImplemented ();
			return cc.CompileAssemblyFromFileBatch (options, fileNames);
		}

		public virtual CompilerResults CompileAssemblyFromSource (CompilerParameters options, params string[] fileNames)
		{
			ICodeCompiler cc = CreateCompiler ();
			if (cc == null)
				throw GetNotImplemented ();
			return cc.CompileAssemblyFromSourceBatch (options, fileNames);
		}

		public virtual string CreateEscapedIdentifier (string value)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw GetNotImplemented ();
			return cg.CreateEscapedIdentifier (value);
		}

#if CONFIGURATION_DEP
		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static CodeDomProvider CreateProvider (string language)
		{
			CompilerInfo ci = GetCompilerInfo (language);
			return (ci == null) ? null : ci.CreateProvider ();
		}
#if NET_4_0
		[ComVisible (false)]
		public static CodeDomProvider CreateProvider (string language, IDictionary<string, string> providerOptions)
		{
			CompilerInfo ci = GetCompilerInfo (language);
			return ci == null ? null : ci.CreateProvider (providerOptions);
		}
#endif

#endif
		public virtual string CreateValidIdentifier (string value)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw GetNotImplemented ();
			return cg.CreateValidIdentifier (value);
		}

		public virtual void GenerateCodeFromCompileUnit (CodeCompileUnit compileUnit, 
			TextWriter writer, CodeGeneratorOptions options)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw GetNotImplemented ();
			cg.GenerateCodeFromCompileUnit (compileUnit, writer, options);
		}

		public virtual void GenerateCodeFromExpression (CodeExpression expression, TextWriter writer, CodeGeneratorOptions options)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw GetNotImplemented ();
			cg.GenerateCodeFromExpression (expression, writer, options);
		}

		public virtual void GenerateCodeFromMember (CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
		{
			// Documented to always throw an exception (if not overriden)
			throw GetNotImplemented ();
			// Note: the pattern is different from other GenerateCodeFrom* because 
			// ICodeGenerator doesn't have a GenerateCodeFromMember member
		}

		public virtual void GenerateCodeFromNamespace (CodeNamespace codeNamespace, TextWriter writer, CodeGeneratorOptions options)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw GetNotImplemented ();
			cg.GenerateCodeFromNamespace (codeNamespace, writer, options);
		}

		public virtual void GenerateCodeFromStatement (CodeStatement statement, TextWriter writer, CodeGeneratorOptions options)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw GetNotImplemented ();
			cg.GenerateCodeFromStatement (statement, writer, options);
		}

		public virtual void GenerateCodeFromType (CodeTypeDeclaration codeType, TextWriter writer, CodeGeneratorOptions options)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw GetNotImplemented ();
			cg.GenerateCodeFromType (codeType, writer, options);
		}

#if CONFIGURATION_DEP
		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static CompilerInfo[] GetAllCompilerInfo ()
		{

			return (Config == null) ? null : Config.CompilerInfos;
		}


		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static CompilerInfo GetCompilerInfo (string language)
		{
			if (language == null)
				throw new ArgumentNullException ("language");
			if (Config == null)
				return null;
			CompilerCollection cc = Config.Compilers;
			return cc[language];
		}

		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static string GetLanguageFromExtension (string extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");

			if (Config != null) 
				return Config.Compilers.GetLanguageFromExtension (extension);
			return null;
		}
#endif

		public virtual string GetTypeOutput (CodeTypeReference type)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw GetNotImplemented ();
			return cg.GetTypeOutput (type);
		}

#if CONFIGURATION_DEP
		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static bool IsDefinedExtension (string extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");

			if (Config != null)
				return (Config.Compilers.GetCompilerInfoForExtension (extension) != null);
			
			return false;
		}

		[ComVisible (false)]
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		public static bool IsDefinedLanguage (string language)
		{
			if (language == null)
				throw new ArgumentNullException ("language");

			if (Config != null)
				return (Config.Compilers.GetCompilerInfoForLanguage (language) != null);

			return false;
		}
#endif

		public virtual bool IsValidIdentifier (string value)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw GetNotImplemented ();
			return cg.IsValidIdentifier (value);
		}

		public virtual CodeCompileUnit Parse (TextReader codeStream)
		{
			ICodeParser cp = CreateParser ();
			if (cp == null)
				throw GetNotImplemented ();
			return cp.Parse (codeStream);
		}

		public virtual bool Supports (GeneratorSupport supports)
		{
			ICodeGenerator cg = CreateGenerator ();
			if (cg == null)
				throw GetNotImplemented ();
			return cg.Supports (supports);
		}

#if CONFIGURATION_DEP
		static CodeDomConfigurationHandler Config {
			get { return ConfigurationManager.GetSection ("system.codedom") as CodeDomConfigurationHandler; }
		}
#endif
		
		//
		// This is used to prevent confusing Moma about methods not implemented.
		//
		Exception GetNotImplemented ()
		{
			return new NotImplementedException ();
		}		
#endif
	}
}
