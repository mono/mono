//
// System.CodeDom CodeAttributeDeclarationCollection Class implementation
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
	public class CodeAttributeDeclarationCollection 
		: CollectionBase
	{
		//
		// Constructors
		//
		public CodeAttributeDeclarationCollection()
		{
		}

		public CodeAttributeDeclarationCollection( CodeAttributeDeclaration[] value )
		{
			AddRange( value );
		}

		public CodeAttributeDeclarationCollection( CodeAttributeDeclarationCollection value )
		{
			AddRange( value );
		}

		//
		// Properties
		//
		public CodeAttributeDeclaration this[int index]
		{
			get {
				return (CodeAttributeDeclaration)List[index];
			}
			set {
				List[index] = value;
			}
		}

		//
		// Methods
		//
		public void Add (CodeAttributeDeclaration value)
		{
			List.Add (value);
		}

		public void AddRange (CodeAttributeDeclaration [] value)
		{
			foreach (CodeAttributeDeclaration elem in value) 
				Add( elem );
		}

		public void AddRange (CodeAttributeDeclarationCollection value)
		{
			foreach (CodeAttributeDeclaration elem in value) 
				Add( elem );
		}

		public bool Contains( CodeAttributeDeclaration value )
		{
			return List.Contains( value );
		}

		public void CopyTo( CodeAttributeDeclaration[] array, int index )
		{
			List.CopyTo( array, index );
		}

		public int IndexOf( CodeAttributeDeclaration value )
		{
			return List.IndexOf( value );
		}

		public void Insert( int index, CodeAttributeDeclaration value )
		{
			List.Insert( index, value );
		}

		public void Remove( CodeAttributeDeclaration value )
		{
			int index = IndexOf( value );
			if ( index < 0 )
				throw( new ArgumentException( "The specified object is not found in the collection" ) );
			RemoveAt( index );
		}
	}
}
