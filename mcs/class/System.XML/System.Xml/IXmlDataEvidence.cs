//
// IXmlDataEvidence.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
#if NET_2_0

using System;
using System.Security.Policy;

namespace System.Xml
{

	public interface IXmlDataEvidence
	{
		Evidence[] Evidences { get; } 
	}
}
#endif
