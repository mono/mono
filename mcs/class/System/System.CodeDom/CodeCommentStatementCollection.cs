//
// System.CodeDom CodeCommentStatementCollection Class implementation
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
	public class CodeCommentStatementCollection
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeCommentStatementCollection()
		{
		}
		
		public CodeCommentStatementCollection( CodeCommentStatement[] value )
		{
			AddRange( value );
		}

		public CodeCommentStatementCollection( CodeCommentStatementCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
		public CodeCommentStatement this[int index]
		{
			get {
				return (CodeCommentStatement)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public int Add (CodeCommentStatement value)
		{
			return List.Add( value );
		}

		public void AddRange (CodeCommentStatement [] value )
		{
			foreach ( CodeCommentStatement elem in value )
				Add( elem );
		}
		
		public void AddRange (CodeCommentStatementCollection value)
		{
			foreach ( CodeCommentStatement elem in value )
				Add( elem );
		}

		public bool Contains( CodeCommentStatement value )
		{
			return List.Contains( value );
		}
		
		public void CopyTo( CodeCommentStatement[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeCommentStatement value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeCommentStatement value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeCommentStatement value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
