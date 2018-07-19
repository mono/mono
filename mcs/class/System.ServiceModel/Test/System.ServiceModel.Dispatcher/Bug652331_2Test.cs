//
// Authors:
//	David Straw
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2011 Novell, Inc.  http://www.novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using NUnit.Framework;

using WebServiceMoonlightTest.ServiceReference2;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class Bug652331_2Test
	{
		[Test]
		[Category ("NotWorking")]
		public void Bug652331_3 ()
		{
			// Init service
			ServiceHost serviceHost = new ServiceHost(typeof(Service1), new Uri("http://localhost:" + NetworkHelpers.FindFreePort () + "/Service1"));
			serviceHost.AddServiceEndpoint(typeof(IService1), new BasicHttpBinding(), string.Empty);

			// Enable metadata exchange (WSDL publishing)
			ServiceMetadataBehavior mexBehavior = new ServiceMetadataBehavior();
			mexBehavior.HttpGetEnabled = true;
			serviceHost.Description.Behaviors.Add(mexBehavior);
			serviceHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

			serviceHost.Open();

			try {
				RunClient ();
			} finally {
				serviceHost.Close ();
			}
		}

		void RunClient ()
		{
			var binding = new BasicHttpBinding ();
			var remoteAddress = new EndpointAddress ("http://localhost:" + NetworkHelpers.FindFreePort () + "/Service1");

			var normalClient      = new Service1Client (binding, remoteAddress);
			var collectionClient  = new Service1Client (binding, remoteAddress);
			var nestedClient      = new Service1Client (binding, remoteAddress);
			var dbClient          = new Service1Client (binding, remoteAddress);

			{
				ManualResetEvent wait = new ManualResetEvent (false);
				Exception error = null;
				object result = null;

				normalClient.GetDataCompleted += delegate (object o, GetDataCompletedEventArgs e) {
					try {
						error = e.Error;
						result = e.Error == null ? e.Result : null;
					} finally {
						wait.Set ();
					}
				};
				normalClient.GetDataAsync ();

				Assert.IsTrue (wait.WaitOne (TimeSpan.FromSeconds (20)), "#1 timeout");
				Assert.IsNull (error, "#1.1, inner exception: {0}", error);
				Assert.AreEqual ("A", ((DataType1) result).Id, "#1.2");
			}

			{
				ManualResetEvent wait = new ManualResetEvent (false);
				Exception error = null;
				ObservableCollection<object> result = null;

				collectionClient.GetCollectionDataCompleted += delegate (object sender, GetCollectionDataCompletedEventArgs e) {
					try {
						error = e.Error;
						result = e.Error == null ? e.Result : null;
					} finally {
						wait.Set ();
					}
				};
				collectionClient.GetCollectionDataAsync ();

				Assert.IsTrue (wait.WaitOne (TimeSpan.FromSeconds (20)), "#2 timeout");
				Assert.IsNull (error, "#2.1, inner exception: {0}", error);
				Assert.AreEqual ("B,C", ItemsToString (result.Cast<DataType1> ()), "#2.2");
			}

			{
				ManualResetEvent wait = new ManualResetEvent (false);
				Exception error = null;
				WebServiceMoonlightTest.ServiceReference2.DataType2 result = null;

				nestedClient.GetNestedDataCompleted += delegate (object sender, GetNestedDataCompletedEventArgs e) {
					try {
						error = e.Error;
						result = e.Error == null ? e.Result : null;
					} finally {
						wait.Set ();
					}
				};
				nestedClient.GetNestedDataAsync ();

				Assert.IsTrue (wait.WaitOne (TimeSpan.FromSeconds (20)), "#3 timeout");
				Assert.IsNull (error, "#3.1, inner exception: {0}", error);
				Assert.AreEqual ("D,E", ItemsToString (result.Items.Cast<DataType1> ()), "#3.2");
			}

			{
				ManualResetEvent wait = new ManualResetEvent (false);
				Exception error = null;
				string result = null;

				dbClient.JSMGetDatabasesCompleted += delegate (object sender, JSMGetDatabasesCompletedEventArgs e) {
					try {
						error = e.Error;
						result = e.Error == null ? e.Result : null;
					} finally {
						wait.Set ();
					}
				};
				dbClient.JSMGetDatabasesAsync();

				Assert.IsTrue (wait.WaitOne (TimeSpan.FromSeconds (20)), "#4 timeout");
				Assert.IsNull (error, "#4.1, inner exception: {0}", error);
				Assert.AreEqual ("databases", result, "#4.2");
			}
		}

		string ItemsToString (IEnumerable<DataType1> items)
		{
			return items.Aggregate ((string) null, (result, item) => result == null ? item.Id : result + "," + item.Id);
		}
	}

	public class Service1 : IService1
	{
		public object GetData()
		{
			return new DataType1 { Id = "A" };
		}

		Func<object> gd;
		public IAsyncResult BeginGetData(AsyncCallback cb, object st)
		{
			gd = new Func<object> (GetData);
			return gd.BeginInvoke (cb, st);
		}

		public object EndGetData (IAsyncResult result)
		{
			return gd.EndInvoke (result);
		}

		public ObservableCollection<object> GetCollectionData()
		{
			return new ObservableCollection<object> { new DataType1 { Id = "B" }, new DataType1 { Id = "C" } };
		}

		Func<ObservableCollection<object>> gcd;
		public IAsyncResult BeginGetCollectionData(AsyncCallback cb, object st)
		{
			gcd = new Func<ObservableCollection<object>> (GetCollectionData);
			return gcd.BeginInvoke (cb, st);
		}

		public ObservableCollection<object> EndGetCollectionData (IAsyncResult result)
		{
			return gcd.EndInvoke (result);
		}

		public DataType2 GetNestedData()
		{
			return new DataType2 { Items = new ObservableCollection<object> { new DataType1 { Id = "D" }, new DataType1 { Id = "E" } } };
		}

		Func<DataType2> gnd;
		public IAsyncResult BeginGetNestedData(AsyncCallback cb, object st)
		{
			gnd = new Func<DataType2> (GetNestedData);
			return gnd.BeginInvoke (cb, st);
		}

		public DataType2 EndGetNestedData (IAsyncResult result)
		{
			return gnd.EndInvoke (result);
		}

		public JSMGetDatabasesResponse JSMGetDatabases(JSMGetDatabasesRequest request)
		{
			return new JSMGetDatabasesResponse { JSMGetDatabasesResult = "databases" };
		}

		Func<JSMGetDatabasesRequest, JSMGetDatabasesResponse> gjgdb;
		public IAsyncResult BeginJSMGetDatabases(JSMGetDatabasesRequest request, AsyncCallback callback, object asyncState)
		{
			gjgdb = JSMGetDatabases;
			return gjgdb.BeginInvoke (request, callback, asyncState);
		}

		public JSMGetDatabasesResponse EndJSMGetDatabases(IAsyncResult result)
		{
			return gjgdb.EndInvoke (result);
		}
	}
}


//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.372
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This code was auto-generated by Microsoft.Silverlight.ServiceReference, version 4.0.50826.0
// 
namespace WebServiceMoonlightTest.ServiceReference2 {
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DataType1", Namespace="http://mynamespace")]
    public partial class DataType1 : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string IdField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Id {
            get {
                return this.IdField;
            }
            set {
                if ((object.ReferenceEquals(this.IdField, value) != true)) {
                    this.IdField = value;
                    this.RaisePropertyChanged("Id");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DataType2", Namespace="http://mynamespace")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(WebServiceMoonlightTest.ServiceReference2.DataType1))]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(System.Collections.ObjectModel.ObservableCollection<object>))]
    public partial class DataType2 : object, System.ComponentModel.INotifyPropertyChanged {
        
        private System.Collections.ObjectModel.ObservableCollection<object> ItemsField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.ObjectModel.ObservableCollection<object> Items {
            get {
                return this.ItemsField;
            }
            set {
                if ((object.ReferenceEquals(this.ItemsField, value) != true)) {
                    this.ItemsField = value;
                    this.RaisePropertyChanged("Items");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://mynamespace", ConfigurationName="ServiceReference1.IService1")]
    public interface IService1 {
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://mynamespace/IService1/GetData", ReplyAction="http://mynamespace/IService1/GetDataResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(WebServiceMoonlightTest.ServiceReference2.DataType1))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(WebServiceMoonlightTest.ServiceReference2.DataType2))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.Collections.ObjectModel.ObservableCollection<object>))]
        System.IAsyncResult BeginGetData(System.AsyncCallback callback, object asyncState);
        
        object EndGetData(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://mynamespace/IService1/GetCollectionData", ReplyAction="http://mynamespace/IService1/GetCollectionDataResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(WebServiceMoonlightTest.ServiceReference2.DataType1))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(WebServiceMoonlightTest.ServiceReference2.DataType2))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.Collections.ObjectModel.ObservableCollection<object>))]
        System.IAsyncResult BeginGetCollectionData(System.AsyncCallback callback, object asyncState);
        
        System.Collections.ObjectModel.ObservableCollection<object> EndGetCollectionData(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://mynamespace/IService1/GetNestedData", ReplyAction="http://mynamespace/IService1/GetNestedDataResponse")]
        System.IAsyncResult BeginGetNestedData(System.AsyncCallback callback, object asyncState);
        
        WebServiceMoonlightTest.ServiceReference2.DataType2 EndGetNestedData(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://mynamespace/IService1/JSMGetDatabases", ReplyAction = "http://mynamespace/IService1/JSMGetDatabasesResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object[]))]
        System.IAsyncResult BeginJSMGetDatabases(JSMGetDatabasesRequest request, System.AsyncCallback callback, object asyncState);

        JSMGetDatabasesResponse EndJSMGetDatabases(System.IAsyncResult result);
    }

#region JSMGetDatabases
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName = "JSMGetDatabases", WrapperNamespace = "", IsWrapped = true)]
    public partial class JSMGetDatabasesRequest
    {

        public JSMGetDatabasesRequest()
        {
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName = "JSMGetDatabasesResponse", WrapperNamespace = "", IsWrapped = true)]
    public partial class JSMGetDatabasesResponse
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "", Order = 0)]
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string JSMGetDatabasesResult;

        public JSMGetDatabasesResponse()
        {
        }

        public JSMGetDatabasesResponse(string JSMGetDatabasesResult)
        {
            this.JSMGetDatabasesResult = JSMGetDatabasesResult;
        }
    }
#endregion

    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IService1Channel : WebServiceMoonlightTest.ServiceReference2.IService1, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public object Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((object)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetCollectionDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetCollectionDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public System.Collections.ObjectModel.ObservableCollection<object> Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((System.Collections.ObjectModel.ObservableCollection<object>)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetNestedDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetNestedDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public WebServiceMoonlightTest.ServiceReference2.DataType2 Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((WebServiceMoonlightTest.ServiceReference2.DataType2)(this.results[0]));
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class JSMGetDatabasesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        public JSMGetDatabasesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        public string Result
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class Service1Client : System.ServiceModel.ClientBase<WebServiceMoonlightTest.ServiceReference2.IService1>, WebServiceMoonlightTest.ServiceReference2.IService1 {
        
        private BeginOperationDelegate onBeginGetDataDelegate;
        
        private EndOperationDelegate onEndGetDataDelegate;
        
        private System.Threading.SendOrPostCallback onGetDataCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetCollectionDataDelegate;
        
        private EndOperationDelegate onEndGetCollectionDataDelegate;
        
        private System.Threading.SendOrPostCallback onGetCollectionDataCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetNestedDataDelegate;
        
        private EndOperationDelegate onEndGetNestedDataDelegate;
        
        private System.Threading.SendOrPostCallback onGetNestedDataCompletedDelegate;
        
        private BeginOperationDelegate onBeginOpenDelegate;
        
        private EndOperationDelegate onEndOpenDelegate;
        
        private System.Threading.SendOrPostCallback onOpenCompletedDelegate;
        
        private BeginOperationDelegate onBeginCloseDelegate;
        
        private EndOperationDelegate onEndCloseDelegate;
        
        private System.Threading.SendOrPostCallback onCloseCompletedDelegate;
        
#region JSMGetDatabasesDelegates
        private BeginOperationDelegate onBeginJSMGetDatabasesDelegate;

        private EndOperationDelegate onEndJSMGetDatabasesDelegate;

        private System.Threading.SendOrPostCallback onJSMGetDatabasesCompletedDelegate;
#endregion

        public Service1Client() {
        }
        
        public Service1Client(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public Service1Client(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Service1Client(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Service1Client(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
/*
        public System.Net.CookieContainer CookieContainer {
            get {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    return httpCookieContainerManager.CookieContainer;
                }
                else {
                    return null;
                }
            }
            set {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    httpCookieContainerManager.CookieContainer = value;
                }
                else {
                    throw new System.InvalidOperationException("Unable to set the CookieContainer. Please make sure the binding contains an HttpC" +
                            "ookieContainerBindingElement.");
                }
            }
        }
*/
        
        public event System.EventHandler<GetDataCompletedEventArgs> GetDataCompleted;
        
        public event System.EventHandler<GetCollectionDataCompletedEventArgs> GetCollectionDataCompleted;
        
        public event System.EventHandler<GetNestedDataCompletedEventArgs> GetNestedDataCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> OpenCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> CloseCompleted;

        public event System.EventHandler<JSMGetDatabasesCompletedEventArgs> JSMGetDatabasesCompleted;

        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult WebServiceMoonlightTest.ServiceReference2.IService1.BeginGetData(System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetData(callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        object WebServiceMoonlightTest.ServiceReference2.IService1.EndGetData(System.IAsyncResult result) {
            return base.Channel.EndGetData(result);
        }
        
        private System.IAsyncResult OnBeginGetData(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((WebServiceMoonlightTest.ServiceReference2.IService1)(this)).BeginGetData(callback, asyncState);
        }
        
        private object[] OnEndGetData(System.IAsyncResult result) {
            object retVal = ((WebServiceMoonlightTest.ServiceReference2.IService1)(this)).EndGetData(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetDataCompleted(object state) {
            if ((this.GetDataCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetDataCompleted(this, new GetDataCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetDataAsync() {
            this.GetDataAsync(null);
        }
        
        public void GetDataAsync(object userState) {
            if ((this.onBeginGetDataDelegate == null)) {
                this.onBeginGetDataDelegate = new BeginOperationDelegate(this.OnBeginGetData);
            }
            if ((this.onEndGetDataDelegate == null)) {
                this.onEndGetDataDelegate = new EndOperationDelegate(this.OnEndGetData);
            }
            if ((this.onGetDataCompletedDelegate == null)) {
                this.onGetDataCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetDataCompleted);
            }
            base.InvokeAsync(this.onBeginGetDataDelegate, null, this.onEndGetDataDelegate, this.onGetDataCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult WebServiceMoonlightTest.ServiceReference2.IService1.BeginGetCollectionData(System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetCollectionData(callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Collections.ObjectModel.ObservableCollection<object> WebServiceMoonlightTest.ServiceReference2.IService1.EndGetCollectionData(System.IAsyncResult result) {
            return base.Channel.EndGetCollectionData(result);
        }
        
        private System.IAsyncResult OnBeginGetCollectionData(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((WebServiceMoonlightTest.ServiceReference2.IService1)(this)).BeginGetCollectionData(callback, asyncState);
        }
        
        private object[] OnEndGetCollectionData(System.IAsyncResult result) {
            System.Collections.ObjectModel.ObservableCollection<object> retVal = ((WebServiceMoonlightTest.ServiceReference2.IService1)(this)).EndGetCollectionData(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetCollectionDataCompleted(object state) {
            if ((this.GetCollectionDataCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetCollectionDataCompleted(this, new GetCollectionDataCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetCollectionDataAsync() {
            this.GetCollectionDataAsync(null);
        }
        
        public void GetCollectionDataAsync(object userState) {
            if ((this.onBeginGetCollectionDataDelegate == null)) {
                this.onBeginGetCollectionDataDelegate = new BeginOperationDelegate(this.OnBeginGetCollectionData);
            }
            if ((this.onEndGetCollectionDataDelegate == null)) {
                this.onEndGetCollectionDataDelegate = new EndOperationDelegate(this.OnEndGetCollectionData);
            }
            if ((this.onGetCollectionDataCompletedDelegate == null)) {
                this.onGetCollectionDataCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetCollectionDataCompleted);
            }
            base.InvokeAsync(this.onBeginGetCollectionDataDelegate, null, this.onEndGetCollectionDataDelegate, this.onGetCollectionDataCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult WebServiceMoonlightTest.ServiceReference2.IService1.BeginGetNestedData(System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetNestedData(callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        WebServiceMoonlightTest.ServiceReference2.DataType2 WebServiceMoonlightTest.ServiceReference2.IService1.EndGetNestedData(System.IAsyncResult result) {
            return base.Channel.EndGetNestedData(result);
        }
        
        private System.IAsyncResult OnBeginGetNestedData(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((WebServiceMoonlightTest.ServiceReference2.IService1)(this)).BeginGetNestedData(callback, asyncState);
        }
        
        private object[] OnEndGetNestedData(System.IAsyncResult result) {
            WebServiceMoonlightTest.ServiceReference2.DataType2 retVal = ((WebServiceMoonlightTest.ServiceReference2.IService1)(this)).EndGetNestedData(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetNestedDataCompleted(object state) {
            if ((this.GetNestedDataCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetNestedDataCompleted(this, new GetNestedDataCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetNestedDataAsync() {
            this.GetNestedDataAsync(null);
        }
        
        public void GetNestedDataAsync(object userState) {
            if ((this.onBeginGetNestedDataDelegate == null)) {
                this.onBeginGetNestedDataDelegate = new BeginOperationDelegate(this.OnBeginGetNestedData);
            }
            if ((this.onEndGetNestedDataDelegate == null)) {
                this.onEndGetNestedDataDelegate = new EndOperationDelegate(this.OnEndGetNestedData);
            }
            if ((this.onGetNestedDataCompletedDelegate == null)) {
                this.onGetNestedDataCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetNestedDataCompleted);
            }
            base.InvokeAsync(this.onBeginGetNestedDataDelegate, null, this.onEndGetNestedDataDelegate, this.onGetNestedDataCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginOpen(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(callback, asyncState);
        }
        
        private object[] OnEndOpen(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndOpen(result);
            return null;
        }
        
        private void OnOpenCompleted(object state) {
            if ((this.OpenCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.OpenCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void OpenAsync() {
            this.OpenAsync(null);
        }
        
        public void OpenAsync(object userState) {
            if ((this.onBeginOpenDelegate == null)) {
                this.onBeginOpenDelegate = new BeginOperationDelegate(this.OnBeginOpen);
            }
            if ((this.onEndOpenDelegate == null)) {
                this.onEndOpenDelegate = new EndOperationDelegate(this.OnEndOpen);
            }
            if ((this.onOpenCompletedDelegate == null)) {
                this.onOpenCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnOpenCompleted);
            }
            base.InvokeAsync(this.onBeginOpenDelegate, null, this.onEndOpenDelegate, this.onOpenCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginClose(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginClose(callback, asyncState);
        }
        
        private object[] OnEndClose(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndClose(result);
            return null;
        }
        
        private void OnCloseCompleted(object state) {
            if ((this.CloseCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.CloseCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }

#region JSMGetDatabases
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult IService1.BeginJSMGetDatabases(JSMGetDatabasesRequest request, System.AsyncCallback callback, object asyncState)
        {
            return base.Channel.BeginJSMGetDatabases(request, callback, asyncState);
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        private System.IAsyncResult BeginJSMGetDatabases(System.AsyncCallback callback, object asyncState)
        {
            JSMGetDatabasesRequest inValue = new JSMGetDatabasesRequest();
            return ((IService1)(this)).BeginJSMGetDatabases(inValue, callback, asyncState);
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        JSMGetDatabasesResponse IService1.EndJSMGetDatabases(System.IAsyncResult result)
        {
            return base.Channel.EndJSMGetDatabases(result);
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        private string EndJSMGetDatabases(System.IAsyncResult result)
        {
            JSMGetDatabasesResponse retVal = ((IService1)(this)).EndJSMGetDatabases(result);
            return retVal.JSMGetDatabasesResult;
        }

        private System.IAsyncResult OnBeginJSMGetDatabases(object[] inValues, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginJSMGetDatabases(callback, asyncState);
        }

        private object[] OnEndJSMGetDatabases(System.IAsyncResult result)
        {
            string retVal = this.EndJSMGetDatabases(result);
            return new object[] {
                retVal};
        }

        private void OnJSMGetDatabasesCompleted(object state)
        {
            if ((this.JSMGetDatabasesCompleted != null))
            {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.JSMGetDatabasesCompleted(this, new JSMGetDatabasesCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }

        public void JSMGetDatabasesAsync()
        {
            this.JSMGetDatabasesAsync(null);
        }

        public void JSMGetDatabasesAsync(object userState)
        {
            if ((this.onBeginJSMGetDatabasesDelegate == null))
            {
                this.onBeginJSMGetDatabasesDelegate = new BeginOperationDelegate(this.OnBeginJSMGetDatabases);
            }
            if ((this.onEndJSMGetDatabasesDelegate == null))
            {
                this.onEndJSMGetDatabasesDelegate = new EndOperationDelegate(this.OnEndJSMGetDatabases);
            }
            if ((this.onJSMGetDatabasesCompletedDelegate == null))
            {
                this.onJSMGetDatabasesCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnJSMGetDatabasesCompleted);
            }
            base.InvokeAsync(this.onBeginJSMGetDatabasesDelegate, null, this.onEndJSMGetDatabasesDelegate, this.onJSMGetDatabasesCompletedDelegate, userState);
        }
#endregion
        
        public void CloseAsync() {
            this.CloseAsync(null);
        }
        
        public void CloseAsync(object userState) {
            if ((this.onBeginCloseDelegate == null)) {
                this.onBeginCloseDelegate = new BeginOperationDelegate(this.OnBeginClose);
            }
            if ((this.onEndCloseDelegate == null)) {
                this.onEndCloseDelegate = new EndOperationDelegate(this.OnEndClose);
            }
            if ((this.onCloseCompletedDelegate == null)) {
                this.onCloseCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnCloseCompleted);
            }
            base.InvokeAsync(this.onBeginCloseDelegate, null, this.onEndCloseDelegate, this.onCloseCompletedDelegate, userState);
        }
        
/*
        protected override WebServiceMoonlightTest.ServiceReference2.IService1 CreateChannel() {
            return new Service1ClientChannel(this);
        }
        
        private class Service1ClientChannel : ChannelBase<WebServiceMoonlightTest.ServiceReference2.IService1>, WebServiceMoonlightTest.ServiceReference2.IService1 {
            
            public Service1ClientChannel(System.ServiceModel.ClientBase<WebServiceMoonlightTest.ServiceReference2.IService1> client) : 
                    base(client) {
            }
            
            public System.IAsyncResult BeginGetData(System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[0];
                System.IAsyncResult _result = base.BeginInvoke("GetData", _args, callback, asyncState);
                return _result;
            }
            
            public object EndGetData(System.IAsyncResult result) {
                object[] _args = new object[0];
                object _result = ((object)(base.EndInvoke("GetData", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetCollectionData(System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[0];
                System.IAsyncResult _result = base.BeginInvoke("GetCollectionData", _args, callback, asyncState);
                return _result;
            }
            
            public System.Collections.ObjectModel.ObservableCollection<object> EndGetCollectionData(System.IAsyncResult result) {
                object[] _args = new object[0];
                System.Collections.ObjectModel.ObservableCollection<object> _result = ((System.Collections.ObjectModel.ObservableCollection<object>)(base.EndInvoke("GetCollectionData", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetNestedData(System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[0];
                System.IAsyncResult _result = base.BeginInvoke("GetNestedData", _args, callback, asyncState);
                return _result;
            }
            
            public WebServiceMoonlightTest.ServiceReference2.DataType2 EndGetNestedData(System.IAsyncResult result) {
                object[] _args = new object[0];
                WebServiceMoonlightTest.ServiceReference2.DataType2 _result = ((WebServiceMoonlightTest.ServiceReference2.DataType2)(base.EndInvoke("GetNestedData", _args, result)));
                return _result;
            }
        }
*/
    }
}
#endif

