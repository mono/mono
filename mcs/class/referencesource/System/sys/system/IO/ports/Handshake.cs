// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Type: Handshake
**
** Purpose: Handshake enum type defined here.
**
** Date:  August 2002
**
===========================================================*/


namespace System.IO.Ports 
{

	public enum Handshake
	{
		None,
		XOnXOff,
		RequestToSend,
		RequestToSendXOnXOff
	};

}
