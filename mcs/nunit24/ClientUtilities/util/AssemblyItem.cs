// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.Threading;
using System.Xml.Serialization;

namespace NUnit.Util
{
	/// <summary>
	/// Holds an assembly path and other information needed to
	/// load an assembly. Currently there is no other info.
	/// Used in serialization of NUnit projects.
	/// </summary>
	[Serializable]
	public struct AssemblyItem
	{
		[XmlAttribute]
		public string path;

		public ApartmentState apartment;

		public AssemblyItem( string path ) : this( path, ApartmentState.Unknown ) { }

		public AssemblyItem( string path, ApartmentState apartment )
		{
			if ( !System.IO.Path.IsPathRooted( path ) )
				throw new ArgumentException( "Assembly path must be absolute", "path" );
			this.path = path;
			this.apartment = apartment;
		}
	}
}
