// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.IO;
using System.Collections;

namespace NUnit.Util
{
	/// <summary>
	/// Represents a list of assemblies. It stores paths 
	/// that are added and fires an event whenevever it
	/// changes. All paths must be added as absolute paths.
	/// </summary>
	public class AssemblyList : CollectionBase
	{
		#region Properties and Events
		public string this[int index]
		{
			get { return (string)List[index]; }
			set 
			{ 
				if ( !Path.IsPathRooted( value ) )
					throw new ArgumentException( "Assembly path must be absolute" );
				List[index] = value; 
			}
		}

		public event EventHandler Changed;
		#endregion

		#region Methods
		public string[] ToArray()
		{
			return (string[])InnerList.ToArray( typeof( string ) );
		}

		public void Add( string assemblyPath )
		{
			if ( !Path.IsPathRooted( assemblyPath ) )
				throw new ArgumentException( "Assembly path must be absolute" );
			List.Add( assemblyPath );
		}

		public void Remove( string assemblyPath )
		{
			for( int index = 0; index < this.Count; index++ )
			{
				if ( this[index] == assemblyPath )
					RemoveAt( index );
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			FireChangedEvent();
		}

		protected override void OnInsertComplete(int index, object value)
		{
			FireChangedEvent();
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			FireChangedEvent();
		}

		private void FireChangedEvent()
		{
			if ( Changed != null )
				Changed( this, EventArgs.Empty );
		}
		#endregion
	}
}
