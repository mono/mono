//
// System.Diagnostics.SymbolStore/IMonoSymbolWriter.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// This interface is derived from System.Diagnostics.SymbolStore.ISymbolWriter.
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;
using System.Collections;
using System.IO;
	
namespace Mono.CSharp.Debugger
{
	public interface IMonoSymbolWriter : ISymbolWriter
	{
		// The ISymbolWriter interface has an `IntPtr emitter' argument which
		// seems to be a pointer an unmanaged interface containing the actual
		// symbol writer. I was unable to find any documentation about how
		// exactly this is used - but it seems to be in some proprietary,
		// undocumented DLL.
		//
		// Since I want this interface to be usable on the Windows platform as
		// well, I added this custom constructor. You should use this version
		// of `Initialize' to make sure you're actually using this implementation.
		void Initialize (ModuleBuilder module_builder, string filename);
	}

	public interface ISourceFile
	{
		string FileName {
			get;
		}

		ISourceMethod[] Methods {
			get;
		}

		void AddMethod (ISourceMethod method);
	}

	public interface ISourceMethod
	{
		ISourceLine[] Lines {
			get;
		}

		void AddLine (ISourceLine line);

		ISourceBlock[] Blocks {
			get;
		}

		ILocalVariable[] Locals {
			get;
		}

		void AddLocal (ILocalVariable local);


		ISourceLine Start {
			get;
		}

		ISourceLine End {
			get;
		}

		int Token {
			get;
		}

		MethodInfo MethodInfo {
			get;
		}

		ISourceFile SourceFile {
			get;
		}
	}

	public interface ISourceBlock
	{
		ISourceMethod SourceMethod {
			get;
		}

		ILocalVariable[] Locals {
			get;
		}

		void AddLocal (ILocalVariable local);

		ISourceLine Start {
			get;
		}

		ISourceLine End {
			get;
		}

		int ID {
			get;
		}
	}

	public enum SourceOffsetType
	{
		OFFSET_NONE,
		OFFSET_IL,
		OFFSET_LOCAL,
		OFFSET_PARAMETER
	}

	public interface ISourceLine
	{
		SourceOffsetType OffsetType {
			get;
		}

		int Offset {
			get;
		}

		int Row {
			get;
		}

		int Column {
			get;
		}
	}

	public interface IVariable
	{
		string Name {
			get;
		}

		ISourceLine Line {
			get;
		}

		byte[] Signature {
			get;
		}

		Type Type {
			get;
		}

		int Token {
			get;
		}

		int Index {
			get;
		}
	}

	public interface ILocalVariable : IVariable
	{ }

	public interface IMethodParameter : IVariable
	{ }
}
