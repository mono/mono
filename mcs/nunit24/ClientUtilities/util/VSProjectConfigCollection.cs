// ****************************************************************
// Copyright 2002-2003, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Collections;

namespace NUnit.Util
{
	/// <summary>
	/// A simple collection to hold VSProjectConfigs. Originally,
	/// we used the (NUnit) ProjectConfigCollection, but the
	/// classes have since diverged.
	/// </summary>
	public class VSProjectConfigCollection : CollectionBase
	{
		public VSProjectConfig this[int index]
		{
			get { return List[index] as VSProjectConfig; }
		}

		public VSProjectConfig this[string name]
		{
			get
			{
				foreach ( VSProjectConfig config in InnerList )
					if ( config.Name == name ) return config;

				return null;
			}
		}

		public void Add( VSProjectConfig config )
		{
			List.Add( config );
		}

		public bool Contains( string name )
		{
			foreach( VSProjectConfig config in InnerList )
				if ( config.Name == name ) return true;

			return false;
		}
	}
}
