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
	public enum CryptographicProviderType 
	{
		Dss = 3,
		Fortezza = 4,
		MicrosoftExchange = 5,
		None = 0,
		RsaFull = 1,
		RsqSig = 2,
		Ssl = 6,
		SttAcq = 8,
		SttBrnd = 9,
		SttIss = 11,
		SttMer = 7,
		SttRoot = 10
	}
}
