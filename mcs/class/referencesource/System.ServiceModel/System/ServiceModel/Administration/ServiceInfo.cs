//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    sealed class ServiceInfo
    {
        KeyedByTypeCollection<IServiceBehavior> behaviors;
        EndpointInfoCollection endpoints;
        ServiceHostBase service;
        string serviceName;

        internal ServiceInfo(ServiceHostBase service)
        {
            this.service = service;
            this.behaviors = service.Description.Behaviors;
            this.serviceName = service.Description.Name;
            this.endpoints = new EndpointInfoCollection(service.Description.Endpoints, this.ServiceName);
        }

        public string ConfigurationName
        {
            get { return this.service.Description.ConfigurationName; }
        }

        public string DistinguishedName
        {
            get { return this.serviceName + "@" + this.FirstAddress; }
        }

        public string FirstAddress
        {
            get
            {
                Fx.Assert(null != this.Service.BaseAddresses, "");
                string address = "";
                if (this.Service.BaseAddresses.Count > 0)
                {
                    Fx.Assert(null != this.Service.BaseAddresses[0], "");
                    address = this.Service.BaseAddresses[0].ToString();
                }
                else if (this.Endpoints.Count > 0)
                {
                    Uri addressUri = this.Endpoints[0].Address;
                    if (null != addressUri)
                    {
                        address = addressUri.ToString();
                    }
                }
                return address;
            }
        }

        public string Name
        {
            get { return this.serviceName; }
        }

        public string Namespace
        {
            get { return this.service.Description.Namespace; }
        }

        public string ServiceName
        {
            get { return this.serviceName; }
        }

        public ServiceHostBase Service
        {
            get
            {
                return this.service;
            }
        }

        public KeyedByTypeCollection<IServiceBehavior> Behaviors
        {
            get
            {
                return this.behaviors;
            }
        }

        public CommunicationState State
        {
            get
            {
                return this.Service.State;
            }
        }

        public EndpointInfoCollection Endpoints
        {
            get
            {
                return this.endpoints;
            }
        }

        public string[] Metadata
        {
            get
            {
                string[] result = null;
                ServiceMetadataExtension metadataExtension = service.Extensions.Find<ServiceMetadataExtension>();
                if (null != metadataExtension)
                {
                    Collection<string> metadataStrings = new Collection<string>();
                    try
                    {
                        foreach (MetadataSection section in metadataExtension.Metadata.MetadataSections)
                        {
                            using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
                            {
                                if (section.Metadata is System.Web.Services.Description.ServiceDescription)
                                {
                                    System.Web.Services.Description.ServiceDescription metadata = (System.Web.Services.Description.ServiceDescription)section.Metadata;
                                    metadata.Write(sw);
                                    metadataStrings.Add(sw.ToString());
                                }
                                else if (section.Metadata is System.Xml.XmlElement)
                                {
                                    System.Xml.XmlElement metadata = (System.Xml.XmlElement)section.Metadata;
                                    using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(sw))
                                    {
                                        metadata.WriteTo(xmlWriter);
                                        metadataStrings.Add(sw.ToString());
                                    }
                                }
                                else if (section.Metadata is System.Xml.Schema.XmlSchema)
                                {
                                    System.Xml.Schema.XmlSchema metadata = (System.Xml.Schema.XmlSchema)section.Metadata;
                                    metadata.Write(sw);
                                    metadataStrings.Add(sw.ToString());
                                }
                                else
                                {
                                    metadataStrings.Add(section.Metadata.ToString());
                                }

                            }
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        metadataStrings.Add(e.ToString());
                    }
                    result = new string[metadataStrings.Count];
                    metadataStrings.CopyTo(result, 0);
                }

                return result;
            }
        }
    }
}
