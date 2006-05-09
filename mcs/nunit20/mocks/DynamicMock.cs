using System;

namespace NUnit.Mocks
{
	/// <summary>
	/// Summary description for DynamicMock.
	/// </summary>
	public class DynamicMock : Mock
	{
		private Type type;

		private object mockInstance;

		public object MockInstance
		{
			get 
			{ 
				if ( mockInstance == null )
				{
					MockInterfaceHandler handler = new MockInterfaceHandler( type, this );
					mockInstance = handler.GetTransparentProxy();
				}

				return mockInstance; 
			}
		}

		#region Constructors

		public DynamicMock( Type type ) : this( "Mock" + type.Name, type ) { }

		public DynamicMock( string name, Type type ) : base( name )
		{
//			if ( !type.IsInterface )
//				throw new VerifyException( "DynamicMock constructor requires an interface type" );
			this.type = type;
		}

		#endregion
	}
}
