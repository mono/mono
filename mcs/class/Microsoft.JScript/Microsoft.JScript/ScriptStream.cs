//
// ScriptStream.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;
using System.IO;

namespace Microsoft.JScript {

	public class ScriptStream {

		public static TextWriter Out = Console.Out;
		public static TextWriter Error = Console.Error;


		public static void PrintStackTrace ()
		{
			throw new NotImplementedException ();
		}


		public static void PrintStackTrace (Exception e)
		{
			throw new NotImplementedException ();
		}


		public static void Write (string str)
		{
			Out.Write (str);
		}


		public static void WriteLine (string str)
		{
			Out.WriteLine (str);
		}
	}
}
