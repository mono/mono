//
// System.CodeDom CodeParameterDeclarationExpressionCollection Class implementation
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
	public class CodeParameterDeclarationExpressionCollection
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeParameterDeclarationExpressionCollection()
		{
		}

		public CodeParameterDeclarationExpressionCollection( CodeParameterDeclarationExpression[] value )
		{
			AddRange( value );
		}
		
		public CodeParameterDeclarationExpressionCollection( CodeParameterDeclarationExpressionCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
		public CodeParameterDeclarationExpression this[int index]
		{
			get {
				return (CodeParameterDeclarationExpression)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public int Add (CodeParameterDeclarationExpression value)
		{
			return List.Add( value );
		}

		public void AddRange (CodeParameterDeclarationExpression [] value )
		{
			foreach ( CodeParameterDeclarationExpression elem in value )
				Add( elem );
		}
		
		public void AddRange (CodeParameterDeclarationExpressionCollection value)
		{
			foreach ( CodeParameterDeclarationExpression elem in value )
				Add( elem );
		}

		public bool Contains( CodeParameterDeclarationExpression value )
		{
			return List.Contains( value );
		}
		
		public void CopyTo( CodeParameterDeclarationExpression[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeParameterDeclarationExpression value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeParameterDeclarationExpression value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeParameterDeclarationExpression value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
