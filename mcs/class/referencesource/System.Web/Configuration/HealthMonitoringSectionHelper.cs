//------------------------------------------------------------------------------
// <copyright file="HealthMonitoringSectionHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.ComponentModel;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Compilation;
    using System.Security.Permissions;

    internal class HealthMonitoringSectionHelper {
        static HealthMonitoringSectionHelper s_helper;

        static RuleInfoComparer s_ruleInfoComparer = new RuleInfoComparer();

        HealthMonitoringSection _section;
        internal ProviderInstances _providerInstances;
        internal Hashtable _customEvaluatorInstances;
        internal ArrayList _ruleInfos;
        bool _enabled;

        // Cached matched rules for system events.  For details, see comments for s_eventArrayDimensionSizes
        // in WebEventCodes.cs
        static ArrayList[,] _cachedMatchedRules;

#if DBG
        // Because we assume no two different event type will use the same event code, in debug mode
        // we use this hashtable to make sure our assumption is true.
        Hashtable                       _cachedTypeOfMatchedRulesSystem = new Hashtable();
#endif

        // Cached matched rules based on WebBaseEvent Hashcode, and is for non-system events.
        Hashtable _cachedMatchedRulesForCustomEvents;

        static internal HealthMonitoringSectionHelper GetHelper() {
            if (s_helper == null) {
                s_helper = new HealthMonitoringSectionHelper();
            }

            return s_helper;
        }

        HealthMonitoringSectionHelper() {

            // Handle config exceptions so we can still log messages to the event log.
            try {
                _section = RuntimeConfig.GetAppConfig().HealthMonitoring;
            }
            catch(Exception e) {
                // If InitializationException has not already been set, then this exception
                // is happening because the <healthMonitoring> section has an error.
                // By setting InitializationException, we allow the exception to be displayed in the response.
                // If InitializationException is already set, ignore this exception so we can 
                // display the original in the response.
                if (HttpRuntime.InitializationException == null) {
                    HttpRuntime.InitializationException = e;
                }
                _section = RuntimeConfig.GetAppLKGConfig().HealthMonitoring;
                // WOS 1965670: if we fail to get the section throw the previous error
                if (_section == null) {
                    throw;
                }
            }

            _enabled = _section.Enabled;

            if (!_enabled) {
                return;
            }

            // First run some basic sanity check
            BasicSanityCheck();

            // Init some class members
            _ruleInfos = new ArrayList();
            _customEvaluatorInstances = new Hashtable();
            _providerInstances = new ProviderInstances(_section);
            _cachedMatchedRulesForCustomEvents = new Hashtable(new WebBaseEventKeyComparer());
            _cachedMatchedRules = new ArrayList[WebEventCodes.GetEventArrayDimensionSize(0),
                                                        WebEventCodes.GetEventArrayDimensionSize(1)];

            BuildRuleInfos();

            _providerInstances.CleanupUninitProviders();
        }

        internal bool Enabled {
            get { return _enabled; }
        }

        internal HealthMonitoringSection HealthMonitoringSection {
            get { return _section; }
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
        void BasicSanityCheck() {
            Type type;

            foreach (ProviderSettings providerSettings in _section.Providers) {
                // Make sure the type is valid.
                type = ConfigUtil.GetType(providerSettings.Type, "type", providerSettings);

                // Make sure the type support WebEventProvider
                HandlerBase.CheckAssignableType(providerSettings.ElementInformation.Properties["type"].Source,
                        providerSettings.ElementInformation.Properties["type"].LineNumber,
                        typeof(WebEventProvider), type);

            }

            foreach (EventMappingSettings eventMappingSettings in _section.EventMappings) {
                // Make sure the type is valid.
                type = ConfigUtil.GetType(eventMappingSettings.Type, "type", eventMappingSettings);

                // Make sure startEventCode <= endEventCode
                if (!(eventMappingSettings.StartEventCode <= eventMappingSettings.EndEventCode)) {
                    string attribute;

                    // We don't know which one was specified unless we test it
                    attribute = "startEventCode";
                    if (eventMappingSettings.ElementInformation.Properties[attribute].LineNumber == 0) {
                        attribute = "endEventCode";
                        Debug.Assert(eventMappingSettings.ElementInformation.Properties[attribute].LineNumber != 0,
                                    "eventMappingSettings.ElementInformation.Properties[attribute].LineNumber != 0");
                    }

                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Event_name_invalid_code_range),
                        eventMappingSettings.ElementInformation.Properties[attribute].Source, eventMappingSettings.ElementInformation.Properties[attribute].LineNumber);
                }

                // Make sure the type support WebBaseEvent
                HandlerBase.CheckAssignableType(eventMappingSettings.ElementInformation.Properties["type"].Source,
                            eventMappingSettings.ElementInformation.Properties["type"].LineNumber,
                            typeof(System.Web.Management.WebBaseEvent), type);

                // It's a valid type.  Might as well save it.
                eventMappingSettings.RealType = type;
            }

            foreach (RuleSettings rule in _section.Rules) {

                // Go thru all the Rules, and make sure all referenced provider, eventName
                // and profile exist.

                string provider = rule.Provider;
                if (!String.IsNullOrEmpty(provider)) {
                    ProviderSettings providerSettings = _section.Providers[provider];
                    if (providerSettings == null) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Health_mon_provider_not_found, provider),
                                rule.ElementInformation.Properties["provider"].Source,
                                rule.ElementInformation.Properties["provider"].LineNumber);
                    }
                }

                string profile = rule.Profile;
                if (!String.IsNullOrEmpty(profile)) {
                    if (_section.Profiles[profile] == null) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Health_mon_profile_not_found, profile),
                                rule.ElementInformation.Properties["profile"].Source,
                                rule.ElementInformation.Properties["profile"].LineNumber);
                    }
                }

                if (_section.EventMappings[rule.EventName] == null) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Event_name_not_found, rule.EventName),
                            rule.ElementInformation.Properties["eventName"].Source, rule.ElementInformation.Properties["eventName"].LineNumber);
                }

            }
        }

        void DisplayRuleInfo(RuleInfo ruleInfo) {
#if DEBUG
            Debug.Trace("BuildRuleInfos", "====================== Rule Info =======================");
            Debug.Trace("BuildRuleInfos", "name:" + ruleInfo._ruleSettings.Name);
            Debug.Trace("BuildRuleInfos", "type:" + ruleInfo._eventMappingSettings.RealType.Name);
            Debug.Trace("BuildRuleInfos", "minInstances:" + ruleInfo._minInstances);
            Debug.Trace("BuildRuleInfos", "maxLimit:" + ruleInfo._maxLimit);
            Debug.Trace("BuildRuleInfos", "minInterval:" + ruleInfo._minInterval);
            Debug.Trace("BuildRuleInfos", "provider:" + ruleInfo._ruleSettings.Provider);
            Debug.Trace("BuildRuleInfos", "referenced provider:" + (ruleInfo._referencedProvider == null ? String.Empty : ruleInfo._referencedProvider.GetType().Name));
            Debug.Trace("BuildRuleInfos", "=========================================================");
#endif
        }

        void BuildRuleInfos() {
            Debug.Trace("BuildRuleInfos", "BuildRuleInfos called");

            // Each ruleInfo is an object that takes the information
            // stored in a ruleSettings and merge it with values from profileSettings.

            // At the end, we'll sort the rules based on type (most specific type last)

            foreach (RuleSettings ruleSettings in _section.Rules) {
                RuleInfo ruleInfo = CreateRuleInfo(ruleSettings);
                DisplayRuleInfo(ruleInfo);
                _ruleInfos.Add(ruleInfo);
            }

            _ruleInfos.Sort(s_ruleInfoComparer);
        }

        RuleInfo CreateRuleInfo(RuleSettings ruleSettings) {
            RuleInfo ruleInfo = new RuleInfo(ruleSettings, _section);

            // Inherit values from profile
            MergeValuesWithProfile(ruleInfo);

            // Find out which provider it's referencing
            InitReferencedProvider(ruleInfo);

            // Initialize the cutom evaluator type
            InitCustomEvaluator(ruleInfo);

            return ruleInfo;
        }

        void InitReferencedProvider(RuleInfo ruleInfo) {
            String providerName;
            WebEventProvider provider;

            Debug.Assert(ruleInfo._referencedProvider == null, "ruleInfo._referencedProvider == null");

            providerName = ruleInfo._ruleSettings.Provider;
            if (String.IsNullOrEmpty(providerName)) {
                return;
            }

            provider = _providerInstances[providerName];
            Debug.Assert(provider != null, "provider != null");

            ruleInfo._referencedProvider = provider;
        }

        void MergeValuesWithProfile(RuleInfo ruleInfo) {
            ProfileSettings profileSettings = null;

            if (ruleInfo._ruleSettings.ElementInformation.Properties["profile"].ValueOrigin != PropertyValueOrigin.Default) {
                profileSettings = _section.Profiles[ruleInfo._ruleSettings.Profile];
                Debug.Assert(profileSettings != null, "profileSettings != null");
            }

            if (profileSettings != null && ruleInfo._ruleSettings.ElementInformation.Properties["minInstances"].ValueOrigin == PropertyValueOrigin.Default) {
                ruleInfo._minInstances = profileSettings.MinInstances;
            }
            else {
                ruleInfo._minInstances = ruleInfo._ruleSettings.MinInstances;
            }

            if (profileSettings != null && ruleInfo._ruleSettings.ElementInformation.Properties["maxLimit"].ValueOrigin == PropertyValueOrigin.Default) {
                ruleInfo._maxLimit = profileSettings.MaxLimit;
            }
            else {
                ruleInfo._maxLimit = ruleInfo._ruleSettings.MaxLimit;
            }

            if (profileSettings != null && ruleInfo._ruleSettings.ElementInformation.Properties["minInterval"].ValueOrigin == PropertyValueOrigin.Default) {
                ruleInfo._minInterval = profileSettings.MinInterval;
            }
            else {
                ruleInfo._minInterval = ruleInfo._ruleSettings.MinInterval;
            }

            if (profileSettings != null && ruleInfo._ruleSettings.ElementInformation.Properties["custom"].ValueOrigin == PropertyValueOrigin.Default) {
                ruleInfo._customEvaluator = profileSettings.Custom;
                ruleInfo._customEvaluatorConfig = profileSettings;
            }
            else {
                ruleInfo._customEvaluator = ruleInfo._ruleSettings.Custom;
                ruleInfo._customEvaluatorConfig = ruleInfo._ruleSettings;
            }
        }

        void InitCustomEvaluator(RuleInfo ruleInfo) {
            string customEvaluator = ruleInfo._customEvaluator;

            if (customEvaluator == null ||
                customEvaluator.Trim().Length == 0) {
                ruleInfo._customEvaluatorType = null;
                return;
            }

            ruleInfo._customEvaluatorType = ConfigUtil.GetType(ruleInfo._customEvaluator,
                "custom", ruleInfo._customEvaluatorConfig);

            // Make sure the type support WebBaseEvent
            HandlerBase.CheckAssignableType(ruleInfo._customEvaluatorConfig.ElementInformation.Properties["custom"].Source,
                    ruleInfo._customEvaluatorConfig.ElementInformation.Properties["custom"].LineNumber,
                    typeof(System.Web.Management.IWebEventCustomEvaluator), ruleInfo._customEvaluatorType);

            // Create a public instance of the custom evaluator
            if (_customEvaluatorInstances[ruleInfo._customEvaluatorType] == null) {
                _customEvaluatorInstances[ruleInfo._customEvaluatorType] = HttpRuntime.CreatePublicInstance(ruleInfo._customEvaluatorType);
            }
        }

        // Find the corresponding array of RuleInfo based on the fired event
        internal ArrayList FindFiringRuleInfos(Type eventType, int eventCode) {
            ArrayList foundFiringRuleInfos;
            bool systemEvent = eventCode < WebEventCodes.WebExtendedBase;
            CustomWebEventKey customWebEventKey = null;
            object lockObject;
            int index0 = 0, index1 = 0;

#if DBG
            if (systemEvent) {
                Type    type;
                
                type = (Type)_cachedTypeOfMatchedRulesSystem[eventCode];
                if (type == null) {
                    lock(_cachedTypeOfMatchedRulesSystem) {
                        type = (Type)_cachedTypeOfMatchedRulesSystem[eventCode];
                        if (type == null) {
                            _cachedTypeOfMatchedRulesSystem[eventCode] = eventType;
                        }
                    }
                }

                if (type != null) {
                    Debug.Assert(type == eventType, 
                        "For system events, we assume each event code will map only to one event type. " +
                        "Eventcode= " + eventCode + "; stored type= " + type.ToString() +
                        "; raised event type= " + eventType);
                }
            }
#endif

            // First, we look at the cache to see if we find the array.
            if (systemEvent) {
                WebEventCodes.GetEventArrayIndexsFromEventCode(eventCode, out index0, out index1);
                foundFiringRuleInfos = _cachedMatchedRules[index0, index1];
            }
            else {
                customWebEventKey = new CustomWebEventKey(eventType, eventCode);
                foundFiringRuleInfos = (ArrayList)_cachedMatchedRulesForCustomEvents[customWebEventKey];
            }

            if (foundFiringRuleInfos != null) {
                return foundFiringRuleInfos;
            }

            if (systemEvent) {
                lockObject = _cachedMatchedRules;
            }
            else {
                lockObject = _cachedMatchedRulesForCustomEvents;
            }

            lock (lockObject) {

                if (systemEvent) {
                    foundFiringRuleInfos = _cachedMatchedRules[index0, index1];
                }
                else {
                    Debug.Assert(customWebEventKey != null);
                    foundFiringRuleInfos = (ArrayList)_cachedMatchedRulesForCustomEvents[customWebEventKey];
                }

                if (foundFiringRuleInfos != null) {
                    return foundFiringRuleInfos;
                }

                // Not found in cache.

                ArrayList matchedRules = new ArrayList();

                // Go thru the sorted ruleInfo array and look for matching ruleInfo,
                // starting from the most specific type.
                for (int i = _ruleInfos.Count - 1; i >= 0; i--) {
                    RuleInfo curRule = (RuleInfo)_ruleInfos[i];

                    // Now see if the current rule matches the raised event
                    if (curRule.Match(eventType, eventCode)) {
                        matchedRules.Add(new FiringRuleInfo(curRule));
                    }
                }

                // Then for each matched rule, we need to figure out if the provider it
                // uses is also used by other rules.  We need this info because if multiple rules are
                // using the same provider, we fire the event to the provider only once.
                int count = matchedRules.Count;
                for (int i = 0; i < count; i++) {
                    FiringRuleInfo info1 = (FiringRuleInfo)matchedRules[i];

                    if (info1._ruleInfo._referencedProvider != null) {
                        for (int j = i + 1; j < count; j++) {

                            FiringRuleInfo info2 = (FiringRuleInfo)matchedRules[j];
                            if (info2._ruleInfo._referencedProvider != null &&    // ignore null-provider
                                info2._indexOfFirstRuleInfoWithSameProvider == -1 &&  // ignore rules that were marked already
                                info1._ruleInfo._referencedProvider == info2._ruleInfo._referencedProvider) {   // they are pointing to the same provider

                                // We'll remember the index of the first rule info that share the same
                                // provider. For details on how this index is used, please see
                                // WebBaseEvent.RaiseInternal.
                                if (info1._indexOfFirstRuleInfoWithSameProvider == -1) {
                                    info1._indexOfFirstRuleInfoWithSameProvider = i;
                                }

                                info2._indexOfFirstRuleInfoWithSameProvider = i;

                            }
                        }
                    }
                }


#if DBG
                Debug.Trace("FindRuleInfos", "------------------------------------------------");
                Debug.Trace("FindRuleInfos", "Find ruleInfos for event with type=" + eventType.ToString() +
                    ", EventCode=" + eventCode);
                
                foreach(FiringRuleInfo info in matchedRules) {
                    Debug.Trace("FindRuleInfos", "Provider=" + info._ruleInfo._ruleSettings.Provider +
                        "; eventNameType=" + info._ruleInfo._eventMappingSettings.RealType.ToString() +
                        "; _indexOfFirstRuleInfoWithSameProvider=" + info._indexOfFirstRuleInfoWithSameProvider);
                }
                Debug.Trace("FindRuleInfos", "------------------------------------------------");
#endif

                // save matchedRules in the cache
                if (systemEvent) {
                    _cachedMatchedRules[index0, index1] = matchedRules;
                }
                else {
                    Debug.Assert(customWebEventKey != null);
                    _cachedMatchedRulesForCustomEvents[customWebEventKey] = matchedRules;
                }

                return matchedRules;
            }
        }

        internal class RuleInfo {
            internal string _customEvaluator;
            internal ConfigurationElement _customEvaluatorConfig;

            // The following properties are cached here for performance reason
            internal int _minInstances;
            internal int _maxLimit;
            internal TimeSpan _minInterval;   // in seconds

            internal RuleSettings _ruleSettings;

            internal WebEventProvider _referencedProvider;

            internal Type _customEvaluatorType;

            internal EventMappingSettings _eventMappingSettings;

            internal RuleFiringRecord _ruleFiringRecord;

            internal RuleInfo(RuleSettings ruleSettings, HealthMonitoringSection section) {
                _eventMappingSettings = section.EventMappings[ruleSettings.EventName];

                _ruleSettings = ruleSettings;
                _ruleFiringRecord = new RuleFiringRecord(this);
            }

            internal bool Match(Type eventType, int eventCode) {
                // Fail if the type doesn't match.
                if (!(eventType.Equals(_eventMappingSettings.RealType) ||
                       eventType.IsSubclassOf(_eventMappingSettings.RealType))) {
                    return false;
                }

                // Fail if the event code doesn't match
                if (!(_eventMappingSettings.StartEventCode <= eventCode &&
                        eventCode <= _eventMappingSettings.EndEventCode)) {
                    return false;
                }

                return true;
            }
        }

        internal class FiringRuleInfo {
            internal RuleInfo _ruleInfo;
            internal int _indexOfFirstRuleInfoWithSameProvider;

            internal FiringRuleInfo(RuleInfo ruleInfo) {
                _ruleInfo = ruleInfo;
                _indexOfFirstRuleInfoWithSameProvider = -1;
            }
        }

        internal class ProviderInstances {
            internal Hashtable _instances; // case-insensitive because the providers collection is too.

            [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
            internal ProviderInstances(HealthMonitoringSection section) {
                // Build the array of providers
                // Don't create an instance yet, but only store the providerInfo in the HashTable.
                _instances = CollectionsUtil.CreateCaseInsensitiveHashtable(section.Providers.Count);

                foreach (object obj in section.Providers) {
                    ProviderSettings settings = (ProviderSettings)obj;

                    // Please note we are storing the ProviderSettings in the hashtable.
                    // But if we create an instance of that provider, we will replace
                    // that string with a provider object.
                    _instances.Add(settings.Name, settings);
                }
            }

            WebEventProvider GetProviderInstance(string providerName) {
                WebEventProvider provider;
                object o;

                o = _instances[providerName];
                if (o == null) {
                    return null;
                }

                ProviderSettings providerSettings = o as ProviderSettings;

                if (providerSettings != null) {
                    // If what we got is still a ProviderSettings, it means we haven't created an instance
                    // of it yet.
                    Type type;
                    string typeName = providerSettings.Type;

                    type = BuildManager.GetType(typeName, false);
                    Debug.Assert(type != null, "type != null");

                    if (typeof(IInternalWebEventProvider).IsAssignableFrom(type)) {
                        provider = (WebEventProvider)HttpRuntime.CreateNonPublicInstance(type);
                    }
                    else {
                        provider = (WebEventProvider)HttpRuntime.CreatePublicInstance(type);
                    }

                    using (new ProcessImpersonationContext()) {
                        try {
                            provider.Initialize(providerSettings.Name, providerSettings.Parameters);
                        }
                        catch (ConfigurationErrorsException) {
                            throw;
                        }
                        catch (ConfigurationException e) {
                            throw new ConfigurationErrorsException(e.Message, providerSettings.ElementInformation.Properties["type"].Source,
                                providerSettings.ElementInformation.Properties["type"].LineNumber);
                        }
                        catch {
                            throw;
                        }
                    }

                    Debug.Trace("ProviderInstances", "Create a provider instance: " +
                        "name=" + providerSettings.Name + ";type=" + typeName);

                    _instances[providerName] = provider;
                }
                else {
                    provider = o as WebEventProvider;
                    Debug.Assert(provider != null, "provider != null");
                }

                return provider;
            }

            internal WebEventProvider this[String name] {
                get {
                    return GetProviderInstance(name);
                }
            }

            // Cleanup each provider for which we have NOT created an instance.
            internal void CleanupUninitProviders() {
                ArrayList list = new ArrayList();

                foreach (DictionaryEntry de in _instances) {
                    if (de.Value is ProviderSettings) {
                        list.Add(de.Key);
                    }
                }

                foreach (object o in list) {
                    Debug.Trace("ProviderInstances", "Remove " + (string)o + " from providers");
                    _instances.Remove(o);
                }
            }

            internal bool ContainsKey(string name) {
                return _instances.ContainsKey(name);
            }

            public IDictionaryEnumerator GetEnumerator() {
                return _instances.GetEnumerator();
            }
        }
    }
}
