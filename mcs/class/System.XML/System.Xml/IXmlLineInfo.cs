// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.IXmlLineInfo.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

namespace System.Xml
{
	public interface IXmlLineInfo
	{
		int LineNumber { get; }
		int LinePosition { get; }

		bool HasLineInfo();
	}
}
