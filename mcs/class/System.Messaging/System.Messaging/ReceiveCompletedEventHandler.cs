//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//
using System;

namespace System.Messaging 
{
	[Serializable]
	public delegate void ReceiveCompletedEventHandler(object sender, ReceiveCompletedEventArgs e);
	
}
