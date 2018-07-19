// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Type: Parity
**
** Purpose: Parity enum type defined here.
**
** Date:  August 2002
**
===========================================================*/

using Microsoft.Win32;

namespace System.IO.Ports
{
	public enum Parity  
	{
		None = NativeMethods.NOPARITY,
		Odd = NativeMethods.ODDPARITY,
        Even = NativeMethods.EVENPARITY,
		Mark = NativeMethods.MARKPARITY,
		Space = NativeMethods.SPACEPARITY
	};	
}
	
