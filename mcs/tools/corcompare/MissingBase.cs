// Mono.Util.CorCompare.MissingBase
//
// Author(s):
//   Piers Haken (piersh@friskit.com)
//
// (C) 2001-2002 Piers Haken
using System;
using System.Xml;
using System.Reflection;
using System.Collections;

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
			eltMissing.SetAttribute ("status", Status);
			return eltMissing;
		}
		/// <summary>
		/// The CompletionType of this element (eg Missing)
		/// </summary>
		public virtual CompletionTypes Completion
		{
			get { return CompletionTypes.Missing; }
		}

		/// <summary>
		/// A textual representation of this element's completion
		/// </summary>
		public virtual string Status
		{
			get 
			{
				switch (Completion)
				{
					case CompletionTypes.Missing:
						return "missing";
					case CompletionTypes.Todo:
						return "todo";
					case CompletionTypes.Complete:
						return "complete";
					default:
						throw new Exception ("Invalid CompletionType: "+Completion.ToString ());
				}
			}
		}

		/// <summary>
		/// Creates an XmlElement grouping together a set of sub-elements
		/// </summary>
		/// <param name="name">the name of the element to create</param>
		/// <param name="rgMembers">a list of sub-elements</param>
		/// <param name="ci">the completion info (unused)</param>
		/// <param name="doc">the document in which to create the element</param>
		/// <returns></returns>
		public static XmlElement CreateMemberCollectionElement (string name, ArrayList rgMembers, CompletionInfo ci, XmlDocument doc) 
		{
			XmlElement element = null;
			if (rgMembers != null && rgMembers.Count > 0)
			{
				element = doc.CreateElement(name);
				CompletionInfo ciMember = new CompletionInfo ();
				foreach (MissingBase mm in rgMembers)
				{
					element.AppendChild (mm.CreateXML (doc));
					ciMember.Add (mm.Completion);
				}
				ciMember.SetAttributes (element);
			}
			return element;
		}
	}
}
