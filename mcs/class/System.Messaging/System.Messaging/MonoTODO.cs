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
		public string Description;
		
		public MonoTODOAttribute(string description)
		{
			Description = description;
		}
		
		public MonoTODOAttribute() {}
	}
}
