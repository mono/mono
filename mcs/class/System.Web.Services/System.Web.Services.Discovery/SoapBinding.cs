// 
// System.Web.Services.Discovery.SoapBinding.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.Xml;

namespace System.Web.Services.Discovery {
	public sealed class SoapBinding  {

		#region Fields
		
		public const string Namespace = "http://schemas/xmlsoap.org/disco/schema/soap/";
		
		#endregion // Fields
		
		#region Constructors

		[MonoTODO]
		public SoapBinding () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties
		
		[MonoTODO]
		public string Address {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public XmlQualifiedName Binding {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

	}
}
