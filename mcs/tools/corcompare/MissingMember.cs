using System;
using System.Xml;
using System.Collections;
using Mono.Cecil;

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
		protected MemberReference mInfoMono;
		protected MemberReference mInfoMS;

		public MissingMember (MemberReference infoMono, MemberReference infoMS)
		{
			mInfoMono = infoMono;
			mInfoMS = infoMS;
			m_nodeStatus = new NodeStatus (infoMono, infoMS);
		}

		public override string Name
		{
			get { return Info.Name; }
		}

		public abstract CustomAttributeCollection GetCustomAttributes (MemberReference mref);

		public abstract Accessibility GetAccessibility (MemberReference mref);

		public override NodeStatus Analyze ()
		{
			if (!Status.IsMissing)
			{
				rgAttributes = new ArrayList ();
				nsAttributes = MissingAttribute.AnalyzeAttributes (
					(mInfoMono == null) ? null : GetCustomAttributes (mInfoMono),
					(mInfoMS == null) ? null : GetCustomAttributes (mInfoMS),
					rgAttributes);

				if (mInfoMono != null && mInfoMS != null)
				{
					Accessibility acMono = GetAccessibility (mInfoMono);
					Accessibility acMS = GetAccessibility (mInfoMS);
					if (acMono != acMS)
						Status.AddWarning ("Should be "+AccessibilityToString (acMS));
				}

				m_nodeStatus.Add (nsAttributes);
			}
			return m_nodeStatus;
		}

		/// <summary>
		/// returns the MemberInfo for this member.
		/// if it's a missing member then the microsoft MemberInfo is returned instead
		/// </summary>
		public MemberReference Info
		{
			get { return (mInfoMono != null) ? mInfoMono : mInfoMS; }
		}

		/// <summary>
		/// returns the 'best' info for this member. the 'best' info is the microsoft info, if it's available, otherwise the mono info.
		/// </summary>
		public MemberReference BestInfo
		{
			get { return (mInfoMS != null) ? mInfoMS : mInfoMono; }
		}

		public static string GetUniqueName (MemberReference mi)
		{
			return mi.GetType().Name +  mi.Name;//(mi.MemberType).ToString () + mi.ToString ();
		}
	}
}
