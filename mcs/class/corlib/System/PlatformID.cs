//
// System.PlatformID.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:35:29 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	public enum PlatformID
	{
		Win32S = 0,
		Win32Windows = 1,
		Win32NT = 2,
#if NET_1_1
		WinCE = 3
#endif
		// We can not expose this to userland, since it would break bin compat
		// Unix = 128
	}
}
