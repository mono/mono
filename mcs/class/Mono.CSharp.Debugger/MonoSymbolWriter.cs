//
// System.Diagnostics.SymbolStore/MonoSymbolWriter.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// This is the default implementation of the System.Diagnostics.SymbolStore.ISymbolWriter
// interface.
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

	public class MonoSymbolWriter : IMonoSymbolWriter
	{
		protected string output_filename = null;
		protected Hashtable methods = null;
		protected Hashtable sources = null;

		protected class MySourceInfo
		{
			private MyMethodInfo[] _methods;
			public readonly string FileName;

			public MySourceInfo (string filename)
			{
				this.FileName = filename;

				this._methods = new MyMethodInfo [0];
			}

			public MyMethodInfo[] GetMethods ()
			{
				return _methods;
			}

			public void AddMethod (MyMethodInfo method)
			{
				int i = _methods.Length;
				MyMethodInfo[] new_m = new MyMethodInfo [i + 1];
				Array.Copy (_methods, new_m, i);
				new_m [i] = method;
				_methods = new_m;
			}
		}

		protected struct MyMethodInfo
		{
			public MyMethodInfo (MethodInfo method_info, MySourceInfo source_file) {
				this.MethodInfo = method_info;
				this.SourceFile = source_file;
			}

			public void SetSourceRange (int startLine, int startColumn,
						    int endLine, int endColumn)
			{
			}

			public readonly MethodInfo MethodInfo;
			public readonly MySourceInfo SourceFile;
		}

		protected Object current_method = null;

		//
		// Interface IMonoSymbolWriter
		//

		public MonoSymbolWriter ()
		{
			methods = new Hashtable ();
			sources = new Hashtable ();
		}

		public void Close () {
			CreateDwarfFile (output_filename);
		}

		public void CloseNamespace () {
		}

		public void CloseScope (int endOffset) {
		}

		// Create and return a new IMonoSymbolDocumentWriter.
		public ISymbolDocumentWriter DefineDocument (string url,
							     Guid language,
							     Guid languageVendor,
							     Guid documentType)
		{
			return new MonoSymbolDocumentWriter (url);
		}

		public void DefineField (
			SymbolToken parent,
			string name,
			FieldAttributes attributes,
			byte[] signature,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3)
		{
		}

		public void DefineGlobalVariable (
			string name,
			FieldAttributes attributes,
			byte[] signature,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3)
		{
		}

		public void DefineLocalVariable (string name,
						 FieldAttributes attributes,
						 byte[] signature,
						 SymAddressKind addrKind,
						 int addr1,
						 int addr2,
						 int addr3,
						 int startOffset,
						 int endOffset)
		{
			Console.WriteLine ("WRITE LOCAL: " + name + " " + addr1);
		}

		public void DefineParameter (
			string name,
			ParameterAttributes attributes,
			int sequence,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3)
		{
		}

		public void DefineSequencePoints (
			ISymbolDocumentWriter document,
			int[] offsets,
			int[] lines,
			int[] columns,
			int[] endLines,
			int[] endColumns)
		{
		}

		public void Initialize (IntPtr emitter, string filename, bool fFullBuild)
		{
			throw new NotSupportedException ("Please use the 'Initialize (string filename)' "
							 + "constructor and read the documentation in "
							 + "Mono.CSharp.Debugger/IMonoSymbolWriter.cs");
		}

		// This is documented in IMonoSymbolWriter.cs
		public void Initialize (string filename)
		{
			this.output_filename = filename;
		}

		public void OpenMethod (SymbolToken method)
		{
			// do nothing
		}

		// This is documented in IMonoSymbolWriter.cs
		public void OpenMethod (SymbolToken symbol_token, MethodInfo method_info,
					string source_file)
		{
			int token = symbol_token.GetToken ();
			MySourceInfo source_info;

			if (methods.ContainsKey (token))
				methods.Remove (token);

			if (sources.ContainsKey (source_file))
				source_info = (MySourceInfo) sources [source_file];
			else {
				source_info = new MySourceInfo (source_file);
				sources.Add (source_file, source_info);
			}

			current_method = new MyMethodInfo (method_info, source_info);

			source_info.AddMethod ((MyMethodInfo) current_method);

			methods.Add (token, current_method);

			OpenMethod (symbol_token);
		}

		public void SetMethodSourceRange (ISymbolDocumentWriter startDoc,
						  int startLine, int startColumn,
						  ISymbolDocumentWriter endDoc,
						  int endLine, int endColumn)
		{
			if ((startDoc == null) || (endDoc == null))
				throw new NullReferenceException ();

			if (!(startDoc is MonoSymbolDocumentWriter) || !(endDoc is MonoSymbolDocumentWriter))
				throw new NotSupportedException ("both startDoc and endDoc must be of type "
								 + "MonoSymbolDocumentWriter");

			if (!startDoc.Equals (endDoc))
				throw new NotSupportedException ("startDoc and endDoc must be the same");

			if (current_method != null)
				((MyMethodInfo) current_method).SetSourceRange (startLine, startColumn,
									      endLine, endColumn);
		}

		public void CloseMethod () {
			current_method = null;
		}

		public void OpenNamespace (string name)
		{
		}

		public int OpenScope (int startOffset)
		{
			throw new NotImplementedException ();
		}

		public void SetScopeRange (int scopeID, int startOffset, int endOffset)
		{
		}

		public void SetSymAttribute (SymbolToken parent, string name, byte[] data)
		{
		}

		public void SetUnderlyingWriter (IntPtr underlyingWriter)
		{
		}

		public void SetUserEntryPoint (SymbolToken entryMethod)
		{
		}

		public void UsingNamespace (string fullName)
		{
		}

		//
		// MonoSymbolWriter implementation
		//
		protected void WriteMethod (DwarfFileWriter.DieCompileUnit parent_die, MyMethodInfo method)
		{
			Console.WriteLine ("WRITING METHOD: " + method.MethodInfo.Name);

			DwarfFileWriter.DieSubProgram die;

			die = new DwarfFileWriter.DieSubProgram (parent_die, method.MethodInfo);
		}

		protected void WriteSource (DwarfFileWriter writer, MySourceInfo source)
		{
			Console.WriteLine ("WRITING SOURCE: " + source.FileName);

			DwarfFileWriter.CompileUnit compile_unit = new DwarfFileWriter.CompileUnit (
				writer, source.FileName);

			DwarfFileWriter.DieCompileUnit die = new DwarfFileWriter.DieCompileUnit (compile_unit);

			foreach (MyMethodInfo method in source.GetMethods ())
				WriteMethod (die, method);
		}

		protected void CreateDwarfFile (string filename)
		{
			Console.WriteLine ("WRITING DWARF FILE: " + filename);

			DwarfFileWriter writer = new DwarfFileWriter (filename);

			foreach (MySourceInfo source in sources.Values)
				WriteSource (writer, source);

			writer.Close ();

			Console.WriteLine ("DONE WRITING DWARF FILE");

		}
	}
}
