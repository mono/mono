#if NET_4_0
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

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class Bug652331_2Test
	{
		[Test]
		public void Bug652331_3 ()
		{
			// Init service
			ServiceHost serviceHost = new ServiceHost(typeof(Service1), new Uri("http://localhost:37564/Service1"));
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
			var remoteAddress = new EndpointAddress ("http://localhost:37564/Service1");
			var client = new Service1Client (binding, remoteAddress);

			var waits = new ManualResetEvent [3];
			for (int i = 0; i < waits.Length; i++)
				waits [i] = new ManualResetEvent (false);

			client.GetDataCompleted += delegate (object o, GetDataCompletedEventArgs e) {
				if (e.Error != null)
					throw e.Error;
				Assert.AreEqual ("A", ((DataType1) e.Result).Id, "Normal");
				waits [0].Set ();
			};
			client.GetDataAsync ();

			client.GetCollectionDataCompleted += delegate (object sender, GetCollectionDataCompletedEventArgs e) {
				if (e.Error != null)
					throw e.Error;
				Assert.AreEqual ("B,C", ItemsToString (e.Result.Cast<DataType1> ()), "Collection");
				waits [1].Set ();
			};
			client.GetCollectionDataAsync ();

			client.GetNestedDataCompleted += delegate (object sender, GetNestedDataCompletedEventArgs e) {
				if (e.Error != null)
					throw e.Error;
				Assert.AreEqual ("D,E", ItemsToString (e.Result.Items.Cast<DataType1> ()), "Nested");
				waits [2].Set ();
			};
			client.GetNestedDataAsync ();

			WaitHandle.WaitAll (waits.Cast<WaitHandle> ().ToArray (), TimeSpan.FromMinutes (1));
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
    }
    
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
