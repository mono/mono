//
// System.CodeDom CodeAttributeArgumentCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
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
	public class CodeAttributeArgumentCollection
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeAttributeArgumentCollection()
		{
		}

		public CodeAttributeArgumentCollection( CodeAttributeArgument[] value )
		{
			AddRange( value );
		}

		public CodeAttributeArgumentCollection( CodeAttributeArgumentCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
		public CodeAttributeArgument this[int index]
		{
			get {
				return (CodeAttributeArgument)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public int Add (CodeAttributeArgument value)
		{
			return List.Add( value );
		}

		public void AddRange (CodeAttributeArgument [] value )
		{
			foreach ( CodeAttributeArgument elem in value )
				Add( elem );
		}
		
		public void AddRange (CodeAttributeArgumentCollection value)
		{
			foreach ( CodeAttributeArgument elem in value )
				Add( elem );
		}

		public bool Contains( CodeAttributeArgument value )
		{
			return List.Contains( value );
		}
		
		public void CopyTo( CodeAttributeArgument[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeAttributeArgument value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeAttributeArgument value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeAttributeArgument value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
