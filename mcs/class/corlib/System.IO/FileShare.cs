// FileShare.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Fri, 7 Sep 2001 16:32:26 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.IO {


	/// <summary>
	/// </summary>
	[Flags]
	public enum FileShare : int {

		None = 0,
		Read = 1,
		Write = 2,
		ReadWrite = 3,
		Inheritable = 16,
	} // FileShare

} // System.IO
