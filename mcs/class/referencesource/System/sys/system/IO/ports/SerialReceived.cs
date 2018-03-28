// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Type: SerialData
**
** Purpose: Describes the types of receive events.
**
** Date:  August 2002
**
**
===========================================================*/

using Microsoft.Win32;

namespace System.IO.Ports
{
    public enum SerialData 
	{
	    Chars = NativeMethods.EV_RXCHAR,
		Eof = NativeMethods.EV_RXFLAG,
	}

    public class SerialDataReceivedEventArgs : EventArgs 
    {
        internal SerialData receiveType;
    	internal SerialDataReceivedEventArgs (SerialData eventCode) {
    	    receiveType = eventCode;
    	}

    	public SerialData EventType { 
    	    get { return receiveType; }
    	}
    }

    public delegate void SerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e);
}

