// TreePosition.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Xml {


	/// <summary>
	/// <para>
	///                   Specifies a position relative to the current node.
	///                </para>
	/// </summary>
	public enum TreePosition {

		/// <summary>
		///                No position.
		///             </summary>
		None = 0,

		/// <summary>
		///                The sibling immediately before the current
		///                node/record.
		///             </summary>
		Before = 1,

		/// <summary>
		///                The sibling immediately after the current
		///                node/record.
		///             </summary>
		After = 2,

		/// <summary>
		///                The first child of the current node/record.
		///             </summary>
		FirstChild = 3,

		/// <summary>
		///                The last child of the current node/record.
		///             </summary>
		LastChild = 4,

		/// <summary>
		///                The parent of the current node/record.
		///             </summary>
		Parent = 5,

		/// <summary>
		///                The first attribute of the current
		///                node/record.
		///             </summary>
		FirstAttribute = 6,

		/// <summary>
		///                The last attribute of the current
		///                node/record.
		///             </summary>
		LastAttribute = 7,
	} // TreePosition

} // System.Xml
