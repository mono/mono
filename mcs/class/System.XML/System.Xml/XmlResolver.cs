// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlResolver.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

using System;
using System.Net;

namespace System.Xml
{
	public abstract class XmlResolver
	{
		public abstract ICredentials Credentials { set; }

		public abstract object GetEntity(
			Uri absoluteUri,
			string role,
			Type type);

		public abstract Uri ResolveUri(
			Uri baseUri,
			string relativeUri);
	}
}
