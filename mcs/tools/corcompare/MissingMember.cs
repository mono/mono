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
		protected CompletionTypes completion = CompletionTypes.Missing;
		protected ArrayList rgAttributes;
		protected CompletionInfo ci;

		public MissingMember (MemberInfo infoMono, MemberInfo infoMS) 
		{
			mInfoMono = infoMono;
			mInfoMS = infoMS;
			completion = (infoMono == null) ? CompletionTypes.Missing : CompletionTypes.Complete;
		}

		public override string Name 
		{
			get { return Info.Name; }
		}

		public override CompletionTypes Completion
		{
			get { return completion; }
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

		public override XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltMissing = base.CreateXML (doc);

			XmlElement eltAttributes = MissingBase.CreateMemberCollectionElement ("attributes", rgAttributes, ci, doc);
			if (eltAttributes != null)
				eltMissing.AppendChild (eltAttributes);

			return eltMissing;
		}

		public virtual CompletionInfo Analyze ()
		{
			if (mInfoMono == null)
			{
				completion = CompletionTypes.Missing;
			}
			else
			{
				rgAttributes = new ArrayList ();
				ci = MissingAttribute.AnalyzeAttributes (
					(mInfoMono == null) ? null : mInfoMono.GetCustomAttributes (false),
					(mInfoMS   == null) ? null :   mInfoMS.GetCustomAttributes (false),
					rgAttributes);

				if (ci.cTodo != 0 || ci.cMissing != 0)
					completion = CompletionTypes.Todo;
				else
					completion = CompletionTypes.Complete;
			}
			return ci;
		}
	}
}
