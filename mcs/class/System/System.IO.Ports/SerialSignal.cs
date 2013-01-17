//
// System.IO.Ports.SerialSignal.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

namespace System.IO.Ports
{
	enum SerialSignal {
		None = 0,
		Cd = 1, // Carrier detect 
		Cts = 2, // Clear to send
		Dsr = 4, // Data set ready
		Dtr = 8, // Data terminal ready
		Rts = 16 // Request to send
	}
}

