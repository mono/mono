//
// RdpContentType.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
// See RELAX NG spec 7.2
//

namespace Commons.Xml.Relaxng.Derivative
{
	public enum RdpContentType {
		Invalid = 0,
		Empty   = 1,
		Complex = 2,
		Simple  = 4
	}
}
