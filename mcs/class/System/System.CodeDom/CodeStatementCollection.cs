//
// System.CodeDom CodeStatementCollection Class implementation
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
	public class CodeStatementCollection
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeStatementCollection()
		{
		}

		public CodeStatementCollection( CodeStatement[] value )
		{
			AddRange( value );
		}

		public CodeStatementCollection( CodeStatementCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
		public CodeStatement this[int index]
		{
			get {
				return (CodeStatement)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public int Add (CodeStatement value)
		{
			return List.Add( value );
		}

		public int Add (CodeExpression value)
		{
			return Add( new CodeExpressionStatement( value ) );
		}

		public void AddRange (CodeStatement [] statements )
		{
			foreach ( CodeStatement elem in statements )
				Add( elem );
		}
		
		public void AddRange( CodeStatementCollection value )
		{
			foreach ( CodeStatement elem in value )
				Add( elem );
		}

		public bool Contains( CodeStatement value )
		{
			return List.Contains( value );
		}
		
		public void CopyTo( CodeStatement[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeStatement value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeStatement value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeStatement value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
