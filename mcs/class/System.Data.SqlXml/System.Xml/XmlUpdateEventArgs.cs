//
// XmlUpdateEventArgs.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
#if NET_1_2

using System;
using System.Collections;

namespace System.Xml
{
	public abstract class XmlUpdateEventArgs
	{

		public XmlUpdateEventArgs ()
		{ 
		} 

		public abstract IEnumerable ErrorItems { get; }

		public abstract bool Executed { get; }

		public abstract Exception InnerException { get; }
	}
}
#endif
