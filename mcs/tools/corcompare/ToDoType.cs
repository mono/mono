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
		
		ArrayList missingMethodList = new ArrayList();
		public ArrayList MissingMethods {
			get {
				return missingMethodList;
			}
		}

		ArrayList todoMethodList = new ArrayList();
		public ArrayList ToDoMethods {
			get {
				return todoMethodList;
			}
		}

		ArrayList missingPropertyList = new ArrayList();
		public ArrayList MissingProperties {
			get {
				return missingPropertyList;
			}
		}

		ArrayList todoPropertyList = new ArrayList();
		public ArrayList ToDoProperties {
			get {
				return todoPropertyList;
			}
		}

		ArrayList missingEventList = new ArrayList();
		public ArrayList MissingEvents {
			get {
				return missingEventList;
			}
		}

		ArrayList todoEventList = new ArrayList();
		public ArrayList ToDoEvents {
			get {
				return todoEventList;
			}
		}

		ArrayList missingFieldList = new ArrayList();
		public ArrayList MissingFields {
			get {
				return missingFieldList;
			}
		}

		ArrayList todoFieldList = new ArrayList();
		public ArrayList ToDoFields {
			get {
				return todoFieldList;
			}
		}

		ArrayList missingConstructorList = new ArrayList();
		public ArrayList MissingConstructors {
			get {
				return missingConstructorList;
			}
		}

		ArrayList todoConstructorList = new ArrayList();
		public ArrayList ToDoConstructors {
			get {
				return todoConstructorList;
			}
		}

		ArrayList missingNestedTypeList = new ArrayList();
		public ArrayList MissingNestedTypes {
			get {
				return missingNestedTypeList;
			}
		}

		ArrayList todoNestedTypeList = new ArrayList();
		public ArrayList ToDoNestedTypes {
			get {
				return todoNestedTypeList;
			}
		}

		public ToDoType(Type t) : base(t) {
		}

		public int MissingCount {
			get {
				return missingMethodList.Count + missingPropertyList.Count;
			}
		}

		public int ToDoCount {
			get {
				return todoMethodList.Count + todoPropertyList.Count;
			}
		}
		
		public static int IndexOf(Type t, ArrayList todoTypes) {
			for(int index = 0; index < todoTypes.Count; index++) {
				if (((ToDoType)todoTypes[index]).Name == t.Name) {
					return index;
				}
			}
			return -1;
		}

		public override string Status {
			get {
				return "todo";
			}
		}

		public void AddToDoMember(Type t, MemberInfo info){
			switch (info.MemberType){
				case MemberTypes.Method:
					todoMethodList.Add(new ToDoMethod(info));
					break;
				case MemberTypes.Property:
					todoPropertyList.Add(new ToDoProperty(info));
					break;
				case MemberTypes.Event:
					todoEventList.Add(new ToDoEvent(info));
					break;
				case MemberTypes.Field:
					todoFieldList.Add(new ToDoField(info));
					break;
				case MemberTypes.Constructor:
					todoConstructorList.Add(new ToDoConstructor(info));
					break;
				case MemberTypes.NestedType:
					todoNestedTypeList.Add(new ToDoNestedType(info));
					break;
				default:
					throw new Exception("Didn't code todo member type: " + info.MemberType.ToString());
			}
		}

		public void AddMissingMember(MemberInfo info){
			switch (info.MemberType){
				case MemberTypes.Method:
					missingMethodList.Add(new MissingMethod(info));
					break;
				case MemberTypes.Property:
					missingPropertyList.Add(new MissingProperty(info));
					break;
				case MemberTypes.Event:
					missingEventList.Add(new MissingEvent(info));
					break;
				case MemberTypes.Field:
					missingFieldList.Add(new MissingField(info));
					break;
				case MemberTypes.Constructor:
					missingConstructorList.Add(new MissingConstructor(info));
					break;
				case MemberTypes.NestedType:
					missingNestedTypeList.Add(new MissingNestedType(info));
					break;
				default:
					throw new Exception("Didn't code missing member type: " + info.MemberType.ToString());
			}
		}
		public override XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltClass = base.CreateXML (doc);
			eltClass.SetAttribute ("missing", MissingCount.ToString());
			eltClass.SetAttribute ("todo", ToDoCount.ToString());

			XmlElement eltMember;
			eltMember = CreateMemberCollectionElement ("methods", MissingMethods, ToDoMethods, doc);
			if (eltMember != null) 
			{
				eltClass.AppendChild (eltMember);
			}

			eltMember = CreateMemberCollectionElement ("properties", MissingProperties, ToDoProperties, doc);
			if (eltMember != null) 
			{
				eltClass.AppendChild (eltMember);
			}

			eltMember = CreateMemberCollectionElement ("events", MissingEvents, ToDoEvents, doc);
			if (eltMember != null) 
			{
				eltClass.AppendChild (eltMember);
			}

			eltMember = CreateMemberCollectionElement ("fields", MissingFields, ToDoFields, doc);
			if (eltMember != null) 
			{
				eltClass.AppendChild (eltMember);
			}

			eltMember = CreateMemberCollectionElement ("constructors", MissingConstructors, ToDoConstructors, doc);
			if (eltMember != null) 
			{
				eltClass.AppendChild (eltMember);
			}

			eltMember = CreateMemberCollectionElement ("nestedTypes", MissingNestedTypes, ToDoNestedTypes, doc);
			if (eltMember != null) 
			{
				eltClass.AppendChild (eltMember);
			}
			return eltClass;
		}

		static XmlElement CreateMemberCollectionElement (string name, ArrayList missingList, ArrayList todoList, XmlDocument doc) 
		{
			XmlElement element = null;
			if (missingList.Count > 0 || todoList.Count > 0)
			{
				element = doc.CreateElement(name);
				if (missingList.Count > 0) 
				{
					foreach (IMissingMember missing in missingList) 
						element.AppendChild (missing.CreateXML (doc));
				}
				if (todoList.Count > 0) 
				{
					foreach (IMissingMember missing in todoList) 
						element.AppendChild (missing.CreateXML (doc));
				}
				element.SetAttribute ("missing", missingList.Count.ToString());
				element.SetAttribute ("todo", todoList.Count.ToString());
			}
			return element;
		}

		static XmlElement CreateMissingElement(IMissingMember member, XmlDocument doc) 
		{
			XmlElement missingElement  = doc.CreateElement(member.Type);
			missingElement.SetAttribute("name", (member.Name));
			missingElement.SetAttribute("status", (member.Status));
			return missingElement;
		}
	}
}
