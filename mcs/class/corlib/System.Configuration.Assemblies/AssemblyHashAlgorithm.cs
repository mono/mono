// AssemblyHashAlgorithm.cs
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
	///                   An enumeration of all the hash algorithms used for hashing files
	///                   and for generating the strong name.
	///                </para>
	/// <para>
	///                   A <paramref name="hash function" /><paramref name="H" />
	///                is a transformation that takes an input <paramref name="m" /> and returns a fixed-size
	///                string, which is called the hash value <paramref name="h" /> (that is, <paramref name="h" /> =
	///             <paramref name="H" />(<paramref name="m" />)).
	///                Hash functions with just this property have a variety of general computational
	///                uses, but when employed in cryptography, the hash functions are usually chosen
	///                to have some additional properties.
	///             </para>
	/// <para>
	///                The basic requirements for a cryptographic hash function
	///                are:   
	///             </para>
	/// <list type="bullet">
	/// <item>
	/// <term>
	///                      the input can be of any length
	///                   </term>
	/// </item>
	/// <item>
	/// <term>
	///                      the output has a fixed length
	///                   </term>
	/// </item>
	/// <item>
	/// <term>
	/// <paramref name="H" />(<paramref name="x)" /> is relatively easy to compute for any given x
	///                   </term>
	/// </item>
	/// <item>
	/// <term>
	/// <paramref name="H" />(<paramref name="x" />) is one-way
	///                   </term>
	/// </item>
	/// <item>
	/// <term>
	/// <paramref name="H" />(<paramref name="x" />) is collision-free
	///                   </term>
	/// </item>
	/// </list>
	/// <para>
	///                The hash value
	///                represents concisely the longer message or document from which it was computed;
	///                this value is called the message digest. One can think of a message
	///                digest as a "digital fingerprint" of the larger document. Examples of
	///                well-known hash functions are MD2 and and SHA.
	///             </para>
	/// </summary>
	public enum AssemblyHashAlgorithm {

		/// <summary>
		///                No hash algorithm.
		///             </summary>
		None = 0,

		/// <summary>
		///             The MD5 message-digest algorithm.  MD5 was developed by Rivest in 1991. It is basically MD4 with "safety-belts" and while it is slightly slower than MD4, it is more secure. The algorithm consists of four distinct rounds, which has a slightly different design from that of MD4. Message-digest size, as well as padding requirements, remain the same. 
		///             </summary>
		MD5 = 32771,

		/// <summary>
		///             A revision of the Secure Hash Algorithm that corrects an unpublished flaw in SHA.
		///             </summary>
		SHA1 = 32772,
	} // AssemblyHashAlgorithm

} // System.Configuration.Assemblies
