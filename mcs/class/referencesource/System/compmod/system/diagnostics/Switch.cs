//------------------------------------------------------------------------------
// <copyright file="Switch.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 */

namespace System.Diagnostics {
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Configuration;
    using System.Xml.Serialization;
    using System.Diagnostics.CodeAnalysis;
    
    /// <devdoc>
    /// <para>Provides an <see langword='abstract '/>base class to
    ///    create new debugging and tracing switches.</para>
    /// </devdoc>
    public abstract class Switch {
        private SwitchElementsCollection switchSettings;
        private readonly string description;
        private readonly string displayName;
        private int    switchSetting = 0;
        private volatile bool initialized = false;
        private bool   initializing = false;
        private volatile string switchValueString = String.Empty;
        private StringDictionary attributes;
        private string defaultValue;
        private object m_intializedLock;

        private static List<WeakReference> switches = new List<WeakReference>();
        private static int s_LastCollectionCount;

        private object IntializedLock {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety")]
            get {
                if (m_intializedLock == null) {
                    Object o = new Object();
                    Interlocked.CompareExchange<Object>(ref m_intializedLock, o, null);
                }

                return m_intializedLock;
            }
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.Switch'/>
        /// class.</para>
        /// </devdoc>
        protected Switch(string displayName, string description) : this(displayName, description, "0") {
        }

        protected Switch(string displayName, string description, string defaultSwitchValue) { 
            // displayName is used as a hashtable key, so it can never
            // be null.
            if (displayName == null) displayName = string.Empty;

            this.displayName = displayName;
            this.description = description;

            // Add a weakreference to this switch and cleanup invalid references
            lock (switches) {
                _pruneCachedSwitches();
                switches.Add(new WeakReference(this));
            }

            defaultValue = defaultSwitchValue;
        }

        private static void _pruneCachedSwitches() {
            lock (switches) {
                if (s_LastCollectionCount != GC.CollectionCount(2)) {
                    List<WeakReference> buffer = new List<WeakReference>(switches.Count);
                    for (int i = 0; i < switches.Count; i++) {
                        Switch s = ((Switch)switches[i].Target);
                        if (s != null) {
                            buffer.Add(switches[i]);
                        }
                    }
                    if (buffer.Count < switches.Count) {
                        switches.Clear();
                        switches.AddRange(buffer);
                        switches.TrimExcess();
                    }
                    s_LastCollectionCount = GC.CollectionCount(2);
                }
            }
        }

        [XmlIgnore]
        public StringDictionary Attributes {
            get {
                Initialize();
                if (attributes == null)
                    attributes = new StringDictionary();
                return attributes;
            }
        }

        /// <devdoc>
        ///    <para>Gets a name used to identify the switch.</para>
        /// </devdoc>
        public string DisplayName {
            get {
                return displayName;
            }
        }

        /// <devdoc>
        ///    <para>Gets a description of the switch.</para>
        /// </devdoc>
        public string Description {
            get {
                return (description == null) ? string.Empty : description;
            }
        }

        /// <devdoc>
        ///    <para>
        ///     Indicates the current setting for this switch.
        ///    </para>
        /// </devdoc>
        protected int SwitchSetting {
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "reviewed for thread-safety")]
            get {
                if (!initialized) {
                    if (InitializeWithStatus())
                        OnSwitchSettingChanged();
                }
                return switchSetting;
            }
            [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "reviewed for thread-safety")]
            set {
                bool didUpdate = false;
                lock (IntializedLock) {
                    initialized = true;
                    if (switchSetting != value) {
                        switchSetting = value;
                        didUpdate = true;
                    }
                }

                if (didUpdate) {
                    OnSwitchSettingChanged();
                }
            }
        }

        protected string Value {
            get {
                Initialize();
                return switchValueString;
            }
            set {
                Initialize();
                switchValueString = value;
                try {
                    OnValueChanged();
                }
                catch (ArgumentException e) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.BadConfigSwitchValue, DisplayName), e);
                }
                catch (FormatException e) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.BadConfigSwitchValue, DisplayName), e);
                }
                catch (OverflowException e) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.BadConfigSwitchValue, DisplayName), e);
                }
            }
        }

        private void Initialize() {
            InitializeWithStatus();
        }

        private bool InitializeWithStatus() {
            if (!initialized) {

                lock (IntializedLock) {

                    if (initialized || initializing) {
                        return false;
                    }

                    // This method is re-entrent during intitialization, since calls to OnValueChanged() in subclasses could end up having InitializeWithStatus()
                    // called again, we don't want to get caught in an infinite loop.
                    initializing = true;

                    if (switchSettings == null) {
                        if (!InitializeConfigSettings()) {
                            initialized = true;
                            initializing = false;
                            return false;
                        }
                    }

                    if (switchSettings != null) {
                        SwitchElement mySettings = switchSettings[displayName];
                        if (mySettings != null) {
                            string value = mySettings.Value;
                            if (value != null) {
                                this.Value = value;
                            } else
                                this.Value = defaultValue;

                            try {
                                TraceUtils.VerifyAttributes(mySettings.Attributes, GetSupportedAttributes(), this);
                            } catch (ConfigurationException) {
                                // if VerifyAttributes throws, clean up a little bit so we're not in a bad state. 
                                initialized = false;
                                initializing = false;
                                throw;
                            }

                            attributes = new StringDictionary();
                            attributes.ReplaceHashtable(mySettings.Attributes);
                        } else {
                            // We don't use the property here because we don't want to catch exceptions 
                            // and rethrow them as ConfigurationException.  In this case there's no config. 
                            switchValueString = defaultValue;
                            OnValueChanged();
                        }
                    } else {
                        // We don't use the property here because we don't want to catch exceptions 
                        // and rethrow them as ConfigurationException.  In this case there's no config. 
                        switchValueString = defaultValue;
                        OnValueChanged();
                    }

                    initialized = true;
                    initializing = false;
                }
            }

            return true;
        }

        private bool InitializeConfigSettings() {
            if (switchSettings != null)
                return true;

            if (!DiagnosticsConfiguration.CanInitialize())
                return false;

            // This hashtable is case-insensitive.
            switchSettings = DiagnosticsConfiguration.SwitchSettings;
            return true;
        }

        virtual protected internal string[] GetSupportedAttributes() {
            return null;
        }

        /// <devdoc>
        ///     This method is invoked when a switch setting has been changed.  It will
        ///     be invoked the first time a switch reads its value from the registry
        ///     or environment, and then it will be invoked each time the switch's
        ///     value is changed.
        /// </devdoc>
        protected virtual void OnSwitchSettingChanged() {
        }

        protected virtual void OnValueChanged() {
            SwitchSetting = Int32.Parse(Value, CultureInfo.InvariantCulture);
        }

        internal static void RefreshAll() {

            lock (switches) {
                _pruneCachedSwitches();
                for (int i=0; i<switches.Count; i++) {
                    Switch swtch = ((Switch) switches[i].Target);
                    if (swtch != null) {
                        swtch.Refresh();
                    }
                }
            }
        }
        
        internal void Refresh() {
            lock (IntializedLock) {
                initialized = false;
                switchSettings = null;
                Initialize();
            }
        }
        
    }
}

