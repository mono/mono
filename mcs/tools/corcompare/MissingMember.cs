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

		public static Accessibility GetAccessibility (MemberInfo mi)
		{
			switch (mi.MemberType)
			{
				case MemberTypes.Constructor:
				case MemberTypes.Method:
					MethodBase mb = (MethodBase) mi;
					if (mb.IsPublic)
						return Accessibility.Public;
					else if (mb.IsAssembly)
						return Accessibility.Assembly;
					else if (mb.IsFamilyOrAssembly)
						return Accessibility.FamilyOrAssembly;
					else if (mb.IsFamily)
						return Accessibility.Family;
					else if (mb.IsFamilyAndAssembly)
						return Accessibility.FamilyAndAssembly;
					else if (mb.IsPrivate)
						return Accessibility.Private;
					break;
				case MemberTypes.Field:
					FieldInfo fi = (FieldInfo) mi;
					if (fi.IsPublic)
						return Accessibility.Public;
					else if (fi.IsAssembly)
						return Accessibility.Assembly;
					else if (fi.IsFamilyOrAssembly)
						return Accessibility.FamilyOrAssembly;
					else if (fi.IsFamily)
						return Accessibility.Family;
					else if (fi.IsFamilyAndAssembly)
						return Accessibility.FamilyAndAssembly;
					else if (fi.IsPrivate)
						return Accessibility.Private;
					break;
				case MemberTypes.NestedType:
					Type ti = (Type) mi;
					if (ti.IsNestedPublic)
						return Accessibility.Public;
					if (ti.IsNestedAssembly)
						return Accessibility.Assembly;
					else if (ti.IsNestedFamORAssem)
						return Accessibility.FamilyOrAssembly;
					else if (ti.IsNestedFamily)
						return Accessibility.Family;
					else if (ti.IsNestedFamANDAssem)
						return Accessibility.FamilyAndAssembly;
					else if (ti.IsNestedPrivate)
						return Accessibility.Private;
					break;
				case MemberTypes.Event:
				case MemberTypes.Property:
					return Accessibility.Public;
				default:
					throw new Exception ("Missing handler for MemberType: "+mi.MemberType.ToString ());
			}
			throw new Exception ("Invalid accessibility: "+mi.ToString ());
		}
	}
}
