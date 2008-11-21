// Mono.Util.CorCompare.MissingBase
//
// Author(s):
//   Piers Haken (piersh@friskit.com)
//
// (C) 2001-2002 Piers Haken
using System;
using System.Xml;
using System.Collections;
using Mono.Cecil;

namespace Mono.Util.CorCompare
{
	/// <summary>
	/// Base class for all comparison items
	/// </summary>
	/// <remarks>
	/// 	created by - Piersh
	/// 	created on - 3/3/2002 10:23:24 AM
	/// </remarks>
	public abstract class MissingBase
	{
		protected NodeStatus m_nodeStatus;
		protected ArrayList rgAttributes;
		protected NodeStatus nsAttributes;

		public enum Accessibility
		{
			Public,
			Assembly,
			FamilyOrAssembly,
			Family,
			FamilyAndAssembly,
			Private,
		}

		/// <summary>
		/// The name of the element (eg "System.Xml")
		/// </summary>
		public abstract string Name { get ; }

		/// <summary>
		/// The type of the element (eg "namespace")
		/// </summary>
		public abstract string Type { get; }

		/// <summary>
		/// Generates an XmlElement describint this element
		/// </summary>
		/// <param name="doc">The document in which to create the element</param>
		/// <returns></returns>
		public virtual XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltMissing = doc.CreateElement (Type);
			eltMissing.SetAttribute ("name", Name);
			//Status.status.SetAttributes (eltMissing);
			Status.SetAttributes (eltMissing);

			XmlElement eltAttributes = MissingBase.CreateMemberCollectionElement ("attributes", rgAttributes, nsAttributes, doc);
			if (eltAttributes != null)
				eltMissing.AppendChild (eltAttributes);

			return eltMissing;
		}

		public virtual NodeStatus Status
		{
			get { return m_nodeStatus; }
		}

		public abstract NodeStatus Analyze ();

		/// <summary>
		/// Creates an XmlElement grouping together a set of sub-elements
		/// </summary>
		/// <param name="name">the name of the element to create</param>
		/// <param name="rgMembers">a list of sub-elements</param>
		/// <param name="doc">the document in which to create the element</param>
		/// <returns></returns>
		public static XmlElement CreateMemberCollectionElement (string name, ArrayList rgMembers, NodeStatus ns, XmlDocument doc)
		{
			XmlElement element = null;
			if (rgMembers != null && rgMembers.Count > 0)
			{
				element = doc.CreateElement(name);
				foreach (MissingBase mm in rgMembers)
					element.AppendChild (mm.CreateXML (doc));

				//ns.SetAttributes (element);
			}
			return element;
		}
		protected void AddFakeAttribute (bool fMono, bool fMS, string strName)
		{
			if (fMono || fMS)
			{
				MissingAttribute ma = new MissingAttribute (
					(fMono) ? strName : null,
					(fMS) ? strName : null);
				ma.Analyze ();
				rgAttributes.Add (ma);
				nsAttributes.AddChildren (ma.Status);
			}
		}

		protected void AddFlagWarning (bool fMono, bool fMS, string strName)
		{
			if (!fMono && fMS)
				m_nodeStatus.AddWarning ("Should be " + strName);
			else if (fMono && !fMS)
				m_nodeStatus.AddWarning ("Should not be " + strName);
		}

		protected string AccessibilityToString (Accessibility ac)
		{
			switch (ac)
			{
				case Accessibility.Public:
					return "public";
				case Accessibility.Assembly:
					return "internal";
				case Accessibility.FamilyOrAssembly:
					return "protected internal";
				case Accessibility.Family:
					return "protected";
				case Accessibility.FamilyAndAssembly:
					return "protected";	// TODO:
				case Accessibility.Private:
					return "private";
			}
			throw new Exception ("Invalid accessibility: "+ac.ToString ());
		}
	}
}
