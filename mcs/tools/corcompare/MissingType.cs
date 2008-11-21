// Mono.Util.CorCompare.MissingType
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Xml;
using System.Collections;
using Mono.Cecil;
using Mono.Util.CorCompare.Cecil;

namespace Mono.Util.CorCompare
{

	/// <summary>
	/// 	Represents a class method that missing.
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingType : MissingBase
	{
		// e.g. <class name="System.Byte" status="missing"/>
		// e.g. <class name="System.Array" status="todo" missing="5" todo="6" complete="45">
		TypeDefinition typeMono, typeMS;
//		ArrayList rgAttributes = new ArrayList ();
		ArrayList rgMethods = new ArrayList ();
		ArrayList rgProperties = new ArrayList ();
		ArrayList rgEvents = new ArrayList ();
		ArrayList rgFields = new ArrayList ();
		ArrayList rgConstructors = new ArrayList ();
		ArrayList rgNestedTypes = new ArrayList ();
		ArrayList rgInterfaces = new ArrayList ();
//		NodeStatus nsAttributes = new NodeStatus ();
		NodeStatus nsMethods = new NodeStatus ();
		NodeStatus nsProperties = new NodeStatus ();
		NodeStatus nsEvents = new NodeStatus ();
		NodeStatus nsFields = new NodeStatus ();
		NodeStatus nsConstructors = new NodeStatus ();
		NodeStatus nsNestedTypes = new NodeStatus ();
		NodeStatus nsInterfaces = new NodeStatus ();

		public MissingType (TypeDefinition _typeMono, TypeDefinition _typeMS)
		{
			typeMono = _typeMono;
			typeMS = _typeMS;
			m_nodeStatus = new NodeStatus (_typeMono, _typeMS);
		}

		public override string Name
		{
			get
			{
				TypeDefinition type = TypeInfoBest;
				if (type.DeclaringType != null)
					return type.DeclaringType.Name + "+" + type.Name;
				return type.Name;
			}
		}

		public override string Type
		{
			get
			{
				Console.WriteLine ("For: {0} -> {1}", Name, r);
				return r;
			}

		}

		string r {
			get {
				TypeDefinition type = TypeInfo;

				if (type.IsEnum)
					return "enum";
				else if (type.IsInterface)
					return "interface";
				else if (type.IsValueType)
					return "struct";
				else if (IsDelegate)
					return "delegate";
				else
					return "class";
			}
		}

		public TypeDefinition TypeInfo
		{
			get { return (typeMono != null) ? typeMono : typeMS; }
		}

		public TypeDefinition TypeInfoBest
		{
			get { return (typeMS == null) ? typeMono : typeMS; }
		}

		public bool IsDelegate
		{
			get
			{
				return TypeHelper.IsDelegate (TypeInfoBest);
			}
		}

		public MissingMember CreateMember (MemberReference infoMono, MemberReference infoMS, bool add)
		{
			MemberReference mref = (infoMono != null) ? infoMono : infoMS;
			MissingMember mm;

			if(mref.GetType().Equals(typeof(MethodDefinition)))
			{
				if (((MethodDefinition) mref).IsConstructor) {
					mm = new MissingConstructor((MethodDefinition) infoMono, (MethodDefinition) infoMS);
					if (add) {
						nsConstructors.AddChildren (mm.Status);
						rgConstructors.Add (mm);
					}
				}
				else {
					mm = new MissingMethod ((MethodDefinition) infoMono, (MethodDefinition) infoMS);
					if (add) {
						nsMethods.AddChildren (mm.Status);
						rgMethods.Add (mm);
					}
				}
			}
			else if (mref.GetType().Equals(typeof(PropertyDefinition)))
			{
				mm = new MissingProperty ((PropertyDefinition) infoMono, (PropertyDefinition) infoMS);
				if (add) {
					nsProperties.AddChildren (mm.Status);
					rgProperties.Add (mm);
				}
			}
			else if (mref.GetType().Equals(typeof(EventDefinition)))
			{
				mm = new MissingEvent ((EventDefinition) infoMono, (EventDefinition) infoMS);
				if (add) {
					nsEvents.AddChildren (mm.Status);
					rgEvents.Add (mm);
				}
			}
			else if (mref.GetType().Equals(typeof(FieldDefinition)))
			{
				mm = new MissingField ((FieldDefinition) infoMono, (FieldDefinition) infoMS);
				if (add) {
					nsFields.AddChildren (mm.Status);
					rgFields.Add (mm);
				}
			}
			else if (mref.GetType().Equals(typeof(TypeDefinition)) && ((TypeDefinition)mref).DeclaringType == null)//nested type
			{
				mm = new MissingNestedType ((TypeDefinition) infoMono, (TypeDefinition) infoMS);
				if (add) {
					nsNestedTypes.AddChildren (mm.Status);
					rgNestedTypes.Add (mm);
				}
			}
			else
				throw new Exception ("Unexpected MemberType");

			mm.Analyze ();
			return mm;
		}

		public void AddMember (MemberReference infoMono, MemberReference infoMS)
		{
			CreateMember (infoMono, infoMS, true);
		}

		public override XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltClass = base.CreateXML (doc);
			XmlElement eltMember;

			eltMember = MissingBase.CreateMemberCollectionElement ("methods", rgMethods, nsMethods, doc);
			if (eltMember != null)
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("properties", rgProperties, nsProperties, doc);
			if (eltMember != null)
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("events", rgEvents, nsEvents, doc);
			if (eltMember != null)
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("fields", rgFields, nsFields, doc);
			if (eltMember != null)
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("constructors", rgConstructors, nsConstructors, doc);
			if (eltMember != null)
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("nestedTypes", rgNestedTypes, nsNestedTypes, doc);
			if (eltMember != null)
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("interfaces", rgInterfaces, nsInterfaces, doc);
			if (eltMember != null)
				eltClass.AppendChild (eltMember);

			return eltClass;
		}

		private static void FillMembersMap (Hashtable members, TypeDefinition type) {
			if (type != null) {
				foreach (PropertyDefinition p in type.Properties) {
					if (p.DeclaringType.Equals (type)) {
						string strName = MissingMember.GetUniqueName (p);
						members.Add (strName, p);
					}
				}

				foreach (EventDefinition e in type.Events) {
					if (e.DeclaringType.Equals (type)) {
						string strName = MissingMember.GetUniqueName (e);
						members.Add (strName, e);
					}
				}

				foreach (MethodDefinition c in type.Constructors) {
					if ((c.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Private) {
						string strName = MissingMember.GetUniqueName (c);
						members.Add (strName, c);
					}
				}

				foreach (MethodDefinition m in type.Methods) {
					if ((m.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Private && m.DeclaringType.Equals (type)) {
						string strName = MissingMember.GetUniqueName (m);
						members.Add (strName, m);
					}
				}

				foreach (FieldDefinition f in type.Fields) {
					if ((f.Attributes & FieldAttributes.FieldAccessMask) != FieldAttributes.Private && f.DeclaringType.Equals (type)) {
						string strName = MissingMember.GetUniqueName (f);
						members.Add (strName, f);
					}
				}
			}
		}

		public override NodeStatus Analyze ()
		{
			Hashtable htMono = new Hashtable ();
			FillMembersMap (htMono, typeMono);

			Hashtable htMS = new Hashtable ();
			FillMembersMap (htMS, typeMS);

			Hashtable htMethodsMS = new Hashtable ();

			foreach (MemberReference miMS in htMS)
			{
				string strNameUnique = MissingMember.GetUniqueName (miMS);
				MemberReference miMono = (MemberReference) htMono [strNameUnique];
				AddMember (miMono, miMS);
				if (miMono != null) {
					htMono.Remove (strNameUnique);
				}

				if ( miMS is MethodDefinition && !((MethodDefinition)miMS).IsConstructor)
				{
					string strNameMSFull = miMS.ToString ();
					int ichMS = strNameMSFull.IndexOf (' ');
					string strNameMS = strNameMSFull.Substring (ichMS + 1);
					if (!htMethodsMS.Contains (strNameMS))
						htMethodsMS.Add (strNameMSFull.Substring (ichMS + 1), miMS);
				}
			}
			foreach (MemberReference miMono in htMono.Values)//ADDED MEMBERS (not found in MS)
			{
				MissingMember mm = CreateMember (miMono, null, true);
				if (miMono is MethodDefinition && !((MethodDefinition)miMono).IsConstructor)
				{
					string strNameMonoFull = miMono.ToString ();
					int ichMono = strNameMonoFull.IndexOf (' ');
					string strNameMono = strNameMonoFull.Substring (ichMono + 1);
					MemberReference miMS = (MemberReference) htMethodsMS [strNameMono];
					if (miMS != null)
					{
						string strNameMSFull = miMS.ToString ();
						int ichMS = strNameMSFull.IndexOf (' ');
						string strReturnTypeMS = strNameMSFull.Substring (0, ichMS);
						string strReturnTypeMono = strNameMonoFull.Substring (0, ichMono);
						mm.Status.AddWarning ("Return type mismatch, is: '"+strReturnTypeMono+"' [should be: '"+strReturnTypeMS+"']");
					}
				}
			}

			// compare the attributes
			rgAttributes = new ArrayList ();
			nsAttributes = MissingAttribute.AnalyzeAttributes (
				(typeMono == null) ? null : typeMono.CustomAttributes,
				(typeMS == null) ? null : typeMS.CustomAttributes,
				rgAttributes);

			rgInterfaces = new ArrayList ();
			if (typeMono != null && typeMS != null)
			{
				// compare base types
				string strBaseMono = (typeMono.BaseType == null) ? null : typeMono.BaseType.FullName;
				string strBaseMS   = (  typeMS.BaseType == null) ? null :   typeMS.BaseType.FullName;
				if (strBaseMono != strBaseMS)
				{
					m_nodeStatus.AddWarning ("Base class mismatch, is '"+strBaseMono+"' [should be: '"+strBaseMS+"']");
					//Console.WriteLine ("WARNING: Base class mismatch on "+typeMono.FullName+", is: '"+strBaseMono+"' [should be: '"+strBaseMS+"']");
				}

				// compare the interfaces
				Hashtable htInterfacesMono = new Hashtable ();
				InterfaceCollection rgInterfacesMono = typeMono.Interfaces;
				foreach (TypeReference ifaceMono in rgInterfacesMono)
				{
					if (ifaceMono != null)
					{
						string strName = ifaceMono.FullName;
						htInterfacesMono.Add (strName, ifaceMono);
					}
				}
				InterfaceCollection rgInterfacesMS = typeMS.Interfaces;
				foreach (TypeReference ifaceMS in rgInterfacesMS)
				{
					if (ifaceMS != null)
					{
						string strName = ifaceMS.FullName;
						TypeReference ifaceMono = (TypeReference) htInterfacesMono [strName];
						MissingInterface mi = new MissingInterface (ifaceMono, ifaceMS);
						mi.Analyze ();
						rgInterfaces.Add (mi);
						if (ifaceMono != null)
							htInterfacesMono.Remove (strName);
						nsInterfaces.AddChildren (mi.Status);
					}
				}
				foreach (TypeReference ifaceMono in htInterfacesMono.Values)
				{
					MissingInterface mi = new MissingInterface (ifaceMono, null);
					mi.Analyze ();
					rgInterfaces.Add (mi);
					//Console.WriteLine ("WARNING: additional interface on "+typeMono.FullName+": '"+ifaceMono.FullName+"'");
					nsInterfaces.AddChildren (mi.Status);
				}

				// serializable attribute
				// AddFakeAttribute (typeMono.IsSerializable, typeMS.IsSerializable, "System.SerializableAttribute");
				AddFakeAttribute ((typeMono.Attributes & TypeAttributes.LayoutMask) == TypeAttributes.AutoLayout, (typeMS.Attributes & TypeAttributes.LayoutMask) == TypeAttributes.AutoLayout, "System.AutoLayoutAttribute");
				AddFakeAttribute ((typeMono.Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout, (typeMS.Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout, "System.ExplicitLayoutAttribute");
				AddFakeAttribute ((typeMono.Attributes & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout, (typeMS.Attributes & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout, "System.SequentialLayoutAttribute");

				Accessibility accessibilityMono = GetAccessibility (typeMono);
				Accessibility accessibilityMS   = GetAccessibility (typeMS);
				if (accessibilityMono != accessibilityMS)
					m_nodeStatus.AddWarning ("Should be "+AccessibilityToString (accessibilityMono));

				AddFlagWarning (typeMono.IsSealed, typeMS.IsSealed, "sealed");
				AddFlagWarning (typeMono.IsAbstract, typeMS.IsAbstract, "abstract");
			}

			// sum up the sub-sections
			m_nodeStatus.Add (nsAttributes);
			m_nodeStatus.Add (nsMethods);
			m_nodeStatus.Add (nsProperties);
			m_nodeStatus.Add (nsEvents);
			m_nodeStatus.Add (nsFields);
			m_nodeStatus.Add (nsConstructors);
			m_nodeStatus.Add (nsNestedTypes);
			m_nodeStatus.Add (nsInterfaces);

			return m_nodeStatus;
		}

		static Accessibility GetAccessibility (TypeDefinition type)
		{
			TypeAttributes maskedMVisibility = type.Attributes & TypeAttributes.VisibilityMask;
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
			throw new Exception ("Unexpected error in MissingType.GetAccessibility");
		}
	}
}
