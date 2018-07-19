//------------------------------------------------------------------------------
// <copyright file="DiagnosticsConfigurationHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

#if !LIB
#define TRACE
#define DEBUG
namespace System.Diagnostics {

    using System;
    using System.Collections;
    using System.Diagnostics;    
    using System.Xml;
    using System.Configuration;
    using System.Reflection;
    using System.Globalization;
    
    /// <devdoc>
    ///    The configuration section handler for the diagnostics section of the configuration
    ///    file. The section handler participates in the resolution of configuration settings 
    ///    between the &lt;diagnostics&gt; and &lt;/diagnostics&gt;portion of the .config file.
    /// </devdoc>
    /// <internalonly/>
    [Obsolete("This class has been deprecated.  http://go.microsoft.com/fwlink/?linkid=14202")]
    public class DiagnosticsConfigurationHandler : IConfigurationSectionHandler {

        /// <devdoc>
        ///    <para>Parses the configuration settings between the 
        ///       &lt;diagnostics&gt; and &lt;/diagnostics&gt; portion of the .config file to populate
        ///       the values of 'WebServicesConfiguration' object and returning it.
        ///    </para>
        /// </devdoc>
        /// <internalonly/>
        public virtual object Create(object parent, object configContext, XmlNode section) {
            bool foundSwitches = false;
            bool foundAssert = false;
            bool foundTrace = false;
            bool foundCounters = false;

            HandlerBase.CheckForUnrecognizedAttributes(section);

            // Since the tracing and switch code lives in System.Dll and config is in System.Configuration.dll
            // the settings just go into a hashtable to communicate to the values to the diagnostics code in System.dll
            Hashtable parentConfig = (Hashtable)parent;
            Hashtable config;
            if (parentConfig == null)
                config = new Hashtable();
            else
                config = (Hashtable)parentConfig.Clone();

            foreach (XmlNode child in section.ChildNodes) {
                if (HandlerBase.IsIgnorableAlsoCheckForNonElement(child))
                    continue;

                switch (child.Name) {
                    case "switches":                        
                        if (foundSwitches)
                            throw new ConfigurationErrorsException(SR.GetString(SR.ConfigSectionsUnique, "switches"));
                        foundSwitches = true;
            
                        HandleSwitches(config, child, configContext);
                        break;
                    case "assert":
                        if (foundAssert)
                            throw new ConfigurationErrorsException(SR.GetString(SR.ConfigSectionsUnique, "assert"));
                        foundAssert = true;

                        HandleAssert(config, child, configContext);
                        break;
                    case "trace":
                        if (foundTrace)
                            throw new ConfigurationErrorsException(SR.GetString(SR.ConfigSectionsUnique, "trace"));
                        foundTrace = true;

                        HandleTrace(config, child, configContext);
                        break;
                    case "performanceCounters":                      
                        if (foundCounters)
                            throw new ConfigurationErrorsException(SR.GetString(SR.ConfigSectionsUnique, "performanceCounters"));
                        foundCounters = true;

                        HandleCounters((Hashtable)parent, config, child, configContext);                                                    
                        break;
                    default:
                        HandlerBase.ThrowUnrecognizedElement(child);
                        break;
                } // switch(child.Name)
                
                HandlerBase.CheckForUnrecognizedAttributes(child);
            }
            return config;
        }

        private static void HandleSwitches(Hashtable config, XmlNode switchesNode, object context) {
            Hashtable switches = (Hashtable) new SwitchesDictionarySectionHandler().Create(config["switches"], context, switchesNode);
            IDictionaryEnumerator en = switches.GetEnumerator();
            while (en.MoveNext()) {
                try {
                    Int32.Parse((string) en.Value, CultureInfo.InvariantCulture);
                }
                catch {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Value_must_be_numeric, en.Key));
                }
            }

            config["switches"] = switches;
        }

        private static void HandleAssert(Hashtable config, XmlNode assertNode, object context) {
            bool assertuienabled = false;
            if (HandlerBase.GetAndRemoveBooleanAttribute(assertNode, "assertuienabled", ref assertuienabled) != null)
                config["assertuienabled"] = assertuienabled;

            string logfilename = null;
            if (HandlerBase.GetAndRemoveStringAttribute(assertNode, "logfilename", ref logfilename) != null)
                config["logfilename"] = logfilename;

            HandlerBase.CheckForChildNodes(assertNode);
        }

        private static void HandleCounters(Hashtable parent, Hashtable config, XmlNode countersNode, object context) {            
            int filemappingsize = 0;
            if (HandlerBase.GetAndRemoveIntegerAttribute(countersNode, "filemappingsize", ref filemappingsize) != null) {
                //Should only be handled at machine config level
                if (parent == null)
                    config["filemappingsize"] = filemappingsize;
            }                

            HandlerBase.CheckForChildNodes(countersNode);
        }
        
        private static void HandleTrace(Hashtable config, XmlNode traceNode, object context) {
            bool foundListeners = false;
            bool autoflush = false;
            if (HandlerBase.GetAndRemoveBooleanAttribute(traceNode, "autoflush", ref autoflush) != null)
                config["autoflush"] = autoflush;
                                       
            int indentsize = 0;
            if (HandlerBase.GetAndRemoveIntegerAttribute(traceNode, "indentsize", ref indentsize) != null)
                config["indentsize"] = indentsize;

            foreach (XmlNode traceChild in traceNode.ChildNodes) {
                if (HandlerBase.IsIgnorableAlsoCheckForNonElement(traceChild))
                    continue;
                
                if (traceChild.Name == "listeners") {
                    if (foundListeners) 
                        throw new ConfigurationErrorsException(SR.GetString(SR.ConfigSectionsUnique, "listeners"));
                    foundListeners = true;

                    HandleListeners(config, traceChild, context);
                }
                else {
                    HandlerBase.ThrowUnrecognizedElement(traceChild);
                }
            }
        }

        private static void HandleListeners(Hashtable config, XmlNode listenersNode, object context) {
            HandlerBase.CheckForUnrecognizedAttributes(listenersNode);
            foreach (XmlNode listenersChild in listenersNode.ChildNodes) {
                if (HandlerBase.IsIgnorableAlsoCheckForNonElement(listenersChild))
                    continue;
                
                string name = null, className = null, initializeData = null;
                string op = listenersChild.Name;

                switch (op) {
                    case "add":
                    case "remove":
                    case "clear":
                        break;
                    default:
                        HandlerBase.ThrowUnrecognizedElement(listenersChild);
                        break;
                }

                HandlerBase.GetAndRemoveStringAttribute(listenersChild, "name", ref name);
                HandlerBase.GetAndRemoveStringAttribute(listenersChild, "type", ref className);
                HandlerBase.GetAndRemoveStringAttribute(listenersChild, "initializeData", ref initializeData);
                HandlerBase.CheckForUnrecognizedAttributes(listenersChild);
                HandlerBase.CheckForChildNodes(listenersChild);

                TraceListener newListener = null;
                if (className != null) {  
                    Type t = Type.GetType(className);

                    if (t == null)
                        throw new ConfigurationErrorsException(SR.GetString(SR.Could_not_find_type, className));

                    if (!typeof(TraceListener).IsAssignableFrom(t))
                        throw new ConfigurationErrorsException(SR.GetString(SR.Type_isnt_tracelistener, className));

                    // create a listener with parameterless constructor 
                    if (initializeData == null) {
                        ConstructorInfo ctorInfo = t.GetConstructor(new Type[] {});
                        if (ctorInfo == null)
                            throw new ConfigurationErrorsException(SR.GetString(SR.Could_not_get_constructor, className));
                        newListener = (TraceListener)(SecurityUtils.ConstructorInfoInvoke(ctorInfo, new object[] { }));
                    }
                    // create a listener with a one-string constructor
                    else {
                        ConstructorInfo ctorInfo = t.GetConstructor(new Type[] { typeof(string) });
                        if (ctorInfo == null)
                            throw new ConfigurationErrorsException(SR.GetString(SR.Could_not_get_constructor, className));
                        newListener = (TraceListener)(SecurityUtils.ConstructorInfoInvoke(ctorInfo, new object[] { initializeData }));
                    }
                    if (name != null) {
                        newListener.Name = name;
                    }
                }

                // we already verified above that we only have "add", "remove", or "clear", so we can 
                // switch on the first char here for perf. 
                switch (op[0]) {
                    case 'a':
                        if (newListener == null)
                            throw new ConfigurationErrorsException(SR.GetString(SR.Could_not_create_listener, name));  
    
                        Trace.Listeners.Add(newListener);
    
                        break;
                    case 'r':
                        if (newListener == null) {
                            // no type specified, we'll have to delete by name
    
                            // if no name is specified we can't do anything
                            if (name == null)
                                throw new ConfigurationErrorsException(SR.GetString(SR.Cannot_remove_with_null));
    
                            Trace.Listeners.Remove(name);
                        }
                        else {
                            // remove by listener
                            Trace.Listeners.Remove(newListener);
                        }
                        break;
                    case 'c':
                        Trace.Listeners.Clear();
                        break;
                    default:
                        HandlerBase.ThrowUnrecognizedElement(listenersChild);
                        break;
                }
            }
        }
    }

    internal class SwitchesDictionarySectionHandler : DictionarySectionHandler {
        protected override string KeyAttributeName {
            get { return "name";}
        }

        internal override bool ValueRequired {
            get { return true; }
        }
        
    }
}

#endif

