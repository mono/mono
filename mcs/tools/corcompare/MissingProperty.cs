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

		public override NodeStatus Analyze ()
		{
			m_nodeStatus = base.Analyze ();

			PropertyInfo piMono = (PropertyInfo) mInfoMono;
			PropertyInfo piMS   = (PropertyInfo) mInfoMS;

			MemberInfo miGetMono, miSetMono;
			if (piMono == null)
				miGetMono = miSetMono = null;
			else
			{
				miGetMono = piMono.GetGetMethod ();
				miSetMono = piMono.GetSetMethod ();
			}

			MemberInfo miGetMS, miSetMS;
			if (piMS == null)
				miGetMS = miSetMS = null;
			else
			{
				miGetMS = piMS.GetGetMethod ();
				miSetMS = piMS.GetSetMethod ();
			}

			if (miGetMono != null || miGetMS != null)
			{
				mmGet = new MissingMethod (miGetMono, miGetMS);
				m_nodeStatus.AddChildren (mmGet.Analyze ());
			}
			if (miSetMono != null || miSetMS != null)
			{
				mmSet = new MissingMethod (miSetMono, miSetMS);
				m_nodeStatus.AddChildren (mmSet.Analyze ());
			}
			return m_nodeStatus;
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
