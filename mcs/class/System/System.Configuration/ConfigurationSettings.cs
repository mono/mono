//
// System.Configuration.ConfigurationSettings.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// C) Christopher Podurgiel
//

using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

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

		private static string applicationConfigFileName;
		
		/// <summary>
		///		ConfigurationSettings Constructor.
		/// </summary>
		private ConfigurationSettings ()
		{
			
		}

		/// <summary>
		///		Returns configuration settings for a user-defined configuration section.
		/// </summary>
		/// <param name="sectionName">The name of the configuration section that configuration settings are read from.</param>
		/// <returns></returns>
		public static object GetConfig(string sectionName)
		{
			// NOTE: We can only use Assembly.GetCallingAssembly() if we're called
			//       directly from user code.
			return GetConfig (Assembly.GetCallingAssembly (), sectionName);
		}

		private static object GetConfig(Assembly assembly, string sectionName)
		{
			//Get the full path to the Applicaton Configuration File.
			applicationConfigFileName =  assembly.Location + ".config";

			//Create an instance of an XML Document.
			XmlDocument ConfigurationDocument = new XmlDocument();

			//Try to load the XML Document.
			try
			{ 
				ConfigurationDocument.Load(applicationConfigFileName);
			}
			catch(XmlException e)
			{
				//Error loading the XML Document.  Throw a ConfigurationException.
				throw(new ConfigurationException(e.Message, applicationConfigFileName, e.LineNumber));
			}

			string sectionHandlerName = GetSectionHandlerType(ConfigurationDocument, sectionName);

			XmlNode sectionNode = ConfigurationDocument.SelectSingleNode("/configuration/" + sectionName);
			
			
			
			//If the specified sectionName is not found, then sectionNode will be null.  When calling objNVSHandler.Create(),
			//sectionNode cannot be null.
			 if(sectionNode == null)
			{
				return null;
			}
			

			//Create a new SectionHandler

			//According to the Docs provided by Microsoft, the user can create their own configuration sections, and create a custom
			//handler class for it. The user would specify the class and its assebly in the <configSections> section.  These would be
			//seperated by a comma.

			string sectionHandlerClassName = sectionHandlerName;
			string sectionHandlerAssemblyName = "System";

			//Split the SectionHandler Class Name from the Assembly Name (if provided).
			string[] sectionHandlerArray = sectionHandlerName.Split(new char[]{','}, 2);
			if(sectionHandlerArray.Length == 2)
			{
				sectionHandlerClassName = sectionHandlerArray[0];
				sectionHandlerAssemblyName = sectionHandlerArray[1];
			}
			
			// Load the assembly to use.
			Assembly assem = Assembly.Load(sectionHandlerAssemblyName);
			//Get the class type.
			Type handlerObjectType = assem.GetType(sectionHandlerClassName);
			//Get a reference to the method "Create"
			MethodInfo createMethod = handlerObjectType.GetMethod("Create");
			//Create an Instance of this SectionHandler.
			Object objSectionHandler = Activator.CreateInstance(handlerObjectType);

			//define the arguments to be passed to the "Create" Method.
			Object[] args = new Object[3];
			args[0] = null;
			args[1] = null;
			args[2] = sectionNode;

			object sectionHandlerCollection = createMethod.Invoke(objSectionHandler, args);

			//Return the collection
			return sectionHandlerCollection;

		}


		/// <summary>
		///		Gets the name of the SectionHandler Class that will handle this section.
		/// </summary>
		/// <param name="xmlDoc">An xml Configuration Document.</param>
		/// <param name="sectionName">The name of the configuration section that configuration settings are read from.</param>
		/// <returns>The name of the Handler Object for this configuration section, including the name if its Assembly.</returns>
		[MonoTODO]
		private static string GetSectionHandlerType(XmlDocument xmlDoc, string sectionName)
		{
			//TODO: This method does not account for sectionGroups yet.
			string handlerName = null;

			//<appSettings> is a predefined configuration section. It does not have a definition
			// in the <configSections> section, and will always be handled by the NameValueSectionHandler.
			if(sectionName == "appSettings")
			{
				handlerName = "System.Configuration.NameValueSectionHandler";
			}
			else
			{
				
				string[] sectionPathArray = sectionName.Split(new char[]{'/'});

				//Build an XPath statement.
				string xpathStatement = "/configuration/configSections";
				for (int i=0; i < sectionPathArray.Length; i++)
				{
					if(i < sectionPathArray.Length - 1)
					{
						xpathStatement = xpathStatement + "/sectionGroup[@name='" + sectionPathArray[i] + "']";
					}
					else
					{
						xpathStatement = xpathStatement + "/section[@name='" + sectionPathArray[i] + "']";
					}
				}
				
				//Get all of the <section> node using the xpath statement.
				XmlNode sectionNode = xmlDoc.SelectSingleNode(xpathStatement);

				// if this section isn't found, then there was something wrong with the config document,
				// or the sectionName didn't have a proper definition.
				if(sectionNode == null)
				{
					
					throw (new ConfigurationException("Unrecognized element."));
				}

				handlerName =  sectionNode.Attributes["type"].Value;

			}

			//Return the name of the handler.
			return handlerName;
		}



		/// <summary>
		///		Get the Application Configuration Settings.
		/// </summary>
		public static NameValueCollection AppSettings
		{
			get
			{	
				//Get the Configuration Settings for the "appSettings" section.
				NameValueCollection appSettings = (NameValueCollection)GetConfig(
					Assembly.GetCallingAssembly (), "appSettings");

				return appSettings;
			}
		}

	}
}


