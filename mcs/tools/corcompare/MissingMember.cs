using System;
using System.Xml;
using System.Reflection;

namespace Mono.Util.CorCompare
{

	/// <summary>
	/// 	Represents a member that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Piersh
	/// 	created on - 3/1/2002 3:37:00 am
	/// </remarks>
	abstract class MissingMember : IMissingMember 
	{
		// e.g. <method name="Equals" status="missing"/>
		protected MemberInfo mInfo;

		public MissingMember (MemberInfo info) 
		{
			mInfo = info;
		}

		public virtual string Name 
		{
			get 
			{
				return mInfo.Name;
			}
		}

		public virtual string Status 
		{
			get 
			{
				return "missing";
			}
		}

		public abstract string Type 
		{
			get;
		}

		public XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltMissing = doc.CreateElement (Type);
			eltMissing.SetAttribute ("name", Name);
			eltMissing.SetAttribute ("status", Status);
			return eltMissing;
		}
	}
}
