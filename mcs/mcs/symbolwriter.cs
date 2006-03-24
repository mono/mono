//
// symbolwriter.cs: The symbol writer
//
// Author:
//   Martin Baulig (martin@ximian.com)
//
// (C) 2003 Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {
	public class SymbolWriter : MonoSymbolWriter {
		delegate int GetILOffsetFunc (ILGenerator ig);
		delegate Guid GetGuidFunc (ModuleBuilder mb);

		GetILOffsetFunc get_il_offset_func;
		GetGuidFunc get_guid_func;

		ModuleBuilder module_builder;

		protected SymbolWriter (ModuleBuilder module_builder, string filename)
			: base (filename)
		{
			this.module_builder = module_builder;
		}

		bool Initialize ()
		{
			MethodInfo mi = typeof (ILGenerator).GetMethod (
				"Mono_GetCurrentOffset",
				BindingFlags.Static | BindingFlags.NonPublic);
			if (mi == null)
				return false;

			get_il_offset_func = (GetILOffsetFunc) System.Delegate.CreateDelegate (
				typeof (GetILOffsetFunc), mi);

			mi = typeof (ModuleBuilder).GetMethod (
				"Mono_GetGuid",
				BindingFlags.Static | BindingFlags.NonPublic);
			if (mi == null)
				return false;

			get_guid_func = (GetGuidFunc) System.Delegate.CreateDelegate (
				typeof (GetGuidFunc), mi);

			Location.DefineSymbolDocuments (this);
			Namespace.DefineNamespaces (this);

			return true;
		}

		public void DefineLocalVariable (string name, LocalBuilder builder)
		{
			SignatureHelper sighelper = SignatureHelper.GetLocalVarSigHelper (
				module_builder);
			sighelper.AddArgument (builder.LocalType);
			byte[] signature = sighelper.GetSignature ();

			DefineLocalVariable (name, signature);
		}

		public int OpenScope (ILGenerator ig)
		{
			int offset = get_il_offset_func (ig);
			return OpenScope (offset);
		}

		public void CloseScope (ILGenerator ig)
		{
			int offset = get_il_offset_func (ig);
			CloseScope (offset);
		}

		public void MarkSequencePoint (ILGenerator ig, int row, int column)
		{
			int offset = get_il_offset_func (ig);
			MarkSequencePoint (offset, row, column);
		}

		public void WriteSymbolFile ()
		{
			Guid guid = get_guid_func (module_builder);
			WriteSymbolFile (guid);
		}

		public static SymbolWriter GetSymbolWriter (ModuleBuilder module,
							    string filename)
		{
			SymbolWriter writer = new SymbolWriter (module, filename);
			if (!writer.Initialize ())
				return null;

			return writer;
		}
	}
}
