using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using WebAssembly;
using WebAssembly.Core;

public class HelloWorld
{
	public unsafe static void Dump (ref byte buf, int len, string s) {
		var arr = new byte [len];
		fixed (void *p = &buf) {
			var span = new ReadOnlySpan<byte> (p, len);

			// Send it to JS
			try {
				var js_dump = (JSObject)Runtime.GetGlobalObject ("AotProfileData");
				js_dump.SetObjectProperty ("data", Uint8Array.From (span));
			} catch (Exception ex) {
				Console.WriteLine (ex);
				Environment.Exit (1);
			}
		}
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void StopProfile () {
	}

	public static void Main (String[] args) {
		Console.WriteLine ("Hello, World!");
		StopProfile ();
	}
}
