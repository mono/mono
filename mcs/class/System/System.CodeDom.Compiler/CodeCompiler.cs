//
// System.CodeDom.Compiler.CodeCompiler
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved
//

namespace System.CodeDom.Compiler {

	public abstract class CodeCompiler : CodeGenerator, ICodeCompiler
	{

		[MonoTODO]
		protected CodeCompiler ()
		{
			throw new NotImplementedException ();
		}

		protected abstract string CompilerName {
			get;
		}
	
		protected abstract string FileExtension {
			get;
		}

		protected abstract string CmdArgsFromParameters (
			CompilerParameters options);

		[MonoTODO]
		protected virtual CompilerResults FromDom (
			CompilerParameters options, CodeCompileUnit e)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		protected virtual CompilerResults FromDomBatch(
			CompilerParameters options,CodeCompileUnit[] ea)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual CompilerResults FromFile(
			CompilerParameters options,string fileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual CompilerResults FromFileBatch(
			CompilerParameters options,string[] fileNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual CompilerResults FromSource(
			CompilerParameters options,string source)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual CompilerResults FromSourceBatch(
			CompilerParameters options,string[] sources)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		protected virtual string GetResponseFileCmdArgs(
			CompilerParameters options,string cmdArgs)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		CompilerResults ICodeCompiler.CompileAssemblyFromDom(
			CompilerParameters options,CodeCompileUnit e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		CompilerResults ICodeCompiler.CompileAssemblyFromDomBatch(
			CompilerParameters options,CodeCompileUnit[] ea)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		CompilerResults ICodeCompiler.CompileAssemblyFromFile(
			CompilerParameters options, string fileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		CompilerResults ICodeCompiler.CompileAssemblyFromFileBatch(
			CompilerParameters options, string[] fileNames)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		CompilerResults ICodeCompiler.CompileAssemblyFromSource(
			CompilerParameters options, string source)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		CompilerResults ICodeCompiler.CompileAssemblyFromSourceBatch(
			CompilerParameters options, string[] sources)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static string JoinStringArray(string[] sa, 
			string separator)
		{
			throw new NotImplementedException ();
		}

		protected abstract void ProcessCompilerOutputLine(
			CompilerResults results, string line);

	}
}

