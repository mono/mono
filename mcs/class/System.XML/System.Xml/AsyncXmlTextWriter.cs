//
// AsyncXmlTextWriter.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
//
#if NET_1_2

using System;
using System.IO;

namespace System.Xml
{
	public class AsyncXmlTextWriter : XmlTextWriter
	{

		// TODO
		public AsyncXmlTextWriter (/*Async*/StreamWriter writer)
			: base (writer)
		{
		}

		public virtual IAsyncResult BeginFlush (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}


		public virtual void EndFlush (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		public override void WriteNode (XmlReader reader, bool defaultAttribute)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
