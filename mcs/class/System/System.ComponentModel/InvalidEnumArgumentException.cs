//
// System.ComponentModel.InvalidEnumArgumentException.cs 
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.		http://www.ximian.com
// (C) 2003 Andreas Nahr
//

using System;

namespace System.ComponentModel
{

	public class InvalidEnumArgumentException : ArgumentException
	{

		public InvalidEnumArgumentException ()
		{
		}

		public InvalidEnumArgumentException (string message) : base (message)
		{
		}

		public InvalidEnumArgumentException (string argumentName, int invalidValue, Type enumClass) :
			base (argumentName + " is invalid because this value, " + invalidValue + " is not of type " +
			      enumClass.Name, argumentName)
		{
		}
	}
}

