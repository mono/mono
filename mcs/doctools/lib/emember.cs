// emember.cs - Mono Documentation Lib
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

using System;

namespace Mono.Document.Library {

	public class DocEnumMember {

		string name, val;
		
		public DocEnumMember ()
		{
		}

		public string Name
		{
			get {return name;}
			set {name = value;}
		}
		
		public string Value
		{
			get {return val;}
			set {val = value;}
		}
	}
}
