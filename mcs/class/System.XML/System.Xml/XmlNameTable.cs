// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlNameTable.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

namespace System.Xml
{
	public abstract class XmlNameTable
	{
		public abstract string Add (string name);
		public abstract string Add (char [] buffer, int offset, int length);
		public abstract string Get (string name);
		public abstract string Get (char [] buffer, int offset, int length);
	}
}
