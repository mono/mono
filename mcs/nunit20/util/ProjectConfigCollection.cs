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

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for ProjectConfigCollection.
	/// </summary>
	public class ProjectConfigCollection : CollectionBase
	{
		protected NUnitProject project;

		public ProjectConfigCollection( NUnitProject project ) 
		{ 
			this.project = project;
		}

		#region Properties

		public NUnitProject Project
		{
			get { return project; }
		}

		public ArrayList Names
		{
			get
			{
				ArrayList names = new ArrayList();
				
				foreach( ProjectConfig config in InnerList )
					names.Add( config.Name );

				return names;
			}
		}

		public ProjectConfig this[int index]
		{
			get { return (ProjectConfig)InnerList[index]; }
		}

		public ProjectConfig this[string name]
		{
			get 
			{ 
				int index = IndexOf( name );
				return index >= 0 ? (ProjectConfig)InnerList[index]: null;
			}
		}
		#endregion

		#region Methods

		public void Add( ProjectConfig config )
		{
			List.Add( config );
			config.Project = this.Project;
		}

		public void Add( string name )
		{
			Add( new ProjectConfig( name ) );
		}

		public void Remove( ProjectConfig config )
		{
			string name = config.Name;
			bool wasActive = name == this.Project.ActiveConfigName;
			List.Remove( config );
		}

		public void Remove( string name )
		{
			int index = IndexOf( name );
			if ( index >= 0 )
			{
				bool wasActive = name == this.Project.ActiveConfigName;
				RemoveAt( index );
			}
		}

		public int IndexOf( ProjectConfig config )
		{
			return InnerList.IndexOf( config );
		}

		public int IndexOf( string name )
		{
			for( int index = 0; index < InnerList.Count; index++ )
			{
				ProjectConfig config = (ProjectConfig)InnerList[index];
				if( config.Name == name )
					return index;
			}

			return -1;
		}

		public bool Contains( ProjectConfig config )
		{
			return InnerList.Contains( config );
		}

		public bool Contains( string name )
		{
			return IndexOf( name ) >= 0;
		}

		protected override void OnRemoveComplete( int index, object obj )
		{
			ProjectConfig config = obj as ProjectConfig;
			this.Project.OnProjectChange( ProjectChangeType.RemoveConfig, config.Name );
		}

		protected override void OnInsertComplete( int index, object obj )
		{
			ProjectConfig config = obj as ProjectConfig;
			project.OnProjectChange( ProjectChangeType.AddConfig, config.Name );
			config.Changed += new EventHandler( OnConfigChanged );
		}

		protected override void OnSetComplete( int index, object oldValue, object newValue )
		{
			ProjectConfig oldConfig = oldValue as ProjectConfig;
			ProjectConfig newConfig = newValue as ProjectConfig;
			bool active = oldConfig.Name == project.ActiveConfigName;
			
			project.OnProjectChange( ProjectChangeType.UpdateConfig, newConfig.Name );
		}

		private void OnConfigChanged( object sender, EventArgs e )
		{
			ProjectConfig config = sender as ProjectConfig;
			project.OnProjectChange( ProjectChangeType.UpdateConfig, config.Name );
		}

		#endregion
	}
}
