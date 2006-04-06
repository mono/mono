//
// System.IO.Ports.SerialSignal.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

#if NET_2_0

namespace System.IO.Ports
{
	enum SerialSignal {
		Cd, // Carrier detect 
		Cts, // Clear to send
		Dsr, // Data set ready
		Dtr, // Data terminal ready
		Rts // Request to send
	}
}

#endif

