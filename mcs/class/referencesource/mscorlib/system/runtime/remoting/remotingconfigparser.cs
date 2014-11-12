// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
**  File:    RemotingConfigParser.cs
** 
**  Purpose: Parse remoting configuration files.
**
**
===========================================================*/

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Globalization;
using System.Runtime.Versioning;

namespace System.Runtime.Remoting.Activation {    
   

    internal class RemotingXmlConfigFileData
    {
        // debug settings
        internal static volatile bool LoadTypes = false; // indicates whether we should attempt to load types in config files

    
        //
        // configuration entry storage classes (in alphabetical order)
        //   There is one class for each type of entry in a remoting config file.
        //  

        internal class ChannelEntry
        {
            internal String TypeName;
            internal String AssemblyName;
            internal Hashtable Properties;
            internal bool DelayLoad = false;
            internal ArrayList ClientSinkProviders = new ArrayList();
            internal ArrayList ServerSinkProviders = new ArrayList();

            internal ChannelEntry(String typeName, String assemblyName, Hashtable properties)
            {
                TypeName = typeName;
                AssemblyName = assemblyName;
                Properties = properties;
            } // ChannelEntry
        } // class ChannelEntry


        internal class ClientWellKnownEntry
        {
            internal String TypeName;
            internal String AssemblyName;
            internal String Url;

            internal ClientWellKnownEntry(String typeName, String assemName, String url)
            {
                TypeName = typeName;
                AssemblyName = assemName;
                Url = url;
            }
        } // class ClientWellKnownEntry
        

        internal class ContextAttributeEntry
        {
            internal String TypeName;
            internal String AssemblyName;
            internal Hashtable Properties;

            internal ContextAttributeEntry(String typeName, String assemName, Hashtable properties)
            {
                TypeName = typeName;
                AssemblyName = assemName;
                Properties = properties;
            }
        } // class ContextAttributeEntry

        
        internal class InteropXmlElementEntry
        {
            internal String XmlElementName;
            internal String XmlElementNamespace;
            internal String UrtTypeName;
            internal String UrtAssemblyName;

            internal InteropXmlElementEntry(String xmlElementName, String xmlElementNamespace,
                                            String urtTypeName, String urtAssemblyName)
            {
                XmlElementName = xmlElementName;
                XmlElementNamespace = xmlElementNamespace;
                UrtTypeName = urtTypeName;
                UrtAssemblyName = urtAssemblyName;
            }
        } // class InteropXmlElementEntry

        internal class CustomErrorsEntry {
            internal CustomErrorsModes Mode;
            
            internal CustomErrorsEntry(CustomErrorsModes mode) {
                Mode = mode;    
            }
        }

        internal class InteropXmlTypeEntry
        {
            internal String XmlTypeName;
            internal String XmlTypeNamespace;
            internal String UrtTypeName;
            internal String UrtAssemblyName;

            internal InteropXmlTypeEntry(String xmlTypeName, String xmlTypeNamespace,
                                         String urtTypeName, String urtAssemblyName)
            {
                XmlTypeName = xmlTypeName;
                XmlTypeNamespace = xmlTypeNamespace;
                UrtTypeName = urtTypeName;
                UrtAssemblyName = urtAssemblyName;
            }
        } // class InteropXmlTypeEntry
        

        internal class LifetimeEntry
        {
            // If any of these are false, then the corresponding property wasn't specified
            //   in the config file.
            internal bool IsLeaseTimeSet = false;
            internal bool IsRenewOnCallTimeSet = false;
            internal bool IsSponsorshipTimeoutSet = false;
            internal bool IsLeaseManagerPollTimeSet = false;
        
            private TimeSpan _leaseTime;
            private TimeSpan _renewOnCallTime;
            private TimeSpan _sponsorshipTimeout;
            private TimeSpan _leaseManagerPollTime;
        
            internal TimeSpan LeaseTime {
                get 
                {
                    BCLDebug.Assert(IsLeaseTimeSet == true, "LeaseTime not set");
                    return _leaseTime;
                }
                set 
                { 
                    _leaseTime = value;
                    IsLeaseTimeSet = true;
                }
            }
            
            internal TimeSpan RenewOnCallTime {
                get 
                {
                    BCLDebug.Assert(IsRenewOnCallTimeSet == true, "RenewOnCallTime not set");
                    return _renewOnCallTime;
                }
                set 
                { 
                    _renewOnCallTime = value;
                    IsRenewOnCallTimeSet = true;
                }
            }    
            
            internal TimeSpan SponsorshipTimeout {
                get 
                {
                    BCLDebug.Assert(IsSponsorshipTimeoutSet == true, "SponsorShipTimeout not set");
                    return _sponsorshipTimeout;
                }
                set 
                { 
                    _sponsorshipTimeout = value;
                    IsSponsorshipTimeoutSet = true;
                }
            }
            
            internal TimeSpan LeaseManagerPollTime {
                get 
                {
                    BCLDebug.Assert(IsLeaseManagerPollTimeSet == true, "LeaseManagerPollTime not set");
                    return _leaseManagerPollTime;
                }
                set 
                { 
                    _leaseManagerPollTime = value;
                    IsLeaseManagerPollTimeSet = true;
                }
            }
            
        } // class LifetimeEntry


        internal class PreLoadEntry
        {
            // If TypeName is null, then all types in the assembly specified
            //   should be preloaded.
        
            internal String TypeName;
            internal String AssemblyName;

            public PreLoadEntry(String typeName, String assemblyName)
            {
                TypeName = typeName;
                AssemblyName = assemblyName;
            }
        } // class PreLoadEntry


        internal class RemoteAppEntry
        {
            internal String AppUri;

            internal ArrayList WellKnownObjects = new ArrayList();
            internal ArrayList ActivatedObjects = new ArrayList();
            
            internal RemoteAppEntry(String appUri)
            { 
                AppUri = appUri;
            }

            internal void AddWellKnownEntry(String typeName, String assemName, String url)
            {
                ClientWellKnownEntry cwke = new ClientWellKnownEntry(typeName, assemName, url);
                WellKnownObjects.Add(cwke);
            }   

            internal void AddActivatedEntry(String typeName, String assemName,
                                            ArrayList contextAttributes)
            {
                TypeEntry te = new TypeEntry(typeName, assemName, contextAttributes);
                ActivatedObjects.Add(te);
            }
                                                         
        } // class RemoteAppEntry

            
        internal class ServerWellKnownEntry : TypeEntry
        {
            internal String ObjectURI;
            internal WellKnownObjectMode ObjectMode;
            
            internal ServerWellKnownEntry(
                String typeName, String assemName, ArrayList contextAttributes,
                String objURI, WellKnownObjectMode objMode) :
                    base(typeName, assemName, contextAttributes)
            {
                ObjectURI = objURI;
                ObjectMode = objMode;     
            }
        } // class ServerWellKnownEntry


        internal class SinkProviderEntry
        {
            internal String TypeName;
            internal String AssemblyName;
            internal Hashtable Properties;
            internal ArrayList ProviderData = new ArrayList(); // array of SinkProviderData structures
            internal bool IsFormatter; // Is this a formatter sink provider?

            internal SinkProviderEntry(String typeName, String assemName, Hashtable properties,
                                       bool isFormatter)
            {
                TypeName = typeName;
                AssemblyName = assemName;
                Properties = properties;
                IsFormatter = isFormatter;
            }
        } // class SinkProviderEntry


        internal class TypeEntry
        {
            internal String TypeName;
            internal String AssemblyName;
            internal ArrayList ContextAttributes;
            
            internal TypeEntry(String typeName, String assemName,
                               ArrayList contextAttributes)
            {
                TypeName = typeName;
                AssemblyName = assemName;
                ContextAttributes = contextAttributes;
            }
        } // class TypeEntry


        //
        // end of configuration entry storage classes
        //


        //
        // configuration data access
        //

        internal String ApplicationName = null;  // application name
        internal LifetimeEntry Lifetime = null;  // corresponds to top-level lifetime element
        internal bool UrlObjRefMode = RemotingConfigHandler.UrlObjRefMode;  // should url obj ref's be used?
        internal CustomErrorsEntry CustomErrors = null;

        internal ArrayList ChannelEntries = new ArrayList();
        internal ArrayList InteropXmlElementEntries = new ArrayList();
        internal ArrayList InteropXmlTypeEntries = new ArrayList();
        internal ArrayList PreLoadEntries = new ArrayList();
        internal ArrayList RemoteAppEntries = new ArrayList();
        internal ArrayList ServerActivatedEntries = new ArrayList();
        internal ArrayList ServerWellKnownEntries = new ArrayList();

        
        //
        // end of configuration data access
        //

        
        //
        // modify configuration data (for multiple entry entities)
        //

        internal void AddInteropXmlElementEntry(String xmlElementName, String xmlElementNamespace,
                                                String urtTypeName, String urtAssemblyName)
        {
            TryToLoadTypeIfApplicable(urtTypeName, urtAssemblyName);
            InteropXmlElementEntry ixee = new InteropXmlElementEntry(
                xmlElementName, xmlElementNamespace, urtTypeName, urtAssemblyName);
            InteropXmlElementEntries.Add(ixee);
        }

        internal void AddInteropXmlTypeEntry(String xmlTypeName, String xmlTypeNamespace,
                                             String urtTypeName, String urtAssemblyName)
        {
            TryToLoadTypeIfApplicable(urtTypeName, urtAssemblyName);
            InteropXmlTypeEntry ixte = new InteropXmlTypeEntry(xmlTypeName, xmlTypeNamespace,
                                                               urtTypeName, urtAssemblyName);
            InteropXmlTypeEntries.Add(ixte);
        }

        internal void AddPreLoadEntry(String typeName, String assemblyName)
        {
            TryToLoadTypeIfApplicable(typeName, assemblyName);
            PreLoadEntry ple = new PreLoadEntry(typeName, assemblyName);
            PreLoadEntries.Add(ple);                                                
        }

        internal RemoteAppEntry AddRemoteAppEntry(String appUri)
        {
            RemoteAppEntry rae = new RemoteAppEntry(appUri);
            RemoteAppEntries.Add(rae);
            return rae;
        }

        internal void AddServerActivatedEntry(String typeName, String assemName,
                                              ArrayList contextAttributes)
        {
            TryToLoadTypeIfApplicable(typeName, assemName);
            TypeEntry te = new TypeEntry(typeName, assemName, contextAttributes);
            ServerActivatedEntries.Add(te);
        } 

        internal ServerWellKnownEntry AddServerWellKnownEntry(String typeName, String assemName,
            ArrayList contextAttributes, String objURI, WellKnownObjectMode objMode)
        {
            TryToLoadTypeIfApplicable(typeName, assemName);
            ServerWellKnownEntry swke = new ServerWellKnownEntry(typeName, assemName,
                contextAttributes, objURI, objMode);
            ServerWellKnownEntries.Add(swke);
            return swke;
        }    
        

        // debug settings helper
        private void TryToLoadTypeIfApplicable(String typeName, String assemblyName)
        {
            if (!LoadTypes)
                return;
        
            Assembly asm = Assembly.Load(assemblyName);
            if (asm == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_AssemblyLoadFailed",
                    assemblyName));                    
            }

            Type type = asm.GetType(typeName, false, false);
            if (type == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_BadType",
                    typeName));     
            }
        }        
    
    } // RemotingXmlConfigFileData




    internal static class RemotingXmlConfigFileParser
    {
        // template arrays
        private static Hashtable _channelTemplates = CreateSyncCaseInsensitiveHashtable();
        private static Hashtable _clientChannelSinkTemplates = CreateSyncCaseInsensitiveHashtable();
        private static Hashtable _serverChannelSinkTemplates = CreateSyncCaseInsensitiveHashtable();

        
        private static Hashtable CreateSyncCaseInsensitiveHashtable()
        {
            return Hashtable.Synchronized(CreateCaseInsensitiveHashtable());
        }

        private static Hashtable CreateCaseInsensitiveHashtable()
        {
            return new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        }


        public static RemotingXmlConfigFileData ParseDefaultConfiguration() {
            ConfigNode node;
            
            // <system.runtime.remoting>
            ConfigNode rootNode = new ConfigNode("system.runtime.remoting", null);

            /*
            <application>
                <channels>
                    <channel ref="http client" displayName="http client (delay loaded)" delayLoadAsClientChannel="true" />
                    <channel ref="tcp client" displayName="tcp client (delay loaded)" delayLoadAsClientChannel="true" />
                    <channel ref="ipc client" displayName="ipc client (delay loaded)" delayLoadAsClientChannel="true" />
                </channels>
            </application>
            */
            ConfigNode appNode = new ConfigNode("application", rootNode);
            rootNode.Children.Add(appNode);
            
            ConfigNode channelsNode = new ConfigNode("channels", appNode);
            appNode.Children.Add(channelsNode);

            node = new ConfigNode("channel", appNode);
            node.Attributes.Add(new DictionaryEntry("ref", "http client"));
            node.Attributes.Add(new DictionaryEntry("displayName", "http client (delay loaded)"));
            node.Attributes.Add(new DictionaryEntry("delayLoadAsClientChannel", "true"));
            channelsNode.Children.Add(node);

            node = new ConfigNode("channel", appNode);
            node.Attributes.Add(new DictionaryEntry("ref", "tcp client"));
            node.Attributes.Add(new DictionaryEntry("displayName", "tcp client (delay loaded)"));
            node.Attributes.Add(new DictionaryEntry("delayLoadAsClientChannel", "true"));
            channelsNode.Children.Add(node);
            
            node = new ConfigNode("channel", appNode);
            node.Attributes.Add(new DictionaryEntry("ref", "ipc client"));
            node.Attributes.Add(new DictionaryEntry("displayName", "ipc client (delay loaded)"));
            node.Attributes.Add(new DictionaryEntry("delayLoadAsClientChannel", "true"));
            channelsNode.Children.Add(node);

            /*
            <channels>
                <channel id="http" type="System.Runtime.Remoting.Channels.Http.HttpChannel, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                <channel id="http client" type="System.Runtime.Remoting.Channels.Http.HttpClientChannel, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                <channel id="http server" type="System.Runtime.Remoting.Channels.Http.HttpServerChannel, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                <channel id="tcp" type="System.Runtime.Remoting.Channels.Tcp.TcpChannel, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                <channel id="tcp client" type="System.Runtime.Remoting.Channels.Tcp.TcpClientChannel, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                <channel id="tcp server" type="System.Runtime.Remoting.Channels.Tcp.TcpServerChannel, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                <channel id="ipc" type="System.Runtime.Remoting.Channels.Ipc.IpcChannel, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                <channel id="ipc client" type="System.Runtime.Remoting.Channels.Ipc.IpcClientChannel, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                <channel id="ipc server" type="System.Runtime.Remoting.Channels.Ipc.IpcServerChannel, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
            </channels>
            */
            channelsNode = new ConfigNode("channels", rootNode);
            rootNode.Children.Add(channelsNode);

            node = new ConfigNode("channel", channelsNode);
            node.Attributes.Add(new DictionaryEntry("id", "http"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Http.HttpChannel, " + AssemblyRef.SystemRuntimeRemoting));
            channelsNode.Children.Add(node);

            node = new ConfigNode("channel", channelsNode);
            node.Attributes.Add(new DictionaryEntry("id", "http client"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Http.HttpClientChannel, " + AssemblyRef.SystemRuntimeRemoting));
            channelsNode.Children.Add(node);

            node = new ConfigNode("channel", channelsNode);
            node.Attributes.Add(new DictionaryEntry("id", "http server"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Http.HttpServerChannel, " + AssemblyRef.SystemRuntimeRemoting));
            channelsNode.Children.Add(node);

            node = new ConfigNode("channel", channelsNode);
            node.Attributes.Add(new DictionaryEntry("id", "tcp"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Tcp.TcpChannel, " + AssemblyRef.SystemRuntimeRemoting));
            channelsNode.Children.Add(node);

            node = new ConfigNode("channel", channelsNode);
            node.Attributes.Add(new DictionaryEntry("id", "tcp client"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Tcp.TcpClientChannel, " + AssemblyRef.SystemRuntimeRemoting));
            channelsNode.Children.Add(node);

            node = new ConfigNode("channel", channelsNode);
            node.Attributes.Add(new DictionaryEntry("id", "tcp server"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Tcp.TcpServerChannel, " + AssemblyRef.SystemRuntimeRemoting));
            channelsNode.Children.Add(node);
            
            node = new ConfigNode("channel", channelsNode);
            node.Attributes.Add(new DictionaryEntry("id", "ipc"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Ipc.IpcChannel, " + AssemblyRef.SystemRuntimeRemoting));
            channelsNode.Children.Add(node);

            node = new ConfigNode("channel", channelsNode);
            node.Attributes.Add(new DictionaryEntry("id", "ipc client"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Ipc.IpcClientChannel, " + AssemblyRef.SystemRuntimeRemoting));
            channelsNode.Children.Add(node);

            node = new ConfigNode("channel", channelsNode);
            node.Attributes.Add(new DictionaryEntry("id", "ipc server"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Ipc.IpcServerChannel, " + AssemblyRef.SystemRuntimeRemoting));
            channelsNode.Children.Add(node);
            
            /*
            <channelSinkProviders>
                <clientProviders>
                    <formatter id="soap" type="System.Runtime.Remoting.Channels.SoapClientFormatterSinkProvider, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                    <formatter id="binary" type="System.Runtime.Remoting.Channels.BinaryClientFormatterSinkProvider, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                </clientProviders>
                <serverProviders>
                    <formatter id="soap" type="System.Runtime.Remoting.Channels.SoapServerFormatterSinkProvider, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                    <formatter id="binary" type="System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                    <provider id="wsdl" type="System.Runtime.Remoting.MetadataServices.SdlChannelSinkProvider, System.Runtime.Remoting, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
                </serverProviders>
            </channelSinkProviders>
            */
            ConfigNode channelsSinkNode = new ConfigNode("channelSinkProviders", rootNode);
            rootNode.Children.Add(channelsSinkNode);

            ConfigNode clientProvidersNode = new ConfigNode("clientProviders", channelsSinkNode);
            channelsSinkNode.Children.Add(clientProvidersNode);

            node = new ConfigNode("formatter", clientProvidersNode);
            node.Attributes.Add(new DictionaryEntry("id", "soap"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.SoapClientFormatterSinkProvider, " + AssemblyRef.SystemRuntimeRemoting));
            clientProvidersNode.Children.Add(node);
            
            node = new ConfigNode("formatter", clientProvidersNode);
            node.Attributes.Add(new DictionaryEntry("id", "binary"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.BinaryClientFormatterSinkProvider, " + AssemblyRef.SystemRuntimeRemoting));
            clientProvidersNode.Children.Add(node);

            ConfigNode serverProvidersNode = new ConfigNode("serverProviders", channelsSinkNode);
            channelsSinkNode.Children.Add(serverProvidersNode);

            node = new ConfigNode("formatter", serverProvidersNode);
            node.Attributes.Add(new DictionaryEntry("id", "soap"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.SoapServerFormatterSinkProvider, " + AssemblyRef.SystemRuntimeRemoting));
            serverProvidersNode.Children.Add(node);
            
            node = new ConfigNode("formatter", serverProvidersNode);
            node.Attributes.Add(new DictionaryEntry("id", "binary"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider, " + AssemblyRef.SystemRuntimeRemoting));
            serverProvidersNode.Children.Add(node);

            node = new ConfigNode("provider", serverProvidersNode);
            node.Attributes.Add(new DictionaryEntry("id", "wsdl"));
            node.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.MetadataServices.SdlChannelSinkProvider, " + AssemblyRef.SystemRuntimeRemoting));
            serverProvidersNode.Children.Add(node);

            return ParseConfigNode(rootNode);
        }


        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static RemotingXmlConfigFileData ParseConfigFile(String filename)
        {
            ConfigTreeParser parser = new ConfigTreeParser();
            ConfigNode rootNode = parser.Parse(filename, "/configuration/system.runtime.remoting");

            return ParseConfigNode(rootNode);
        }

        private static RemotingXmlConfigFileData ParseConfigNode(ConfigNode rootNode)
        {
            RemotingXmlConfigFileData configData = new RemotingXmlConfigFileData();
        
            // check to see if this file has a system.runtime.remoting section
            if (rootNode == null)
                return null;

            // process attributes
            foreach (DictionaryEntry entry in rootNode.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {
                case "version":
                {
                    // we ignore the version attribute because this may be used
                    //   by the configuration system
                    break;
                }                
                    
                default: break;
                } // switch
            } // foreach

            ConfigNode appNode = null;       // "application" node
            ConfigNode channelsNode = null;  // "channels" node
            ConfigNode providerNode = null;  // "channelSinkProviders" node
            ConfigNode debugNode = null;     // "debug" node
            ConfigNode customErrorsNode = null;     // "customErrors" node
                        
            foreach (ConfigNode node in rootNode.Children)
            {
                switch (node.Name)
                {
                
                case "application":
                {
                    // there can only be one application node in a config file
                    if (appNode != null)
                        ReportUniqueSectionError(rootNode, appNode, configData);

                    appNode = node;
                    break;
                } // case "application"
                
                case "channels":
                {
                    if (channelsNode != null)
                        ReportUniqueSectionError(rootNode, channelsNode, configData);
                
                    channelsNode = node;
                    break;
                } // case "channels"

                case "channelSinkProviders":
                {
                    if (providerNode != null)
                        ReportUniqueSectionError(rootNode, providerNode, configData);
                
                    providerNode = node;
                    break;
                } // case "channelSinkProviders"

                case "debug":
                {
                    if (debugNode != null)
                        ReportUniqueSectionError(rootNode, debugNode, configData);
                
                    debugNode = node;
                    break;
                } // case "debug"
                
                case "customErrors":
                {
                    if (customErrorsNode != null)
                        ReportUniqueSectionError(rootNode, customErrorsNode, configData);
                
                    customErrorsNode = node;
                    break;
                }// case "customErrors"
                
                default: break;
                } // switch
            } // foreach


            if (debugNode != null)
                ProcessDebugNode(debugNode, configData);

            if (providerNode != null)
                ProcessChannelSinkProviderTemplates(providerNode, configData);

            if (channelsNode != null)
                ProcessChannelTemplates(channelsNode, configData);

            if (appNode != null)
                ProcessApplicationNode(appNode, configData);
            
            if (customErrorsNode != null)
                ProcessCustomErrorsNode(customErrorsNode, configData);                

            return configData;
        } // ParseConfigFile


        private static void ReportError(String errorStr, RemotingXmlConfigFileData configData)
        {
            // <STRIP>NOTE: In the future, this might log all errors to the configData object
            //   instead of throwing immediately.</STRIP>
        
            throw new RemotingException(errorStr);
        } // ReportError

        // means section must be unique
        private static void ReportUniqueSectionError(ConfigNode parent, ConfigNode child,
                                                     RemotingXmlConfigFileData configData)
        {
            ReportError(
                String.Format(
                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_NodeMustBeUnique"),
                    child.Name, parent.Name),
                configData);
        } // ReportUniqueSectionError

        private static void ReportUnknownValueError(ConfigNode node, String value,
                                                        RemotingXmlConfigFileData configData)
        {
            ReportError(
                String.Format(
                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_UnknownValue"),
                    node.Name, value),
                configData);
        } // ReportUnknownValueError

        private static void ReportMissingAttributeError(ConfigNode node, String attributeName,
                                                        RemotingXmlConfigFileData configData)
        {
            ReportMissingAttributeError(node.Name, attributeName, configData);
        } // ReportMissingAttributeError

        private static void ReportMissingAttributeError(String nodeDescription, String attributeName,
                                                        RemotingXmlConfigFileData configData)
        {
            ReportError(
                String.Format(
                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_RequiredXmlAttribute"),
                    nodeDescription, attributeName),
                configData);
        } // ReportMissingAttributeError

        private static void ReportMissingTypeAttributeError(ConfigNode node, String attributeName,
                                                            RemotingXmlConfigFileData configData)
        {
            ReportError(
                String.Format(
                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_MissingTypeAttribute"),
                    node.Name, attributeName),
                configData);
        } // ReportMissingAttributeError

        private static void ReportMissingXmlTypeAttributeError(ConfigNode node, String attributeName,
                                                               RemotingXmlConfigFileData configData)
        {
            ReportError(
                String.Format(
                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_MissingXmlTypeAttribute"),
                    node.Name, attributeName),
                configData);
        } // ReportMissingAttributeError

        private static void ReportInvalidTimeFormatError(String time,
                                                         RemotingXmlConfigFileData configData)
        {
            ReportError(
                String.Format(
                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_InvalidTimeFormat"),
                    time),
                configData);
        } // ReportInvalidTypeFormatError

        // If nodes can be represented as a template, only a template version
        //   can have an 'id' attribute
        private static void ReportNonTemplateIdAttributeError(ConfigNode node, 
                                                              RemotingXmlConfigFileData configData)
        {
            ReportError(
                String.Format(
                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_NonTemplateIdAttribute"),
                    node.Name),
                configData);
        } // ReportNonTemplateIdAttributeError

        private static void ReportTemplateCannotReferenceTemplateError(
            ConfigNode node, 
            RemotingXmlConfigFileData configData)
        {
            ReportError(
                String.Format(
                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_TemplateCannotReferenceTemplate"),
                    node.Name),
                configData);
        } // ReportTemplateCannotReferenceTemplateError

        private static void ReportUnableToResolveTemplateReferenceError(
            ConfigNode node, String referenceName, 
            RemotingXmlConfigFileData configData)
        {
            ReportError(
                String.Format(
                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_UnableToResolveTemplate"),
                    node.Name, referenceName),
                configData);
        } // ReportUnableToResolveTemplateReferenceError

        private static void ReportAssemblyVersionInfoPresent(
            String assemName, String entryDescription,
            RemotingXmlConfigFileData configData)
        {
            // for some entries, version information is not allowed in the assembly name
            ReportError(
                String.Format(
                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_VersionPresent"),
                    assemName, entryDescription),
                configData);
        } // ReportAssemblyVersionInfoPresent
       

        private static void ProcessDebugNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {
                case "loadTypes":
                    RemotingXmlConfigFileData.LoadTypes = 
                        Convert.ToBoolean((String)entry.Value, CultureInfo.InvariantCulture);
                    break;
                    
                default: break;
                } // switch
            } // foreach
            
        } // ProcessDebugNode
        

        private static void ProcessApplicationNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                if (key.Equals("name"))
                    configData.ApplicationName = (String)entry.Value;
            }
        
            foreach (ConfigNode childNode in node.Children)
            {
                switch (childNode.Name)
                {
                case "channels": ProcessChannelsNode(childNode, configData); break;
                case "client": ProcessClientNode(childNode, configData); break;                                       
                case "lifetime": ProcessLifetimeNode(node, childNode, configData); break;
                case "service": ProcessServiceNode(childNode, configData); break;
                case "soapInterop": ProcessSoapInteropNode(childNode, configData); break;         

                default: break;
                } // switch
            } // foreach
        } // ProcessApplicationNode

        private static void ProcessCustomErrorsNode(ConfigNode node, RemotingXmlConfigFileData configData) {
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                if (key.Equals("mode")) {
                    string value = (string)entry.Value;
                    CustomErrorsModes mode = CustomErrorsModes.On;
                     
                    if (String.Compare(value, "on", StringComparison.OrdinalIgnoreCase) == 0)                                        
                        mode = CustomErrorsModes.On; 
                    else if (String.Compare(value, "off", StringComparison.OrdinalIgnoreCase) == 0)
                        mode = CustomErrorsModes.Off; 
                    else if (String.Compare(value, "remoteonly", StringComparison.OrdinalIgnoreCase) == 0)
                        mode = CustomErrorsModes.RemoteOnly; 
                    else
                        ReportUnknownValueError(node, value, configData);
                    
                    configData.CustomErrors = new RemotingXmlConfigFileData.CustomErrorsEntry(mode);
                }                                                        
            }
                        
        }

        private static void ProcessLifetimeNode(ConfigNode parentNode, ConfigNode node, RemotingXmlConfigFileData configData)
        {
            if (configData.Lifetime != null)
                ReportUniqueSectionError(node, parentNode, configData);
        
            configData.Lifetime = new RemotingXmlConfigFileData.LifetimeEntry();

            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {
                
                case "leaseTime": 
                    configData.Lifetime.LeaseTime = ParseTime((String)entry.Value, configData);
                    break;
                    
                case "sponsorshipTimeout":
                    configData.Lifetime.SponsorshipTimeout = ParseTime((String)entry.Value, configData);
                    break;

                case "renewOnCallTime":
                    configData.Lifetime.RenewOnCallTime = ParseTime((String)entry.Value, configData);
                    break;

                case "leaseManagerPollTime":
                    configData.Lifetime.LeaseManagerPollTime = ParseTime((String)entry.Value, configData);
                    break;

                default: break;
                
                } // switch
            } // foreach
            
        } // ProcessLifetimeNode


        // appears under "application"
        private static void ProcessServiceNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {       
            foreach (ConfigNode childNode in node.Children)
            {
                switch (childNode.Name)
                {
                case "wellknown": ProcessServiceWellKnownNode(childNode, configData); break;
                case "activated": ProcessServiceActivatedNode(childNode, configData); break;

                default: break;
                } // switch
            } // foreach
        } // ProcessServiceNode


        // appears under "application"
        private static void ProcessClientNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            String remoteAppUri = null;

            // process attributes
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {
                case "url": remoteAppUri = (String)entry.Value; break;

                case "displayName": break; // displayName is ignored (used by config utility for labelling the application)

                default: break;
                } // switch
            } // foreach attribute

            RemotingXmlConfigFileData.RemoteAppEntry remoteApp =
                configData.AddRemoteAppEntry(remoteAppUri);            

            // process child nodes
            foreach (ConfigNode childNode in node.Children)
            {
                switch (childNode.Name)
                {
                case "wellknown": ProcessClientWellKnownNode(childNode, configData, remoteApp); break;
                case "activated": ProcessClientActivatedNode(childNode, configData, remoteApp); break;

                default: break;
                } // switch
            } // foreach child node


            // if there are any activated entries, we require a remote app url.
            if ((remoteApp.ActivatedObjects.Count > 0) && (remoteAppUri == null))
                ReportMissingAttributeError(node, "url", configData);
        } // ProcessClientNode


        // appears under "application"
        private static void ProcessSoapInteropNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            // process attributes
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {
                case "urlObjRef":
                {
                    configData.UrlObjRefMode = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                    break;
                }
                
                default: break;
                } // switch
            } // foreach attribute
        
            foreach (ConfigNode childNode in node.Children)
            {
                switch (childNode.Name)
                {
                case "preLoad": ProcessPreLoadNode(childNode, configData); break;
                case "interopXmlElement": ProcessInteropXmlElementNode(childNode, configData); break;
                case "interopXmlType": ProcessInteropXmlTypeNode(childNode, configData); break;

                default: break;
                } // switch
            }
        } // ProcessSoapInteropNode


        // appears under "application"
        private static void ProcessChannelsNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            foreach (ConfigNode childNode in node.Children)
            {
                if (childNode.Name.Equals("channel"))
                {
                    RemotingXmlConfigFileData.ChannelEntry channelEntry =
                        ProcessChannelsChannelNode(childNode, configData, false);
                    configData.ChannelEntries.Add(channelEntry);
                }
            } // foreach
        } // ProcessInteropNode


        // appears under "application/service"
        private static void ProcessServiceWellKnownNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            String typeName = null;
            String assemName = null;
            ArrayList contextAttributes = new ArrayList();
            
            String objectURI = null;
            
            WellKnownObjectMode objectMode = WellKnownObjectMode.Singleton;
            bool objectModeFound = false;

            // examine attributes
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {
                case "displayName": break; // displayName is ignored (used by config utility for labelling the application)
                
                case "mode":
                {
                    String value = (String)entry.Value;
                    objectModeFound = true;
                    if (String.CompareOrdinal(value, "Singleton") == 0)
                        objectMode = WellKnownObjectMode.Singleton;
                    else
                    if (String.CompareOrdinal(value, "SingleCall") == 0)
                        objectMode = WellKnownObjectMode.SingleCall;
                    else
                        objectModeFound = false;
                    break;
                } // case "mode"

                case "objectUri": objectURI = (String)entry.Value; break;

                case "type":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out typeName, out assemName);
                    break;
                } // case "type"


                default: break;
                } // switch
            } // foreach

            // examine child nodes
            foreach (ConfigNode childNode in node.Children)
            {
                switch (childNode.Name)
                {
                case "contextAttribute":
                {
                    contextAttributes.Add(ProcessContextAttributeNode(childNode, configData));
                    break;
                } // case "contextAttribute"

                case "lifetime":
                {
                    // <
                     break;
                } // case "lifetime"


                default: break;

                } // switch
            } // foreach child node
            

            // check for errors
            if (!objectModeFound)
            {
                ReportError(
                    Environment.GetResourceString("Remoting_Config_MissingWellKnownModeAttribute"),
                    configData);
            }                   
            
            if ((typeName == null) || (assemName == null))
                ReportMissingTypeAttributeError(node, "type", configData);


            // objectURI defaults to typeName if not specified
            if (objectURI == null)
                objectURI = typeName + ".soap";

            configData.AddServerWellKnownEntry(typeName, assemName, contextAttributes,
                objectURI, objectMode);
        } // ProcessServiceWellKnownNode


        // appears under "application/service"
        private static void ProcessServiceActivatedNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            String typeName = null;
            String assemName = null;
            ArrayList contextAttributes = new ArrayList();
        
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {
                case "type":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out typeName, out assemName);
                    break;
                } // case "type" 

                default: break;
                } // switch
            } // foreach attribute


            foreach (ConfigNode childNode in node.Children)
            {
                switch (childNode.Name)
                {
                case "contextAttribute":
                {
                    contextAttributes.Add(ProcessContextAttributeNode(childNode, configData));
                    break;
                } // case "contextattribute"

                case "lifetime":
                {
                    // <
                    break;
                } // case "lifetime"

                default: break;

                } // switch
            } // foreach child node

            // check for errors 
            if ((typeName == null) || (assemName == null))
                ReportMissingTypeAttributeError(node, "type", configData);

            if (CheckAssemblyNameForVersionInfo(assemName))
                ReportAssemblyVersionInfoPresent(assemName, "service activated", configData);
            
            configData.AddServerActivatedEntry(typeName, assemName, contextAttributes);
        } // ProcessServiceActivatedNode


        // appears under "application/client"
        private static void ProcessClientWellKnownNode(ConfigNode node, RemotingXmlConfigFileData configData,
            RemotingXmlConfigFileData.RemoteAppEntry remoteApp)
        {
            String typeName = null;
            String assemName = null;
            String url = null;

            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {                
                case "displayName": break; // displayName is ignored (used by config utility for labelling the application)
                
                case "type":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out typeName, out assemName);
                    break;
                } // case "type" 

                case "url": url = (String)entry.Value; break;

                default: break;
                } // switch
            } // foreach

            // check for errors    
            if (url == null)
                ReportMissingAttributeError("WellKnown client", "url", configData);

            if ((typeName == null) || (assemName == null))
                ReportMissingTypeAttributeError(node, "type", configData);

            if (CheckAssemblyNameForVersionInfo(assemName))
                ReportAssemblyVersionInfoPresent(assemName, "client wellknown", configData);

            remoteApp.AddWellKnownEntry(typeName, assemName, url);
        } // ProcessClientWellKnownNode


        // appears under "application/client"
        private static void ProcessClientActivatedNode(ConfigNode node, RemotingXmlConfigFileData configData,
            RemotingXmlConfigFileData.RemoteAppEntry remoteApp)
        {
            String typeName = null;
            String assemName = null;
            ArrayList contextAttributes = new ArrayList();
        
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {
                case "type":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out typeName, out assemName);
                    break;
                } // case "type" 

                default: break;
                } // switch
            } // foreach

            foreach (ConfigNode childNode in node.Children)
            {
                switch (childNode.Name)
                {
                case "contextAttribute":
                {
                    contextAttributes.Add(ProcessContextAttributeNode(childNode, configData));
                    break;
                } // case "contextAttribute"

                default: break;
                } // switch
            } // foreach child node

            // check for errors
            if ((typeName == null) || (assemName == null))
                ReportMissingTypeAttributeError(node, "type", configData);

            remoteApp.AddActivatedEntry(typeName, assemName, contextAttributes);
        } // ProcessClientActivatedNode


        private static void ProcessInteropXmlElementNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            String xmlElementName = null;
            String xmlElementNamespace = null;
            String urtTypeName = null;
            String urtAssemName = null;
        
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {
                case "xml":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out xmlElementName, out xmlElementNamespace);
                    break;
                }
                
                case "clr":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out urtTypeName, out urtAssemName);
                    break;
                } // case "clr" 

                default: break;
                } // switch
            } // foreach

            // check for errors   
            if ((xmlElementName == null) || (xmlElementNamespace == null))
                ReportMissingXmlTypeAttributeError(node, "xml", configData);

            if ((urtTypeName == null) || (urtAssemName == null))
                ReportMissingTypeAttributeError(node, "clr", configData);
            
            configData.AddInteropXmlElementEntry(xmlElementName, xmlElementNamespace,
                                                 urtTypeName, urtAssemName);
        } // ProcessInteropNode


        private static void ProcessInteropXmlTypeNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            String xmlTypeName = null;
            String xmlTypeNamespace = null;
            String urtTypeName = null;
            String urtAssemName = null;
        
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String key = entry.Key.ToString();
                switch (key)
                {
                case "xml":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out xmlTypeName, out xmlTypeNamespace);
                    break;
                }
                
                case "clr":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out urtTypeName, out urtAssemName);
                    break;
                } // case "type" 

                default: break;
                } // switch
            } // foreach

            // check for errors   
            if ((xmlTypeName == null) || (xmlTypeNamespace == null))
                ReportMissingXmlTypeAttributeError(node, "xml", configData);

            if ((urtTypeName == null) || (urtAssemName == null))
                ReportMissingTypeAttributeError(node, "clr", configData);
            
            configData.AddInteropXmlTypeEntry(xmlTypeName, xmlTypeNamespace,
                                              urtTypeName, urtAssemName);
        } // ProcessInteropNode


        private static void ProcessPreLoadNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            String typeName = null;
            String assemblyName = null;
        
            foreach (DictionaryEntry entry in node.Attributes)
            {   
                String key = entry.Key.ToString();
                switch (key)
                {
                case "type":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out typeName, out assemblyName);
                    break;
                }
                
                case "assembly":
                {   
                    assemblyName = (String)entry.Value;
                    break;
                } // case "type" 

                default: break;
                } // switch
            } // foreach

            // check for errors   
            if (assemblyName == null)
            {
                ReportError(
                    Environment.GetResourceString("Remoting_Config_PreloadRequiresTypeOrAssembly"),
                    configData);
            }
            
            configData.AddPreLoadEntry(typeName, assemblyName);
        } // ProcessPreLoadNode



        private static RemotingXmlConfigFileData.ContextAttributeEntry
        ProcessContextAttributeNode(ConfigNode node, RemotingXmlConfigFileData configData)
        {
            String typeName = null;
            String assemName = null;
            Hashtable properties = CreateCaseInsensitiveHashtable();

            // examine attributes
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String lowercaseKey = ((String)entry.Key).ToLower(CultureInfo.InvariantCulture);
                switch (lowercaseKey)
                {
                
                case "type":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out typeName, out assemName);
                    break;
                } // case "type" 

                default:
                    properties[lowercaseKey] = entry.Value;
                    break;
                } // switch
            } // foreach attribute

            // check for errors        
            if ((typeName == null) || (assemName == null))
                 ReportMissingTypeAttributeError(node, "type", configData);

            RemotingXmlConfigFileData.ContextAttributeEntry attributeEntry =
                new RemotingXmlConfigFileData.ContextAttributeEntry(
                    typeName, assemName, properties);

            return attributeEntry;          
        } // ProcessContextAttributeNode
        

        // appears under "application/client"
        private static RemotingXmlConfigFileData.ChannelEntry
        ProcessChannelsChannelNode(ConfigNode node, RemotingXmlConfigFileData configData,
                                   bool isTemplate)
        {
            String id = null;
            String typeName = null;
            String assemName = null;
            Hashtable properties = CreateCaseInsensitiveHashtable();

            bool delayLoad = false;
            
            RemotingXmlConfigFileData.ChannelEntry channelTemplate = null;

            // examine attributes
            foreach (DictionaryEntry entry in node.Attributes)
            {
                String keyStr = (String)entry.Key;
                switch (keyStr)
                {
                case "displayName": break; // displayName is ignored (used by config utility for labelling the application)
                
                case "id":
                {
                    if (!isTemplate)
                    {
                        ReportNonTemplateIdAttributeError(node, configData);
                    }
                    else
                        id = ((String)entry.Value).ToLower(CultureInfo.InvariantCulture);

                    break;
                } // case "id"
                
                case "ref":
                {
                    if (isTemplate)
                    {
                        ReportTemplateCannotReferenceTemplateError(node, configData);
                    }
                    else
                    {
                        channelTemplate =
                            (RemotingXmlConfigFileData.ChannelEntry)_channelTemplates[entry.Value];
                        if (channelTemplate == null)
                        {
                            ReportUnableToResolveTemplateReferenceError(
                                node, entry.Value.ToString(), configData);
                        }
                        else
                        {
                            // load template data
                            typeName = channelTemplate.TypeName;
                            assemName = channelTemplate.AssemblyName;

                            foreach (DictionaryEntry param in channelTemplate.Properties)
                            {
                                properties[param.Key] = param.Value;
                            }
                        }
                    }

                    break;
                } // case "ref"
                
                case "type":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out typeName, out assemName);
                    break;
                } // case "type" 

                case "delayLoadAsClientChannel":
                {
                    delayLoad = Convert.ToBoolean((String)entry.Value, CultureInfo.InvariantCulture);
                    break;
                } // case "delayLoadAsClientChannel"

                default:
                    properties[keyStr] = entry.Value;
                    break;
                } // switch
            } // foreach attribute

            // check for errors        
            if ((typeName == null) || (assemName == null))
                ReportMissingTypeAttributeError(node, "type", configData);          

            RemotingXmlConfigFileData.ChannelEntry channelEntry =
                new RemotingXmlConfigFileData.ChannelEntry(typeName, assemName, properties);

            channelEntry.DelayLoad = delayLoad;                                                            


            // look for sink providers
            foreach (ConfigNode childNode in node.Children)
            {
                switch (childNode.Name)
                {
                case "clientProviders": 
                    ProcessSinkProviderNodes(childNode, channelEntry, configData, false); 
                    break;
                case "serverProviders": 
                    ProcessSinkProviderNodes(childNode, channelEntry, configData, true);
                    break;

                 default: break;
                } // switch              
            } // foreach

            // if we reference a template and didn't specify any sink providers, we
            //   should copy over the providers from the template
            if (channelTemplate != null)
            {                
                // <

                if (channelEntry.ClientSinkProviders.Count == 0)
                {
                    channelEntry.ClientSinkProviders = channelTemplate.ClientSinkProviders;
                }
                if (channelEntry.ServerSinkProviders.Count == 0)
                {
                    channelEntry.ServerSinkProviders = channelTemplate.ServerSinkProviders;
                }
            }
            

            if (isTemplate)
            {
                _channelTemplates[id] = channelEntry;
                return null;
            }
            else
            {
                return channelEntry;
            }
        } // ProcessChannelsChannelNode





        //
        // process sink provider data
        //

        private static void ProcessSinkProviderNodes(ConfigNode node,
            RemotingXmlConfigFileData.ChannelEntry channelEntry, RemotingXmlConfigFileData configData,
            bool isServer)
        {
            // look for sink providers
            foreach (ConfigNode childNode in node.Children)
            {
                RemotingXmlConfigFileData.SinkProviderEntry entry = 
                    ProcessSinkProviderNode(childNode, configData, false, isServer);
                if (isServer)
                    channelEntry.ServerSinkProviders.Add(entry);
                else
                    channelEntry.ClientSinkProviders.Add(entry);
            } // foreach
            
        } // ProcessSinkProviderNodes


        private static RemotingXmlConfigFileData.SinkProviderEntry 
        ProcessSinkProviderNode(ConfigNode node, RemotingXmlConfigFileData configData,
                                bool isTemplate, bool isServer)
        {
            bool isFormatter = false;

            // Make sure the node is a "formatter" or "provider".
            String nodeName = node.Name;
            if (nodeName.Equals("formatter"))
                isFormatter = true;
            else
            if (nodeName.Equals("provider"))
                isFormatter = false;
            else
            {
                ReportError(
                    Environment.GetResourceString("Remoting_Config_ProviderNeedsElementName"),
                    configData);                    
            }

            String id = null;
            String typeName = null;
            String assemName = null;
            Hashtable properties = CreateCaseInsensitiveHashtable();

            RemotingXmlConfigFileData.SinkProviderEntry template = null;


            foreach (DictionaryEntry entry in node.Attributes)
            {
                String keyStr = (String)entry.Key;
                switch (keyStr)
                {
                case "id":
                {
                    if (!isTemplate)
                    {
                        // only templates can have the id attribute
                        ReportNonTemplateIdAttributeError(node, configData);
                    }
                    else
                        id = (String)entry.Value;

                    break;
                } // case "id"

                case "ref":
                {
                    if (isTemplate)
                    {
                        ReportTemplateCannotReferenceTemplateError(node, configData);
                    }
                    else
                    {
                        if (isServer)
                        {
                            template = (RemotingXmlConfigFileData.SinkProviderEntry)
                                _serverChannelSinkTemplates[entry.Value];
                        }
                        else
                        {
                            template = (RemotingXmlConfigFileData.SinkProviderEntry)
                                _clientChannelSinkTemplates[entry.Value];
                        }
                        
                        if (template == null)
                        {
                            ReportUnableToResolveTemplateReferenceError(
                                node, entry.Value.ToString(), configData);
                        }
                        else
                        {
                            // load template data
                            typeName = template.TypeName;
                            assemName = template.AssemblyName;

                            foreach (DictionaryEntry param in template.Properties)
                            {
                                properties[param.Key] = param.Value;
                            }
                        }
                    }

                    break;
                } // case "ref"
                
                case "type":
                {
                    RemotingConfigHandler.ParseType((String)entry.Value, out typeName, out assemName);
                    break;
                } // case "type" 

                default:
                    properties[keyStr] = entry.Value;
                    break;
                } // switch
            } // foreach attribute

            // check for errors        
            if ((typeName == null) || (assemName == null))
                ReportMissingTypeAttributeError(node, "type", configData);

            RemotingXmlConfigFileData.SinkProviderEntry sinkProviderEntry = 
                new RemotingXmlConfigFileData.SinkProviderEntry(typeName, assemName, properties,
                                                                isFormatter);

            // start storing sink data
            foreach (ConfigNode childNode in node.Children)
            {
                SinkProviderData providerData = 
                    ProcessSinkProviderData(childNode, configData);
                sinkProviderEntry.ProviderData.Add(providerData);
            } // foreach


            // if we reference a template and didn't specify any provider data, we
            //   should copy over the provider data from the template
            if (template != null)
            {   
                // <

                if (sinkProviderEntry.ProviderData.Count == 0)
                {
                    sinkProviderEntry.ProviderData = template.ProviderData;   
                }
            }


            if (isTemplate)
            {
                if (isServer)
                    _serverChannelSinkTemplates[id] = sinkProviderEntry;
                else
                    _clientChannelSinkTemplates[id] = sinkProviderEntry;
                return null;
            }
            else
            {
                return sinkProviderEntry;
            }
        } // ProcessSinkProviderNode
        

        // providerData will already contain an object with the same name as the config node
        private static SinkProviderData ProcessSinkProviderData(ConfigNode node, 
            RemotingXmlConfigFileData configData)
        {
            SinkProviderData providerData = new SinkProviderData(node.Name);

            foreach (ConfigNode childNode in node.Children)
            {
                SinkProviderData childData = ProcessSinkProviderData(childNode, configData);
                providerData.Children.Add(childData);               
            }

            foreach (DictionaryEntry entry in node.Attributes)
            {
                providerData.Properties[entry.Key] = entry.Value;
            }
            
            return providerData;            
        } // ProcessSinkProviderData



        //
        // process template nodes
        //

        private static void ProcessChannelTemplates(ConfigNode node, RemotingXmlConfigFileData configData)
        {
        
            foreach (ConfigNode childNode in node.Children)
            {
                switch (childNode.Name)
                {
                case "channel": ProcessChannelsChannelNode(childNode, configData, true); break;

                default: break;
                } // switch
            }
        } // ProcessChannelTemplates


        private static void ProcessChannelSinkProviderTemplates(ConfigNode node, RemotingXmlConfigFileData configData)        
        {                   
            foreach (ConfigNode childNode in node.Children)
            {
                switch (childNode.Name)
                {
                case "clientProviders": ProcessChannelProviderTemplates(childNode, configData, false); break;
                case "serverProviders": ProcessChannelProviderTemplates(childNode, configData, true); break;

                default: break;
                }
            }
        } // ProcessChannelSinkProviderTemplates


        private static void ProcessChannelProviderTemplates(ConfigNode node, RemotingXmlConfigFileData configData,
                                                            bool isServer)        
        {
            foreach (ConfigNode childNode in node.Children)
            {
                ProcessSinkProviderNode(childNode, configData, true, isServer);
            }
        } // ProcessClientProviderTemplates


        // assembly names aren't supposed to have version information in some places
        //   so we use this method to make sure that only an assembly name is
        //   specified.
        private static bool CheckAssemblyNameForVersionInfo(String assemName)
        {
            if (assemName == null)
                return false;

            // if the assembly name has a comma, we know that version information is present
            int index = assemName.IndexOf(',');
            return (index != -1);
        } // CheckAssemblyNameForVersionInfo


        private static TimeSpan ParseTime(String time, RemotingXmlConfigFileData configData)
        {
            // time formats, e.g.
            //   10D -> 10 days
            //   10H -> 10 hours
            //   10M -> 10 minutes
            //   10S -> 10 seconds
            //   10MS -> 10 milliseconds
            //   10 -> default is seconds: 10 seconds

            String specifiedTime = time;

            String metric = "s"; // default is seconds
            int metricLength = 0;

            char lastChar = ' ';
            if (time.Length > 0)
                lastChar = time[time.Length - 1];

            TimeSpan span = TimeSpan.FromSeconds(0);         
            
            try
            {                                              
                if (!Char.IsDigit(lastChar))
                {
                    if (time.Length == 0)
                        ReportInvalidTimeFormatError(specifiedTime, configData);

                    time = time.ToLower(CultureInfo.InvariantCulture);

                    metricLength = 1;
                    if (time.EndsWith("ms", StringComparison.Ordinal))
                        metricLength = 2;
                    metric = time.Substring(time.Length - metricLength, metricLength);
                }   
                
                int value = Int32.Parse(time.Substring(0, time.Length - metricLength), CultureInfo.InvariantCulture);   
     
                switch (metric)
                {
                    case "d": span = TimeSpan.FromDays(value); break;
                    case "h": span = TimeSpan.FromHours(value); break;
                    case "m": span = TimeSpan.FromMinutes(value); break;
                    case "s": span = TimeSpan.FromSeconds(value); break;
                    case "ms": span = TimeSpan.FromMilliseconds(value); break;

                    default:
                    {
                        ReportInvalidTimeFormatError(specifiedTime, configData);
                        break;
                    }
                } // switch
                
            } 
            catch (Exception)
            {
                ReportInvalidTimeFormatError(specifiedTime, configData);
            }

            return span;
        } // ParseTime        
        
    } // class RemotingXmlConfigFileParser



} // namespace
