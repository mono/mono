// Mono.Util.CorCompare.MissingProperty
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Text;
using System.Xml;
using Mono.Cecil;

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
		public MissingProperty (PropertyDefinition infoMono, PropertyDefinition infoMS) : base (infoMono, infoMS) { }

		public override string Type
		{
			get { return "property"; }
		}

		protected MissingMethod mmGet;
		protected MissingMethod mmSet;

		public override CustomAttributeCollection GetCustomAttributes (MemberReference mref) {
			return ((PropertyDefinition) mref).CustomAttributes;
		}

		public override Accessibility GetAccessibility (MemberReference mref) {
			//PropertyDefinition member = (PropertyDefinition) mref;
			return Accessibility.Public;
		}

		public override NodeStatus Analyze ()
		{
			m_nodeStatus = base.Analyze ();

			PropertyDefinition piMono = (PropertyDefinition) mInfoMono;
			PropertyDefinition piMS = (PropertyDefinition) mInfoMS;

			MethodDefinition miGetMono, miSetMono;
			if (piMono == null)
				miGetMono = miSetMono = null;
			else
			{
				miGetMono = piMono.GetMethod;
				miSetMono = piMono.SetMethod;
			}

			MethodDefinition miGetMS, miSetMS;
			if (piMS == null)
				miGetMS = miSetMS = null;
			else
			{
				miGetMS = piMS.GetMethod;
				miSetMS = piMS.SetMethod;
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

			if (piMono != null && piMS != null)
			{
				string strTypeMono = piMono.PropertyType.FullName;
				string strTypeMS   =   piMS.PropertyType.FullName;
				if (strTypeMono != strTypeMS)
					Status.AddWarning ("Invalid type: is '"+strTypeMono+"', should be '"+strTypeMS+"'");
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
