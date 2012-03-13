using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using System.ServiceModel.Dispatcher;

using System.Text;
using NUnit.Framework;

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.261
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Client.EvalServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Eval", Namespace="http://schemas.datacontract.org/2004/07/WcfServiceLibrary1")]
    [System.SerializableAttribute()]
    public partial class Eval : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Client.EvalServiceReference.EvalItem[] itemsListField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Collections.Generic.Dictionary<string, Client.EvalServiceReference.EvalItem> itemsMapField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
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
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Client.EvalServiceReference.EvalItem[] itemsList {
            get {
                return this.itemsListField;
            }
            set {
                if ((object.ReferenceEquals(this.itemsListField, value) != true)) {
                    this.itemsListField = value;
                    this.RaisePropertyChanged("itemsList");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.Generic.Dictionary<string, Client.EvalServiceReference.EvalItem> itemsMap {
            get {
                return this.itemsMapField;
            }
            set {
                if ((object.ReferenceEquals(this.itemsMapField, value) != true)) {
                    this.itemsMapField = value;
                    this.RaisePropertyChanged("itemsMap");
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
    [System.Runtime.Serialization.DataContractAttribute(Name="EvalItem", Namespace="http://schemas.datacontract.org/2004/07/WcfServiceLibrary1")]
    [System.SerializableAttribute()]
    public partial class EvalItem : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ItemField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<System.DateTime> ItemTimeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<Client.EvalServiceReference.EvalType> etypeField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Item {
            get {
                return this.ItemField;
            }
            set {
                if ((object.ReferenceEquals(this.ItemField, value) != true)) {
                    this.ItemField = value;
                    this.RaisePropertyChanged("Item");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> ItemTime {
            get {
                return this.ItemTimeField;
            }
            set {
                if ((this.ItemTimeField.Equals(value) != true)) {
                    this.ItemTimeField = value;
                    this.RaisePropertyChanged("ItemTime");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<Client.EvalServiceReference.EvalType> etype {
            get {
                return this.etypeField;
            }
            set {
                if ((this.etypeField.Equals(value) != true)) {
                    this.etypeField = value;
                    this.RaisePropertyChanged("etype");
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
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="EvalType", Namespace="http://schemas.datacontract.org/2004/07/WcfServiceLibrary1")]
    public enum EvalType : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        SIMPLE = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        COMPLEX = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        NONE = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="EvalServiceReference.IEvalService")]
    public interface IEvalService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IEvalService/SubmitEval", ReplyAction="http://tempuri.org/IEvalService/SubmitEvalResponse")]
        void SubmitEval(Client.EvalServiceReference.Eval eval);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IEvalService/GetEvals", ReplyAction="http://tempuri.org/IEvalService/GetEvalsResponse")]
        Client.EvalServiceReference.Eval[] GetEvals();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IEvalService/RemoveEval", ReplyAction="http://tempuri.org/IEvalService/RemoveEvalResponse")]
        void RemoveEval(string id);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IEvalServiceChannel : Client.EvalServiceReference.IEvalService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class EvalServiceClient : System.ServiceModel.ClientBase<Client.EvalServiceReference.IEvalService>, Client.EvalServiceReference.IEvalService {
        
        public EvalServiceClient() {
        }
        
        public EvalServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public EvalServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public EvalServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public EvalServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void SubmitEval(Client.EvalServiceReference.Eval eval) {
            base.Channel.SubmitEval(eval);
        }
        
        public Client.EvalServiceReference.Eval[] GetEvals() {
            return base.Channel.GetEvals();
        }
        
        public void RemoveEval(string id) {
            base.Channel.RemoveEval(id);
        }
    }
}


namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	class SerializeNullableWithDictionaryTest
	{

		[Test]
		public void TestNullableWithDictionary()
		{
			string Name      = "GetEvalsResult";
			string Wrapper   = "GetEvalsResponse";
			string Namespace = "http://tempuri.org/";
			
			Type type = typeof(Client.EvalServiceReference.Eval[]);
			IEnumerable<Type> know_types = new List<Type>();
			
			// This is the XML generated by WCF Server
			string xml = "<GetEvalsResponse xmlns=\"http://tempuri.org/\">      <GetEvalsResult xmlns:a=\"http://schemas.datacontract.org/2004/07/WcfServiceLibrary1\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">        <a:Eval>          <a:Id>8215784f-bf5f-4df8-b239-34a0a029a54e</a:Id>          <a:itemsList>            <a:EvalItem>              <a:Item>Item on List 3</a:Item>              <a:ItemTime>2012-03-04T04:04:00</a:ItemTime>              <a:etype>COMPLEX</a:etype>            </a:EvalItem>            <a:EvalItem i:nil=\"true\" />          </a:itemsList>          <a:itemsMap xmlns:b=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">            <b:KeyValueOfstringEvalItemo8PfwX7N>              <b:Key>Item3</b:Key>              <b:Value>                <a:Item>Item 2 on Map</a:Item>                <a:ItemTime i:nil=\"true\" />                <a:etype>NONE</a:etype>              </b:Value>            </b:KeyValueOfstringEvalItemo8PfwX7N>            <b:KeyValueOfstringEvalItemo8PfwX7N>              <b:Key />              <b:Value i:nil=\"true\" />            </b:KeyValueOfstringEvalItemo8PfwX7N>          </a:itemsMap>        </a:Eval>      </GetEvalsResult>    </GetEvalsResponse>";
			
			Client.EvalServiceReference.Eval[] evals = null;
			
			StringBuilder stringBuilder = new StringBuilder ();
			
			var ds = new DataContractSerializer (type, Name, Namespace, know_types);
			
			using (var xr = XmlDictionaryReader.CreateDictionaryReader ( XmlReader.Create (new StringReader (xml))))
			{	
				xr.ReadStartElement (Wrapper, Namespace);

				for (xr.MoveToContent (); xr.NodeType == XmlNodeType.Element; xr.MoveToContent ()) {
					XmlQualifiedName key = new XmlQualifiedName (xr.LocalName, xr.NamespaceURI);
					
				if ( Name == key.Name && Namespace == key.Namespace)
					break;
				}
					
				evals = (Client.EvalServiceReference.Eval[])ds.ReadObject (xr, true);				
			}
			
			using (var xw = XmlDictionaryWriter.CreateDictionaryWriter ( XmlWriter.Create( new StringWriter(stringBuilder)))) {
				ds.WriteObject (xw, evals);
			}
			
			string actualXml = stringBuilder.ToString ();
			
			Assert.AreEqual (evals.Length, 1, "evals.Length missmatch");
			
			Client.EvalServiceReference.Eval eval = evals[0];
			
			Assert.AreEqual (eval.Id, "8215784f-bf5f-4df8-b239-34a0a029a54e", "eval.Id missmatch");
			Assert.AreEqual (eval.itemsList.Length, 2, "eval.itemsList.Length missmatch");
			
			Client.EvalServiceReference.EvalItem evalItem = eval.itemsList[0];
			
			Assert.AreEqual (evalItem.Item, "Item on List 3", "evalItem.Item missmatch");
			Assert.AreEqual (evalItem.ItemTime , DateTime.Parse("2012-03-04T04:04:00"), "evalItem.ItemTime missmatch");
			Assert.AreEqual (evalItem.etype, Client.EvalServiceReference.EvalType.COMPLEX, "evalItem.etype missmatch");
			
			Client.EvalServiceReference.EvalItem evalItem2 = eval.itemsList[1];
			
			Assert.IsNull (evalItem2, "evalItem2 missmatch");
			
			Assert.AreEqual (eval.itemsMap.Count, 2, "eval.itemsMap.Count missmatch");
			
			Client.EvalServiceReference.EvalItem evalItem3 = eval.itemsMap["Item3"];
			
			Assert.AreEqual (evalItem3.Item, "Item 2 on Map", "evalItem3.Item missmatch");
			Assert.IsNull (evalItem3.ItemTime, "evalItem3.ItemTime missmatch");
			Assert.AreEqual (evalItem3.etype,  Client.EvalServiceReference.EvalType.NONE, "evalItem3.etype missmatch");
			
			Client.EvalServiceReference.EvalItem evalItem4 = eval.itemsMap[""];
			
			Assert.IsNull(evalItem4, "Item 2 on Map", "evalItem4");
			
			Client.EvalServiceReference.Eval[] evals2 = null;
			
			using (var xr = XmlDictionaryReader.CreateDictionaryReader ( XmlReader.Create (new StringReader (actualXml))))
			{
				evals2 = (Client.EvalServiceReference.Eval[])ds.ReadObject (xr, true);	
			}
			
			Assert.AreEqual (evals2.Length, 1, "evals2.Length missmatch");
			
			Client.EvalServiceReference.Eval eval2 = evals2[0];
			
			Assert.AreEqual (eval2.Id, "8215784f-bf5f-4df8-b239-34a0a029a54e", "eval2.Id missmatch");
			Assert.AreEqual (eval2.itemsList.Length, 2, "eval2.itemsList.Length missmatch");
			
			Client.EvalServiceReference.EvalItem eval2Item = eval2.itemsList[0];
			
			Assert.AreEqual (eval2Item.Item, "Item on List 3", "eval2Item.Item missmatch");
			Assert.AreEqual (eval2Item.ItemTime , DateTime.Parse("2012-03-04T04:04:00"), "eval2Item.ItemTime missmatch");
			Assert.AreEqual (eval2Item.etype, Client.EvalServiceReference.EvalType.COMPLEX, "eval2Item.etype missmatch");
			
			Client.EvalServiceReference.EvalItem eval2Item2 = eval2.itemsList[1];
			
			Assert.IsNull (eval2Item2,  "eval2Item2 missmatch");
			
			Assert.AreEqual (eval2.itemsMap.Count, 2, "eval2.itemsMap.Count missmatch");
			
			Client.EvalServiceReference.EvalItem eval2Item3 = eval2.itemsMap["Item3"];
			
			Assert.AreEqual (eval2Item3.Item, "Item 2 on Map", "eval2Item3.Item missmatch");
			Assert.IsNull (eval2Item3.ItemTime, "eval2Item3.ItemTime missmatch");
			Assert.AreEqual (eval2Item3.etype,  Client.EvalServiceReference.EvalType.NONE,  "eval2Item3.etype missmatch");
			
			Client.EvalServiceReference.EvalItem eval2Item4 = eval.itemsMap[""];
			
			Assert.IsNull(eval2Item4, "eval2Item4 missmatch");
		}
	}
}

