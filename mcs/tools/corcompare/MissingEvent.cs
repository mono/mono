// Mono.Util.CorCompare.MissingEvent
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;
using System.Xml;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class event that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/24/2002 10:43:57 PM
	/// </remarks>
	class MissingEvent : MissingProperty {
		// e.g. <method name="Equals" status="missing"/>
		public MissingEvent (MemberInfo infoMono, MemberInfo infoMS) : base (infoMono, infoMS) {}
		MissingMethod mmRaise;

		public override string Type {
			get {
				return "event";
			}
		}
		/// <summary>
		/// a place holder for the method used to set the value of this property
		/// </summary>
		public virtual MissingMethod RaiseMethod
		{
			get { return mmRaise; }
			set
			{
				if (mmRaise != null)
					m_nodeStatus.SubChildren (mmRaise.Status);
				mmRaise = value;
				if (mmRaise != null)
					m_nodeStatus.AddChildren (mmRaise.Status);
			}
		}

		public override XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltMember = base.CreateXML (doc);

			if (mInfoMono != null && mmRaise != null)
			{
				XmlElement eltAccessors = (XmlElement) eltMember.SelectSingleNode ("accessors");
				if (eltAccessors == null)
				{
					eltAccessors = doc.CreateElement ("accessors");
					eltMember.AppendChild (eltAccessors);
				}

				if (mmRaise != null)
				{
					XmlElement eltRaise = mmRaise.CreateXML (doc);
					eltAccessors.AppendChild (eltRaise);
				}
			}
			return eltMember;
		}
	}
}
