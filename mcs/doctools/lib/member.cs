// member.cs - Mono Documentation Lib
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

using System;
using System.IO;
using System.Collections;

namespace Mono.Util.MonoDoc.Lib {

	public class DocMember {

		string name, args, fullargs;
		ArrayList _params;
		bool isCtor, isDtor, isMethod, isField, isProperty, isEvent = false;

		public DocMember ()
		{
			_params = new ArrayList ();
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
		
		public bool IsCtor
		{
			get {return isCtor;}
			set {isCtor = value;}
		}

		public bool IsDtor
		{
			get {return isDtor;}
			set {isDtor = value;}
		}

		public bool IsMethod
		{
			get {return isMethod;}
			set {isMethod = value;}
		}

		public bool IsField
		{
			get {return isField;}
			set {isField = value;}
		}

		public bool IsProperty
		{
			get {return isProperty;}
			set {isProperty = value;}
		}
		
		public bool IsEvent
		{
			get {return isEvent;}
			set {isEvent = value;}
		}
	}
}
