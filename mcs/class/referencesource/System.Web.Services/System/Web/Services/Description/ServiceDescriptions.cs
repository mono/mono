//------------------------------------------------------------------------------
// <copyright file="ServiceDescriptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.IO;
    using System.ComponentModel;

    /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class ServiceDescriptionCollection : ServiceDescriptionBaseCollection {
        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.ServiceDescriptionCollection"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescriptionCollection() : base(null) { }

        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescription this[int index] {
            get { return (ServiceDescription)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.this1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescription this[string ns] {
            get { return (ServiceDescription)Table[ns]; }
        }

        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(ServiceDescription serviceDescription) {
            return List.Add(serviceDescription);
        }
        
        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, ServiceDescription serviceDescription) {
            List.Insert(index, serviceDescription);
        }
        
        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(ServiceDescription serviceDescription) {
            return List.IndexOf(serviceDescription);
        }
        
        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(ServiceDescription serviceDescription) {
            return List.Contains(serviceDescription);
        }
        
        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(ServiceDescription serviceDescription) {
            List.Remove(serviceDescription);
        }
        
        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(ServiceDescription[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.GetKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string GetKey(object value) {
            string ns = ((ServiceDescription)value).TargetNamespace;
            if (ns == null) 
                return string.Empty;
            return ns;
        }

        Exception ItemNotFound(XmlQualifiedName name, string type) {
            return new Exception(Res.GetString(Res.WebDescriptionMissingItem, type, name.Name, name.Namespace));
        }

        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.GetMessage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Message GetMessage(XmlQualifiedName name) {
            ServiceDescription sd = GetServiceDescription(name);
            Message message = null;
            while (message == null && sd != null) {
                message = sd.Messages[name.Name];
                sd = sd.Next;
            }
            if (message == null) throw ItemNotFound(name, "message");
            return message;
        }

        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.GetPortType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PortType GetPortType(XmlQualifiedName name) {
            ServiceDescription sd = GetServiceDescription(name);
            PortType portType = null;
            while (portType == null && sd != null) {
                portType = sd.PortTypes[name.Name];
                sd = sd.Next;
            }
            if (portType == null) throw ItemNotFound(name, "message");
            return portType;
        }

        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.GetService"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Service GetService(XmlQualifiedName name) {
            ServiceDescription sd = GetServiceDescription(name);
            Service service = null;
            while (service == null && sd != null) {
                service = sd.Services[name.Name];
                sd = sd.Next;
            }
            if (service == null) throw ItemNotFound(name, "service");
            return service;
        }

        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.GetBinding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Binding GetBinding(XmlQualifiedName name) {
            ServiceDescription sd = GetServiceDescription(name);
            Binding binding = null;
            while (binding == null && sd != null) {
                binding = sd.Bindings[name.Name];
                sd = sd.Next;
            }
            if (binding == null) throw ItemNotFound(name, "binding");
            return binding;
        }

        ServiceDescription GetServiceDescription(XmlQualifiedName name) {
            ServiceDescription serviceDescription = this[name.Namespace];
            if (serviceDescription == null) {
                throw new ArgumentException(Res.GetString(Res.WebDescriptionMissing, name.ToString(), name.Namespace), "name");
            }
            return serviceDescription;
        }

        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((ServiceDescription)value).SetParent((ServiceDescriptionCollection)parent);
        }

        /// <include file='doc\ServiceDescriptions.uex' path='docs/doc[@for="ServiceDescriptionBaseCollection.OnInsertComplete"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnInsertComplete(int index, object value) {
            string key = GetKey(value);
            if (key != null) {
                ServiceDescription item = (ServiceDescription)Table[key];
                ((ServiceDescription)value).Next = (ServiceDescription)Table[key];
                Table[key] = value;
            }
            SetParent(value, this);
        }
    }
}
