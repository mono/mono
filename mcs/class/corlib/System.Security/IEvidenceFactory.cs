// System.Security.IEvidenceFactory
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) Ximian, Inc. 2001

using System.Security.Policy;

namespace System.Security
{
	public interface IEvidenceFactory
	{
		Evidence Evidence { get; }
	}
}
