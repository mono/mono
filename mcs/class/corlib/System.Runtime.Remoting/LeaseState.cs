// LeaseState.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Runtime.Remoting {


	/// <summary>
	/// <para>
	///                   The different lease states are
	///                </para>
	/// <para>
	///                   Null := The lease is not initialized.
	///                </para>
	/// <para>
	///                   Initial := <SPAN>Lease has been created, but not yet active.</SPAN></para>
	/// <para>
	///                   Active := <SPAN>The lease is active and has not expired.</SPAN></para>
	/// <para>
	///                   Renewing := <SPAN>Lease has expired and is looking for sponsorship.</SPAN></para>
	/// <para>
	///                   Expired := <SPAN>Lease has expired and cannot be renewed.</SPAN></para>
	/// </summary>
	public enum LeaseState {
		Null = 0,
		Initial = 1,
		Active = 2,
		Renewing = 3,
		Expired = 4,
	} // LeaseState

} // System.Runtime.Remoting
