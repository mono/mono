//
// System.Diagnostics.DiagnosticsConfigurationHandler.cs
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// Author: 
//	John R. Hicks <angryjohn69@nc.rr.com>
//
// (C) 2002
//
using System;
using System.Configuration;
using System.Xml;

namespace System.Diagnostics
{
	public class DiagnosticsConfigurationHandler :
		IConfigurationSectionHandler
	{
		public DiagnosticsConfigurationHandler()
		{
			
		}
		
		public virtual object Create(
		                             object parent,
		                             object configContext,
		                             XmlNode section)
		{
		    throw new NotImplementedException();                         	
		}
		
		
		~DiagnosticsConfigurationHandler() 
		{
			
		}
	}
}

