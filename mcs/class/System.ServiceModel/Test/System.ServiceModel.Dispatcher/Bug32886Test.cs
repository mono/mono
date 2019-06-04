//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using NUnit.Framework;

using WebServiceMoonlightTest.ServiceReference1;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class Bug32886
	{
		[Test]
		public void Bug32886_Test () // test in one of the comment
		{
			// Init service
			int port = NetworkHelpers.FindFreePort ();
			ServiceHost serviceHost = new ServiceHost (typeof (TempConvertSoapImpl), new Uri ("http://localhost:" + port + "/TempConvertSoap"));
			serviceHost.AddServiceEndpoint (typeof (TempConvertSoap), new BasicHttpBinding (), string.Empty);

			// Enable metadata exchange (WSDL publishing)
			var mexBehavior = new ServiceMetadataBehavior ();
			mexBehavior.HttpGetEnabled = true;
			serviceHost.Description.Behaviors.Add (mexBehavior);
			serviceHost.AddServiceEndpoint (typeof (IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding (), "mex");

			serviceHost.Open ();

			try {
				// client
				var binding = new BasicHttpBinding ();
				var remoteAddress = new EndpointAddress ("http://localhost:" + port + "/TempConvertSoap");
				var client = new TempConvertSoapClient (binding, remoteAddress);

				var wait = new ManualResetEvent (false);

				Exception error = null;
				string result = null;

				client.CelsiusToFahrenheitCompleted += delegate (object o, CelsiusToFahrenheitCompletedEventArgs e) {
					try {
						error = e.Error;
						result = e.Error == null ? e.Result : null;
					} finally {
						wait.Set ();
					}
				};

				client.CelsiusToFahrenheitAsync ("24.5");

				Assert.IsTrue (wait.WaitOne (TimeSpan.FromSeconds (20)), "timeout");
				Assert.IsNull (error, "#1, inner exception: {0}", error);
				Assert.AreEqual ("76.1", result, "#2");
			} finally {
				serviceHost.Close ();
			}
		}

		class TempConvertSoapImpl : TempConvertSoap
		{
			public FahrenheitToCelsiusResponse FarenheitToCelsius (FahrenheitToCelsiusRequest request)
			{
				var farenheit = double.Parse (request.Body.Fahrenheit, CultureInfo.InvariantCulture);
				var celsius = ((farenheit - 32) / 9) * 5;
				return new FahrenheitToCelsiusResponse (new FahrenheitToCelsiusResponseBody (celsius.ToString (CultureInfo.InvariantCulture)));
			}

			public CelsiusToFahrenheitResponse CelsiusToFarenheit (CelsiusToFahrenheitRequest request)
			{
				var celsius = double.Parse (request.Body.Celsius, CultureInfo.InvariantCulture);
				var farenheit = ((celsius * 9) / 5) + 32;
				return new CelsiusToFahrenheitResponse (new CelsiusToFahrenheitResponseBody (farenheit.ToString (CultureInfo.InvariantCulture)));
			}

			Func<FahrenheitToCelsiusRequest,FahrenheitToCelsiusResponse> farenheitToCelsius;
			Func<CelsiusToFahrenheitRequest,CelsiusToFahrenheitResponse> celsiusToFarenheit;

			public IAsyncResult BeginFahrenheitToCelsius (FahrenheitToCelsiusRequest request, AsyncCallback callback, object asyncState)
			{
				if (farenheitToCelsius == null)
					farenheitToCelsius = new Func<FahrenheitToCelsiusRequest,FahrenheitToCelsiusResponse> (FarenheitToCelsius);
				return farenheitToCelsius.BeginInvoke (request, callback, asyncState);
			}

			public FahrenheitToCelsiusResponse EndFahrenheitToCelsius (IAsyncResult result)
			{
				return farenheitToCelsius.EndInvoke (result);
			}

			public IAsyncResult BeginCelsiusToFahrenheit (CelsiusToFahrenheitRequest request, AsyncCallback callback, object asyncState)
			{
				if (celsiusToFarenheit == null)
					celsiusToFarenheit = new Func<CelsiusToFahrenheitRequest,CelsiusToFahrenheitResponse> (CelsiusToFarenheit);
				return celsiusToFarenheit.BeginInvoke (request, callback, asyncState);
			}

			public CelsiusToFahrenheitResponse EndCelsiusToFahrenheit (IAsyncResult result)
			{
				return celsiusToFarenheit.EndInvoke (result);
			}
		}
	}
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This code was auto-generated by SlSvcUtil, version 5.0.61118.0
// 


[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute (Namespace = "http://www.w3schools.com/webservices/", ConfigurationName = "TempConvertSoap")]
public interface TempConvertSoap
{

	[System.ServiceModel.OperationContractAttribute (AsyncPattern = true, Action = "http://www.w3schools.com/webservices/FahrenheitToCelsius", ReplyAction = "*")]
	System.IAsyncResult BeginFahrenheitToCelsius (FahrenheitToCelsiusRequest request, System.AsyncCallback callback, object asyncState);

	FahrenheitToCelsiusResponse EndFahrenheitToCelsius (System.IAsyncResult result);

	[System.ServiceModel.OperationContractAttribute (AsyncPattern = true, Action = "http://www.w3schools.com/webservices/CelsiusToFahrenheit", ReplyAction = "*")]
	System.IAsyncResult BeginCelsiusToFahrenheit (CelsiusToFahrenheitRequest request, System.AsyncCallback callback, object asyncState);

	CelsiusToFahrenheitResponse EndCelsiusToFahrenheit (System.IAsyncResult result);
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute (IsWrapped = false)]
public partial class FahrenheitToCelsiusRequest
{

	[System.ServiceModel.MessageBodyMemberAttribute (Name = "FahrenheitToCelsius", Namespace = "http://www.w3schools.com/webservices/", Order = 0)]
	public FahrenheitToCelsiusRequestBody Body;

	public FahrenheitToCelsiusRequest ()
	{
	}

	public FahrenheitToCelsiusRequest (FahrenheitToCelsiusRequestBody Body)
	{
		this.Body = Body;
	}
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
[System.Runtime.Serialization.DataContractAttribute (Namespace = "http://www.w3schools.com/webservices/")]
public partial class FahrenheitToCelsiusRequestBody
{

	[System.Runtime.Serialization.DataMemberAttribute (EmitDefaultValue = false, Order = 0)]
	public string Fahrenheit;

	public FahrenheitToCelsiusRequestBody ()
	{
	}

	public FahrenheitToCelsiusRequestBody (string Fahrenheit)
	{
		this.Fahrenheit = Fahrenheit;
	}
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute (IsWrapped = false)]
public partial class FahrenheitToCelsiusResponse
{

	[System.ServiceModel.MessageBodyMemberAttribute (Name = "FahrenheitToCelsiusResponse", Namespace = "http://www.w3schools.com/webservices/", Order = 0)]
	public FahrenheitToCelsiusResponseBody Body;

	public FahrenheitToCelsiusResponse ()
	{
	}

	public FahrenheitToCelsiusResponse (FahrenheitToCelsiusResponseBody Body)
	{
		this.Body = Body;
	}
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
[System.Runtime.Serialization.DataContractAttribute (Namespace = "http://www.w3schools.com/webservices/")]
public partial class FahrenheitToCelsiusResponseBody
{

	[System.Runtime.Serialization.DataMemberAttribute (EmitDefaultValue = false, Order = 0)]
	public string FahrenheitToCelsiusResult;

	public FahrenheitToCelsiusResponseBody ()
	{
	}

	public FahrenheitToCelsiusResponseBody (string FahrenheitToCelsiusResult)
	{
		this.FahrenheitToCelsiusResult = FahrenheitToCelsiusResult;
	}
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute (IsWrapped = false)]
public partial class CelsiusToFahrenheitRequest
{

	[System.ServiceModel.MessageBodyMemberAttribute (Name = "CelsiusToFahrenheit", Namespace = "http://www.w3schools.com/webservices/", Order = 0)]
	public CelsiusToFahrenheitRequestBody Body;

	public CelsiusToFahrenheitRequest ()
	{
	}

	public CelsiusToFahrenheitRequest (CelsiusToFahrenheitRequestBody Body)
	{
		this.Body = Body;
	}
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
[System.Runtime.Serialization.DataContractAttribute (Namespace = "http://www.w3schools.com/webservices/")]
public partial class CelsiusToFahrenheitRequestBody
{

	[System.Runtime.Serialization.DataMemberAttribute (EmitDefaultValue = false, Order = 0)]
	public string Celsius;

	public CelsiusToFahrenheitRequestBody ()
	{
	}

	public CelsiusToFahrenheitRequestBody (string Celsius)
	{
		this.Celsius = Celsius;
	}
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute (IsWrapped = false)]
public partial class CelsiusToFahrenheitResponse
{

	[System.ServiceModel.MessageBodyMemberAttribute (Name = "CelsiusToFahrenheitResponse", Namespace = "http://www.w3schools.com/webservices/", Order = 0)]
	public CelsiusToFahrenheitResponseBody Body;

	public CelsiusToFahrenheitResponse ()
	{
	}

	public CelsiusToFahrenheitResponse (CelsiusToFahrenheitResponseBody Body)
	{
		this.Body = Body;
	}
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
[System.Runtime.Serialization.DataContractAttribute (Namespace = "http://www.w3schools.com/webservices/")]
public partial class CelsiusToFahrenheitResponseBody
{

	[System.Runtime.Serialization.DataMemberAttribute (EmitDefaultValue = false, Order = 0)]
	public string CelsiusToFahrenheitResult;

	public CelsiusToFahrenheitResponseBody ()
	{
	}

	public CelsiusToFahrenheitResponseBody (string CelsiusToFahrenheitResult)
	{
		this.CelsiusToFahrenheitResult = CelsiusToFahrenheitResult;
	}
}

[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
public interface TempConvertSoapChannel : TempConvertSoap, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
public partial class FahrenheitToCelsiusCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
{

	private object[] results;

	public FahrenheitToCelsiusCompletedEventArgs (object[] results, System.Exception exception, bool cancelled, object userState) :
	base (exception, cancelled, userState)
	{
		this.results = results;
	}

	public string Result {
		get {
			base.RaiseExceptionIfNecessary ();
			return ((string)(this.results [0]));
		}
	}
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
public partial class CelsiusToFahrenheitCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
{

	private object[] results;

	public CelsiusToFahrenheitCompletedEventArgs (object[] results, System.Exception exception, bool cancelled, object userState) :
	base (exception, cancelled, userState)
	{
		this.results = results;
	}

	public string Result {
		get {
			base.RaiseExceptionIfNecessary ();
			return ((string)(this.results [0]));
		}
	}
}

[System.Diagnostics.DebuggerStepThroughAttribute ()]
[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.ServiceModel", "4.0.0.0")]
public partial class TempConvertSoapClient : System.ServiceModel.ClientBase<TempConvertSoap>, TempConvertSoap
{

	private BeginOperationDelegate onBeginFahrenheitToCelsiusDelegate;

	private EndOperationDelegate onEndFahrenheitToCelsiusDelegate;

	private System.Threading.SendOrPostCallback onFahrenheitToCelsiusCompletedDelegate;

	private BeginOperationDelegate onBeginCelsiusToFahrenheitDelegate;

	private EndOperationDelegate onEndCelsiusToFahrenheitDelegate;

	private System.Threading.SendOrPostCallback onCelsiusToFahrenheitCompletedDelegate;

	private BeginOperationDelegate onBeginOpenDelegate;

	private EndOperationDelegate onEndOpenDelegate;

	private System.Threading.SendOrPostCallback onOpenCompletedDelegate;

	private BeginOperationDelegate onBeginCloseDelegate;

	private EndOperationDelegate onEndCloseDelegate;

	private System.Threading.SendOrPostCallback onCloseCompletedDelegate;

	public TempConvertSoapClient ()
	{
	}

	public TempConvertSoapClient (string endpointConfigurationName) :
	base (endpointConfigurationName)
	{
	}

	public TempConvertSoapClient (string endpointConfigurationName, string remoteAddress) :
	base (endpointConfigurationName, remoteAddress)
	{
	}

	public TempConvertSoapClient (string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
	base (endpointConfigurationName, remoteAddress)
	{
	}

	public TempConvertSoapClient (System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
	base (binding, remoteAddress)
	{
	}

	public System.Net.CookieContainer CookieContainer {
		get {
			System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager> ();
			if ((httpCookieContainerManager != null)) {
				return httpCookieContainerManager.CookieContainer;
			} else {
				return null;
			}
		}
		set {
			System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager> ();
			if ((httpCookieContainerManager != null)) {
				httpCookieContainerManager.CookieContainer = value;
			} else {
				throw new System.InvalidOperationException ("Unable to set the CookieContainer. Please make sure the binding contains an HttpC" +
					"ookieContainerBindingElement.");
			}
		}
	}

	public event System.EventHandler<FahrenheitToCelsiusCompletedEventArgs> FahrenheitToCelsiusCompleted;

	public event System.EventHandler<CelsiusToFahrenheitCompletedEventArgs> CelsiusToFahrenheitCompleted;

	public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> OpenCompleted;

	public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> CloseCompleted;

	[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
	System.IAsyncResult TempConvertSoap.BeginFahrenheitToCelsius (FahrenheitToCelsiusRequest request, System.AsyncCallback callback, object asyncState)
	{
		return base.Channel.BeginFahrenheitToCelsius (request, callback, asyncState);
	}

	[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
	private System.IAsyncResult BeginFahrenheitToCelsius (string Fahrenheit, System.AsyncCallback callback, object asyncState)
	{
		FahrenheitToCelsiusRequest inValue = new FahrenheitToCelsiusRequest ();
		inValue.Body = new FahrenheitToCelsiusRequestBody ();
		inValue.Body.Fahrenheit = Fahrenheit;
		return ((TempConvertSoap)(this)).BeginFahrenheitToCelsius (inValue, callback, asyncState);
	}

	[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
	FahrenheitToCelsiusResponse TempConvertSoap.EndFahrenheitToCelsius (System.IAsyncResult result)
	{
		return base.Channel.EndFahrenheitToCelsius (result);
	}

	[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
	private string EndFahrenheitToCelsius (System.IAsyncResult result)
	{
		FahrenheitToCelsiusResponse retVal = ((TempConvertSoap)(this)).EndFahrenheitToCelsius (result);
		return retVal.Body.FahrenheitToCelsiusResult;
	}

	private System.IAsyncResult OnBeginFahrenheitToCelsius (object[] inValues, System.AsyncCallback callback, object asyncState)
	{
		string Fahrenheit = ((string)(inValues [0]));
		return this.BeginFahrenheitToCelsius (Fahrenheit, callback, asyncState);
	}

	private object[] OnEndFahrenheitToCelsius (System.IAsyncResult result)
	{
		string retVal = this.EndFahrenheitToCelsius (result);
		return new object[] {
			retVal
		};
	}

	private void OnFahrenheitToCelsiusCompleted (object state)
	{
		if ((this.FahrenheitToCelsiusCompleted != null)) {
			InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
			this.FahrenheitToCelsiusCompleted (this, new FahrenheitToCelsiusCompletedEventArgs (e.Results, e.Error, e.Cancelled, e.UserState));
		}
	}

	public void FahrenheitToCelsiusAsync (string Fahrenheit)
	{
		this.FahrenheitToCelsiusAsync (Fahrenheit, null);
	}

	public void FahrenheitToCelsiusAsync (string Fahrenheit, object userState)
	{
		if ((this.onBeginFahrenheitToCelsiusDelegate == null)) {
			this.onBeginFahrenheitToCelsiusDelegate = new BeginOperationDelegate (this.OnBeginFahrenheitToCelsius);
		}
		if ((this.onEndFahrenheitToCelsiusDelegate == null)) {
			this.onEndFahrenheitToCelsiusDelegate = new EndOperationDelegate (this.OnEndFahrenheitToCelsius);
		}
		if ((this.onFahrenheitToCelsiusCompletedDelegate == null)) {
			this.onFahrenheitToCelsiusCompletedDelegate = new System.Threading.SendOrPostCallback (this.OnFahrenheitToCelsiusCompleted);
		}
		base.InvokeAsync (this.onBeginFahrenheitToCelsiusDelegate, new object[] {
			Fahrenheit
		}, this.onEndFahrenheitToCelsiusDelegate, this.onFahrenheitToCelsiusCompletedDelegate, userState);
	}

	[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
	System.IAsyncResult TempConvertSoap.BeginCelsiusToFahrenheit (CelsiusToFahrenheitRequest request, System.AsyncCallback callback, object asyncState)
	{
		return base.Channel.BeginCelsiusToFahrenheit (request, callback, asyncState);
	}

	[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
	private System.IAsyncResult BeginCelsiusToFahrenheit (string Celsius, System.AsyncCallback callback, object asyncState)
	{
		CelsiusToFahrenheitRequest inValue = new CelsiusToFahrenheitRequest ();
		inValue.Body = new CelsiusToFahrenheitRequestBody ();
		inValue.Body.Celsius = Celsius;
		return ((TempConvertSoap)(this)).BeginCelsiusToFahrenheit (inValue, callback, asyncState);
	}

	[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
	CelsiusToFahrenheitResponse TempConvertSoap.EndCelsiusToFahrenheit (System.IAsyncResult result)
	{
		return base.Channel.EndCelsiusToFahrenheit (result);
	}

	[System.ComponentModel.EditorBrowsableAttribute (System.ComponentModel.EditorBrowsableState.Advanced)]
	private string EndCelsiusToFahrenheit (System.IAsyncResult result)
	{
		CelsiusToFahrenheitResponse retVal = ((TempConvertSoap)(this)).EndCelsiusToFahrenheit (result);
		return retVal.Body.CelsiusToFahrenheitResult;
	}

	private System.IAsyncResult OnBeginCelsiusToFahrenheit (object[] inValues, System.AsyncCallback callback, object asyncState)
	{
		string Celsius = ((string)(inValues [0]));
		return this.BeginCelsiusToFahrenheit (Celsius, callback, asyncState);
	}

	private object[] OnEndCelsiusToFahrenheit (System.IAsyncResult result)
	{
		string retVal = this.EndCelsiusToFahrenheit (result);
		return new object[] {
			retVal
		};
	}

	private void OnCelsiusToFahrenheitCompleted (object state)
	{
		if ((this.CelsiusToFahrenheitCompleted != null)) {
			InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
			this.CelsiusToFahrenheitCompleted (this, new CelsiusToFahrenheitCompletedEventArgs (e.Results, e.Error, e.Cancelled, e.UserState));
		}
	}

	public void CelsiusToFahrenheitAsync (string Celsius)
	{
		this.CelsiusToFahrenheitAsync (Celsius, null);
	}

	public void CelsiusToFahrenheitAsync (string Celsius, object userState)
	{
		if ((this.onBeginCelsiusToFahrenheitDelegate == null)) {
			this.onBeginCelsiusToFahrenheitDelegate = new BeginOperationDelegate (this.OnBeginCelsiusToFahrenheit);
		}
		if ((this.onEndCelsiusToFahrenheitDelegate == null)) {
			this.onEndCelsiusToFahrenheitDelegate = new EndOperationDelegate (this.OnEndCelsiusToFahrenheit);
		}
		if ((this.onCelsiusToFahrenheitCompletedDelegate == null)) {
			this.onCelsiusToFahrenheitCompletedDelegate = new System.Threading.SendOrPostCallback (this.OnCelsiusToFahrenheitCompleted);
		}
		base.InvokeAsync (this.onBeginCelsiusToFahrenheitDelegate, new object[] {
			Celsius
		}, this.onEndCelsiusToFahrenheitDelegate, this.onCelsiusToFahrenheitCompletedDelegate, userState);
	}

	private System.IAsyncResult OnBeginOpen (object[] inValues, System.AsyncCallback callback, object asyncState)
	{
		return ((System.ServiceModel.ICommunicationObject)(this)).BeginOpen (callback, asyncState);
	}

	private object[] OnEndOpen (System.IAsyncResult result)
	{
		((System.ServiceModel.ICommunicationObject)(this)).EndOpen (result);
		return null;
	}

	private void OnOpenCompleted (object state)
	{
		if ((this.OpenCompleted != null)) {
			InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
			this.OpenCompleted (this, new System.ComponentModel.AsyncCompletedEventArgs (e.Error, e.Cancelled, e.UserState));
		}
	}

	public void OpenAsync ()
	{
		this.OpenAsync (null);
	}

	public void OpenAsync (object userState)
	{
		if ((this.onBeginOpenDelegate == null)) {
			this.onBeginOpenDelegate = new BeginOperationDelegate (this.OnBeginOpen);
		}
		if ((this.onEndOpenDelegate == null)) {
			this.onEndOpenDelegate = new EndOperationDelegate (this.OnEndOpen);
		}
		if ((this.onOpenCompletedDelegate == null)) {
			this.onOpenCompletedDelegate = new System.Threading.SendOrPostCallback (this.OnOpenCompleted);
		}
		base.InvokeAsync (this.onBeginOpenDelegate, null, this.onEndOpenDelegate, this.onOpenCompletedDelegate, userState);
	}

	private System.IAsyncResult OnBeginClose (object[] inValues, System.AsyncCallback callback, object asyncState)
	{
		return ((System.ServiceModel.ICommunicationObject)(this)).BeginClose (callback, asyncState);
	}

	private object[] OnEndClose (System.IAsyncResult result)
	{
		((System.ServiceModel.ICommunicationObject)(this)).EndClose (result);
		return null;
	}

	private void OnCloseCompleted (object state)
	{
		if ((this.CloseCompleted != null)) {
			InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
			this.CloseCompleted (this, new System.ComponentModel.AsyncCompletedEventArgs (e.Error, e.Cancelled, e.UserState));
		}
	}

	public void CloseAsync ()
	{
		this.CloseAsync (null);
	}

	public void CloseAsync (object userState)
	{
		if ((this.onBeginCloseDelegate == null)) {
			this.onBeginCloseDelegate = new BeginOperationDelegate (this.OnBeginClose);
		}
		if ((this.onEndCloseDelegate == null)) {
			this.onEndCloseDelegate = new EndOperationDelegate (this.OnEndClose);
		}
		if ((this.onCloseCompletedDelegate == null)) {
			this.onCloseCompletedDelegate = new System.Threading.SendOrPostCallback (this.OnCloseCompleted);
		}
		base.InvokeAsync (this.onBeginCloseDelegate, null, this.onEndCloseDelegate, this.onCloseCompletedDelegate, userState);
	}

	protected override TempConvertSoap CreateChannel ()
	{
		return new TempConvertSoapClientChannel (this);
	}

	private class TempConvertSoapClientChannel : ChannelBase<TempConvertSoap>, TempConvertSoap
	{

		public TempConvertSoapClientChannel (System.ServiceModel.ClientBase<TempConvertSoap> client) :
		base (client)
		{
		}

		public System.IAsyncResult BeginFahrenheitToCelsius (FahrenheitToCelsiusRequest request, System.AsyncCallback callback, object asyncState)
		{
			object[] _args = new object[1];
			_args [0] = request;
			System.IAsyncResult _result = base.BeginInvoke ("FahrenheitToCelsius", _args, callback, asyncState);
			return _result;
		}

		public FahrenheitToCelsiusResponse EndFahrenheitToCelsius (System.IAsyncResult result)
		{
			object[] _args = new object[0];
			FahrenheitToCelsiusResponse _result = ((FahrenheitToCelsiusResponse)(base.EndInvoke ("FahrenheitToCelsius", _args, result)));
			return _result;
		}

		public System.IAsyncResult BeginCelsiusToFahrenheit (CelsiusToFahrenheitRequest request, System.AsyncCallback callback, object asyncState)
		{
			object[] _args = new object[1];
			_args [0] = request;
			System.IAsyncResult _result = base.BeginInvoke ("CelsiusToFahrenheit", _args, callback, asyncState);
			return _result;
		}

		public CelsiusToFahrenheitResponse EndCelsiusToFahrenheit (System.IAsyncResult result)
		{
			object[] _args = new object[0];
			CelsiusToFahrenheitResponse _result = ((CelsiusToFahrenheitResponse)(base.EndInvoke ("CelsiusToFahrenheit", _args, result)));
			return _result;
		}
	}
}
#endif
