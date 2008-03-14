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
	public static class SymbolWriter
	{
		public static bool HasSymbolWriter {
			get { return symwriter != null; }
		}

		private static SymbolWriterImpl symwriter;

		protected class SymbolWriterImpl : MonoSymbolWriter {
			delegate int GetILOffsetFunc (ILGenerator ig);
			delegate Guid GetGuidFunc (ModuleBuilder mb);

			GetILOffsetFunc get_il_offset_func;
			GetGuidFunc get_guid_func;

			ModuleBuilder module_builder;

			public SymbolWriterImpl (ModuleBuilder module_builder, string filename)
				: base (filename)
			{
				this.module_builder = module_builder;
			}

			public int GetILOffset (ILGenerator ig)
			{
				return get_il_offset_func (ig);
			}

			public void WriteSymbolFile ()
			{
				Guid guid = get_guid_func (module_builder);
				WriteSymbolFile (guid);
			}

			public bool Initialize ()
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

				return true;
			}
		}

		public static void DefineLocalVariable (string name, LocalBuilder builder)
		{
			if (symwriter != null) {
				int index = MonoDebuggerSupport.GetLocalIndex (builder);
				symwriter.DefineLocalVariable (index, name);
			}
		}

		public static void OpenMethod (ISourceFile file, ISourceMethod method,
					       int start_row, int start_column,
					       int end_row, int end_column)
		{
			if (symwriter != null)
				symwriter.OpenMethod (file, method, start_row, start_column,
						      end_row, end_column);
		}

		public static void CloseMethod ()
		{
			if (symwriter != null)
				symwriter.CloseMethod ();
		}

		public static int OpenScope (ILGenerator ig)
		{
			if (symwriter != null) {
				int offset = symwriter.GetILOffset (ig);
				return symwriter.OpenScope (offset);
			} else {
				return -1;
			}
		}

		public static void CloseScope (ILGenerator ig)
		{
			if (symwriter != null) {
				int offset = symwriter.GetILOffset (ig);
				symwriter.CloseScope (offset);
			}
		}

		public static int DefineNamespace (string name, SourceFileEntry source,
						   string[] using_clauses, int parent)
		{
			if (symwriter != null)
				return symwriter.DefineNamespace (name, source, using_clauses, parent);
			else
				return -1;
		}

		public static void MarkSequencePoint (ILGenerator ig, int row, int column)
		{
			if (symwriter != null) {
				int offset = symwriter.GetILOffset (ig);
				symwriter.MarkSequencePoint (offset, row, column);
			}
		}

		public static void WriteSymbolFile ()
		{
			if (symwriter != null)
				symwriter.WriteSymbolFile ();
		}

		public static bool Initialize (ModuleBuilder module, string filename)
		{
			symwriter = new SymbolWriterImpl (module, filename);
			if (!symwriter.Initialize ()) {
				symwriter = null;
				return false;
			}

			return true;
		}
	}
}
