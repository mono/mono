// Mono.Util.CorCompare.MissingProperty
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a missing property from a class
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingProperty : MissingMember 
	{
		// e.g. <property name="Length" status="missing"/>
		public MissingProperty (MemberInfo infoMono, MemberInfo infoMS) : base (infoMono, infoMS) {}

		public override string Type 
		{
			get { return "property"; }
		}

		MissingMethod mmGet;
		MissingMethod mmSet;

		/// <summary>
		/// a place holder for the method used to get the value of this property
		/// </summary>
		public virtual MissingMethod GetMethod
		{
			get { return mmGet; }
			set
			{
				mmGet = value;
				if (mInfoMono != null &&
					mmGet != null &&
					mmGet.Completion != CompletionTypes.Complete)
				{
					completion = CompletionTypes.Todo;
				}
			}
		}

		/// <summary>
		/// a place holder for the method used to set the value of this property
		/// </summary>
		public virtual MissingMethod SetMethod
		{
			get { return mmSet; }
			set
			{
				mmSet = value;
				if (mInfoMono != null &&
					mmSet != null &&
					mmSet.Completion != CompletionTypes.Complete)
				{
					completion = CompletionTypes.Todo;
				}
			}
		}

		public override XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltProperty = base.CreateXML (doc);

			if (mInfoMono != null)	// missing
			{
				if (mmGet != null || mmSet != null)
				{
					XmlElement eltAccessors = doc.CreateElement ("accessors");
					eltProperty.AppendChild (eltAccessors);

					if (mmGet != null)
					{
						XmlElement eltGet = mmGet.CreateXML (doc);
						eltAccessors.AppendChild (eltGet);
					}
					if (mmSet != null)
					{
						XmlElement eltSet = mmSet.CreateXML (doc);
						eltAccessors.AppendChild (eltSet);
					}
				}
			}
			return eltProperty;
		}
	}
}
