//
// AssemblyStripper.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.IO;

using Mono.Cecil.Binary;
using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;

namespace Mono.Cecil {

	class AssemblyStripper {

		AssemblyDefinition assembly;
		BinaryWriter writer;

		Image original;
		Image stripped;

		ReflectionWriter reflection_writer;
		MetadataWriter metadata_writer;

		TablesHeap original_tables;
		TablesHeap stripped_tables;

		AssemblyStripper (AssemblyDefinition assembly, BinaryWriter writer)
		{
			this.assembly = assembly;
			this.writer = writer;
		}

		void Strip ()
		{
			FullLoad ();
			ClearMethodBodies ();
			CopyOriginalImage ();
			PatchMethods ();
			PatchFields ();
			PatchResources ();
			Write ();
		}

		void FullLoad ()
		{
			assembly.MainModule.FullLoad ();
		}

		void ClearMethodBodies ()
		{
			foreach (TypeDefinition type in assembly.MainModule.Types) {
				ClearMethodBodies (type.Constructors);
				ClearMethodBodies (type.Methods);
			}
		}

		static void ClearMethodBodies (ICollection methods)
		{
			foreach (MethodDefinition method in methods) {
				if (!method.HasBody)
					continue;

				method.Body.ExceptionHandlers.Clear();
				method.Body.Variables.Clear ();
				method.Body.Instructions.Clear ();
				method.Body.CilWorker.Emit (OpCodes.Ret);
			}
		}

		void CopyOriginalImage ()
		{
			original = assembly.MainModule.Image;
			stripped = Image.CreateImage();

			stripped.Accept (new CopyImageVisitor (original));

			assembly.MainModule.Image = stripped;

			original_tables = original.MetadataRoot.Streams.TablesHeap;
			stripped_tables = stripped.MetadataRoot.Streams.TablesHeap;

			TableCollection tables = original_tables.Tables;
			foreach (IMetadataTable table in tables)
				stripped_tables.Tables.Add(table);

			stripped_tables.Valid = original_tables.Valid;
			stripped_tables.Sorted = original_tables.Sorted;

			reflection_writer = new ReflectionWriter (assembly.MainModule);
			reflection_writer.StructureWriter = new StructureWriter (assembly, writer);
			reflection_writer.CodeWriter.Stripped = true;

			metadata_writer = reflection_writer.MetadataWriter;

			PatchHeap (metadata_writer.StringWriter, original.MetadataRoot.Streams.StringsHeap);
			PatchHeap (metadata_writer.GuidWriter, original.MetadataRoot.Streams.GuidHeap);
			PatchHeap (metadata_writer.UserStringWriter, original.MetadataRoot.Streams.UserStringsHeap);
			PatchHeap (metadata_writer.BlobWriter, original.MetadataRoot.Streams.BlobHeap);

			if (assembly.EntryPoint != null)
				metadata_writer.EntryPointToken = assembly.EntryPoint.MetadataToken.ToUInt ();
		}

		static void PatchHeap (MemoryBinaryWriter heap_writer, MetadataHeap heap)
		{
			if (heap == null)
				return;

			heap_writer.BaseStream.Position = 0;
			heap_writer.Write (heap.Data);
		}

		void PatchMethods ()
		{
			MethodTable methodTable = (MethodTable) stripped_tables [MethodTable.RId];
			if (methodTable == null)
				return;

			for (int i = 0; i < methodTable.Rows.Count; i++) {
				MethodRow methodRow = methodTable[i];

				MetadataToken methodToken = MetadataToken.FromMetadataRow (TokenType.Method, i);

				MethodDefinition method = (MethodDefinition) assembly.MainModule.LookupByToken (methodToken);

				methodRow.RVA = reflection_writer.CodeWriter.WriteMethodBody (method);
			}
		}

		void PatchFields ()
		{
			FieldRVATable fieldRvaTable = (FieldRVATable) stripped_tables [FieldRVATable.RId];
			if (fieldRvaTable == null)
				return;

			for (int i = 0; i < fieldRvaTable.Rows.Count; i++) {
				FieldRVARow fieldRvaRow = fieldRvaTable [i];

				MetadataToken fieldToken = new MetadataToken (TokenType.Field, fieldRvaRow.Field);

				FieldDefinition field = (FieldDefinition) assembly.MainModule.LookupByToken (fieldToken);

				fieldRvaRow.RVA = metadata_writer.GetDataCursor ();
				metadata_writer.AddData (field.InitialValue.Length + 3 & (~3));
				metadata_writer.AddFieldInitData (field.InitialValue);
			}
		}

		void PatchResources ()
		{
			ManifestResourceTable resourceTable = (ManifestResourceTable) stripped_tables [ManifestResourceTable.RId];
			if (resourceTable == null)
				return;

			for (int i = 0; i < resourceTable.Rows.Count; i++) {
				ManifestResourceRow resourceRow = resourceTable [i];

				if (resourceRow.Implementation.RID != 0)
					continue;

				foreach (Resource resource in assembly.MainModule.Resources) {
					EmbeddedResource er = resource as EmbeddedResource;
					if (er == null)
						continue;

					if (resource.Name != original.MetadataRoot.Streams.StringsHeap [resourceRow.Name])
						continue;

					resourceRow.Offset = metadata_writer.AddResource (er.Data);
				}
			}
		}

		void Write ()
		{
			stripped.MetadataRoot.Accept (metadata_writer);
		}

		public static void StripAssembly (AssemblyDefinition assembly, string file)
		{
			using (FileStream fs = new FileStream (file, FileMode.Create, FileAccess.Write, FileShare.None)) {
				new AssemblyStripper (assembly, new BinaryWriter (fs)).Strip ();
			}
		}
	}
}
