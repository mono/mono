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

		protected MissingMethod mmGet;
		protected MissingMethod mmSet;

		/// <summary>
		/// a place holder for the method used to get the value of this property
		/// </summary>
		public virtual MissingMethod GetMethod
		{
			get { return mmGet; }
			set
			{
				if (mmGet != null)
					m_nodeStatus.SubChildren (mmGet.Status);
				mmGet = value;
				if (mmGet != null)
					m_nodeStatus.AddChildren (mmGet.Status);
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
				if (mmSet != null)
					m_nodeStatus.SubChildren (mmSet.Status);
				mmSet = value;
				if (mmSet != null)
					m_nodeStatus.AddChildren (mmSet.Status);
			}
		}
		public override XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltMember = base.CreateXML (doc);

			if (mInfoMono != null)	// missing
			{
				if (mmGet != null || mmSet != null)
				{
					XmlElement eltAccessors = doc.CreateElement ("accessors");
					eltMember.AppendChild (eltAccessors);

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
			return eltMember;
		}
	}
}
