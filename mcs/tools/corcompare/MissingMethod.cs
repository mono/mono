// Mono.Util.CorCompare.MissingMethod
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Text;
using Mono.Cecil;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class method that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingMethod : MissingMember
	{
		// e.g. <method name="Equals" status="missing"/>
		public MissingMethod (MethodDefinition infoMono, MethodDefinition infoMS) : base (infoMono, infoMS) { }

		public override string Name {
			get {
				string s = Info.ToString();
				int index = s.IndexOf(' ');
				return s.Substring(index + 1);
			}
		}

		public override CustomAttributeCollection GetCustomAttributes (MemberReference mref) {
			return ((MethodDefinition) mref).CustomAttributes;
		}

		public override Accessibility GetAccessibility (MemberReference mref) {
			MethodDefinition member = (MethodDefinition) mref;
			MethodAttributes maskedMemberAccess = member.Attributes & MethodAttributes.MemberAccessMask;
			if (maskedMemberAccess == MethodAttributes.Public)
				return Accessibility.Public;
			else if (maskedMemberAccess == MethodAttributes.Assem)
				return Accessibility.Assembly;
			else if (maskedMemberAccess == MethodAttributes.FamORAssem)
				return Accessibility.FamilyOrAssembly;
			else if (maskedMemberAccess == MethodAttributes.Family)
				return Accessibility.Family;
			else if (maskedMemberAccess == MethodAttributes.FamANDAssem)
				return Accessibility.FamilyAndAssembly;
			else if (maskedMemberAccess == MethodAttributes.Private)
				return Accessibility.Private;
			throw new Exception ("Missing handler for Member " + mref.Name);
		}

		public override string Type {
			get {
				return "method";
			}
		}

		public override NodeStatus Analyze ()
		{
			m_nodeStatus = base.Analyze ();

			if (mInfoMono != null && mInfoMS != null)
			{
				MethodDefinition miMono = (MethodDefinition) mInfoMono;
				MethodDefinition miMS = (MethodDefinition) mInfoMS;

				AddFlagWarning (miMono.IsAbstract, miMS.IsAbstract, "abstract");
				AddFlagWarning (miMono.IsStatic, miMS.IsStatic, "static");
				AddFlagWarning (miMono.IsVirtual && !miMono.IsFinal, miMS.IsVirtual && !miMS.IsFinal, "virtual");
				AddFlagWarning (miMono.IsConstructor, miMS.IsConstructor, "a constructor");
				//AddFlagWarning (miMono.IsFinal, miMS.IsFinal, "sealed");
			}
			return m_nodeStatus;
		}
	}
}
