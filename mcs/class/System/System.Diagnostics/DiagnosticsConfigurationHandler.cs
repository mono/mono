//
// System.Diagnostics.DiagnosticsConfigurationHandler.cs
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
	/// <summary>
	/// The configuration section handler for the diagnostics section of
	/// the configuration file.  The section handler participates in the 
	/// resolution of configuration settings between the &lt;diagnostics&gt; 
	/// and &lt;/diagnostics&gt; portion of the .config file.
	/// </summary>
	public class DiagnosticsConfigurationHandler :
		IConfigurationSectionHandler
	{
		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="DiagnosticsConfigurationHandler">
		/// DiagnosticsConfigurationHandler</see> class.
		/// </summary>
		public DiagnosticsConfigurationHandler()
		{
			
		}
		
		/// <summary>
		/// Parses the configuration settings between the
		/// &lt;diagnostics&gt; and &lt;/diagnostics&gt; portion of the
		/// .config file to populate the values of the object and return it.
		/// </summary>
		/// <param name="parent">
		/// Reference to the &quot;default&quot; value provided by the parent
		/// IConfigurationSectionHandler.
		/// </param>
		/// <param name="configContext">
		/// [To be supplied]
		/// </param>
		/// <param name="section">
		/// [To be supplied]
		/// </param>
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
