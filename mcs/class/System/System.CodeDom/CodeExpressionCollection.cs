//
// System.CodeDom CodeExpressionCollection Class implementation
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
	public class CodeExpressionCollection
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeExpressionCollection()
		{
		}
		
		public CodeExpressionCollection( CodeExpression[] value )
		{
			AddRange( value );
		}

		public CodeExpressionCollection( CodeExpressionCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
		public CodeExpression this[int index]
		{
			get {
				return (CodeExpression)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public void Add (CodeExpression value)
		{
			List.Add( value );
		}

		public void AddRange (CodeExpression [] value )
		{
			foreach ( CodeExpression elem in value )
				Add( elem );
		}
		
		public void AddRange (CodeExpressionCollection value)
		{
			foreach ( CodeExpression elem in value )
				Add( elem );
		}

		public bool Contains( CodeExpression value )
		{
			return List.Contains( value );
		}
		
		public void CopyTo( CodeExpression[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeExpression value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeExpression value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeExpression value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
