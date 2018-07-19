// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Type: SerialPinChange
**
** Purpose: Used to describe which pin has changed on a PinChanged event.
**
** Date:  August 2002
**
**
===========================================================*/

using Microsoft.Win32;

namespace System.IO.Ports
{
	public enum SerialPinChange
	{
		CtsChanged = NativeMethods.EV_CTS,
		DsrChanged = NativeMethods.EV_DSR,
		CDChanged = NativeMethods.EV_RLSD,
		Ring = NativeMethods.EV_RING,
		Break = NativeMethods.EV_BREAK, 
	}

    public class SerialPinChangedEventArgs : EventArgs 
    {
        private SerialPinChange pinChanged;
        
    	internal SerialPinChangedEventArgs (SerialPinChange eventCode) {
    	    pinChanged = eventCode;
    	}

    	public SerialPinChange EventType { 
    	    get { return pinChanged; }
    	}
    }

    public delegate void SerialPinChangedEventHandler(object sender, SerialPinChangedEventArgs e);
}

