//
// System.IO.Ports.SerialPinChange.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

namespace System.IO.Ports 
{
	public enum SerialPinChange 
	{
		CtsChanged = 8,
		DsrChanged = 16,
		CDChanged = 32,
		Break = 64,
		Ring = 256
	} 
}


