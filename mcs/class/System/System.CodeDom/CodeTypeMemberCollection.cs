//
// System.CodeDom CodeTypeMemberCollection Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;
using System.Collections;

namespace System.CodeDom 
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeTypeMemberCollection
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeTypeMemberCollection()
		{
		}
		
		public CodeTypeMemberCollection( CodeTypeMember[] value )
		{
			AddRange( value );
		}

		public CodeTypeMemberCollection( CodeTypeMemberCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
		public CodeTypeMember this[int index]
		{
			get {
				return (CodeTypeMember)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public void Add (CodeTypeMember value)
		{
			List.Add( value );
		}

		public void AddRange (CodeTypeMember [] value )
		{
			foreach ( CodeTypeMember elem in value )
				Add( elem );
		}
		
		public void AddRange (CodeTypeMemberCollection value)
		{
			foreach ( CodeTypeMember elem in value )
				Add( elem );
		}

		public bool Contains( CodeTypeMember value )
		{
			return List.Contains( value );
		}
		
		public void CopyTo( CodeTypeMember[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeTypeMember value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeTypeMember value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeTypeMember value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
