// EntityHandling.cs
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
	///                Specifies how entities are handled.
	///             </summary>
	public enum EntityHandling {

		/// <summary>
		/// <para>
		///                   Expand all entities. This is the default.
		///                </para>
		/// <para>
		///                   Nodes of NodeType EntityReference are not returned. The entity text is
		///                   expanded in place of the entity references.
		///                </para>
		/// </summary>
		ExpandEntities = 1,

		/// <summary>
		/// <para>Expand character entities and return general
		///                   entities as nodes (NodeType=XmlNodeType.EntityReference, Name=the name of the
		///                   entity, HasValue=
		///                   false).</para>
		/// <para>You must call <see cref="M:System.Xml.XmlReader.ResolveEntity" /> to see what the general entities expand to. This
		///                allows you to optimize entity handling by only expanding the entity the
		///                first time it is used.</para>
		/// <para>If you call <see cref="M:System.Xml.XmlReader.GetAttribute(System.String)" /> 
		///             , general entities are also expanded as entities are of
		///             no interest in this case.</para>
		/// </summary>
		ExpandCharEntities = 2,
	} // EntityHandling

} // System.Xml
