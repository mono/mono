// Mono.Util.CorCompare.MissingEvent
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Xml;
using Mono.Cecil;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class event that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/24/2002 10:43:57 PM
	/// </remarks>
	class MissingEvent : MissingMember {
		// e.g. <method name="Equals" status="missing"/>
		public MissingEvent (EventDefinition infoMono, EventDefinition infoMS) : base (infoMono, infoMS) { }
		MissingMethod mmAdd;
		MissingMethod mmRemove;
		MissingMethod mmRaise;

		public override string Type {
			get {
				return "event";
			}
		}

		public override CustomAttributeCollection GetCustomAttributes (MemberReference mref) {
			return ((EventDefinition) mref).CustomAttributes;
		}

		public override Accessibility GetAccessibility (MemberReference mref) {
			//EventDefinition member = (EventDefinition) mref;
			return Accessibility.Public;
		}

		public override NodeStatus Analyze ()
		{
			m_nodeStatus = base.Analyze ();

			EventDefinition eiMono = (EventDefinition) mInfoMono;
			EventDefinition eiMS = (EventDefinition) mInfoMS;

			MethodDefinition miAddMono, miRemoveMono, miRaiseMono;
			if (eiMono == null)
				miAddMono = miRemoveMono = miRaiseMono = null;
			else
			{
				miAddMono = eiMono.AddMethod;
				miRemoveMono = eiMono.RemoveMethod;
				miRaiseMono = eiMono.InvokeMethod;
			}

			MethodDefinition miAddMS, miRemoveMS, miRaiseMS;
			if (eiMS == null)
				miAddMS = miRemoveMS = miRaiseMS = null;
			else
			{
				miAddMS = eiMS.AddMethod;
				miRemoveMS = eiMS.RemoveMethod;
				miRaiseMS = eiMS.InvokeMethod;
			}

			if (miAddMono != null || miAddMS != null)
			{
				mmAdd = new MissingMethod (miAddMono, miAddMS);
				m_nodeStatus.AddChildren (mmAdd.Analyze ());
			}
			if (miRemoveMono != null || miRemoveMS != null)
			{
				mmRemove = new MissingMethod (miRemoveMono, miRemoveMS);
				m_nodeStatus.AddChildren (mmRemove.Analyze ());
			}
			if (miRaiseMono != null || miRaiseMS != null)
			{
				mmRaise = new MissingMethod (miRemoveMono, miRemoveMS);
				m_nodeStatus.AddChildren (mmRaise.Analyze ());
			}
			return m_nodeStatus;
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
				if (mmAdd != null)
				{
					XmlElement eltAdd = mmAdd.CreateXML (doc);
					eltAccessors.AppendChild (eltAdd);
				}
				if (mmRemove != null)
				{
					XmlElement eltRemove = mmRemove.CreateXML (doc);
					eltAccessors.AppendChild (eltRemove);
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
