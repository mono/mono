// type.cs - Mono Documentation Lib
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

using System;
using System.Collections;

namespace Mono.Document.Library {

	public class DocType : IComparable {

		string name, _namespace, fileroot, language, summary, remarks;
		bool isClass, isDelegate, isEnum, isInterface, isStructure, isNested = false;
		ArrayList classes, delegates, enums, interfaces, structures, constructors,
		events, fields, methods, operators, properties, seealsos, _params, members;

		public DocType ()
		{
			classes = new ArrayList ();
			delegates = new ArrayList ();
			enums = new ArrayList ();
			interfaces  = new ArrayList ();
			structures = new ArrayList ();
			constructors = new ArrayList ();
			events = new ArrayList ();
			fields = new ArrayList ();
			methods = new ArrayList ();
			operators = new ArrayList ();
			properties = new ArrayList ();
			seealsos = new ArrayList ();
			_params = new ArrayList ();
			members = new ArrayList ();
		}

		public int CompareTo (Object value)
		{
			if (value == null)
				return 1;
			if (!(value is DocType))
				throw new ArgumentException ();
			if (this.Namespace != (value as DocType).Namespace)
				return String.Compare ((value as DocType).Namespace, this.Namespace);
			if (Score (this) > Score (value as DocType))
				return 1;
			else if (Score (this) < Score (value as DocType))
				return -1;
			else
				return String.Compare ((value as DocType).Name, this.Name);
		}

		private int Score (DocType type)
		{
			if (type.IsClass)
				return 5;
			else if (type.IsDelegate)
				return 4;
			else if (type.IsEnum)
				return 3;
			else if (type.IsInterface)
				return 2;
			else if (type.IsStructure)
				return 1;
			else
				return 0;
		}

		public void Sort ()
		{
			classes.Sort ();
			delegates.Sort ();
			enums.Sort ();
			interfaces.Sort ();
			structures.Sort ();
			constructors.Sort ();
			events.Sort ();
			fields.Sort ();
			methods.Sort ();
			operators.Sort ();
			properties.Sort ();
		}
		
		public void AddClass (DocType type)
		{
			classes.Add (type);
		}

		public void AddDelegate (DocType type)
		{
			delegates.Add (type);
		}

		public void AddEnum (DocType type)
		{
			enums.Add (type);
		}

		public void AddInterface (DocType type)
		{
			interfaces.Add (type);
		}

		public void AddStructure (DocType type)
		{
			structures.Add (type);
		}

		public void AddConstructor (DocMember member)
		{
			constructors.Add (member);
		}

		public void AddEvent (DocMember member)
		{
			events.Add (member);
		}

		public void AddField (DocMember member)
		{
			fields.Add (member);
		}

		public void AddMethod (DocMember member)
		{
			methods.Add (member);
		}

		public void AddOperator (DocMember member)
		{
			operators.Add (member);
		}

		public void AddProperty (DocMember member)
		{
			properties.Add (member);
		}

		public void AddSeeAlso (DocSeeAlso see)
		{
			seealsos.Add (see);
		}

		public void AddParam (DocParam param)
		{
			_params.Add (param);
		}

		public void AddEnumMember (DocEnumMember member)
		{
			members.Add (member);
		}

		//Meta Information
		public bool IsNested
		{
			get {return isNested;}
			set {isNested = value;}
		}

		public string Type
		{
			get {
				if (IsClass)
					return "class";
				else if (IsDelegate)
					return "delegate";
				else if (IsEnum)
					return "enum";
				else if (IsInterface)
					return "interface";
				else
					return "structure";
			}
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
		
		public string FileRoot
		{
			get {return fileroot;}
			set {fileroot = value;}
		}

		public string FileLanguage
		{
			get {return fileroot+"/"+language;}
		}

		public string FileNamespace
		{
			get {return fileroot+"/"+language+"/"+_namespace;}
		}

		public string FilePath
		{
			get {return fileroot+"/"+language+"/"+_namespace+"/"+name+".xml";}
		}

		public string Language
		{
			get {return language;}
			set {language = value;}
		}
		
		public string Summary
		{
			get {return summary;}
			set {summary = value;}
		}

		public string Remarks
		{
			get {return remarks;}
			set {remarks = value;}
		}

		public bool IsClass
		{
			get {return isClass;}
			set {isClass = value;}
		}

		public bool IsDelegate
		{
			get {return isDelegate;}
			set {isDelegate = value;}
		}

		public bool IsEnum
		{
			get {return isEnum;}
			set {isEnum = value;}
		}

		public bool IsInterface
		{
			get {return isInterface;}
			set {isInterface = value;}
		}

		public bool IsStructure
		{
			get {return isStructure;}
			set {isStructure = value;}
		}
		
		//Type level lists
		public ArrayList Classes
		{
			get {return classes;}
		}

		public ArrayList Delegates
		{
			get {return delegates;}
		}

		public ArrayList Enums
		{
			get {return enums;}
		}

		public ArrayList Interfaces
		{
			get {return interfaces;}
		}

		public ArrayList Structures
		{
			get {return structures;}
		}

		//Member level lists
		public ArrayList Constructors
		{
			get {return constructors;}
		}

		public ArrayList Events
		{
			get {return events;}
		}

		public ArrayList Fields
		{
			get {return fields;}
		}

		public ArrayList Methods
		{
			get {return methods;}
		}

		public ArrayList Operators
		{
			get {return operators;}
		}

		public ArrayList Properties
		{
			get {return properties;}
		}
		
		public ArrayList SeeAlsos
		{
			get {return seealsos;}
		}
		
		public ArrayList Params
		{
			get {return _params;}
		}
		
		public ArrayList EnumMembers
		{
			get {return members;}
		}
	}
}
