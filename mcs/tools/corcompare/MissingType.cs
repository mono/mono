// Mono.Util.CorCompare.MissingType
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Xml;
using System.Reflection;
using System.Collections;

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
		Type typeMono, typeMS;
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

		public MissingType (Type _typeMono, Type _typeMS)
		{
			typeMono = _typeMono;
			typeMS = _typeMS;
			m_nodeStatus = new NodeStatus (_typeMono, _typeMS);
		}

		public override string Name 
		{
			get
			{
				Type type = TypeInfoBest;
				if (type.DeclaringType != null)
					return type.DeclaringType.Name + "+" + type.Name;
				return type.Name;
			}
		}

		public override string Type
		{
			get
			{
				Type type = TypeInfo;
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

		public Type TypeInfo
		{
			get { return (typeMono != null) ? typeMono : typeMS; }
		}

		public Type TypeInfoBest
		{
			get { return (typeMS == null) ? typeMono : typeMS; }
		}

		public bool IsDelegate
		{
			get
			{
				Type typeBest = TypeInfoBest;
				if (typeBest.IsEnum || typeBest.IsInterface || typeBest.IsValueType)
					return false;
				Type type = typeBest.BaseType;
				while (type != null)
				{
					if (type.FullName == "System.Delegate")
						return true;
					type = type.BaseType;
				}
				return false;
			}
		}

		public MissingMember CreateMember (MemberInfo infoMono, MemberInfo infoMS)
		{
			MemberTypes mt = (infoMono != null) ? infoMono.MemberType : infoMS.MemberType;
			MissingMember mm;
			switch (mt)
			{
				case MemberTypes.Method:
					mm = new MissingMethod (infoMono, infoMS);
					break;
				case MemberTypes.Property:
					mm = new MissingProperty (infoMono, infoMS);
					break;
				case MemberTypes.Event:
					mm = new MissingEvent (infoMono, infoMS);
					break;
				case MemberTypes.Field:
					mm = new MissingField (infoMono, infoMS);
					break;
				case MemberTypes.Constructor:
					mm = new MissingConstructor (infoMono, infoMS);
					break;
				case MemberTypes.NestedType:
					mm = new MissingNestedType (infoMono, infoMS);
					break;
				default:
					throw new Exception ("Unexpected MemberType: " + mt.ToString());
			}
			mm.Analyze ();
			return mm;
		}


		public void AddMember (MissingMember mm)
		{
			switch (mm.Info.MemberType)
			{
				case MemberTypes.Method:
					nsMethods.AddChildren (mm.Status);
					rgMethods.Add (mm);
					break;
				case MemberTypes.Property:
					nsProperties.AddChildren (mm.Status);
					rgProperties.Add (mm);
					break;
				case MemberTypes.Event:
					nsEvents.AddChildren (mm.Status);
					rgEvents.Add (mm);
					break;
				case MemberTypes.Field:
					nsFields.AddChildren (mm.Status);
					rgFields.Add (mm);
					break;
				case MemberTypes.Constructor:
					nsConstructors.AddChildren (mm.Status);
					rgConstructors.Add (mm);
					break;
				case MemberTypes.NestedType:
					nsNestedTypes.AddChildren (mm.Status);
					rgNestedTypes.Add (mm);
					break;
				default:
					throw new Exception ("Unexpected MemberType: " + mm.Info.ToString());
			}
		}

		public void AddMember (MemberInfo infoMono, MemberInfo infoMS)
		{
			AddMember (CreateMember (infoMono, infoMS));
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

		public override NodeStatus Analyze ()
		{
			Hashtable htMono = new Hashtable ();
			if (typeMono != null)
			{
				ArrayList rgIgnoreMono = new ArrayList ();
				foreach (MemberInfo miMono in typeMono.GetMembers (BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					if (typeMono == miMono.DeclaringType)
					{
						string strName = MissingMember.GetUniqueName (miMono);
						htMono.Add (strName, miMono);

						// ignore any property/event accessors
						if (miMono.MemberType == MemberTypes.Property)
						{
							PropertyInfo pi = (PropertyInfo) miMono;
							MemberInfo miGet = pi.GetGetMethod ();
							if (miGet != null)
								rgIgnoreMono.Add (miGet);
							MemberInfo miSet = pi.GetSetMethod ();
							if (miSet != null)
								rgIgnoreMono.Add (miSet);
						}
						else if (miMono.MemberType == MemberTypes.Event)
						{
							EventInfo ei = (EventInfo) miMono;
							MemberInfo miAdd = ei.GetAddMethod ();
							if (miAdd != null)
								rgIgnoreMono.Add (miAdd);
							MemberInfo miRemove = ei.GetRemoveMethod ();
							if (miRemove != null)
								rgIgnoreMono.Add (miRemove);
							MemberInfo miRaise = ei.GetRaiseMethod ();
							if (miRaise != null)
								rgIgnoreMono.Add (miRaise);
						}
					}
				}
				foreach (MemberInfo miIgnore in rgIgnoreMono)
					htMono.Remove (MissingMember.GetUniqueName (miIgnore));
			}
			Hashtable htMethodsMS = new Hashtable ();
			if (typeMS != null)
			{
				ICollection colMembersMS = typeMS.GetMembers (BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				Hashtable htIgnoreMS = new Hashtable ();
				foreach (MemberInfo miMS in colMembersMS)
				{
					// ignore any property/event accessors
					if (miMS.MemberType == MemberTypes.Property)
					{
						PropertyInfo pi = (PropertyInfo) miMS;
						MemberInfo miGet = pi.GetGetMethod ();
						if (miGet != null)
							htIgnoreMS.Add (miGet, miMS);
						MemberInfo miSet = pi.GetSetMethod ();
						if (miSet != null)
							htIgnoreMS.Add (miSet, miMS);
					}
					else if (miMS.MemberType == MemberTypes.Event)
					{
						EventInfo ei = (EventInfo) miMS;
						MemberInfo miAdd = ei.GetAddMethod ();
						if (miAdd != null)
							htIgnoreMS.Add (miAdd, miMS);
						MemberInfo miRemove = ei.GetRemoveMethod ();
						if (miRemove != null)
							htIgnoreMS.Add (miRemove, miMS);
						MemberInfo miRaise = ei.GetRaiseMethod ();
						if (miRaise != null)
							htIgnoreMS.Add (miRaise, miMS);
					}
				}
				foreach (MemberInfo miMS in colMembersMS)
				{
					if (miMS != null && miMS.DeclaringType == typeMS && !htIgnoreMS.Contains (miMS))
					{
						string strNameUnique = MissingMember.GetUniqueName (miMS);
						MemberInfo miMono = (MemberInfo) htMono [strNameUnique];

						MissingMember mm = CreateMember (miMono, miMS);

						bool fVisibleMS = IsVisible (miMS);
						if (miMono == null)
						{
							if (fVisibleMS)
								AddMember (mm);
						}
						else
						{
							if (miMono.MemberType != miMS.MemberType)
							{
								//AddMember (null, miMS);
								//MissingMember mm2 = CreateMember (miMono, null);
								//mm2.Status.AddWarning ("MemberType mismatch, is: '" + miMono.MemberType.ToString () + "' [should be: '" + miMS.MemberType.ToString ()+"']");
								//AddMember (mm2);
								mm.Status.AddWarning ("MemberType mismatch, is: '" + miMono.MemberType.ToString () + "' [should be: '" + miMS.MemberType.ToString ()+"']");
								AddMember (mm);
							}
							else if (fVisibleMS || IsVisible (miMono))
							{
								AddMember (mm);
							}

							htMono.Remove (strNameUnique);
						}

						switch (miMS.MemberType)
						{
							case MemberTypes.Method:
							{
								string strNameMSFull = miMS.ToString ();
								int ichMS = strNameMSFull.IndexOf (' ');
								string strNameMS = strNameMSFull.Substring (ichMS + 1);
								if (!htMethodsMS.Contains (strNameMS))
									htMethodsMS.Add (strNameMSFull.Substring (ichMS + 1), miMS);
								break;
							}
						}
					}
				}
			}
			foreach (MemberInfo miMono in htMono.Values)
			{
				if (IsVisible (miMono))
				{
					MissingMember mm = CreateMember (miMono, null);
					switch (miMono.MemberType)
					{
						case MemberTypes.Method:
						{
							string strNameMonoFull = miMono.ToString ();
							int ichMono = strNameMonoFull.IndexOf (' ');
							string strNameMono = strNameMonoFull.Substring (ichMono + 1);
							MemberInfo miMS = (MemberInfo) htMethodsMS [strNameMono];
							if (miMS != null)
							{
								string strNameMSFull = miMS.ToString ();
								int ichMS = strNameMSFull.IndexOf (' ');
								string strReturnTypeMS = strNameMSFull.Substring (0, ichMS);
								string strReturnTypeMono = strNameMonoFull.Substring (0, ichMono);
								mm.Status.AddWarning ("Return type mismatch, is: '"+strReturnTypeMono+"' [should be: '"+strReturnTypeMS+"']");
								//Console.WriteLine ("WARNING: Return type mismatch on "+miMS.DeclaringType.FullName+"."+strNameMono+", is: '"+strReturnTypeMono+"' [should be: '"+strReturnTypeMS+"']");
							}
							break;
						}
					}
					AddMember (mm);
				}
			}

			// compare the attributes
			rgAttributes = new ArrayList ();
			nsAttributes = MissingAttribute.AnalyzeAttributes (
				(typeMono == null) ? null : typeMono.GetCustomAttributes (false),
				(  typeMS == null) ? null :   typeMS.GetCustomAttributes (false),
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
				Type [] rgInterfacesMono = typeMono.GetInterfaces ();
				foreach (Type ifaceMono in rgInterfacesMono)
				{
					if (ifaceMono != null)
					{
						string strName = ifaceMono.FullName;
						htInterfacesMono.Add (strName, ifaceMono);
					}
				}
				Type [] rgInterfacesMS = typeMS.GetInterfaces ();
				foreach (Type ifaceMS in rgInterfacesMS)
				{
					if (ifaceMS != null)
					{
						string strName = ifaceMS.FullName;
						Type ifaceMono = (Type) htInterfacesMono [strName];
						MissingInterface mi = new MissingInterface (ifaceMono, ifaceMS);
						mi.Analyze ();
						rgInterfaces.Add (mi);
						if (ifaceMono != null)
							htInterfacesMono.Remove (strName);
						nsInterfaces.AddChildren (mi.Status);
					}
				}
				foreach (Type ifaceMono in htInterfacesMono.Values)
				{
					MissingInterface mi = new MissingInterface (ifaceMono, null);
					mi.Analyze ();
					rgInterfaces.Add (mi);
					//Console.WriteLine ("WARNING: additional interface on "+typeMono.FullName+": '"+ifaceMono.FullName+"'");
					nsInterfaces.AddChildren (mi.Status);
				}

				// serializable attribute
				AddFakeAttribute (typeMono.IsSerializable, typeMS.IsSerializable, "System.SerializableAttribute");
				AddFakeAttribute (typeMono.IsAutoLayout, typeMS.IsAutoLayout, "System.AutoLayoutAttribute");
				AddFakeAttribute (typeMono.IsExplicitLayout, typeMS.IsExplicitLayout, "System.ExplicitLayoutAttribute");
				AddFakeAttribute (typeMono.IsLayoutSequential, typeMS.IsLayoutSequential, "System.SequentialLayoutAttribute");

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

		static bool IsVisible (MemberInfo mi)
		{
			// this is just embarrasing, couldn't they have virtualized this?
			switch (mi.MemberType)
			{
				case MemberTypes.Constructor:
				case MemberTypes.Method:
					return !((MethodBase) mi).IsPrivate && !((MethodBase) mi).IsFamilyAndAssembly && !((MethodBase) mi).IsAssembly;
				case MemberTypes.Field:
					return !((FieldInfo) mi).IsPrivate && !((FieldInfo) mi).IsFamilyAndAssembly && !((FieldInfo) mi).IsAssembly;
				case MemberTypes.NestedType:
					return !((Type) mi).IsNestedPrivate && !((Type) mi).IsNestedAssembly && !((Type) mi).IsNestedFamANDAssem;
				case MemberTypes.Property:	// great, now we have to look at the methods
					PropertyInfo pi = (PropertyInfo) mi;
					MethodInfo miAccessor = pi.GetGetMethod ();
					if (miAccessor == null)
						miAccessor = pi.GetSetMethod ();
					if (miAccessor == null)
						return false;
					return IsVisible (miAccessor);
				case MemberTypes.Event:	// ditto
					EventInfo ei = (EventInfo) mi;
					MethodInfo eiAccessor = ei.GetAddMethod ();
					if (eiAccessor == null)
						eiAccessor = ei.GetRemoveMethod ();
					if (eiAccessor == null)
						eiAccessor = ei.GetRaiseMethod ();
					if (eiAccessor == null)
						return false;
					return IsVisible (eiAccessor);
				default:
					throw new Exception ("Missing handler for MemberType: "+mi.MemberType.ToString ());
			}
		}

		static Accessibility GetAccessibility (Type type)
		{
			if (type.IsPublic)
				return Accessibility.Public;
			else if (type.IsNotPublic)
				return Accessibility.Private;
			return MissingMember.GetAccessibility (type);
		}
	}
}
