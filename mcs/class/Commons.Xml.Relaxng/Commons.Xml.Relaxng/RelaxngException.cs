//
// Commons.Xml.Relaxng.General.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//
using System;
using System.Collections;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng.Derivative;

namespace Commons.Xml.Relaxng
{
	public class RelaxngException : Exception
	{
		string debugXml;

		public RelaxngException () : base () {}
		public RelaxngException (string message) : base (message) {}
		public RelaxngException (string message, Exception innerException)
			: base (message, innerException) {}
		public RelaxngException (string message, RdpPattern invalidatedPattern)
			: base (message)
		{
			debugXml = RdpUtil.DebugRdpPattern (invalidatedPattern, new Hashtable ());
		}
	}

}

