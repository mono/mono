// member.cs - Mono Documentation Lib
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

using System;
using System.IO;
using System.Collections;

namespace Mono.Document.Library {

	public class DocMember : IComparable {

		string name, args, fullargs;
		ArrayList _params;
		bool isConstructor, isEvent, isField, isMethod, isOperator, isProperty = false;

		public DocMember ()
		{
			_params = new ArrayList ();
		}

		public int CompareTo (Object value)
		{
			if (value == null)
				return 1;
			if (!(value is DocMember))
				throw new ArgumentException ();
			return String.Compare ((value as DocMember).Name+(value as DocMember).Args, this.Name+this.Args);
		}

		public ArrayList Params
		{
			get {return _params;}
		}

		public void AddParam (DocParam param)
		{
			_params.Add (param);
		}

		public string Name
		{
			get {return name;}
			set {name = value;}
		}

		public string Args
		{
			get {return args;}
			set {args = value;}
		}

		public string FullArgs
		{
			get {return fullargs;}
			set {fullargs = value;}
		}
		
		public string FullName
		{
			get {return name+" "+args;}
		}
		
		public bool IsConstructor
		{
			get {return isConstructor;}
			set {isConstructor = value;}
		}

		public bool IsEvent
		{
			get {return isEvent;}
			set {isEvent = value;}
		}

		public bool IsField
		{
			get {return isField;}
			set {isField = value;}
		}

		public bool IsMethod
		{
			get {return isMethod;}
			set {isMethod = value;}
		}

		public bool IsOperator
		{
			get {return isOperator;}
			set {isOperator = value;}
		}

		public bool IsProperty
		{
			get {return isProperty;}
			set {isProperty = value;}
		}
	}
}
