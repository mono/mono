//
// symbolwriter.cs: The debug symbol writer
//
// Authors: Martin Baulig (martin@ximian.com)
//          Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2010 Novell, Inc
//

using System;
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

		class SymbolWriterImpl : MonoSymbolWriter {
#if !NET_4_0
			delegate int GetILOffsetFunc (ILGenerator ig);
			GetILOffsetFunc get_il_offset_func;

			delegate Guid GetGuidFunc (ModuleBuilder mb);
			GetGuidFunc get_guid_func;
#endif

			ModuleBuilder module_builder;

			public SymbolWriterImpl (ModuleBuilder module_builder, string filename)
				: base (filename)
			{
				this.module_builder = module_builder;
			}

			public int GetILOffset (ILGenerator ig)
			{
#if NET_4_0
				return ig.ILOffset;
#else
				return get_il_offset_func (ig);
#endif
			}

			public void WriteSymbolFile ()
			{
				Guid guid;
#if NET_4_0
				guid = module_builder.ModuleVersionId;
#else
				guid = get_guid_func (module_builder);
#endif
				WriteSymbolFile (guid);
			}

			public bool Initialize ()
			{
#if !NET_4_0
				MethodInfo mi;

				mi = typeof (ILGenerator).GetMethod (
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
#endif

				Location.DefineSymbolDocuments (this);
				return true;
			}
		}

		public static void DefineLocalVariable (string name, LocalBuilder builder)
		{
			if (symwriter != null) {
				symwriter.DefineLocalVariable (builder.LocalIndex, name);
			}
		}

		public static SourceMethodBuilder OpenMethod (ICompileUnit file, int ns_id,
							      IMethodDef method)
		{
			if (symwriter != null)
				return symwriter.OpenMethod (file, ns_id, method);
			else
				return null;
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

		public static int DefineNamespace (string name, CompileUnitEntry source,
						   string[] using_clauses, int parent)
		{
			if (symwriter != null)
				return symwriter.DefineNamespace (name, source, using_clauses, parent);
			else
				return -1;
		}

		public static void DefineAnonymousScope (int id)
		{
			if (symwriter != null)
				symwriter.DefineAnonymousScope (id);
		}

		public static void DefineScopeVariable (int scope, LocalBuilder builder)
		{
			if (symwriter != null) {
				symwriter.DefineScopeVariable (scope, builder.LocalIndex);
			}
		}

		public static void DefineScopeVariable (int scope)
		{
			if (symwriter != null)
				symwriter.DefineScopeVariable (scope, -1);
		}

		public static void DefineCapturedLocal (int scope_id, string name,
							string captured_name)
		{
			if (symwriter != null)
				symwriter.DefineCapturedLocal (scope_id, name, captured_name);
		}

		public static void DefineCapturedParameter (int scope_id, string name,
							    string captured_name)
		{
			if (symwriter != null)
				symwriter.DefineCapturedParameter (scope_id, name, captured_name);
		}

		public static void DefineCapturedThis (int scope_id, string captured_name)
		{
			if (symwriter != null)
				symwriter.DefineCapturedThis (scope_id, captured_name);
		}

		public static void DefineCapturedScope (int scope_id, int id, string captured_name)
		{
			if (symwriter != null)
				symwriter.DefineCapturedScope (scope_id, id, captured_name);
		}

		public static void OpenCompilerGeneratedBlock (EmitContext ec)
		{
			if (symwriter != null) {
				int offset = symwriter.GetILOffset (ec.ig);
				symwriter.OpenCompilerGeneratedBlock (offset);
			}
		}

		public static void CloseCompilerGeneratedBlock (EmitContext ec)
		{
			if (symwriter != null) {
				int offset = symwriter.GetILOffset (ec.ig);
				symwriter.CloseCompilerGeneratedBlock (offset);
			}
		}

		public static void StartIteratorBody (EmitContext ec)
		{
			if (symwriter != null) {
				int offset = symwriter.GetILOffset (ec.ig);
				symwriter.StartIteratorBody (offset);
			}
		}

		public static void EndIteratorBody (EmitContext ec)
		{
			if (symwriter != null) {
				int offset = symwriter.GetILOffset (ec.ig);
				symwriter.EndIteratorBody (offset);
			}
		}

		public static void StartIteratorDispatcher (EmitContext ec)
		{
			if (symwriter != null) {
				int offset = symwriter.GetILOffset (ec.ig);
				symwriter.StartIteratorDispatcher (offset);
			}
		}

		public static void EndIteratorDispatcher (EmitContext ec)
		{
			if (symwriter != null) {
				int offset = symwriter.GetILOffset (ec.ig);
				symwriter.EndIteratorDispatcher (offset);
			}
		}

		public static void MarkSequencePoint (ILGenerator ig, Location loc)
		{
			if (symwriter != null) {
				SourceFileEntry file = loc.SourceFile.SourceFileEntry;
				int offset = symwriter.GetILOffset (ig);
				symwriter.MarkSequencePoint (
					offset, file, loc.Row, loc.Column, loc.Hidden);
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

		public static void Reset ()
		{
			symwriter = null;
		}
	}
}
