// AssemblyVersionCompatibility.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Configuration.Assemblies {


	/// <summary>
	/// <para>
	///                   Defines the different flavors of assembly version compatibility.
	///                   The AssemblyVersionCompatibility defines the compatibility of an
	///                   assembly with other versions of the same assembly, indicating if it cannot
	///                   execute side-by-side with other versions (e.g., due to conflicts over a device
	///                   driver).
	///                </para>
	/// <para>
	///                   If no compatibility is specified, an assembly is side-by-side
	///                   compatible in all scopes.
	///                </para>
	/// <para>
	///                   An assembly cannot be more specific with regard to exactly which
	///                   previous versions it is not side-by-side compatible with. Hence, if the AssemblyVersionCompatibility
	///                   is specified, it means the assembly is non side-by-side with all know versions.
	///                   If not specified, it means it is side-by-side with all known versions.
	///                </para>
	/// </summary>
	public enum AssemblyVersionCompatibility {

		/// <summary>
		///             The assembly cannot execute with other versions if they are executing in the same process.
		///             </summary>
		SameMachine = 1,

		/// <summary>
		///             The assembly cannot execute with other versions if they are executing in the same process.
		///             </summary>
		SameProcess = 2,

		/// <summary>
		///             The assembly cannot execute with other versions if they executing in the same application domain.
		///             </summary>
		SameDomain = 3,
	} // AssemblyVersionCompatibility

} // System.Configuration.Assemblies
