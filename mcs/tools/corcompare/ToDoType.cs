// Mono.Util.CorCompare.ToDoType
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;
using System.Collections;
using System.Xml;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class that is marked with MonoTODO
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class ToDoType : MissingType
	{
		// e.g. <class name="System.Array" status="todo" missing="5" todo="6" complete="45">
		Type tMono;
		ArrayList rgAttributes = new ArrayList ();
		ArrayList rgMethods = new ArrayList ();
		ArrayList rgProperties = new ArrayList ();
		ArrayList rgEvents = new ArrayList ();
		ArrayList rgFields = new ArrayList ();
		ArrayList rgConstructors = new ArrayList ();
		ArrayList rgNestedTypes = new ArrayList ();
		CompletionInfo ci;
		CompletionInfo ciAttributes = new CompletionInfo ();
		CompletionInfo ciMethods = new CompletionInfo ();
		CompletionInfo ciProperties = new CompletionInfo ();
		CompletionInfo ciEvents = new CompletionInfo ();
		CompletionInfo ciFields = new CompletionInfo ();
		CompletionInfo ciConstructors = new CompletionInfo ();
		CompletionInfo ciNestedTypes = new CompletionInfo ();

		public ToDoType (Type t, Type _tMono) : base(t) 
		{
			tMono = _tMono;
		}

		public override CompletionTypes Completion 
		{
			get { return CompletionTypes.Todo; }
		}

		public void AddMember (MemberInfo infoMono, MemberInfo infoMS)
		{
			MemberTypes mt = (infoMono != null) ? infoMono.MemberType : infoMS.MemberType;
			MissingMember mm;
			switch (mt)
			{
				case MemberTypes.Method:
					mm = new MissingMethod (infoMono, infoMS);
					mm.Analyze ();
					ciMethods.Add (mm.Completion);
					rgMethods.Add (mm);
					break;
				case MemberTypes.Property:
					mm = new MissingProperty (infoMono, infoMS);
					mm.Analyze ();
					ciProperties.Add (mm.Completion);
					rgProperties.Add (mm);
					break;
				case MemberTypes.Event:
					mm = new MissingEvent (infoMono, infoMS);
					mm.Analyze ();
					ciEvents.Add (mm.Completion);
					rgEvents.Add (mm);
					break;
				case MemberTypes.Field:
					mm = new MissingField (infoMono, infoMS);
					mm.Analyze ();
					ciFields.Add (mm.Completion);
					rgFields.Add (mm);
					break;
				case MemberTypes.Constructor:
					mm = new MissingConstructor (infoMono, infoMS);
					mm.Analyze ();
					ciConstructors.Add (mm.Completion);
					rgConstructors.Add (mm);
					break;
				case MemberTypes.NestedType:
					mm = new MissingNestedType (infoMono, infoMS);
					mm.Analyze ();
					ciNestedTypes.Add (mm.Completion);
					rgNestedTypes.Add (mm);
					break;
				default:
					throw new Exception ("Unexpected MemberType: " + mt.ToString());
			}
		}
		public override XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltClass = base.CreateXML (doc);
			ci.SetAttributes (eltClass);

			XmlElement eltMember;

			eltMember = MissingBase.CreateMemberCollectionElement ("attributes", rgAttributes, ciAttributes, doc);
			if (eltMember != null) 
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("methods", rgMethods, ciMethods, doc);
			if (eltMember != null) 
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("properties", rgProperties, ciProperties, doc);
			if (eltMember != null) 
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("events", rgEvents, ciEvents, doc);
			if (eltMember != null) 
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("fields", rgFields, ciFields, doc);
			if (eltMember != null) 
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("constructors", rgConstructors, ciConstructors, doc);
			if (eltMember != null) 
				eltClass.AppendChild (eltMember);

			eltMember = MissingBase.CreateMemberCollectionElement ("nestedTypes", rgNestedTypes, ciNestedTypes, doc);
			if (eltMember != null) 
				eltClass.AppendChild (eltMember);

			return eltClass;
		}

		public override CompletionInfo Analyze ()
		{
			Hashtable htMono = new Hashtable ();
			if (tMono != null)
			{
				foreach (MemberInfo miMono in tMono.GetMembers (BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public))
				{
					if (tMono == miMono.DeclaringType)
					{
						string strName = miMono.ToString ();
						htMono.Add (strName, miMono);
					}
				}
			}
			if (theType != null)
			{
				foreach (MemberInfo miMS in theType.GetMembers (BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public))
				{
					if (miMS != null && miMS.DeclaringType == theType)
					{
						string strName = miMS.ToString ();
						MemberInfo miMono = (MemberInfo) htMono [strName];

						if (miMono == null)
						{
							if (IsVisible (miMS))
							{
								AddMember (null, miMS);
							}
						}
						else
						{
							if (miMono.MemberType == miMS.MemberType)
							{
								htMono.Remove (strName);
								AddMember (miMono, miMS);
							}
							else
							{
								Console.WriteLine ("WARNING!!! MemberType mismatch on "+miMS.DeclaringType.FullName + "." + miMS.Name + " [is '" + miMono.MemberType.ToString () + "', should be '" + miMS.MemberType.ToString ()+"']");
							}
						}
					}
				}
			}
			foreach (MemberInfo miMono in htMono.Values)
			{
				AddMember (miMono, null);
			}

			// sort out the properties
			foreach (MissingProperty property in rgProperties)
			{
				MemberInfo infoBest = property.BestInfo;
				if (infoBest is PropertyInfo)
				{
					PropertyInfo pi = (PropertyInfo) property.BestInfo;
					MethodInfo miGet = pi.GetGetMethod ();
					MethodInfo miSet = pi.GetSetMethod ();

					MissingMethod mmGet = FindMethod (miGet);
					MissingMethod mmSet = FindMethod (miSet);
					
					if (mmGet != null)
					{
						ciMethods.Sub (mmGet.Completion);
						rgMethods.Remove (mmGet);
						property.GetMethod = mmGet;
					}
					if (mmSet != null)
					{
						ciMethods.Sub (mmSet.Completion);
						rgMethods.Remove (mmSet);
						property.SetMethod = mmSet;
					}
				}
				else
				{
					// TODO: handle the mistmatch case.
				}
			}

			// compare the attributes
			rgAttributes = new ArrayList ();
			ciAttributes = MissingAttribute.AnalyzeAttributes (
				(tMono   == null) ? null :   tMono.GetCustomAttributes (false),
				(theType == null) ? null : theType.GetCustomAttributes (false),
				rgAttributes);

			// sum up the sub-sections
			ci = new CompletionInfo ();
			ci.Add (ciAttributes);
			ci.Add (ciMethods);
			ci.Add (ciProperties);
			ci.Add (ciEvents);
			ci.Add (ciFields);
			ci.Add (ciConstructors);
			ci.Add (ciNestedTypes);

			return ci;
		}

		MissingMethod FindMethod (MethodInfo mi)
		{
			if (mi != null)
			{
				string strName = mi.Name;
				foreach (MissingMethod method in rgMethods)
				{
					if (strName == method.Info.Name)
						return method;
				}
			}
			return null;
		}

		static bool IsVisible (MemberInfo mi)
		{
			switch (mi.MemberType)
			{
				case MemberTypes.Constructor:
				case MemberTypes.Method:
					return !((MethodBase) mi).IsPrivate;
				case MemberTypes.Field:
					return !((FieldInfo) mi).IsPrivate;
				case MemberTypes.NestedType:
					return !((Type) mi).IsNestedPrivate;
				case MemberTypes.Event:
				case MemberTypes.Property:
					return true;
				default:
					throw new Exception ("Missing handler for MemberType: "+mi.MemberType.ToString ());
			}
		}
	}
}
