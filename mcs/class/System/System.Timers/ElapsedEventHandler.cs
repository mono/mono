//
// System.Timers.ElapsedEventHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.Timers
{
	[Serializable]
	public delegate void ElapsedEventHandler (object sender, ElapsedEventArgs e);
}

