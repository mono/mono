using System;
using System.IO;

using Mono.PEToolkit;
using Mono.PEToolkit.Metadata;


public sealed class MDDump {

	private MDDump() {}


	public static uint Dump (string peFile)
	{
		using (Image pe = new Image (peFile)) {
			pe.Open ();
			pe.ReadHeaders ();

			Console.WriteLine (pe);

			if (pe.IsCLI) {
				pe.DumpStreamHeader("#~");
				pe.DumpStreamHeader("#-");
				pe.DumpStreamHeader("#Strings");
				pe.DumpStreamHeader("#US");
				pe.DumpStreamHeader("#GUID");
				pe.DumpStreamHeader("#Blob");

				Console.WriteLine("CLI image detected, dumping metadata tables.");
				TablesHeap tabs = pe.MetaDataRoot.TablesHeap;

				foreach (MDTable t in tabs.Tables) {
					t.Dump (Console.Out);
				}

				/*		
				MethodIL il = pe.MetaDataRoot.GetMethodBody(1);
				Console.WriteLine(il);
				il.DumpHexBytecode(Console.Out);
				*/
			}

			FileStream out_file = new FileStream ("out.dll", FileMode.Create);
			BinaryWriter binary_writer = new BinaryWriter (out_file);
			pe.WriteHeaders (binary_writer);
			out_file.Close ();
		}

		return 0;
	}





	public static void Main (string [] args) {
		if (args.Length == 0) {
			Console.WriteLine ("mddump <PE file>");
		} else {
			Dump (args [0]);
		}
	}

}
