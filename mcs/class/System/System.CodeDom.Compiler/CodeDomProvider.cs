//
// System.CodeDom.Compiler.CodeDomProvider.cs
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2002 Ximian, Inc.
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

using System.ComponentModel;
using System.IO;

namespace System.CodeDom.Compiler
{
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
		public abstract ICodeCompiler CreateCompiler();

		public abstract ICodeGenerator CreateGenerator();
		
		public virtual ICodeGenerator CreateGenerator (string fileName)
		{
			return CreateGenerator();
		}

		public virtual ICodeGenerator CreateGenerator (TextWriter output)
		{
			return CreateGenerator();
		}

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
			return CreateCompiler ().CompileAssemblyFromDomBatch (options, compilationUnits);
		}

		public static CodeDomProvider CreateProvider (string language)
		{
			return GetCompilerInfo (language).CreateProvider ();
		}

		public virtual void GenerateCodeFromCompileUnit (CodeCompileUnit compileUnit, 
			TextWriter writer, CodeGeneratorOptions options)
		{
			CreateGenerator ().GenerateCodeFromCompileUnit (compileUnit, writer, options);
		}

		public virtual void GenerateCodeFromStatement (CodeStatement statement, TextWriter writer, CodeGeneratorOptions options)
		{
			CreateGenerator ().GenerateCodeFromStatement (statement, writer, options);
		}

		[MonoTODO ("Not implemented")]
		public static CompilerInfo GetCompilerInfo (string language)
		{
			return null;
		}

		public virtual bool Supports (GeneratorSupport supports)
		{
			return CreateGenerator ().Supports (supports);
		}

#endif

	}
}
