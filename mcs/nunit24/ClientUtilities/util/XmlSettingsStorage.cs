// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.ComponentModel;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for XmlSettingsStorage.
	/// </summary>
	public class XmlSettingsStorage : MemorySettingsStorage
	{
		private string filePath;

		public XmlSettingsStorage( string filePath )
		{
			this.filePath = filePath;
		}

		public override void LoadSettings()
		{
			FileInfo info = new FileInfo(filePath);
			if ( !info.Exists || info.Length == 0 )
				return;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load( filePath );

				foreach( XmlElement element in doc.DocumentElement["Settings"].ChildNodes )
				{
					if ( element.Name != "Setting" )
						throw new ApplicationException( "Unknown element in settings file: " + element.Name );

					if ( !element.HasAttribute( "name" ) )
						throw new ApplicationException( "Setting must have 'name' attribute" );

					if ( !element.HasAttribute( "value" ) )
						throw new ApplicationException( "Setting must have 'value' attribute" );

					settings[ element.GetAttribute( "name" ) ] = element.GetAttribute( "value" );
				}
			}
			catch( Exception ex )
			{
				throw new ApplicationException( "Error loading settings file", ex );
			}
		}

		public override void SaveSettings()
		{
			string dirPath = Path.GetDirectoryName( filePath );
			if ( !Directory.Exists( dirPath ) )
				Directory.CreateDirectory( dirPath );

			XmlTextWriter writer = new XmlTextWriter(  filePath, System.Text.Encoding.UTF8 );
			writer.Formatting = Formatting.Indented;

			writer.WriteProcessingInstruction( "xml", "version=\"1.0\"" );
			writer.WriteStartElement( "NUnitSettings" );
			writer.WriteStartElement( "Settings" );

			ArrayList keys = new ArrayList( settings.Keys );
			keys.Sort();

			foreach( string name in keys )
			{
				object val = settings[name];
				if ( val != null )
				{
					writer.WriteStartElement( "Setting");
					writer.WriteAttributeString( "name", name );
					writer.WriteAttributeString( "value", val.ToString() );
					writer.WriteEndElement();
				}
			}

			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.Close();
		}
	}
}
