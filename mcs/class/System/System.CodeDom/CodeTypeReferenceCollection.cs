//
// System.CodeDom CodeTypeReferenceCollection Class implementation
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
	public class CodeTypeReferenceCollection
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeTypeReferenceCollection()
		{
		}

		public CodeTypeReferenceCollection( CodeTypeReference[] value )
		{
			AddRange( value );
		}

		public CodeTypeReferenceCollection( CodeTypeReferenceCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
		public CodeTypeReference this[int index]
		{
			get {
				return (CodeTypeReference)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public int Add (CodeTypeReference value)
		{
			return List.Add( value );
		}

		public void Add (string value)
		{
			Add (new CodeTypeReference (value));
		}

		public void Add (Type value)
		{
			Add (new CodeTypeReference (value));
		}

		public void AddRange (CodeTypeReference [] value )
		{
			foreach ( CodeTypeReference elem in value )
				Add( elem );
		}
		
		public void AddRange (CodeTypeReferenceCollection value)
		{
			foreach ( CodeTypeReference elem in value )
				Add( elem );
		}

		public bool Contains( CodeTypeReference value )
		{
			return List.Contains( value );
		}
		
		public void CopyTo( CodeTypeReference[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeTypeReference value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeTypeReference value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeTypeReference value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
