using System;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Mono.Util.CorCompare
{

	/// <summary>
	/// 	Represents a generic member that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Piersh
	/// 	created on - 3/1/2002 3:37:00 am
	/// </remarks>
	abstract class MissingMember : MissingBase
	{
		// e.g. <method name="Equals" status="missing"/>
		protected MemberInfo mInfoMono;
		protected MemberInfo mInfoMS;

		public MissingMember (MemberInfo infoMono, MemberInfo infoMS) 
		{
			mInfoMono = infoMono;
			mInfoMS = infoMS;
			m_nodeStatus = new NodeStatus (infoMono, infoMS);
		}

		public override string Name 
		{
			get { return Info.Name; }
		}

		public override NodeStatus Analyze ()
		{
			if (!Status.IsMissing)
			{
				rgAttributes = new ArrayList ();
				nsAttributes = MissingAttribute.AnalyzeAttributes (
					(mInfoMono == null) ? null : mInfoMono.GetCustomAttributes (false),
					(mInfoMS   == null) ? null :   mInfoMS.GetCustomAttributes (false),
					rgAttributes);
				m_nodeStatus.Add (nsAttributes);
			}
			return m_nodeStatus;
		}

		/// <summary>
		/// returns the MemberInfo for this member.
		/// if it's a missing member then the microsoft MemberInfo is returned instead
		/// </summary>
		public MemberInfo Info
		{
			get { return (mInfoMono != null) ? mInfoMono : mInfoMS; }
		}

		/// <summary>
		/// returns the 'best' info for this member. the 'best' info is the microsoft info, if it's available, otherwise the mono info.
		/// </summary>
		public MemberInfo BestInfo
		{
			get { return (mInfoMS != null) ? mInfoMS : mInfoMono; }
		}

		public static string GetUniqueName (MemberInfo mi)
		{
			return (mi.MemberType).ToString () + mi.ToString ();
		}

	}
}
