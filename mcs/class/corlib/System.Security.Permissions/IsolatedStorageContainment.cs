// IsolatedStorageContainment.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:41:57 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Security.Permissions {

	[Serializable]
	public enum IsolatedStorageContainment
	{
		None = 0x00,
		DomainIsolationByUser = 0x10,
		AssemblyIsolationByUser = 0x20,
		DomainIsolationByRoamingUser = 0x50,
		AssemblyIsolationByRoamingUser = 0x60,
		AdministerIsolatedStorageByUser = 0x70,
		UnrestrictedIsolatedStorage = 0xF0,
	};

} // System.Security.Permissions
