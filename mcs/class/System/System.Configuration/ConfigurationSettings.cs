//
// System.Configuration.ConfigurationSettings.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.XPath;
using System.Windows.Forms;

namespace System.Configuration
{
	/// <summary>
	///		Component class.
	/// </summary>
	/// <remarks>
	///		Longer description
	/// </remarks>

	public sealed class ConfigurationSettings
	{

		static NameValueCollection _nvcAppSettings;
		private static string applicationConfigFileName;
		
		/// <summary>
		///		ConfigurationSettings Constructor.
		/// </summary>
		public ConfigurationSettings ()
		{
			
		}

		/// <summary>
		///		Returns configuration settings for a user-defined configuration section.
		/// </summary>
		/// <param name="sectionName"></param>
		/// <returns></returns>
		public static object GetConfig(string sectionName)
		{
			//Create an instance of an XML Document.
			XmlDocument ConfigurationDocument = new XmlDocument();
			XmlNode sectionNode;


			
			/*
			 * LAMESPEC: The .config file that needs to be parsed is the name of the application, plus ".config"
			 * ie. "Myapplication.exe.config"
			 * The only way I could find to get the name of the application is through System.Forms.Application.ExecutablePath, this
			 * may be an incorrect way to get this information.  It works properly on a windows machine when building an executable,
			 * however, I'm not sure how this would work under other platforms.
			*/
			//Get the full path to the Applicaton Configuration File.
			applicationConfigFileName = Application.ExecutablePath + ".config";
			
			//Create a new NameValueSectionHandler
			NameValueSectionHandler objNVSHandler = new NameValueSectionHandler();

			//Try to load the XML Document.
			try
			{ 
				ConfigurationDocument.Load(applicationConfigFileName);
				sectionNode = ConfigurationDocument.SelectSingleNode("//" + sectionName);
			}
			catch(XmlException)
			{
				//If an XmlException is thrown, it probably means that the .config file we are trying to load doesn't exist.
				return null;
			}
			catch(XPathException)
			{	
				//An XPathException is thrown if there was a problem with our XPath statement.
				return null;	
			}
			
			//If the specified sectionName is not found, then sectionNode will be null, and we can't pass
			// a null to objNVSHandler.Create()
			 if(sectionNode == null)
			{
				return null;
			}
			
			//Create a NameValueSecitonHandler and add it to the NameValueCollection
			object readOnlyNVCollection = objNVSHandler.Create(null, null, sectionNode);

			
			//Return the collection
			return readOnlyNVCollection;
		}

		/// <summary>
		///		Get the Application Configuration Settings.
		/// </summary>
		public static NameValueCollection AppSettings
		{
			get
			{	//Define appSettings as a NameValueCollection.
				NameValueCollection appSettings;

				//Get the Configuration Settings for the "appSettings" section.
				appSettings = (NameValueCollection) GetConfig("appSettings");;

				return appSettings;
			}
		}

	}
}


