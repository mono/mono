// 
// System.Web.Services.Description.HttpPostProtocolReflector.cs
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

	internal class HttpPostProtocolReflector : HttpSimpleProtocolReflector 
	{
		#region Constructors

		public HttpPostProtocolReflector ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public override string ProtocolName {
			get { return "HttpPost"; }
		}

		#endregion // Properties

		#region Methods

		protected override void BeginClass ()
		{
			base.BeginClass ();
			
			HttpBinding hb = new HttpBinding ();
			hb.Verb = "POST";
			Binding.Extensions.Add (hb);
		}

		protected override bool ReflectMethod ()
		{
			if (!base.ReflectMethod ()) return false;
			
			MimeContentBinding mcb = new MimeContentBinding ();
			mcb.Type = "application/x-www-form-urlencoded";
			OperationBinding.Input.Extensions.Add (mcb);
			return true;
		}

		#endregion
	}
}
