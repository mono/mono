#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.IO;
using System.Xml;

namespace NUnit.Util
{
	/// <summary>
	/// This class allows loading information about
	/// configurations and assemblies in a Visual
	/// Studio project file and inspecting them.
	/// Only the most common project types are
	/// supported and an exception is thrown if
	/// an attempt is made to load an invalid
	/// file or one of an unknown type.
	/// </summary>
	public class VSProject
	{
		#region Static and Instance Variables

		/// <summary>
		/// VS Project extentions
		/// </summary>
		private static readonly string[] validExtensions = { ".csproj", ".vbproj", ".vjsproj", ".vcproj" };
		
		/// <summary>
		/// VS Solution extension
		/// </summary>
		private static readonly string solutionExtension = ".sln";

		/// <summary>
		/// Path to the file storing this project
		/// </summary>
		private string projectPath;

		/// <summary>
		/// Collection of configs for the project
		/// </summary>
		private VSProjectConfigCollection configs;

		#endregion

		#region Constructor

		public VSProject( string projectPath )
		{
			this.projectPath = Path.GetFullPath( projectPath );
			configs = new VSProjectConfigCollection();		

			Load();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the project.
		/// </summary>
		public string Name
		{
			get { return Path.GetFileNameWithoutExtension( projectPath ); }
		}

		/// <summary>
		/// The path to the project
		/// </summary>
		public string ProjectPath
		{
			get { return projectPath; }
		}

		/// <summary>
		/// Our collection of configurations
		/// </summary>
		public VSProjectConfigCollection Configs
		{
			get { return configs; }
		}

		#endregion

		#region Static Methods

		public static bool IsProjectFile( string path )
		{
			string extension = Path.GetExtension( path );

			foreach( string validExtension in validExtensions )
				if ( extension == validExtension )
					return true;

			return false;
		}

		public static bool IsSolutionFile( string path )
		{
			return Path.GetExtension( path ) == solutionExtension;
		}

		#endregion

		#region Instance Methods

		public void Load()
		{
			if ( !IsProjectFile( projectPath ) )
				ThrowInvalidFileType( projectPath );

			string projectDirectory = Path.GetFullPath( Path.GetDirectoryName( projectPath ) );

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load( projectPath );

				string extension = Path.GetExtension( projectPath );
				string assemblyName;

				switch ( extension )
				{
					case ".vcproj":
						foreach ( XmlNode configNode in doc.SelectNodes( "/VisualStudioProject/Configurations/Configuration" ) )
						{
							string name = configNode.Attributes["Name"].Value;
							string outputPath = configNode.Attributes["OutputDirectory"].Value;
							string outputDirectory = Path.Combine( projectDirectory, outputPath );
							XmlNode toolNode = configNode.SelectSingleNode( "Tool[@Name='VCLinkerTool']" );
							assemblyName = Path.GetFileName( toolNode.Attributes["OutputFile"].Value );
							string assemblyPath = Path.Combine( outputDirectory, assemblyName );

							VSProjectConfig config = new VSProjectConfig ( name );
							config.Assemblies.Add( assemblyPath );

							this.configs.Add( config );
						}
					
						break;

					case ".csproj":
					case ".vbproj":
					case ".vjsproj":
						XmlNode settingsNode = doc.SelectSingleNode( "/VisualStudioProject/*/Build/Settings" );
			
						assemblyName = settingsNode.Attributes["AssemblyName"].Value;
						string outputType = settingsNode.Attributes["OutputType"].Value;

						if ( outputType == "Exe" || outputType == "WinExe" )
							assemblyName = assemblyName + ".exe";
						else
							assemblyName = assemblyName + ".dll";

						XmlNodeList nodes = settingsNode.SelectNodes("Config");
						if ( nodes != null ) 
							foreach ( XmlNode configNode in nodes )
							{
								string name = configNode.Attributes["Name"].Value;
								string outputPath = configNode.Attributes["OutputPath"].Value;
								string outputDirectory = Path.Combine( projectDirectory, outputPath );
								string assemblyPath = Path.Combine( outputDirectory, assemblyName );
				
								VSProjectConfig config = new VSProjectConfig ( name );
								config.Assemblies.Add( assemblyPath );

								configs.Add( config );
							}

						break;

					default:
						break;
				}
			}
			catch( FileNotFoundException )
			{
				throw;
			}
			catch( Exception e )
			{
				ThrowInvalidFormat( projectPath, e );
			}
		}

		private void ThrowInvalidFileType( string projectPath )
		{
			throw new ArgumentException( 
				string.Format( "Invalid project file type: {0}", 
								Path.GetFileName( projectPath ) ) );
		}

		private void ThrowInvalidFormat( string projectPath, Exception e )
		{
			throw new ArgumentException( 
				string.Format( "Invalid project file format: {0}", 
								Path.GetFileName( projectPath ) ), e );
		}

		#endregion
	}
}
