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
	public enum HashAlgorithm
	{
		Mac = 32773,
		Md2 = 32769,
		Md4 = 32770,
		Md5 = 32771,
		None = 0,
		Sha = 32772
	}
}
