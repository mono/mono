// 
// System.Web.Services.Protocols.SoapExtension.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;

namespace System.Web.Services.Protocols {
	public abstract class SoapExtension {

		#region Fields

		Stream stream;

		#endregion

		#region Constructors

		[MonoTODO]
		protected SoapExtension ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public virtual Stream ChainStream (Stream stream)
		{
			throw new NotImplementedException ();
		}

		public abstract object GetInitializer (Type serviceType);
		public abstract object GetInitializer (LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute);
		public abstract void Initialize (object initializer);
		public abstract void ProcessMessage (SoapMessage message);

		#endregion // Methods
	}
}
