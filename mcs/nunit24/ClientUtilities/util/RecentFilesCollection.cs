// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Collections;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for RecentFilesCollection.
	/// </summary>
	public class RecentFilesCollection : ReadOnlyCollectionBase
	{
		public void Add( RecentFileEntry entry )
		{
			InnerList.Add( entry );
		}

		public void Insert( int index, RecentFileEntry entry )
		{
			InnerList.Insert( index, entry );
		}

		public void Remove( string fileName )
		{
			int index = IndexOf( fileName );
			if ( index != -1 )
				RemoveAt( index );
		}

		public void RemoveAt( int index )
		{
			InnerList.RemoveAt( index );
		}

		public int IndexOf( string fileName )
		{
			for( int index = 0; index < InnerList.Count; index++ )
				if ( this[index].Path == fileName )
					return index;
			return -1;
		}

		public RecentFileEntry this[int index]
		{
			get { return (RecentFileEntry)InnerList[index]; }
		}

		public void Clear()
		{
			InnerList.Clear();
		}
	}
}
