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
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Threading;
using NUnit.Core;

namespace NUnit.Util
{
	/// <summary>
	/// Types of changes that may occur to a config
	/// </summary>
	public enum ProjectChangeType
	{
		ActiveConfig,
		AddConfig,
		RemoveConfig,
		UpdateConfig,
		Other
	}

	/// <summary>
	///  Arguments for a project event
	/// </summary>
	public class ProjectEventArgs : EventArgs
	{
		public ProjectChangeType type;
		public string configName;

		public ProjectEventArgs( ProjectChangeType type, string configName )
		{
			this.type = type;
			this.configName = configName;
		}
	}

	/// <summary>
	/// Delegate to be used to handle project events
	/// </summary>
	public delegate void ProjectEventHandler( object sender, ProjectEventArgs e );

	/// <summary>
	/// Class that represents an NUnit test project
	/// </summary>
	public class NUnitProject
	{
		#region Static and instance variables

		/// <summary>
		/// Used to generate default names for projects
		/// </summary>
		private static int projectSeed = 0;

		/// <summary>
		/// The extension used for test projects
		/// </summary>
		private static readonly string nunitExtension = ".nunit";

		/// <summary>
		/// Path to the file storing this project
		/// </summary>
		protected string projectPath;

		/// <summary>
		///  Whether the project is dirty
		/// </summary>
		protected bool isDirty = false;
		
		/// <summary>
		/// Collection of configs for the project
		/// </summary>
		protected ProjectConfigCollection configs;

		/// <summary>
		/// The currently active configuration
		/// </summary>
		private ProjectConfig activeConfig;

		/// <summary>
		/// Flag indicating that this project is a
		/// temporary wrapper for an assembly.
		/// </summary>
		private bool isAssemblyWrapper = false;

		#endregion

		#region Constructor

		public NUnitProject( string projectPath )
		{
			this.projectPath = Path.GetFullPath( projectPath );
			configs = new ProjectConfigCollection( this );		
		}

		#endregion

		#region Static Methods

		// True if it's one of our project types
		public static bool IsProjectFile( string path )
		{
			return Path.GetExtension( path ) == nunitExtension;
		}

		// True if it's ours or one we can load
		public static bool CanLoadAsProject( string path )
		{
			return	IsProjectFile( path ) ||
					VSProject.IsProjectFile( path ) ||
					VSProject.IsSolutionFile( path );
		}

		public static string GenerateProjectName()
		{
			return string.Format( "Project{0}", ++projectSeed );
		}

		public static NUnitProject EmptyProject()
		{
			return new NUnitProject( GenerateProjectName() );
		}

		public static NUnitProject NewProject()
		{
			NUnitProject project = EmptyProject();

			project.Configs.Add( "Debug" );
			project.Configs.Add( "Release" );
			project.IsDirty = false;

			return project;
		}

		/// <summary>
		/// Return a test project by either loading it from
		/// the supplied path, creating one from a VS file
		/// or wrapping an assembly.
		/// </summary>
		public static NUnitProject LoadProject( string path )
		{
			if ( NUnitProject.IsProjectFile( path ) )
			{
				NUnitProject project = new NUnitProject( path );
				project.Load();
				return project;
			}
			else if ( VSProject.IsProjectFile( path ) )
				return NUnitProject.FromVSProject( path );
			else if ( VSProject.IsSolutionFile( path ) )
				return NUnitProject.FromVSSolution( path );
			else
				return NUnitProject.FromAssembly( path );
			
		}

		/// <summary>
		/// Creates a project to wrap a list of assemblies
		/// </summary>
		public static NUnitProject FromAssemblies( string[] assemblies )
		{
			// if only one assembly is passed in then the configuration file
			// should follow the name of the assembly. This will only happen
			// if the LoadAssembly method is called. Currently the console ui
			// does not differentiate between having one or multiple assemblies
			// passed in.
			if ( assemblies.Length == 1)
				return NUnitProject.FromAssembly(assemblies[0]);


			NUnitProject project = NUnitProject.EmptyProject();
			ProjectConfig config = new ProjectConfig( "Default" );
			foreach( string assembly in assemblies )
			{
				string fullPath = Path.GetFullPath( assembly );

				if ( !File.Exists( fullPath ) )
					throw new FileNotFoundException( string.Format( "Assembly not found: {0}", fullPath ) );
				
				config.Assemblies.Add( fullPath );
			}

			project.Configs.Add( config );

			// TODO: Deduce application base, and provide a
			// better value for loadpath and project path
			// analagous to how new projects are handled
			string basePath = Path.GetDirectoryName( Path.GetFullPath( assemblies[0] ) );
			project.projectPath = Path.Combine( basePath, project.Name + ".nunit" );

			project.IsDirty = true;

			return project;
		}

		/// <summary>
		/// Creates a project to wrap an assembly
		/// </summary>
		public static NUnitProject FromAssembly( string assemblyPath )
		{
			if ( !File.Exists( assemblyPath ) )
				throw new FileNotFoundException( string.Format( "Assembly not found: {0}", assemblyPath ) );

			string fullPath = Path.GetFullPath( assemblyPath );

			NUnitProject project = new NUnitProject( fullPath );
			
			ProjectConfig config = new ProjectConfig( "Default" );
			config.Assemblies.Add( fullPath );
			project.Configs.Add( config );

			project.isAssemblyWrapper = true;
			project.IsDirty = false;

			return project;
		}

		public static NUnitProject FromVSProject( string vsProjectPath )
		{
			NUnitProject project = new NUnitProject( Path.GetFullPath( vsProjectPath ) );

			VSProject vsProject = new VSProject( vsProjectPath );
			project.Add( vsProject );

			project.isDirty = false;

			return project;
		}

		public static NUnitProject FromVSSolution( string solutionPath )
		{
			NUnitProject project = new NUnitProject( Path.GetFullPath( solutionPath ) );

			string solutionDirectory = Path.GetDirectoryName( solutionPath );
			StreamReader reader = new StreamReader( solutionPath );

			char[] delims = { '=', ',' };
			char[] trimchars = { ' ', '"' };

			string line = reader.ReadLine();
			while ( line != null )
			{
				if ( line.StartsWith( "Project" ) )
				{
					string[] parts = line.Split( delims );
					string vsProjectPath = Path.Combine( solutionDirectory, parts[2].Trim(trimchars) );
					
					if ( VSProject.IsProjectFile( vsProjectPath ) )
						project.Add( new VSProject( vsProjectPath ) );
				}

				line = reader.ReadLine();
			}

			project.isDirty = false;

			return project;
		}

		/// <summary>
		/// Figure out the proper name to be used when saving a file.
		/// </summary>
		public static string ProjectPathFromFile( string path )
		{
			string fileName = Path.GetFileNameWithoutExtension( path ) + nunitExtension;
			return Path.Combine( Path.GetDirectoryName( path ), fileName );
		}

		#endregion

		#region Properties and Events

		public static int ProjectSeed
		{
			get { return projectSeed; }
			set { projectSeed = value; }
		}

		/// <summary>
		/// The path to which a project will be saved.
		/// </summary>
		public string ProjectPath
		{
			get { return projectPath; }
			set 
			{
				projectPath = Path.GetFullPath( value );
				isDirty = true;
			}
		}

		/// <summary>
		/// The base path for the project is the
		/// directory part of the project path.
		/// </summary>
		public string BasePath
		{
			get { return Path.GetDirectoryName( projectPath ); }
		}

		/// <summary>
		/// The name of the project.
		/// </summary>
		public string Name
		{
			get { return Path.GetFileNameWithoutExtension( projectPath ); }
		}

		public ProjectConfig ActiveConfig
		{
			get 
			{ 
				// In case the previous active config was removed
				if ( activeConfig != null && !configs.Contains( activeConfig ) )
					activeConfig = null;
				
				// In case no active config is set or it was removed
				if ( activeConfig == null && configs.Count > 0 )
					activeConfig = configs[0];
				
				return activeConfig; 
			}
		}

		// Safe access to name of the active config
		public string ActiveConfigName
		{
			get
			{
				ProjectConfig config = ActiveConfig;
				return config == null ? null : config.Name;
			}
		}

		public bool IsLoadable
		{
			get
			{
				return	ActiveConfig != null &&
					ActiveConfig.Assemblies.Count > 0;
			}
		}

		// A project made from a single assembly is treated
		// as a transparent wrapper for some purposes until
		// a change is made to it.
		public bool IsAssemblyWrapper
		{
			get { return isAssemblyWrapper; }
		}

		public string ConfigurationFile
		{
			get 
			{ 
				// TODO: Check this
				return isAssemblyWrapper
					  ? Path.GetFileName( projectPath ) + ".config"
					  : Path.GetFileNameWithoutExtension( projectPath ) + ".config";
			}
		}

		public bool IsDirty
		{
			get { return isDirty; }
			set { isDirty = value; }
		}

		public ProjectConfigCollection Configs
		{
			get { return configs; }
		}

		public event ProjectEventHandler Changed;

		#endregion

		#region Instance Methods

		public void SetActiveConfig( int index )
		{
			activeConfig = configs[index];
			OnProjectChange( ProjectChangeType.ActiveConfig, activeConfig.Name );
		}

		public void SetActiveConfig( string name )
		{
			foreach( ProjectConfig config in configs )
			{
				if ( config.Name == name )
				{
					activeConfig = config;
					OnProjectChange( ProjectChangeType.ActiveConfig, activeConfig.Name );
					break;
				}
			}
		}

		public void OnProjectChange( ProjectChangeType type, string configName )
		{
			isDirty = true;

			if ( isAssemblyWrapper )
			{
				projectPath = Path.ChangeExtension( projectPath, ".nunit" );
				isAssemblyWrapper = false;
			}

			if ( Changed != null )
				Changed( this, new ProjectEventArgs( type, configName ) );

			if ( type == ProjectChangeType.RemoveConfig && activeConfig.Name == configName )
			{
				if ( configs.Count > 0 )
					SetActiveConfig( 0 );
			}
		}

		public void Add( VSProject vsProject )
		{
			foreach( VSProjectConfig vsConfig in vsProject.Configs )
			{
				string name = vsConfig.Name;

				if ( !this.Configs.Contains( name ) )
					this.Configs.Add( name );

				ProjectConfig config = this.Configs[name];

				foreach ( string assembly in vsConfig.Assemblies )
					config.Assemblies.Add( assembly );
			}
		}

		public void Load()
		{
			XmlTextReader reader = new XmlTextReader( projectPath );

			string activeConfigName = null;
			ProjectConfig currentConfig = null;
			
			try
			{
				reader.MoveToContent();
				if ( reader.NodeType != XmlNodeType.Element || reader.Name != "NUnitProject" )
					throw new ProjectFormatException( 
						"Invalid project format: <NUnitProject> expected.", 
						reader.LineNumber, reader.LinePosition );

				while( reader.Read() )
					if ( reader.NodeType == XmlNodeType.Element )
						switch( reader.Name )
						{
							case "Settings":
								if ( reader.NodeType == XmlNodeType.Element )
									activeConfigName = reader.GetAttribute( "activeconfig" );
								break;

							case "Config":
								if ( reader.NodeType == XmlNodeType.Element )
								{
									string configName = reader.GetAttribute( "name" );
									currentConfig = new ProjectConfig( configName );
									currentConfig.BasePath = reader.GetAttribute( "appbase" );
									currentConfig.ConfigurationFile = reader.GetAttribute( "configfile" );

									string binpath = reader.GetAttribute( "binpath" );
									string type = reader.GetAttribute( "binpathtype" );
									if ( type == null )
										if ( binpath == null )
											currentConfig.BinPathType = BinPathType.Auto;
										else
											currentConfig.BinPathType = BinPathType.Manual;
									else
										currentConfig.BinPathType = (BinPathType)Enum.Parse( typeof( BinPathType ), type, true );
									Configs.Add( currentConfig );
									if ( configName == activeConfigName )
										activeConfig = currentConfig;
								}
								else if ( reader.NodeType == XmlNodeType.EndElement )
									currentConfig = null;
								break;

							case "assembly":
								if ( reader.NodeType == XmlNodeType.Element && currentConfig != null )
								{
									string path = reader.GetAttribute( "path" );
									string test = reader.GetAttribute( "test" );
									bool hasTests = test == null ? true : bool.Parse( test );
									currentConfig.Assemblies.Add( 
										Path.Combine( currentConfig.BasePath, path ),
										hasTests );
								}
								break;

							default:
								break;
						}

				this.IsDirty = false;
			}
			catch( XmlException e )
			{
				throw new ProjectFormatException(
					string.Format( "Invalid project format: {0}", e.Message ),
					e.LineNumber, e.LinePosition );
			}
			catch( Exception e )
			{
				throw new ProjectFormatException( 
					string.Format( "Invalid project format: {0} Line {1}, Position {2}", 
					e.Message, reader.LineNumber, reader.LinePosition ),
					reader.LineNumber, reader.LinePosition );
			}
			finally
			{
				reader.Close();
			}
		}

		public void Save()
		{
			projectPath = ProjectPathFromFile( projectPath );

			XmlTextWriter writer = new XmlTextWriter(  projectPath, System.Text.Encoding.UTF8 );
			writer.Formatting = Formatting.Indented;

			writer.WriteStartElement( "NUnitProject" );
			
			if ( configs.Count > 0 )
			{
				writer.WriteStartElement( "Settings" );
				writer.WriteAttributeString( "activeconfig", ActiveConfigName );
				writer.WriteEndElement();
			}
			
			foreach( ProjectConfig config in Configs )
			{
				writer.WriteStartElement( "Config" );
				writer.WriteAttributeString( "name", config.Name );
				if ( config.RelativeBasePath != null )
					writer.WriteAttributeString( "appbase", config.RelativeBasePath );
				
				string configFile = config.ConfigurationFile;
				if ( configFile != null && configFile != this.ConfigurationFile )
					writer.WriteAttributeString( "configfile", config.ConfigurationFile );
				
				if ( config.BinPathType == BinPathType.Manual )
					writer.WriteAttributeString( "binpath", config.PrivateBinPath );
				else
					writer.WriteAttributeString( "binpathtype", config.BinPathType.ToString() );

				foreach( AssemblyListItem assembly in config.Assemblies )
				{
					writer.WriteStartElement( "assembly" );
					writer.WriteAttributeString( "path", config.RelativePathTo( assembly.FullPath ) );
					if ( !assembly.HasTests )
						writer.WriteAttributeString( "test", "false" );
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
			}

			writer.WriteEndElement();

			writer.Close();
			this.IsDirty = false;

			// Once we save a project, it's no longer
			// loaded as an assembly wrapper on reload.
			this.isAssemblyWrapper = false;
		}

		public void Save( string projectPath )
		{
			this.ProjectPath = projectPath;
			Save();
		}

		#endregion
	}
}
