// WellKnownObjectMode.cs
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
	///                   Enum 
	///                      for wellknown object modes.
	///                   </para>
	/// </summary>
	public enum WellKnownObjectMode {

		/// <summary>
		/// <para>
		///                   Every 
		///                      message is dispatched to the same object instance
		///                   </para>
		/// </summary>
		Singleton = 1,

		/// <summary>
		/// <para>
		///                   Every 
		///                      message is dispatched to a new object instance.
		///                   </para>
		/// </summary>
		SingleCall = 2,
	} // WellKnownObjectMode

} // System.Runtime.Remoting
