//
// CmdLineException.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using System.Globalization;

	public class CmdLineException : Exception
	{
		public CmdLineException (CmdLineError errorCode, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}


		public CmdLineException (CmdLineError errorCode, string context,
					 CultureInfo culture)
		{
			throw new NotImplementedException ();
		}


		public override string Message {
			get { throw new NotImplementedException (); }
		}


		public string ResourceKey (CmdLineError errorCode)
		{
			throw new NotImplementedException ();
		}
	}
}