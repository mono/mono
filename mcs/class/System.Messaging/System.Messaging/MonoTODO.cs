//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
//	(C) Ximian, Inc.  http://www.ximian.com
//
using System;

namespace System.Messaging 
{
	internal class MonoTODOAttribute : Attribute 
	{		
                string comment;
		
		public MonoTODOAttribute (string comment)
		{
			this.comment = comment;
		}
		
		public MonoTODOAttribute() {}

		public string Comment {
			get { return comment; }
		}
	}
}
