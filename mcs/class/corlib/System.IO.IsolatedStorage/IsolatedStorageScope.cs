// IsolatedStorageScope.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:41:21 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.IO.IsolatedStorage {


	[Flags]
	public enum IsolatedStorageScope {

		None = 0,
		User = 1,
		Domain = 2,
		Assembly = 4,

		// Documented in "C# In A Nutshell"
		Roaming = 8
	} // IsolatedStorageScope

} // System.IO.IsolatedStorage
