//
// Mono.ILASM.DebuggingInfo.cs
//
// Author(s):
//  Martin Baulig (martin@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using PEAPI;
using System;
using System.IO;
using System.Collections;
using Mono.CSharp.Debugger;

namespace Mono.ILASM {

	public class SymbolWriter
	{
		string filename;
		MonoSymbolFile file;
		ArrayList sources;
		SourceMethod current_method;
		SourceFile current_source;

		public SymbolWriter (string filename)
		{
			this.filename = filename;

			file = new MonoSymbolFile ();

			sources = new ArrayList ();
		}

		public SourceMethod BeginMethod (MethodDef method, Location location)
		{
			current_method = new SourceMethod (method, location);
			current_source.AddMethod (current_method);
			return current_method;
		}

		public void EndMethod (Location location)
		{
			current_method.EndLine = location.line;
			current_method = null;
		}

		public void BeginSourceFile (string filename)
		{
			current_source = new SourceFile (file, filename);
			sources.Add (current_source);
		}

		public void EndSourceFile ()
		{
			current_source = null;
		}

		public void Write ()
		{
			foreach (SourceFile source in sources)
				source.Write ();

			using (FileStream stream = new FileStream (
				       filename, FileMode.Create, FileAccess.Write)) {
				byte[] data = file.CreateSymbolFile ();
				stream.Write (data, 0, data.Length);
			}
		}
	}

	public class SourceFile : SourceFileEntry
	{
		private ArrayList methods = new ArrayList ();

		public SourceFile (MonoSymbolFile file, string filename)
			: base (file, filename)
		{ }

		public void AddMethod (SourceMethod method)
		{
			methods.Add (method);
		}

		public void Write ()
		{
			foreach (SourceMethod method in methods)
				method.Write (this);
		}
	}

	public class SourceMethod
	{
		MethodDef method;
		ArrayList lines;
		public int StartLine, EndLine;

		public SourceMethod (MethodDef method, Location start)
		{
			this.method = method;
			this.StartLine = start.line;

			lines = new ArrayList ();
			MarkLocation (start.line, 0);
		}

		public void MarkLocation (int line, uint offset)
		{
			lines.Add (new LineNumberEntry (line, (int) offset));
		}

		public void Write (SourceFile file)
		{
			PEAPI.MethodDef pemethod = method.PeapiMethodDef;

			LineNumberEntry[] lne = new LineNumberEntry [lines.Count];
			lines.CopyTo (lne);

			int num_params = method.ParamTypeList ().Length;
			uint token = ((uint) PEAPI.MDTable.Method << 24) | pemethod.Row;

			file.DefineMethod (
				method.Name, (int) token, num_params, null, lne, null,
				StartLine, EndLine, 0);
		}
	}
}
