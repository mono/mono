//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
//	(C) Ximian, Inc.  http://www.ximian.com
//
using System;

namespace System.Messaging 
{
	[Serializable]
	public enum EncryptionAlgorithm 
	{
		None = 0,
		Rc2 = 26114,
		Rc4 = 26625
	}
}
