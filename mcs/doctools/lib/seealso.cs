// seealso.cs - Mono Documentation Lib
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

using System;

namespace Mono.Document.Library {

	public class DocSeeAlso {

		string name, cref;
		
		public DocSeeAlso ()
		{
		}

		public string Name
		{
			get {return name;}
			set {name = value;}
		}
		
		public string Cref
		{
			get {return cref;}
			set {cref = value;}
		}

	}
}
