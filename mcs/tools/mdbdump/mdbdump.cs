using System;
using System.IO;
using Mono.CompilerServices.SymbolWriter;
using Mono.Cecil;
using System.Text;
using System.Xml;
using System.Globalization;

public class MdbDump
{
	public static int Main (String[] args)
	{
		if (args.Length < 1) {
			Console.Error.WriteLine ("Usage: mdbdump <assembly>");
			return 1;
		}

		using (var assembly = AssemblyDefinition.ReadAssembly (args[0])) {

			var f = MonoSymbolFile.ReadSymbolFile (args[0] + ".mdb");
			DumpSymbolFile (assembly, f, Console.Out);
		}

		return 0;
	}

	static string ChecksumToString (byte[] checksum)
	{
		var sb = new StringBuilder (checksum.Length * 2);
		for (int i = 0; i < checksum.Length; i++) {
			sb.Append ("0123456789abcdef"[checksum[i] >> 4]);
			sb.Append ("0123456789abcdef"[checksum[i] & 0x0F]);
		}

		return sb.ToString ();
	}

	static string IntToHex (int value)
	{
		return "0x" + value.ToString ("x", CultureInfo.InvariantCulture);
	}

	static void DumpSymbolFile (AssemblyDefinition assembly, MonoSymbolFile symbolFile, TextWriter output)
	{
		using (XmlTextWriter writer = new XmlTextWriter (output)) {
			writer.Formatting = Formatting.Indented;

			writer.WriteStartDocument ();

			writer.WriteStartElement ("symbols");
			writer.WriteAttributeString ("guid", symbolFile.Guid.ToString ());

			writer.WriteStartElement ("files");
			foreach (var file in symbolFile.Sources) {
				writer.WriteStartElement ("file");
				writer.WriteAttributeString ("id", file.Index.ToString ());
				writer.WriteAttributeString ("name", Path.GetFileName (file.FileName));
				var checksum = file.Checksum;
				if (checksum != null)
					writer.WriteAttributeString ("checksum", ChecksumToString (checksum));

				writer.WriteEndElement ();
			}
			writer.WriteEndElement ();

			writer.WriteStartElement ("methods");
			foreach (var method in symbolFile.Methods) {
				writer.WriteStartElement ("method");
				writer.WriteAttributeString ("token", IntToHex (method.Token));
				writer.WriteAttributeString ("signature", assembly.MainModule.LookupToken (method.Token).ToString ());

				var il_entries = method.GetLineNumberTable ();
				writer.WriteStartElement ("sequencepoints");
				foreach (var entry in il_entries.LineNumbers) {
					writer.WriteStartElement ("entry");
					writer.WriteAttributeString ("il", IntToHex (entry.Offset));
					writer.WriteAttributeString ("row", entry.Row.ToString ());
					writer.WriteAttributeString ("col", entry.Column.ToString ());
					if (entry.EndRow != -1 || entry.EndColumn != -1) {
						writer.WriteAttributeString ("end_row", entry.EndRow.ToString ());
						writer.WriteAttributeString ("end_col", entry.EndColumn.ToString ());
					}
					writer.WriteAttributeString ("file_ref", entry.File.ToString ());
					writer.WriteEndElement ();
				}
				writer.WriteEndElement ();

				writer.WriteStartElement ("locals");
				foreach (var local in method.GetLocals ()) {
					writer.WriteStartElement ("entry");
					writer.WriteAttributeString ("name", local.Name);
					writer.WriteAttributeString ("il_index", local.Index.ToString ());
					writer.WriteAttributeString ("scope_ref", local.BlockIndex.ToString ());
					writer.WriteEndElement ();
				}
				writer.WriteEndElement ();

				writer.WriteStartElement ("scopes");
				foreach (var scope in method.GetCodeBlocks ()) {
					writer.WriteStartElement ("entry");
					writer.WriteAttributeString ("index", scope.Index.ToString ());
					writer.WriteAttributeString ("start", IntToHex (scope.StartOffset));
					writer.WriteAttributeString ("end", IntToHex (scope.EndOffset));
					writer.WriteEndElement ();
				}
				writer.WriteEndElement ();

				writer.WriteEndElement ();
			}
			writer.WriteEndElement ();

			writer.WriteEndElement ();
			writer.WriteEndDocument ();
		}
	}
}
