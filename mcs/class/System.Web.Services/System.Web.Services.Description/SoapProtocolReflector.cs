// 
// System.Web.Services.Description.SoapProtocolReflector.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[MonoTODO ("This class is based on conjecture and guesswork.")]
	internal sealed class SoapProtocolReflector : ProtocolReflector {

		#region Fields

		SoapBinding soapBinding;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SoapProtocolReflector ()
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public SoapBinding SoapBinding {
			get { return soapBinding; }
		}

		public override string ProtocolName {
			get { return "Soap"; }
		}

		#endregion // Properties

		#region Methods

                [MonoTODO]
                protected override void BeginClass ()
                {
                        throw new NotImplementedException ();
                }

                [MonoTODO]
                protected override void EndClass ()
                {
                        throw new NotImplementedException ();
                }

		[MonoTODO]
		protected override bool ReflectMethod ()
		{
			throw new NotImplementedException ();
		}

                [MonoTODO]
                protected override string ReflectMethodBinding ()
                {
                        throw new NotImplementedException ();
                }

		#endregion
	}
}
