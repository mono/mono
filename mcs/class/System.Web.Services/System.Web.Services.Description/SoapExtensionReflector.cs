// 
// System.Web.Services.Description.SoapExtensionReflector.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public abstract class SoapExtensionReflector {

		#region Fields

		ProtocolReflector reflectionContext;
		
		#endregion // Fields

		#region Constructors
	
		public SoapExtensionReflector ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public ProtocolReflector ReflectionContext {
			get { return reflectionContext; }
			set { reflectionContext = value; }
		}

		#endregion // Properties

		#region Methods

		public abstract void ReflectMethod ();

		#endregion
	}
}
