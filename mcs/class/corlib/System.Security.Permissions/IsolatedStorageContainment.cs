// IsolatedStorageContainment.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Security.Permissions {


	/// <summary>
	/// <para>Specifies the permitted use of isolated storage.</para>
	/// </summary>
	/// <remarks>
	/// <para>Isolated storage uses evidence to specify a unique storage area for use by an application or 
	///                   component. Isolated storage provides true isolation in that the identity of
	///                   an application uniquely determines the root of a virtual file system that only
	///                   that application can access. Thus, rather than many applications and components sharing
	///                   a common resource like the file system or registry, each has its own
	///                   file area inherently assigned to it. Each file area is fully isolated from other
	///                   applications, making it essentially private.</para>
	/// <para>Three basic kinds of identity are used when assigning 
	///                   isolated storage: </para>
	/// <list type="bullet">
	/// <item>
	/// <term>
	/// <see langword="User" /> - Code is 
	///                      identified based on an authenticated user identity, or a unique anonymous user
	///                      identity is used if no authenticated user identity exists.</term>
	/// </item>
	/// <item>
	/// <term>
	/// <see langword="Domain" /> - Code is 
	///                      identified based on evidence associated with the application domain. Web
	///                      application identity is derived from the site's URL, while local code identity
	///                      is based on the application directory path.</term>
	/// </item>
	/// <item>
	/// <term>
	/// <see langword="Assembly" /> - Code is identified cryptographically by strong 
	///                      name (for example, Microsoft.Office.* or Microsoft.Office.Word) or publisher
	///                      (based on public key).</term>
	/// </item>
	/// </list>
	/// <para>These identities may be grouped together, in which case the 
	///                identities are applied one after another until the desired isolated storage is
	///                created. This grouping of identities is useful in many different applications.
	///                For example, isolation by user and then by domain allows the storage of user parameters,
	///                such as preferences of user data for use by a specific application. An application might store a
	///                user's identifying information, save the state from the last time the
	///                application was run, use the store to record an audit trail of past
	///                application usage, and save preferences for options of the application.</para>
	/// <para>If data is stored by domain, user, and assembly, the assembly is private 
	///                in the sense that only code in that assembly may access the data, yet the
	///                data store is also isolated by the application in which it runs so that the assembly
	///                does not represent a potential leak by exposing data to other applications. In particular,
	///                this addresses the Web site isolation requirement of a web
	///                browser, where (without special trust) data of one site should never be leaked to another
	///                at the client. The scenario for this feature is similar to that of storage
	///                isolated by user and domain, with the third party code possessing its own
	///                container within the scope of the application in which it is used. </para>
	/// <para>Isolation by assembly and user could be used for user 
	///                data that applies across multiple applications, for example, license
	///                information, user's personal information (name, authentication credentials, and
	///                so on) that is independent of application. </para>
	/// <para>
	/// <see cref="T:System.Security.Permissions.IsolatedStorageContainment" /> exposes 
	///             flags
	///             that determine whether an application is allowed to use isolated storage and, if
	///             so, what identity combinations are allowed to use it.</para>
	/// <para> All isolated storage is implicitly contained by machine; that is, storage is 
	///                not shared between machines, nor is data accessible across machines with this
	///                storage feature.</para>
	/// </remarks>
	/// <seealso topic="cpconisolatedstorage" />
	public enum IsolatedStorageContainment {

		/// <summary>
		/// <para>Use of isolated storage is not 
		///                   allowed.</para>
		/// </summary>
		None = 0,

		/// <summary>
		/// <para>Storage is isolated first by user and then by domain. 
		///                   Data can only be accessed within the context of the same application and only when run by the same user.</para>
		/// <para>This also allows isolation by user, domain, and 
		///                   assembly. This is helpful when a third-party assembly wants to keep a private data store.</para>
		/// </summary>
		DomainIsolationByUser = 1,

		/// <summary>
		/// <para>Storage is isolated first by user and then by code 
		///                   assembly. This provides a data store for the assembly that is accessible in any
		///                   domain context. The per-assembly data compartment requires additional trust as
		///                   it potentially provides a "tunnel" between applications that could compromise the data isolation of applications in
		///                   particular Web sites.</para>
		/// </summary>
		AssemblyIsolationByUser = 2,

		/// <summary>
		/// <para>Limited administration ability for the isolated storage data store. Allows browsing and
		///                   deletion of the entire user store, but not read access other than
		///                   the user's own domain/assembly identity.</para>
		/// </summary>
		AdministerIsolatedStorageByUser = 5,

		/// <summary>
		/// <para>Use of isolated storage is allowed without restriction. Code has full access to any part of
		///                   the user store, regardless of the identity of the domain or assembly. This use of isolated
		///                   storage includes the ability to enumerate
		///                   the contents of the isolated storage data store.</para>
		/// </summary>
		UnrestrictedIsolatedStorage = 7,
	} // IsolatedStorageContainment

} // System.Security.Permissions
