// type.cs - Mono Documentation Lib
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

using System;
using System.Collections;

namespace Mono.Util.MonoDoc.Lib {

	public class DocType {

		string name, _namespace;
		bool isClass, isInterface, isEnum, isStructure, isDelegate, isNested = false;
		ArrayList enums, ctors, dtors, methods, properties, fields, events;

		public DocType ()
		{
			enums = new ArrayList ();
			ctors = new ArrayList ();
			dtors = new ArrayList ();
			methods = new ArrayList ();
			properties = new ArrayList ();
			fields = new ArrayList ();
			events = new ArrayList ();
		}

		public void AddCtor (DocMember member)
		{
			ctors.Add (member);
		}

		public void AddDtor (DocMember member)
		{
			dtors.Add (member);
		}

		public void AddMethod (DocMember member)
		{
			methods.Add (member);
		}
		
		public void AddProperty (DocMember member)
		{
			properties.Add (member);
		}

		public void AddField (DocMember member)
		{
			fields.Add (member);
		}
		
		public void AddEvent (DocMember member)
		{
			events.Add (member);
		}

		public string Name
		{
			get {return name;}
			set {name = value;}
		}
		
		public string Namespace
		{
			get {return _namespace;}
			set {_namespace = value;}
		}

		public string FullName
		{
			get {return _namespace+"."+name;}
		}

		public bool IsInterface
		{
			get {return isInterface;}
			set {isInterface = value;}
		}

		public bool IsClass
		{
			get {return isClass;}
			set {isClass = value;}
		}

		public bool IsStructure
		{
			get {return isStructure;}
			set {isStructure = value;}
		}
		
		public bool IsEnum
		{
			get {return isEnum;}
			set {isEnum = value;}
		}

		public bool IsDelegate
		{
			get {return isDelegate;}
			set {isDelegate = value;}
		}
		
		public bool IsNested
		{
			get {return isNested;}
			set {isNested = value;}
		}

		public ArrayList Enums
		{
			get {return enums;}
		}

		public ArrayList Ctors
		{
			get {return ctors;}
		}

		public ArrayList Dtors
		{
			get {return dtors;}
		}
		
		public ArrayList Methods
		{
			get {return methods;}
		}
		
		public ArrayList Properties
		{
			get {return properties;}
		}
		
		public ArrayList Fields
		{
			get {return fields;}
		}
		
		public ArrayList Events
		{
			get {return events;}
		}
	}
}
