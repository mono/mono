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

namespace NUnit.Util
{
	using System;
	using System.Drawing;

	/// <summary>
	/// FormSettings holds settings for NUnitForm
	/// </summary>
	public class FormSettings : SettingsGroup
	{
		private static readonly string NAME = "Form";

		private static readonly string MAXIMIZED = "maximized";
		private static readonly string WIDTH = "width";
		private static readonly string HEIGHT = "height";
		private static readonly string XLOCATION = "x-location";
		private static readonly string YLOCATION = "y-location";
		private static readonly string TREE_SPLITTER_POSITION = "tree-splitter-position";
		private static readonly string TAB_SPLITTER_POSITION = "tab-splitter-position";

		public static readonly int DEFAULT_WIDTH = 756;
		public static readonly int MIN_WIDTH = 160;

		public static readonly int DEFAULT_HEIGHT = 512;
		public static readonly int MIN_HEIGHT = 32; 
		
		public static readonly int DEFAULT_XLOCATION = 10;
		
		public static readonly int DEFAULT_YLOCATION = 10;

		public static readonly int TREE_DEFAULT_POSITION = 300;
		public static readonly int TREE_MIN_POSITION = 240;
		
		public static readonly int TAB_DEFAULT_POSITION = 119;
		public static readonly int TAB_MIN_POSITION = 100;

		public FormSettings( ) : base( NAME, UserSettings.GetStorageImpl( NAME ) ) { }

		public FormSettings( SettingsStorage storage ) : base( NAME, storage ) { }

		public FormSettings( SettingsGroup parent ) : base( NAME, parent ) { }

		private Point location = Point.Empty;
		private Size size = Size.Empty;
		private int treeSplitterPosition = -1;
		private int tabSplitterPosition = -1;

		public bool IsMaximized
		{
			get
			{
				return LoadIntSetting( MAXIMIZED, 0 ) == 1 ? true : false;
			}

			set
			{
				SaveIntSetting( MAXIMIZED, value ? 1 : 0 );
			}
		}

		public Point Location
		{
			get 
			{
				if ( location == Point.Empty )
				{
					int x = LoadIntSetting( XLOCATION, DEFAULT_XLOCATION );
					int y = LoadIntSetting( YLOCATION, DEFAULT_YLOCATION );

					location = new Point(x, y);

					if ( !IsValidLocation( location ) )
						location = new Point( DEFAULT_XLOCATION, DEFAULT_YLOCATION );
				}
				
				return location; 
			}
			set 
			{ 
				location = value;
				SaveSetting( XLOCATION, location.X );
				SaveSetting( YLOCATION, location.Y );
			}
		}

		private bool IsValidLocation( Point location )
		{
			Rectangle myArea = new Rectangle( location, this.Size );
			bool intersect = false;
			foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
			{
			  intersect |= myArea.IntersectsWith(screen.WorkingArea);
			}
			return intersect;
		}

		public Size Size
		{
			get 
			{ 
				if ( size == Size.Empty )
				{
					int width = LoadIntSetting( WIDTH, DEFAULT_WIDTH );
					if ( width < MIN_WIDTH ) width = MIN_WIDTH;
					int height = LoadIntSetting( HEIGHT, DEFAULT_HEIGHT );
					if ( height < MIN_HEIGHT ) height = MIN_HEIGHT;

					size = new Size(width, height);
				}

				return size;
			}
			set
			{ 
				size = value;
				SaveIntSetting( WIDTH, size.Width );
				SaveIntSetting( HEIGHT, size.Height );
			}
		}

		public int TreeSplitterPosition
		{
			get 
			{
				if ( treeSplitterPosition == -1 )
				{
					treeSplitterPosition = 
						LoadIntSetting( TREE_SPLITTER_POSITION, TREE_DEFAULT_POSITION );

					if ( treeSplitterPosition < TREE_MIN_POSITION  || treeSplitterPosition > this.Size.Width )
						treeSplitterPosition = TREE_MIN_POSITION;
				}
				
				return treeSplitterPosition; 
			}
			set 
			{ 
				treeSplitterPosition = value;
				SaveSetting( TREE_SPLITTER_POSITION, treeSplitterPosition );
			}
		}

		public int TabSplitterPosition
		{
			get 
			{
				if ( tabSplitterPosition == -1 )
				{
					tabSplitterPosition = 
						LoadIntSetting( TAB_SPLITTER_POSITION, TAB_DEFAULT_POSITION );
					
					if ( tabSplitterPosition < TAB_MIN_POSITION || tabSplitterPosition > this.Size.Height )
						tabSplitterPosition = TAB_MIN_POSITION;
				}
				
				return tabSplitterPosition; 
			}
			set 
			{ 
				tabSplitterPosition = value;
				SaveSetting( TAB_SPLITTER_POSITION, tabSplitterPosition );
			}
		}
	}
}
