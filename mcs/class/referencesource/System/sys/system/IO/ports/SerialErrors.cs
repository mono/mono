// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Type: SerialError
**
** Purpose: Describes the types of serial port errors.
**
** Date:  August 2002
**
**
===========================================================*/

using Microsoft.Win32;

namespace System.IO.Ports
{
    public enum SerialError {
        TXFull = NativeMethods.CE_TXFULL,
        RXOver = NativeMethods.CE_RXOVER,
        Overrun = NativeMethods.CE_OVERRUN, 
        RXParity = NativeMethods.CE_PARITY,
        Frame = NativeMethods.CE_FRAME, 
    }

    public class SerialErrorReceivedEventArgs : EventArgs {
        private SerialError errorType;
        
        internal SerialErrorReceivedEventArgs(SerialError eventCode) {
            errorType = eventCode;
        }

        public SerialError EventType { 
            get { return errorType; }
        }
    }

    public delegate void SerialErrorReceivedEventHandler(object sender, SerialErrorReceivedEventArgs e);
}

