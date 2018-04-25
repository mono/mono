//------------------------------------------------------------------------------
// <copyright file="BlobPersonalizationState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    /// <devdoc>
    /// </devdoc>
    internal sealed class BlobPersonalizationState : PersonalizationState {

        private const int PersonalizationVersion = (int)PersonalizationVersions.WhidbeyRTM;
        private const string WebPartManagerPersonalizationID = "__wpm";

        private bool _isPostRequest;
        private IDictionary _personalizedControls;

        private IDictionary _sharedState;
        private IDictionary _userState;
        private byte[] _rawUserData;

        private IDictionary _extractedState;

        /// <devdoc>
        /// </devdoc>
        public BlobPersonalizationState(WebPartManager webPartManager) : base(webPartManager) {
            // 


            // Note that we don't use the IsPostBack property of Page because that
            // is based on the presence of view state, which could be on the query string
            // in a non-POST request as well. Instead we use the actual verb associated
            // with the request.
            // Note that there are other types of HttpVerb besides GET and POST.  We only
            // save personalization data for POST requests.  (VSWhidbey 423433)
            _isPostRequest = (webPartManager.Page.Request.HttpVerb == HttpVerb.POST);
        }

        /// <internalonly />
        public override bool IsEmpty {
            get {
                return ((_extractedState == null) || (_extractedState.Count == 0));
            }
        }

        /// <devdoc>
        /// </devdoc>
        private bool IsPostRequest {
            get {
                return _isPostRequest;
            }
        }

        /// <devdoc>
        /// </devdoc>
        private PersonalizationScope PersonalizationScope {
            get {
                return WebPartManager.Personalization.Scope;
            }
        }

        /// <devdoc>
        /// This is for symmetry with the UserState property.
        /// </devdoc>
        private IDictionary SharedState {
            get {
                return _sharedState;
            }
        }

        /// <devdoc>
        /// User state is always loaded even if the WebPartManager is in shared
        /// scope. So we on-demand deserialize the bytes.
        /// </devdoc>
        private IDictionary UserState {
            get {
                if (_rawUserData != null) {
                    _userState = DeserializeData(_rawUserData);
                    _rawUserData = null;
                }

                if (_userState == null) {
                    _userState = new HybridDictionary(/* caseInsensitive */ false);
                }

                return _userState;
            }
        }

        /// <devdoc>
        /// Does the work of applying personalization data into a control
        /// </devdoc>
        private void ApplyPersonalization(Control control, string personalizationID, bool isWebPartManager,
                                          PersonalizationScope extractScope, GenericWebPart genericWebPart) {
            Debug.Assert(control != null);
            Debug.Assert(!String.IsNullOrEmpty(personalizationID));

            if (_personalizedControls == null) {
                _personalizedControls = new HybridDictionary(/* caseInsensitive */ false);
            }
            else {
                // We shouldn't be applying personalization to the same control more than once
                if (_personalizedControls.Contains(personalizationID)) {
                    throw new InvalidOperationException(SR.GetString(SR.BlobPersonalizationState_CantApply, personalizationID));
                }
            }

            IDictionary personalizableProperties =
                PersonalizableAttribute.GetPersonalizablePropertyEntries(control.GetType());

            if (SharedState == null) {
                throw new InvalidOperationException(SR.GetString(SR.BlobPersonalizationState_NotLoaded));
            }

            PersonalizationInfo sharedInfo = (PersonalizationInfo)SharedState[personalizationID];
            PersonalizationInfo userInfo = null;
            IDictionary defaultProperties = null;
            IDictionary initialProperties = null;
            PersonalizationDictionary customInitialProperties = null;

            // WebPart.SetPersonalizationDirty() should only mark a control as dirty in the following circumstances:
            // 1. During its IPersonalizable.Load() method
            // 2. During its IVersioningPersonalizable.Load() method
            // 3. During or after its ITrackingPersonalizable.EndLoad() method
            // By exclusion, WebPart.SetPersonalizationDirty() should be a no-op in the following circumstances:
            // 1. Before its IPersonalizable.Load() method
            // 2. While we are setting the values of its [Personalizable] properties
            // (VSWhidbey 392533)
            ControlInfo ci = new ControlInfo();
            ci._allowSetDirty = false;
            _personalizedControls[personalizationID] = ci;

            if (sharedInfo != null && sharedInfo._isStatic && !sharedInfo.IsMatchingControlType(control)) {
                // Mismatch in saved data, so ignore it
                sharedInfo = null;
                if (PersonalizationScope == PersonalizationScope.Shared) {
                    SetControlDirty(control, personalizationID, isWebPartManager, true);
                }
            }

            IPersonalizable customPersonalizable = control as IPersonalizable;
            ITrackingPersonalizable trackingPersonalizable = control as ITrackingPersonalizable;

            // The WebPart on which to set HasSharedData and HasUserData
            WebPart hasDataWebPart = null;
            if (!isWebPartManager) {
                if (genericWebPart != null) {
                    hasDataWebPart = genericWebPart;
                }
                else {
                    Debug.Assert(control is WebPart);
                    hasDataWebPart = (WebPart)control;
                }
            }

            try {
                if (trackingPersonalizable != null) {
                    trackingPersonalizable.BeginLoad();
                }

                if (PersonalizationScope == PersonalizationScope.User) {
                    if (UserState == null) {
                        throw new InvalidOperationException(SR.GetString(SR.BlobPersonalizationState_NotLoaded));
                    }

                    userInfo = (PersonalizationInfo)UserState[personalizationID];

                    if (userInfo != null && userInfo._isStatic && !userInfo.IsMatchingControlType(control)) {
                        // Mismatch in saved data, so ignore it
                        userInfo = null;
                        SetControlDirty(control, personalizationID, isWebPartManager, true);
                    }

                    if (customPersonalizable != null) {
                        PersonalizationDictionary customProperties = MergeCustomProperties(
                            sharedInfo, userInfo, isWebPartManager, hasDataWebPart, ref customInitialProperties);
                        if (customProperties != null) {
                            ci._allowSetDirty = true;
                            customPersonalizable.Load(customProperties);
                            ci._allowSetDirty = false;
                        }
                    }

                    if (!isWebPartManager) {
                        // Properties do not apply to the WebPartManager

                        IDictionary unusedSharedProperties = null;
                        IDictionary unusedUserProperties = null;

                        // To compute default properties in user scope, we must first
                        // apply the shared properties. Only differences detected from
                        // shared scope are to be persisted.
                        if (sharedInfo != null) {
                            IDictionary properties = sharedInfo._properties;

                            if ((properties != null) && (properties.Count != 0)) {
                                hasDataWebPart.SetHasSharedData(true);
                                unusedSharedProperties = SetPersonalizedProperties(control, personalizableProperties,
                                                                                   properties, PersonalizationScope.Shared);
                            }
                        }
                        defaultProperties = GetPersonalizedProperties(control, personalizableProperties, null, null,
                                                                      extractScope);

                        // Now apply the user properties and hang on to the initial values
                        if (userInfo != null) {
                            IDictionary properties = userInfo._properties;

                            if ((properties != null) && (properties.Count != 0)) {
                                hasDataWebPart.SetHasUserData(true);
                                // We pass the extractScope as the PersonalizationScope in which to set the properties.  For
                                // a shared WebPart, we want to only apply the user values to user properties, and not to
                                // shared properties.  However, for an unshared WebPart, we want to apply the user values
                                // to both user and shared properties, since there is effectively no difference for an
                                // unshared WebPart. (VSWhidbey 349356)
                                unusedUserProperties = SetPersonalizedProperties(control, personalizableProperties,
                                                                                 properties, extractScope);
                            }

                            if ((trackingPersonalizable == null) || (trackingPersonalizable.TracksChanges == false)) {
                                initialProperties = properties;
                            }
                        }

                        bool hasUnusedProperties = ((unusedSharedProperties != null) || (unusedUserProperties != null));
                        if (hasUnusedProperties) {
                            IVersioningPersonalizable versioningPersonalizable = control as IVersioningPersonalizable;
                            if (versioningPersonalizable != null) {
                                IDictionary unusedProperties = null;

                                // Merge any unused properties, so they can be handed off to the
                                // control via IVersioningPersonalizable
                                if (unusedSharedProperties != null) {
                                    unusedProperties = unusedSharedProperties;
                                    if (unusedUserProperties != null) {
                                        foreach (DictionaryEntry entry in unusedUserProperties) {
                                            unusedProperties[entry.Key] = entry.Value;
                                        }
                                    }
                                }
                                else {
                                    unusedProperties = unusedUserProperties;
                                }

                                ci._allowSetDirty = true;
                                versioningPersonalizable.Load(unusedProperties);
                                ci._allowSetDirty = false;
                            }
                            else {
                                // There were some unused properties, and they couldn't be loaded.
                                // Mark this control as dirty, so we clean up its personalization
                                // state later...
                                SetControlDirty(control, personalizationID, isWebPartManager, true);
                            }
                        }
                    }
                }
                else {
                    // Shared Personalization Scope

                    if (customPersonalizable != null) {
                        PersonalizationDictionary customProperties = MergeCustomProperties(
                            sharedInfo, userInfo, isWebPartManager, hasDataWebPart, ref customInitialProperties);
                        if (customProperties != null) {
                            ci._allowSetDirty = true;
                            customPersonalizable.Load(customProperties);
                            ci._allowSetDirty = false;
                        }
                    }

                    if (!isWebPartManager) {
                        IDictionary unusedProperties = null;

                        // Compute default properties. These are basically what was persisted
                        // in the markup
                        defaultProperties = GetPersonalizedProperties(control, personalizableProperties, null, null,
                                                                      extractScope);

                        // Now apply shared properties and hang on to the initial values
                        if (sharedInfo != null) {
                            IDictionary properties = sharedInfo._properties;

                            if ((properties != null) && (properties.Count != 0)) {
                                hasDataWebPart.SetHasSharedData(true);
                                unusedProperties = SetPersonalizedProperties(control, personalizableProperties,
                                                                             properties, PersonalizationScope.Shared);
                            }

                            if ((trackingPersonalizable == null) ||
                                (trackingPersonalizable.TracksChanges == false)) {
                                initialProperties = properties;
                            }
                        }

                        if (unusedProperties != null) {
                            IVersioningPersonalizable versioningPersonalizable = control as IVersioningPersonalizable;
                            if (versioningPersonalizable != null) {
                                ci._allowSetDirty = true;
                                versioningPersonalizable.Load(unusedProperties);
                                ci._allowSetDirty = false;
                            }
                            else {
                                // There were some unused properties, and they couldn't be loaded.
                                // Mark this control as dirty, so we clean up its personalization
                                // state later...
                                SetControlDirty(control, personalizationID, isWebPartManager, true);
                            }
                        }
                    }
                }
            }
            finally {
                ci._allowSetDirty = true;
                if (trackingPersonalizable != null) {
                    trackingPersonalizable.EndLoad();
                }
            }

            // Track this as one of the personalized controls
            ci._control = control;
            ci._personalizableProperties = personalizableProperties;
            ci._defaultProperties = defaultProperties;
            ci._initialProperties = initialProperties;
            ci._customInitialProperties = customInitialProperties;
        }

        /// <internalonly />
        public override void ApplyWebPartPersonalization(WebPart webPart) {
            ValidateWebPart(webPart);

            // Do not apply personalization to the UnauthorizedWebPart.  It is never rendered
            // in the page, so there is no point to applying the personalization to it.
            // The personalization data from the original WebPart will be round-tripped in
            // ExtractWebPartPersonalization().  We do apply personalization to the ErrorWebPart,
            // because we want it to render with many of the personalized property values of the
            // original WebPart.
            if (webPart is UnauthorizedWebPart) {
                return;
            }

            string personalizationID = CreatePersonalizationID(webPart, null);

            // In ApplyPersonalization(), we need to extract the default properites in the same scope we will
            // extract the properties in ExtractPersonalization().
            PersonalizationScope extractScope = PersonalizationScope;
            if ((extractScope == PersonalizationScope.User) && (!webPart.IsShared)) {
                // This implies a user owned WebPart in User mode, so extract all
                // the properties
                extractScope = PersonalizationScope.Shared;
            }

            ApplyPersonalization(webPart, personalizationID, /* isWebPartManager */ false, extractScope,
                                 /* genericWebPart */ null);

            GenericWebPart genericWebPart = webPart as GenericWebPart;
            if (genericWebPart != null) {
                Control containedControl = genericWebPart.ChildControl;
                personalizationID = CreatePersonalizationID(containedControl, genericWebPart);

                ApplyPersonalization(containedControl, personalizationID, /* isWebPartManager */ false, extractScope,
                                     genericWebPart);
            }
        }

        /// <internalonly />
        public override void ApplyWebPartManagerPersonalization() {
            ApplyPersonalization(WebPartManager, WebPartManagerPersonalizationID, /* isWebPartManager */ true,
                                 PersonalizationScope, /* genericWebPart */ null);
        }

        /// <devdoc>
        /// Returns false if the set of new properties are the same as old ones; true if there
        /// are differences.
        /// </devdoc>
        private bool CompareProperties(IDictionary newProperties, IDictionary oldProperties) {
            int newCount = 0;
            int oldCount = 0;

            if (newProperties != null) {
                newCount = newProperties.Count;
            }
            if (oldProperties != null) {
                oldCount = oldProperties.Count;
            }

            if (newCount != oldCount) {
                return true;
            }

            if (newCount != 0) {
                foreach (DictionaryEntry entry in newProperties) {
                    object name = entry.Key;
                    object newValue = entry.Value;

                    if (oldProperties.Contains(name)) {
                        object oldValue = oldProperties[name];

                        if (Object.Equals(newValue, oldValue) == false) {
                            return true;
                        }
                    }
                    else {
                        return true;
                    }
                }
            }

            return false;
        }

        private string CreatePersonalizationID(string ID, string genericWebPartID) {
            Debug.Assert(!String.IsNullOrEmpty(ID));
            if (!String.IsNullOrEmpty(genericWebPartID)) {
                return ID + Control.ID_SEPARATOR + genericWebPartID;
            }
            else {
                return ID;
            }
        }

        private string CreatePersonalizationID(Control control, WebPart associatedGenericWebPart) {
            if (associatedGenericWebPart != null) {
                return CreatePersonalizationID(control.ID, associatedGenericWebPart.ID);
            }

            return CreatePersonalizationID(control.ID, null);
        }

        /// <devdoc>
        /// Deserializes personalization data packed as a blob of binary data
        /// into a dictionary with personalization IDs mapped to
        /// PersonalizationInfo objects.
        /// </devdoc>
        private static IDictionary DeserializeData(byte[] data) {
            IDictionary deserializedData = null;

            if ((data != null) && (data.Length > 0)) {
                Exception deserializationException = null;
                int version = -1;

                object[] items = null;
                int offset = 0;

                // Deserialize the data
                try {
                    ObjectStateFormatter formatter =
                        new ObjectStateFormatter(null /* Page(used to determine encryption mode) */, false /*throwOnErrorDeserializing*/);

                    if (!HttpRuntime.DisableProcessRequestInApplicationTrust) {
                        // This is more of a consistency and defense-in-depth fix.  Currently we believe
                        // only user code or code with restricted permissions will be running on the stack.
                        // However, to mirror the fix for Session State, and also to hedge against future
                        // scenarios where our current assumptions may change, we should restrict the running
                        // thread to only the permission set currently defined for the app domain.
                        // VSWhidbey 427533
                        if (HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust) {
                            HttpRuntime.NamedPermissionSet.PermitOnly();
                        }
                    }

                    items = (object[])formatter.DeserializeWithAssert(new MemoryStream(data));
                    if (items != null && items.Length != 0) {
                        version = (int)items[offset++];
                    }
                }
                catch (Exception e) {
                    deserializationException = e;
                }

                if (version == (int)PersonalizationVersions.WhidbeyBeta2 || version == (int)PersonalizationVersions.WhidbeyRTM) {
                    try {
                        // Build up the dictionary of PersonalizationInfo objects
                        int infoListCount = (int)items[offset++];

                        if (infoListCount > 0) {
                            deserializedData = new HybridDictionary(infoListCount, /* caseInsensitive */ false);
                        }

                        for (int i = 0; i < infoListCount; i++) {
                            string controlID;
                            bool isStatic;
                            Type controlType = null;
                            VirtualPath controlVPath = null;

                            // If this is a dynamic WebPart or control, the Type is not saved in personalization,
                            // so the first item is the controlID.  If this is a static WebPart or control, the
                            // first item is the control Type.
                            object item = items[offset++];
                            if (item is string) {
                                controlID = (string)item;
                                isStatic = false;
                            }
                            else {
                                controlType = (Type)item;
                                if (controlType == typeof(UserControl)) {
                                    controlVPath = VirtualPath.CreateNonRelativeAllowNull((string)items[offset++]);
                                }
                                controlID = (string)items[offset++];
                                isStatic = true;
                            }

                            IDictionary properties = null;
                            int propertyCount = (int)items[offset++];
                            if (propertyCount > 0) {
                                properties = new HybridDictionary(propertyCount, /* caseInsensitive */ false);
                                for (int j = 0; j < propertyCount; j++) {
                                    string propertyName = ((IndexedString)items[offset++]).Value;
                                    object propertyValue = items[offset++];

                                    properties[propertyName] = propertyValue;
                                }
                            }

                            PersonalizationDictionary customProperties = null;
                            int customPropertyCount = (int)items[offset++];
                            if (customPropertyCount > 0) {
                                customProperties = new PersonalizationDictionary(customPropertyCount);
                                for (int j = 0; j < customPropertyCount; j++) {
                                    string propertyName = ((IndexedString)items[offset++]).Value;
                                    object propertyValue = items[offset++];
                                    PersonalizationScope propertyScope =
                                        (bool)items[offset++] ? PersonalizationScope.Shared : PersonalizationScope.User;
                                    bool isSensitive = false;
                                    if (version == (int)PersonalizationVersions.WhidbeyRTM) {
                                        isSensitive = (bool)items[offset++];
                                    }

                                    customProperties[propertyName] =
                                        new PersonalizationEntry(propertyValue, propertyScope, isSensitive);
                                }
                            }

                            PersonalizationInfo info = new PersonalizationInfo();
                            info._controlID = controlID;
                            info._controlType = controlType;
                            info._controlVPath = controlVPath;
                            info._isStatic = isStatic;
                            info._properties = properties;
                            info._customProperties = customProperties;

                            deserializedData[controlID] = info;
                        }
                    }
                    catch (Exception e) {
                        deserializationException = e;
                    }
                }

                // Check that there was no deserialization error, and that
                // the data conforms to our known version
                if ((deserializationException != null) ||
                    (version != (int)PersonalizationVersions.WhidbeyBeta2 && version != (int)PersonalizationVersions.WhidbeyRTM)) {
                    throw new ArgumentException(SR.GetString(SR.BlobPersonalizationState_DeserializeError),
                                                "data", deserializationException);
                }
            }

            if (deserializedData == null) {
                deserializedData = new HybridDictionary(/* caseInsensitive */ false);
            }

            return deserializedData;
        }

        /// <devdoc>
        /// Does the actual work of extracting personalizated data from a control
        /// </devdoc>
        private void ExtractPersonalization(Control control, string personalizationID, bool isWebPartManager,
                                            PersonalizationScope scope, bool isStatic, GenericWebPart genericWebPart) {
            Debug.Assert(control != null);
            Debug.Assert(!String.IsNullOrEmpty(personalizationID));

            if (_extractedState == null) {
                _extractedState = new HybridDictionary(/* caseInsensitive */ false);
            }

            if (_personalizedControls == null) {
                throw new InvalidOperationException(SR.GetString(SR.BlobPersonalizationState_NotApplied));
            }

            ControlInfo ci = (ControlInfo)_personalizedControls[personalizationID];
            // The ControlInfo should always have been already created in ApplyPersonalization().
            // However, it  will be null if the Control's ID has changed since we loaded personalization data.
            // This is not supported, but we should throw a helpful exception. (VSWhidbey 372354)
            if (ci == null) {
                throw new InvalidOperationException(SR.GetString(SR.BlobPersonalizationState_CantExtract, personalizationID));
            }

            ITrackingPersonalizable trackingPersonalizable = control as ITrackingPersonalizable;
            IPersonalizable customPersonalizable = control as IPersonalizable;

            IDictionary properties = ci._initialProperties;
            PersonalizationDictionary customProperties = ci._customInitialProperties;
            bool changed = false;

            try {
                if (trackingPersonalizable != null) {
                    trackingPersonalizable.BeginSave();
                }

                if (!IsPostRequest) {
                    // In non-POST requests, we only save those WebParts that indicated explicitely that
                    // they have changed. For other WebParts, we just round-trip the initial state
                    // that was loaded.
                    if (ci._dirty) {
                        // Always save IPersonalizable data if the WebPart has indicated that it is dirty
                        if (customPersonalizable != null) {
                            PersonalizationDictionary tempCustomProperties = new PersonalizationDictionary();

                            customPersonalizable.Save(tempCustomProperties);
                            if ((tempCustomProperties.Count != 0) ||
                                ((customProperties != null) && (customProperties.Count != 0))) {
                                if (scope == PersonalizationScope.User) {
                                    tempCustomProperties.RemoveSharedProperties();
                                }
                                customProperties = tempCustomProperties;
                            }
                        }

                        if (!isWebPartManager) {
                            // WebPartManager does not have personalizable properties
                            properties =
                                GetPersonalizedProperties(control, ci._personalizableProperties,
                                                          ci._defaultProperties, ci._initialProperties, scope);
                        }
                        changed = true;
                    }
                }
                else {
                    bool extractProperties = true;
                    bool diffWithInitialProperties = true;

                    if (ci._dirty) {
                        // WebPart is indicating that it is dirty, so there is no need
                        // for us to perform a diff
                        diffWithInitialProperties = false;
                    }
                    else if ((trackingPersonalizable != null) &&
                             (trackingPersonalizable.TracksChanges) &&
                             (ci._dirty == false)) {
                        // WebPart is indicating that it is not dirty, and since it
                        // tracks dirty-ness, theres no need to do additional work.
                        extractProperties = false;
                    }

                    if (extractProperties) {
                        // Always save IPersonalizable data if the WebPart has indicated that it is dirty
                        if (customPersonalizable != null && (ci._dirty || customPersonalizable.IsDirty)) {
                            PersonalizationDictionary tempCustomProperties = new PersonalizationDictionary();
                            customPersonalizable.Save(tempCustomProperties);

                            // The new custom properties should be used either if they are
                            // non-empty, or they are, but the original ones weren't, since
                            // that implies a change as well.
                            if ((tempCustomProperties.Count != 0) ||
                                ((customProperties != null) && (customProperties.Count != 0))) {
                                if (tempCustomProperties.Count != 0) {
                                    if (scope == PersonalizationScope.User) {
                                        tempCustomProperties.RemoveSharedProperties();
                                    }
                                    customProperties = tempCustomProperties;
                                }
                                else {
                                    customProperties = null;
                                }

                                // No point doing the diff, since we've already determined that the
                                // custom properties are dirty.
                                diffWithInitialProperties = false;
                                changed = true;
                            }
                        }

                        if (!isWebPartManager) {
                            // WebPartManager does not have personalizable properties

                            IDictionary newProperties =
                                GetPersonalizedProperties(control, ci._personalizableProperties,
                                                          ci._defaultProperties, ci._initialProperties, scope);

                            if (diffWithInitialProperties) {
                                bool different = CompareProperties(newProperties, ci._initialProperties);
                                if (different == false) {
                                    extractProperties = false;
                                }
                            }

                            if (extractProperties) {
                                properties = newProperties;
                                changed = true;
                            }
                        }
                    }
                }
            }
            finally {
                if (trackingPersonalizable != null) {
                    trackingPersonalizable.EndSave();
                }
            }

            PersonalizationInfo extractedInfo = new PersonalizationInfo();
            extractedInfo._controlID = personalizationID;
            if (isStatic) {
                UserControl uc = control as UserControl;
                if (uc != null) {
                    extractedInfo._controlType = typeof(UserControl);
                    extractedInfo._controlVPath = uc.TemplateControlVirtualPath;
                }
                else {
                    extractedInfo._controlType = control.GetType();
                }
            }
            extractedInfo._isStatic = isStatic;
            extractedInfo._properties = properties;
            extractedInfo._customProperties = customProperties;
            _extractedState[personalizationID] = extractedInfo;

            if (changed) {
                SetDirty();
            }

            if ((properties != null && properties.Count > 0) ||
                (customProperties != null && customProperties.Count > 0)) {

                // The WebPart on which to set HasSharedData and HasUserData
                WebPart hasDataWebPart = null;
                if (!isWebPartManager) {
                    if (genericWebPart != null) {
                        hasDataWebPart = genericWebPart;
                    }
                    else {
                        Debug.Assert(control is WebPart);
                        hasDataWebPart = (WebPart)control;
                    }
                }

                if (hasDataWebPart != null) {
                    if (PersonalizationScope == PersonalizationScope.Shared) {
                        hasDataWebPart.SetHasSharedData(true);
                    }
                    else {
                        hasDataWebPart.SetHasUserData(true);
                    }
                }
            }
        }

        /// <internalonly />
        public override void ExtractWebPartPersonalization(WebPart webPart) {
            ValidateWebPart(webPart);

            // Round-trip the personalization data for a ProxyWebPart
            ProxyWebPart proxyWebPart = webPart as ProxyWebPart;
            if (proxyWebPart != null) {
                RoundTripWebPartPersonalization(proxyWebPart.OriginalID, proxyWebPart.GenericWebPartID);
                return;
            }

            PersonalizationScope extractScope = PersonalizationScope;
            if ((extractScope == PersonalizationScope.User) && (!webPart.IsShared)) {
                // This implies a user owned WebPart in User mode, so save all
                // the properties
                extractScope = PersonalizationScope.Shared;
            }

            bool isStatic = webPart.IsStatic;
            string personalizationID = CreatePersonalizationID(webPart, null);
            ExtractPersonalization(webPart, personalizationID, /* isWebPartManager */ false, extractScope, isStatic,
                                   /* genericWebPart */ null);

            GenericWebPart genericWebPart = webPart as GenericWebPart;
            if (genericWebPart != null) {
                Control containedControl = genericWebPart.ChildControl;
                personalizationID = CreatePersonalizationID(containedControl, genericWebPart);
                ExtractPersonalization(containedControl, personalizationID, /* isWebPartManager */ false,
                                       extractScope, isStatic, genericWebPart);
            }
        }

        /// <internalonly />
        public override void ExtractWebPartManagerPersonalization() {
            ExtractPersonalization(WebPartManager, WebPartManagerPersonalizationID, /* isWebPartManager */ true,
                                   PersonalizationScope, /* isStatic */ true, /* genericWebPart */ null);
        }

        // Returns the AuthorizationFilter string for a WebPart before it is instantiated.
        // Returns null if there is no personalized value for AuthorizationFilter, or if the
        // personalized value has a type other than string.
        public override string GetAuthorizationFilter(string webPartID) {
            if (String.IsNullOrEmpty(webPartID)) {
                throw ExceptionUtil.ParameterNullOrEmpty("webPartID");
            }

            return GetPersonalizedValue(webPartID, "AuthorizationFilter") as string;
        }

        /// <devdoc>
        /// </devdoc>
        internal static IDictionary GetPersonalizedProperties(Control control, PersonalizationScope scope) {
            IDictionary personalizableProperties =
                PersonalizableAttribute.GetPersonalizablePropertyEntries(control.GetType());

            return GetPersonalizedProperties(control, personalizableProperties, null, null, scope);
        }

        /// <devdoc>
        /// Does the work of retrieving personalized properties. If the scope is User, the shared
        /// personalizable properties are not retrieved. If a non-null defaultPropertyState is
        /// handed in, only the properties that are different from the default values are retrieved.
        /// </devdoc>
        private static IDictionary GetPersonalizedProperties(Control control,
                                                             IDictionary personalizableProperties,
                                                             IDictionary defaultPropertyState,
                                                             IDictionary initialPropertyState,
                                                             PersonalizationScope scope) {
            Debug.Assert(control != null);

            if (personalizableProperties.Count == 0) {
                return null;
            }

            bool ignoreSharedProperties = (scope == PersonalizationScope.User);
            IDictionary properties = null;

            foreach (DictionaryEntry entry in personalizableProperties) {
                PersonalizablePropertyEntry property = (PersonalizablePropertyEntry)entry.Value;

                if (ignoreSharedProperties && (property.Scope == PersonalizationScope.Shared)) {
                    continue;
                }

                PropertyInfo pi = property.PropertyInfo;
                Debug.Assert(pi != null);

                // 
                string name = (string)entry.Key;
                object value = FastPropertyAccessor.GetProperty(control, name, control.DesignMode);
                bool saveProperty = true;

                // Only compare to default value if there is no initial value.
                if ((initialPropertyState == null || !initialPropertyState.Contains(name)) && defaultPropertyState != null) {
                    object defaultValue = defaultPropertyState[name];
                    if (Object.Equals(value, defaultValue)) {
                        saveProperty = false;
                    }
                }

                if (saveProperty) {
                    if (properties == null) {
                        properties = new HybridDictionary(personalizableProperties.Count, /* caseInsensitive */ false);
                    }

                    properties[name] = value;
                }
            }

            return properties;
        }

        // Returns the value of a personalized property on a control
        // Returns null if there is no personalized value for the property
        private object GetPersonalizedValue(string personalizationID, string propertyName) {
            Debug.Assert(!String.IsNullOrEmpty(personalizationID));
            Debug.Assert(!String.IsNullOrEmpty(propertyName));

            if (SharedState == null) {
                throw new InvalidOperationException(SR.GetString(SR.BlobPersonalizationState_NotLoaded));
            }

            PersonalizationInfo sharedInfo = (PersonalizationInfo)SharedState[personalizationID];

            IDictionary sharedProperties = (sharedInfo != null) ? sharedInfo._properties : null;
            if (PersonalizationScope == PersonalizationScope.Shared) {
                if (sharedProperties != null) {
                    return sharedProperties[propertyName];
                }
            }
            else {
                if (UserState == null) {
                    throw new InvalidOperationException(SR.GetString(SR.BlobPersonalizationState_NotLoaded));
                }

                PersonalizationInfo userInfo = (PersonalizationInfo)UserState[personalizationID];
                IDictionary userProperties = (userInfo != null) ? userInfo._properties : null;
                if (userProperties != null && userProperties.Contains(propertyName)) {
                    return userProperties[propertyName];
                }
                else if (sharedProperties != null) {
                    return sharedProperties[propertyName];
                }
            }

            return null;
        }

        /// <devdoc>
        /// </devdoc>
        public void LoadDataBlobs(byte[] sharedData, byte[] userData) {
            _sharedState = DeserializeData(sharedData);
            _rawUserData = userData;
        }

        // Returns a PersonalizationDictionary containing a merged view of the custom properties
        // in both the sharedInfo and the userInfo.
        private PersonalizationDictionary MergeCustomProperties(PersonalizationInfo sharedInfo,
                                                                PersonalizationInfo userInfo,
                                                                bool isWebPartManager, WebPart hasDataWebPart,
                                                                ref PersonalizationDictionary customInitialProperties) {
            PersonalizationDictionary customProperties = null;

            bool hasSharedCustomProperties = (sharedInfo != null && sharedInfo._customProperties != null);
            bool hasUserCustomProperties = (userInfo != null && userInfo._customProperties != null);

            // Fill or set the customProperties dictionary
            if (hasSharedCustomProperties && hasUserCustomProperties) {
                customProperties = new PersonalizationDictionary();
                foreach (DictionaryEntry entry in sharedInfo._customProperties) {
                    customProperties[(string)entry.Key] = (PersonalizationEntry)entry.Value;
                }
                foreach (DictionaryEntry entry in userInfo._customProperties) {
                    customProperties[(string)entry.Key] = (PersonalizationEntry)entry.Value;
                }
            }
            else if (hasSharedCustomProperties) {
                customProperties = sharedInfo._customProperties;
            }
            else if (hasUserCustomProperties) {
                customProperties = userInfo._customProperties;
            }

            // Set the customInitialProperties dictionary
            if (PersonalizationScope == PersonalizationScope.Shared && hasSharedCustomProperties) {
                customInitialProperties = sharedInfo._customProperties;
            }
            else if (PersonalizationScope == PersonalizationScope.User && hasUserCustomProperties) {
                customInitialProperties = userInfo._customProperties;
            }

            // Set the HasSharedData and HasUserData flags
            if (hasSharedCustomProperties && !isWebPartManager) {
                hasDataWebPart.SetHasSharedData(true);
            }
            if (hasUserCustomProperties && !isWebPartManager) {
                hasDataWebPart.SetHasUserData(true);
            }

            return customProperties;
        }


        private void RoundTripWebPartPersonalization(string ID, string genericWebPartID) {
            if (String.IsNullOrEmpty(ID)) {
                throw ExceptionUtil.ParameterNullOrEmpty("ID");
            }

            // Round-trip personalization for control/WebPart
            string personalizationID = CreatePersonalizationID(ID, genericWebPartID);
            RoundTripWebPartPersonalization(personalizationID);

            // Round-trip personalization for GenericWebPart, if necessary
            if (!String.IsNullOrEmpty(genericWebPartID)) {
                string genericPersonalizationID = CreatePersonalizationID(genericWebPartID, null);
                RoundTripWebPartPersonalization(genericPersonalizationID);
            }
        }

        private void RoundTripWebPartPersonalization(string personalizationID) {
            Debug.Assert(personalizationID != null);
            // Can't check that personalizationID is valid, since there may be no data
            // for even a valid ID.

            if (PersonalizationScope == PersonalizationScope.Shared) {
                if (SharedState == null) {
                    throw new InvalidOperationException(SR.GetString(SR.BlobPersonalizationState_NotLoaded));
                }
                if (SharedState.Contains(personalizationID)) {
                    _extractedState[personalizationID] = (PersonalizationInfo)SharedState[personalizationID];
                }
            }
            else {
                if (UserState == null) {
                    throw new InvalidOperationException(SR.GetString(SR.BlobPersonalizationState_NotLoaded));
                }
                if (UserState.Contains(personalizationID)) {
                    _extractedState[personalizationID] = (PersonalizationInfo)UserState[personalizationID];
                }
            }
        }

        /// <devdoc>
        /// </devdoc>
        public byte[] SaveDataBlob() {
            return SerializeData(_extractedState);
        }

        /// <devdoc>
        /// Serializes a dictionary of IDs mapped to PersonalizationInfo
        /// objects into a binary blob.
        /// </devdoc>
        private static byte[] SerializeData(IDictionary data) {
            byte[] serializedData = null;

            if ((data == null) || (data.Count == 0)) {
                return serializedData;
            }

            ArrayList infoList = new ArrayList();
            foreach (DictionaryEntry entry in data) {
                PersonalizationInfo info = (PersonalizationInfo)entry.Value;

                if (((info._properties != null) && (info._properties.Count != 0)) ||
                    ((info._customProperties != null) && (info._customProperties.Count != 0))){
                    infoList.Add(info);
                }
            }

            if (infoList.Count != 0) {
                ArrayList items = new ArrayList();

                items.Add(PersonalizationVersion);
                items.Add(infoList.Count);

                foreach (PersonalizationInfo info in infoList) {
                    // Only need to save the type information for static WebParts
                    if (info._isStatic) {
                        items.Add(info._controlType);
                        if (info._controlVPath != null) {
                            items.Add(info._controlVPath.AppRelativeVirtualPathString);
                        }
                    }

                    items.Add(info._controlID);

                    int propertyCount = 0;
                    if (info._properties != null) {
                        propertyCount = info._properties.Count;
                    }
                    items.Add(propertyCount);
                    if (propertyCount != 0) {
                        foreach (DictionaryEntry propertyEntry in info._properties) {
                            items.Add(new IndexedString((string)propertyEntry.Key));
                            items.Add(propertyEntry.Value);
                        }
                    }

                    int customPropertyCount = 0;
                    if (info._customProperties != null) {
                        customPropertyCount = info._customProperties.Count;
                    }
                    items.Add(customPropertyCount);
                    if (customPropertyCount != 0) {
                        foreach (DictionaryEntry customPropertyEntry in info._customProperties) {
                            items.Add(new IndexedString((string)customPropertyEntry.Key));
                            PersonalizationEntry personalizationEntry = (PersonalizationEntry)customPropertyEntry.Value;
                            items.Add(personalizationEntry.Value);
                            // PERF: Add a boolean instead of the Enum value
                            items.Add(personalizationEntry.Scope == PersonalizationScope.Shared);
                            // The IsSensitive property was added between Whidbey Beta2 and Whidbey RTM.
                            // VSWhidbey 502554 and 536907
                            items.Add(personalizationEntry.IsSensitive);
                        }
                    }
                }

                if (items.Count != 0) {
                    ObjectStateFormatter formatter = new ObjectStateFormatter(null, false);
                    MemoryStream ms = new MemoryStream(1024);
                    object[] state = items.ToArray();

                    if (!HttpRuntime.DisableProcessRequestInApplicationTrust){ 
                        // This is more of a consistency and defense-in-depth fix.  Currently we believe
                        // only user code or code with restricted permissions will be running on the stack.
                        // However, to mirror the fix for Session State, and also to hedge against future
                        // scenarios where our current assumptions may change, we should restrict the running
                        // thread to only the permission set currently defined for the app domain.
                        // VSWhidbey 491449
                        if (HttpRuntime.NamedPermissionSet != null && HttpRuntime.ProcessRequestInApplicationTrust) {
                            HttpRuntime.NamedPermissionSet.PermitOnly();
                        }
                    }

                    formatter.SerializeWithAssert(ms, state);

                    serializedData = ms.ToArray();
                }
            }

            return serializedData;
        }

        /// <devdoc>
        /// Only actually sets the control as dirty if we have already started applying personalization
        /// data (info != null), and we are forcing the control to be dirty (forceSetDirty), or the control
        /// has called SetPersonalizationDirty() at the right time (info._allowSetDirty).
        /// </devdoc>
        private void SetControlDirty(Control control, string personalizationID, bool isWebPartManager,
                                     bool forceSetDirty) {
            Debug.Assert(control != null);
            Debug.Assert(!String.IsNullOrEmpty(personalizationID));

            if (_personalizedControls == null) {
                throw new InvalidOperationException(SR.GetString(SR.BlobPersonalizationState_NotApplied));
            }

            ControlInfo info = (ControlInfo)_personalizedControls[personalizationID];
            if (info != null && (forceSetDirty || info._allowSetDirty)) {
                info._dirty = true;
            }
        }

        /// <devdoc>
        /// Called by WebPartPersonalization to copy the personalized values from one control
        /// to another.
        /// </devdoc>
        internal static IDictionary SetPersonalizedProperties(Control control, IDictionary propertyState) {
            IDictionary personalizableProperties =
                PersonalizableAttribute.GetPersonalizablePropertyEntries(control.GetType());

            // We pass PersonalizationScope.Shared, since we want to apply all values to their properties.
            return SetPersonalizedProperties(control, personalizableProperties, propertyState, PersonalizationScope.Shared);
        }

        /// <devdoc>
        /// Does the work of setting personalized properties
        /// </devdoc>
        private static IDictionary SetPersonalizedProperties(Control control, IDictionary personalizableProperties,
                                                             IDictionary propertyState, PersonalizationScope scope) {
            if (personalizableProperties.Count == 0) {
                // all properties were not used
                return propertyState;
            }

            if ((propertyState == null) || (propertyState.Count == 0)) {
                return null;
            }

            IDictionary unusedProperties = null;

            foreach (DictionaryEntry entry in propertyState) {
                string name = (string)entry.Key;
                object value = entry.Value;

                PersonalizablePropertyEntry property = (PersonalizablePropertyEntry)personalizableProperties[name];
                bool propertySet = false;

                // Do not apply a user value to a shared property.  This scenario can happen if there
                // is already User data for a property, then the property is changed from Personalizable(User)
                // to Personalizable(Shared). (VSWhidbey 349456)
                if (property != null &&
                    (scope == PersonalizationScope.Shared || property.Scope == PersonalizationScope.User)) {

                    PropertyInfo pi = property.PropertyInfo;
                    Debug.Assert(pi != null);

                    // If SetProperty() throws an exception, the property will be added to the unusedProperties collection
                    try {
                        FastPropertyAccessor.SetProperty(control, name, value, control.DesignMode);
                        propertySet = true;
                    }
                    catch {
                    }
                }

                if (!propertySet) {
                    if (unusedProperties == null) {
                        unusedProperties = new HybridDictionary(propertyState.Count, /* caseInsensitive */ false);
                    }

                    unusedProperties[name] = value;
                }
            }

            return unusedProperties;
        }

        /// <internalonly />
        public override void SetWebPartDirty(WebPart webPart) {
            ValidateWebPart(webPart);

            string personalizationID;

            personalizationID = CreatePersonalizationID(webPart, null);
            SetControlDirty(webPart, personalizationID, /* isWebPartManager */ false, /* forceSetDirty */ false);

            GenericWebPart genericWebPart = webPart as GenericWebPart;
            if (genericWebPart != null) {
                Control containedControl = genericWebPart.ChildControl;
                personalizationID = CreatePersonalizationID(containedControl, genericWebPart);

                SetControlDirty(containedControl, personalizationID, /* isWebPartManager */ false, /* forceSetDirty */ false);
            }
        }

        /// <internalonly />
        public override void SetWebPartManagerDirty() {
            SetControlDirty(WebPartManager, WebPartManagerPersonalizationID, /* isWebPartManager */ true,
                            /* forceSetDirty */ false);
        }


        /// <devdoc>
        /// Used to track personalization information, i.e. the data,
        /// and the associated object type and ID.
        /// </devdoc>
        private sealed class PersonalizationInfo {
            public Type _controlType;
            public VirtualPath _controlVPath;
            public string _controlID;
            public bool _isStatic;

            public IDictionary _properties;
            public PersonalizationDictionary _customProperties;

            public bool IsMatchingControlType(Control c) {
                if (c is ProxyWebPart) {
                    // This code path is currently never hit, since we only load personalization data
                    // for ErrorWebPart, and we only replace dynamic WebParts with the ErrorWebPart,
                    // and we only check IsMatchingControlType() for static WebParts.  However, if this
                    // ever changes in the future, we will want to return true for ProxyWebParts.
                    return true;
                }
                else if (_controlType == null) {
                    // _controlType will be null if there is no longer a Type on the system with the
                    // saved type name.
                    return false;
                }
                else if (_controlType == typeof(UserControl)) {
                    UserControl uc = c as UserControl;
                    if (uc != null) {
                        return uc.TemplateControlVirtualPath == _controlVPath;
                    }
                    return false;
                }
                else {
                    return _controlType.IsAssignableFrom(c.GetType());
                }
            }
        }


        /// <devdoc>
        /// Used to track personalization information for a Control instance.
        /// </devdoc>
        private sealed class ControlInfo {
            public Control _control;
            public IDictionary _personalizableProperties;
            public bool _dirty;
            public bool _allowSetDirty;

            public IDictionary _defaultProperties;
            public IDictionary _initialProperties;
            public PersonalizationDictionary _customInitialProperties;
        }

        private enum PersonalizationVersions {
            WhidbeyBeta2 = 1,
            WhidbeyRTM = 2,
        }
    }
}
