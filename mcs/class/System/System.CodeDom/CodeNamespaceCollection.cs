//
// System.CodeDom CodeNamespaceCollection Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;
using System.Collections;

namespace System.CodeDom 
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeNamespaceCollection
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeNamespaceCollection()
		{
		}

		public CodeNamespaceCollection( CodeNamespace[] value )
		{
			AddRange( value );
		}

		public CodeNamespaceCollection( CodeNamespaceCollection value )
		{
			AddRange( value );
		}
		
		//
		// Properties
		//
		public CodeNamespace this[int index]
		{
			get {
				return (CodeNamespace)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public int Add (CodeNamespace value)
		{
			return List.Add( value ); 
		}

		public void AddRange (CodeNamespace [] value )
		{
			foreach ( CodeNamespace elem in value )
				Add( elem );
		}
		
		public void AddRange (CodeNamespaceCollection value)
		{
			foreach ( CodeNamespace elem in value )
				Add( elem );
		}

		public bool Contains( CodeNamespace value )
		{
			return List.Contains( value );
		}
		
		public void CopyTo( CodeNamespace[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeNamespace value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeNamespace value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeNamespace value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
