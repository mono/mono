// 
// System.Web.Services.Description.HttpGetProtocolReflector.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Ximian, Inc.
//

using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.Xml;

namespace System.Web.Services.Description {

	internal class HttpGetProtocolReflector : HttpSimpleProtocolReflector 
	{
		#region Constructors

		public HttpGetProtocolReflector ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public override string ProtocolName {
			get { return "HttpGet"; }
		}

		#endregion // Properties

		#region Methods

		protected override void BeginClass ()
		{
			base.BeginClass ();
			
			HttpBinding hb = new HttpBinding ();
			hb.Verb = "GET";
			Binding.Extensions.Add (hb);
		}

		protected override bool ReflectMethod ()
		{
			if (!base.ReflectMethod ()) return false;
			
			OperationBinding.Input.Extensions.Add (new HttpUrlEncodedBinding());
			return true;
		}

		#endregion
	}
}
