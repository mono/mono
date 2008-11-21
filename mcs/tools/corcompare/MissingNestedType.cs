// Mono.Util.CorCompare.MissingNestedType
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
//using System.Reflection;
using Mono.Cecil;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class event that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/24/2002 10:43:57 PM
	/// </remarks>
	class MissingNestedType : MissingMember {
		// e.g. <method name="Equals" status="missing"/>
		public MissingNestedType (TypeDefinition infoMono, TypeDefinition infoMS) : base (infoMono, infoMS) { }

		public override string Type {
			get {
				return "nestedType";
			}
		}

		public override Accessibility GetAccessibility (MemberReference mref) {
			TypeDefinition member = (TypeDefinition) mref;
			TypeAttributes maskedMVisibility = member.Attributes & TypeAttributes.VisibilityMask;
			if (maskedMVisibility == TypeAttributes.Public)
				return Accessibility.Public;
			else if (maskedMVisibility == TypeAttributes.NestedAssembly)
				return Accessibility.Assembly;
			else if (maskedMVisibility == TypeAttributes.NestedFamORAssem)
				return Accessibility.FamilyOrAssembly;
			else if (maskedMVisibility == TypeAttributes.NestedFamily)
				return Accessibility.Family;
			else if (maskedMVisibility == TypeAttributes.NestedFamANDAssem)
				return Accessibility.FamilyAndAssembly;
			else if (maskedMVisibility == TypeAttributes.NestedPrivate)
				return Accessibility.Private;
			throw new Exception ("Missing handler for Member " + mref.Name);
		}

		public override CustomAttributeCollection GetCustomAttributes (MemberReference mref) {
			return ((TypeDefinition) mref).CustomAttributes;
		}

		public override string Name
		{
			get { return Info.DeclaringType.Name + "+" + Info.Name; }
		}

	}
}
