//
// ScriptStream.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;
	using System.IO;

	public class ScriptStream
	{
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
			throw new NotImplementedException ();
		}


		public static void WriteLine (string str)
		{
			throw new NotImplementedException ();
		}
	}
}