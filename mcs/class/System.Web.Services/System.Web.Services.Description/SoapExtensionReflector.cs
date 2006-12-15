// 
// System.Web.Services.Description.SoapExtensionReflector.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Web.Services.Protocols;
using System.Xml;

namespace System.Web.Services.Description {
	public abstract class SoapExtensionReflector {

		#region Fields

		ProtocolReflector reflectionContext;
		
		#endregion // Fields

		#region Constructors
	
		protected SoapExtensionReflector ()
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

#if NET_2_0
		public
#else
		internal
#endif
		virtual void ReflectDescription ()
		{
		}

		public abstract void ReflectMethod ();

		#endregion
	}

	abstract class SoapBindingExtensionReflector : SoapExtensionReflector
	{
		public abstract SoapBinding CreateSoapBinding ();
		public abstract SoapAddressBinding CreateSoapAddressBinding ();
		public abstract SoapOperationBinding CreateSoapOperationBinding ();
		public abstract SoapHeaderBinding CreateSoapHeaderBinding ();
		public abstract SoapBodyBinding CreateSoapBodyBinding ();
		public abstract string EncodingNS { get; }

#if NET_2_0
		public
#else
		internal
#endif
		override void ReflectDescription ()
		{
			SoapBinding sb = CreateSoapBinding ();
			sb.Transport = SoapBinding.HttpTransport;
			sb.Style = ((SoapTypeStubInfo) ReflectionContext.TypeInfo).SoapBindingStyle;
			ReflectionContext.Binding.Extensions.Add (sb);

			SoapAddressBinding abind = CreateSoapAddressBinding ();
			abind.Location = ReflectionContext.ServiceUrl;
			ReflectionContext.Port.Extensions.Add (abind);
		}

		public override void ReflectMethod ()
		{
			SoapMethodStubInfo method = (SoapMethodStubInfo) ReflectionContext.MethodStubInfo;

			SoapOperationBinding sob = CreateSoapOperationBinding ();
			
			sob.SoapAction = method.Action;
			sob.Style = method.SoapBindingStyle;
			ReflectionContext.OperationBinding.Extensions.Add (sob);
			
			AddOperationMsgBindings (method, ReflectionContext.OperationBinding.Input);
			AddOperationMsgBindings (method, ReflectionContext.OperationBinding.Output);

			foreach (SoapHeaderMapping hf in method.Headers) {
				if (hf.Custom) continue;
				
				SoapHeaderBinding hb = CreateSoapHeaderBinding ();
				hb.Message = new XmlQualifiedName (ReflectionContext.Operation.Name + hf.HeaderType.Name, ReflectionContext.ServiceDescription.TargetNamespace);
				hb.Part = hf.HeaderType.Name;
				hb.Use = method.Use;
				
				if (method.Use != SoapBindingUse.Literal)
					hb.Encoding = EncodingNS;

				if ((hf.Direction & SoapHeaderDirection.Out) != 0)
					ReflectionContext.OperationBinding.Output.Extensions.Add (hb);
				if ((hf.Direction & SoapHeaderDirection.In) != 0)
					ReflectionContext.OperationBinding.Input.Extensions.Add (hb);
			}
		}

		void AddOperationMsgBindings (SoapMethodStubInfo method, MessageBinding msg)
		{
			SoapBodyBinding sbbo = CreateSoapBodyBinding ();
			msg.Extensions.Add (sbbo);
			sbbo.Use = method.Use;
			if (method.Use == SoapBindingUse.Encoded) {
				sbbo.Namespace = ReflectionContext.ServiceDescription.TargetNamespace;
				sbbo.Encoding = EncodingNS;
			}
		}
	}

	class Soap11BindingExtensionReflector : SoapBindingExtensionReflector
	{
		public override SoapBinding CreateSoapBinding ()
		{
			return new SoapBinding ();
		}
		public override SoapAddressBinding CreateSoapAddressBinding ()
		{
			return new SoapAddressBinding ();
		}
		public override SoapOperationBinding CreateSoapOperationBinding ()
		{
			return new SoapOperationBinding ();
		}
		public override SoapHeaderBinding CreateSoapHeaderBinding ()
		{
			return new SoapHeaderBinding ();
		}
		public override SoapBodyBinding CreateSoapBodyBinding ()
		{
			return new SoapBodyBinding ();
		}

		public override string EncodingNS {
			get { return EncodingNamespace; }
		}

		public const string EncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
	}

#if NET_2_0
	class Soap12BindingExtensionReflector : SoapBindingExtensionReflector
	{
		public override SoapBinding CreateSoapBinding ()
		{
			return new Soap12Binding ();
		}
		public override SoapAddressBinding CreateSoapAddressBinding ()
		{
			return new Soap12AddressBinding ();
		}
		public override SoapOperationBinding CreateSoapOperationBinding ()
		{
			return new Soap12OperationBinding ();
		}
		public override SoapHeaderBinding CreateSoapHeaderBinding ()
		{
			return new Soap12HeaderBinding ();
		}
		public override SoapBodyBinding CreateSoapBodyBinding ()
		{
			return new Soap12BodyBinding ();
		}

		public override string EncodingNS {
			get { return EncodingNamespace; }
		}

		public const string EncodingNamespace = "http://www.w3.org/2003/05/soap-encoding";
	}
#endif
}
