//
// System.ComponentModel.InvalidEnumArgumentException.cs 
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2002 Ximian, Inc.		http://www.ximian.com
//

using System;

namespace System.ComponentModel
{
	[Serializable]
	public class InvalidEnumArgumentException : ArgumentException
	{
		string msg = String.Empty;
		
		public InvalidEnumArgumentException () : base ()
		{
		}

		public InvalidEnumArgumentException (string message)
		{
			msg = message;
		}

		public InvalidEnumArgumentException (string argumentName, int invalidValue, Type enumClass)
		{
			msg = argumentName + " is invalid because this value, " + invalidValue + " is not of type " + enumClass.Name;
		}

		public override string Message {
			get {
				if (ParamName == String.Empty)
					return msg;
				else
					return ParamName + ": " + msg;
			}
		}
	}
}
