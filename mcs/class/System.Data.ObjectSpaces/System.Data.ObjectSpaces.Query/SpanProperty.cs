//
// System.Data.ObjectSpaces.Query.SpanProperty
//
//
// Author:
//	Richard Thombs (stony@stony.org)
//

#if NET_1_2

using System;
using System.Xml;

namespace System.Data.ObjectSpaces.Query
{
	[MonoTODO()]
	public class SpanProperty
	{
		public SpanProperty parent;

		[MonoTODO()]
		public SpanProperty(string name)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public SpanProperty(string name,SpanPropertyCollection subSpan)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public string ToXmlString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public void WriteXml(XmlWriter xmlw)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public string FullName
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public string Name
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public SpanProperty Owner
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public SpanPropertyCollection SubSpan
		{
			get { throw new NotImplementedException(); }
		}
	}
}

#endif
