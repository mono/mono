// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using XAML3 = System.Windows.Markup;
using System.Xaml;
using MS.Internal.Xaml.Runtime;
using System.Xaml.Schema;
using MS.Internal.Xaml.Parser;
using System.Xaml.MS.Impl;
using System.Xaml.Permissions;
using System.IO;
using System.Windows.Markup;
using System.Windows;

namespace MS.Internal.Xaml.Context
{
    internal class ObjectWriterContext : XamlContext
    {
        private XamlContextStack<ObjectWriterFrame> _stack;

        private object _rootInstance = null;

        ServiceProviderContext _serviceProviderContext;
        XamlRuntime _runtime;
        int _savedDepth;     // The depth of the "saved" part this context is based on.
        bool _nameResolutionComplete;
        XamlObjectWriterSettings _settings;
        List<NameScopeInitializationCompleteSubscriber> _nameScopeInitializationCompleteSubscribers;

        public ObjectWriterContext(XamlSavedContext savedContext, 
            XamlObjectWriterSettings settings, INameScope rootNameScope, XamlRuntime runtime)
            : base(savedContext.SchemaContext)
        {
            _stack = new XamlContextStack<ObjectWriterFrame>(savedContext.Stack, false);
            if (settings != null)
            {
                _settings = settings.StripDelegates();
            }
            _runtime = runtime;
            BaseUri = savedContext.BaseUri;
            // If the bottom of the stack is a (no XamlType) Value (reparse) then back-up onto it.
            // Otherwise add a blank frame to isolate template use from the saved context.
            switch(savedContext.SaveContextType)
            {
            case SavedContextType.Template:
                // Templates always need a root namescope, to isolate them from the rest of the doc
                INameScopeDictionary rootNameScopeDictionary = null;
                if (rootNameScope == null)
                {
#if TARGETTING35SP1
                    rootNameScopeDictionary = new NameScopeDictionary(new NameScope());
#else
                    rootNameScopeDictionary = new NameScope();
#endif
                }
                else
                {
                    rootNameScopeDictionary = rootNameScope as INameScopeDictionary;

                    if (rootNameScopeDictionary == null)
                    {
                        rootNameScopeDictionary = new NameScopeDictionary(rootNameScope);
                    }
                }

                // Push an extra frame to ensure that the template NameScope is
                // not part of the saved context.  Otherwise, the namescope 
                // will hold things alive as long as the template is alive
                _stack.PushScope();
                _savedDepth = _stack.Depth;
                _stack.CurrentFrame.NameScopeDictionary = rootNameScopeDictionary;
                _stack.PushScope();
                break;

            case SavedContextType.ReparseValue:
            case SavedContextType.ReparseMarkupExtension:
                Debug.Assert(rootNameScope == null, "Cannot pass a new namescope in to a reparse context");
                _savedDepth = _stack.Depth - 1;
                break;
            }
        }

        public ObjectWriterContext(XamlSchemaContext schemaContext,
            XamlObjectWriterSettings settings, INameScope rootNameScope, XamlRuntime runtime)
            : base(schemaContext)
        {
            _stack = new XamlContextStack<ObjectWriterFrame>(() => new ObjectWriterFrame());

            INameScopeDictionary rootNameScopeDictionary = null;
            if (rootNameScope == null)
            {
#if TARGETTING35SP1
                rootNameScopeDictionary = new NameScopeDictionary(new NameScope());
#else
                rootNameScopeDictionary = new NameScope();
#endif
            }
            else
            {
                rootNameScopeDictionary = rootNameScope as INameScopeDictionary;

                if (rootNameScopeDictionary == null)
                {
                    rootNameScopeDictionary = new NameScopeDictionary(rootNameScope);
                }
            }
            _stack.CurrentFrame.NameScopeDictionary = rootNameScopeDictionary;
            _stack.PushScope();  // put a blank sentinal frame on the stack.
            if (settings != null)
            {
                _settings = settings.StripDelegates();
            }
            _runtime = runtime;
            _savedDepth = 0;
        }

        public override Assembly LocalAssembly
        {
            get
            {
                Assembly result = base.LocalAssembly;
                if (result == null && _settings != null && _settings.AccessLevel != null)
                {
                    result = Assembly.Load(_settings.AccessLevel.AssemblyAccessToAssemblyName);
                    base.LocalAssembly = result;
                }
                return result;
            }
            protected set { base.LocalAssembly = value; }
        }

        internal ICheckIfInitialized IsInitializedCallback { get; set; }

        internal bool NameResolutionComplete
        {
            get { return _nameResolutionComplete; }
            set
            {
                // Resolution should never become incomplete once it's complete
                Debug.Assert(!_nameResolutionComplete || value);
                _nameResolutionComplete = value;
            }
        }

        internal XamlRuntime Runtime
        {
            get
            {
                return _runtime;
            }
        }

        #region ServiceProvider Interfaces

        // This class doesn't implement the IServiceProvider.  That is done
        // with worker classes ValueConverterContext or MarkupConverterContext.
        // The worker class implements IServiceProvider but uses the real
        // context for the implementation of the actual services.

        internal Type ServiceProvider_Resolve(string qName)
        {
            // As soon as we have the necessary setting on ObjectWriter, we need to start passing
            // the local assembly into the context; currently, this will only return publics.
            XamlType xamlType = ServiceProvider_ResolveXamlType(qName);
            if (xamlType == null || xamlType.UnderlyingType == null)
            {
                XamlTypeName name = XamlTypeName.Parse(qName, this._serviceProviderContext);
                xamlType = this.GetXamlType(name, true, true);
                throw new XamlParseException(SR.Get(SRID.TypeNotFound, xamlType.GetQualifiedName()));
            }
            return xamlType.UnderlyingType;
        }

        internal XamlType ServiceProvider_ResolveXamlType(string qName)
        {
            return ResolveXamlType(qName, true);
        }

        internal AmbientPropertyValue ServiceProvider_GetFirstAmbientValue(IEnumerable<XamlType> ceilingTypes, XamlMember[] properties)
        {
            List<AmbientPropertyValue> valueList = FindAmbientValues(ceilingTypes, /*searchLiveStackOnly*/false, /*types*/null, properties, true);
            return (valueList.Count == 0) ? null : valueList[0];
        }

        internal object ServiceProvider_GetFirstAmbientValue(XamlType[] types)
        {
            List<object> valueList = FindAmbientValues(types, true);
            return (valueList.Count == 0) ? null : valueList[0];
        }

        internal IEnumerable<AmbientPropertyValue> ServiceProvider_GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, XamlMember[] properties)
        {
            List<AmbientPropertyValue> valueList = FindAmbientValues(ceilingTypes, /*searchLiveStackOnly*/false, /*types*/null, properties, /*stopAfterFirst*/ false);
            return valueList;
        }

        internal IEnumerable<object> ServiceProvider_GetAllAmbientValues(XamlType[] types)
        {
            List<object> valueList = FindAmbientValues(types, false);
            return valueList;
        }

        internal IEnumerable<AmbientPropertyValue> ServiceProvider_GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, XamlMember[] properties)
        {
            List<AmbientPropertyValue> valueList = FindAmbientValues(ceilingTypes, searchLiveStackOnly, types, properties, false);
            return valueList;
        }

        private static void CheckAmbient(XamlMember xamlMember)
        {
            if (!xamlMember.IsAmbient)
            {
                throw new ArgumentException(SR.Get(SRID.NotAmbientProperty, xamlMember.DeclaringType.Name, xamlMember.Name), "xamlMember");
            }
        }

        private static void CheckAmbient(XamlType xamlType)
        {
            if (!xamlType.IsAmbient)
            {
                throw new ArgumentException(SR.Get(SRID.NotAmbientType, xamlType.Name), "xamlType");
            }
        }

        internal XamlObjectWriterSettings ServiceProvider_GetSettings()
        {
            if (_settings == null)
            {
                _settings = new XamlObjectWriterSettings();
            }
            return _settings;
        }

       #endregion

        // -----  abstracts overriden from XamlContext.

        public override void AddNamespacePrefix(String prefix, string xamlNS)
        {
            _stack.CurrentFrame.AddNamespace(prefix, xamlNS);
        }

        public override string FindNamespaceByPrefix(string prefix)
        {
            string xamlNs;
            ObjectWriterFrame frame = _stack.CurrentFrame;

            while (frame.Depth > 0)
            {
                if (frame.TryGetNamespaceByPrefix(prefix, out xamlNs))
                {
                    return xamlNs;
                }
                frame = (ObjectWriterFrame)frame.Previous;
            }
            return null;
        }

        public override IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
        {
            ObjectWriterFrame frame = _stack.CurrentFrame;
            Dictionary<string, string> keys = new Dictionary<string, string>();

            while (frame.Depth > 0)
            {
                if (frame._namespaces != null)
                {
                    foreach (NamespaceDeclaration namespaceDeclaration in frame.GetNamespacePrefixes())
                    {
                        if (!keys.ContainsKey(namespaceDeclaration.Prefix))
                        {
                            keys.Add(namespaceDeclaration.Prefix, null);
                            yield return namespaceDeclaration;
                        }
                    }
                }
                frame = (ObjectWriterFrame)frame.Previous;
            }
        }

        // Method to easily add the lineinfo to a Xaml Exception.
        public XamlException WithLineInfo(XamlException ex)
        {
            ex.SetLineInfo(LineNumber, LinePosition);
            return ex;
        }

        // ----- methods to support the Service Providers

        internal ServiceProviderContext ServiceProviderContext    
        {
            get
            {
                if (null == _serviceProviderContext)
                {
                    _serviceProviderContext = new ServiceProviderContext(this);
                }
                return _serviceProviderContext;
            }
        }

        internal XamlType GetDestinationType()
        {
            ObjectWriterFrame frame = _stack.CurrentFrame;

            if (frame == null)
            {
                return null;
            }

            if (frame.Instance != null && frame.XamlType == null)
            {
                //
                // Text/TypeConverter, we need to go up a frame
                frame = frame.Previous as ObjectWriterFrame;
            }

            if (frame.Member == XamlLanguage.Initialization)
            {
                return frame.XamlType;
            }
            return frame.Member.Type;
        }

        private List<AmbientPropertyValue> FindAmbientValues(IEnumerable<XamlType> ceilingTypesEnumerable,
                                                             bool searchLiveStackOnly,
                                                             IEnumerable<XamlType> types,
                                                             XamlMember[] properties,
                                                             bool stopAfterFirst)
        {
            ArrayHelper.ForAll<XamlMember>(properties, CheckAmbient);
            List<XamlType> ceilingTypes = ArrayHelper.ToList<XamlType>(ceilingTypesEnumerable);

            List<AmbientPropertyValue> retList = new List<AmbientPropertyValue>();

            // Start the search for ambient properties and types starting with the parent frame.
            ObjectWriterFrame frame = _stack.PreviousFrame;
            ObjectWriterFrame lowerFrame = _stack.CurrentFrame;

            while (frame.Depth >= 1)
            {
                if (searchLiveStackOnly && frame.Depth <= SavedDepth)
                {
                    break;
                }

                object inst = frame.Instance;

                if (types != null)
                {
                    foreach (XamlType type in types)
                    {
                        if (frame.XamlType != null && frame.XamlType.CanAssignTo(type))
                        {
                            if (inst != null)
                            {
                                AmbientPropertyValue apValue = new AmbientPropertyValue(null, inst);
                                retList.Add(apValue);
                            }
                        }
                    }
                }

                if (properties != null)
                {
                    foreach (XamlMember prop in properties)
                    {
                        bool returnAmbientValue = false;
                        object value = null;

                        if (frame.XamlType != null && frame.XamlType.CanAssignTo(prop.DeclaringType))
                        {
                            if (inst != null)
                            {
                                // If we are searching from inside the target Ambient property,
                                // (like StaticResource inside a ResourceDictionary)
                                // and the object is bottom-up, then it won't be assigned to
                                // the object but will only exist on the parse stack.
                                // If it is top-down it will be attached to the instance already
                                // and the normal path will serve.
                                if (prop == frame.Member && lowerFrame.Instance != null
                                    && lowerFrame.XamlType != null && !lowerFrame.XamlType.IsUsableDuringInitialization)
                                {
                                    // One last thing to check:  If the object we are inside is a ME
                                    // then we are inside a call to ProvideValue and we don't want to
                                    // return a reference to ourselves to ourselves.
                                    if (!typeof(MarkupExtension).IsAssignableFrom(lowerFrame.Instance.GetType()))
                                    {
                                        returnAmbientValue = true;
                                        value = lowerFrame.Instance;
                                    }
                                }
                                else
                                {   // The Ambient Property is either Fully build or not set.

                                    // FIRST: Ask the object (via IQueryAmbient interface) if it has a value for this property.
                                    // This is usefull to prevent needless creation of empty lazy properties.
                                    var ambientCtrl = inst as IQueryAmbient;

                                    // If there is no ambientControl or if ambientControl says YES, then get the property value.
                                    if (ambientCtrl == null || ambientCtrl.IsAmbientPropertyAvailable(prop.Name))
                                    {
                                        // Demand for XamlLoadPermission will fail if we're trying to
                                        // access an internal property from a partial-trust ME/TC
                                        returnAmbientValue = true;
                                        value = _runtime.GetValue(inst, prop);
                                    }
                                }
                            }

                            if (returnAmbientValue)
                            {
                                AmbientPropertyValue apValue = new AmbientPropertyValue(prop, value);
                                retList.Add(apValue);
                            }
                        }
                    }
                }

                if (stopAfterFirst && retList.Count > 0)
                {
                    break;
                }

                if (ceilingTypes != null)
                {
                    if (ceilingTypes.Contains(frame.XamlType))
                    {
                        break;
                    }
                }

                lowerFrame = frame;
                frame = (ObjectWriterFrame)frame.Previous;
                Debug.Assert(frame != null);
            }

            return retList;
        }

        private List<object> FindAmbientValues(XamlType[] types, bool stopAfterFirst)
        {
            ArrayHelper.ForAll<XamlType>(types, CheckAmbient);

            List<object> retList = new List<object>();

            // Start the search for ambient properties with the parent frame.
            ObjectWriterFrame frame = _stack.PreviousFrame;
            ObjectWriterFrame lowerFrame = _stack.CurrentFrame;

            while (frame.Depth >= 1)
            {
                foreach (XamlType type in types)
                {
                    object inst = frame.Instance;

                    if (frame.XamlType != null && frame.XamlType.CanAssignTo(type))
                    {
                        if (inst != null)
                        {
                            retList.Add(inst);
                            if (stopAfterFirst)
                            {
                                return retList;
                            }
                        }
                    }
                }

                lowerFrame = frame;
                frame = (ObjectWriterFrame)frame.Previous;
                Debug.Assert(frame != null);
            }

            return retList;
        }

        // ----- new public methods.

        public void PushScope()
        {
            _stack.PushScope();
        }

        // Don't call this - serious workaround
        public void LiftScope()
        {
            _stack.Depth--;
        }

        // Don't call this - serious workaround
        public void UnLiftScope()
        {
            _stack.Depth++;
        }

        public void PopScope()
        {
            _stack.PopScope();
        }
        
        /// <summary>
        /// Total depth of the stack SavedDepth+LiveDepth
        /// </summary>
        public int Depth
        {
            get { return _stack.Depth; }
        }

        /// <summary>
        /// The Depth of the Saved (template) part.
        /// </summary>
        public int SavedDepth
        {
            get { return _savedDepth; }
        }

        /// <summary>
        /// The Depth of the Stack above the Saved (template) part
        /// </summary>
        public int LiveDepth
        {
            get { return Depth - SavedDepth; }
        }

        public XamlType CurrentType
        {
            get { return _stack.CurrentFrame.XamlType; }
            set { _stack.CurrentFrame.XamlType = value; }
        }

        public XamlType ParentType
        {
            get { return _stack.PreviousFrame.XamlType; }
        }

        public XamlType GrandParentType
        {
            get { return (_stack.PreviousPreviousFrame != null) ? _stack.PreviousPreviousFrame.XamlType : null; }
        }

        public XamlMember CurrentProperty
        {
            get { return _stack.CurrentFrame.Member; }
            set { _stack.CurrentFrame.Member = value; }
        }

        public XamlMember ParentProperty
        {
            get { return _stack.PreviousFrame.Member; }
        }

        public XamlMember GrandParentProperty
        {
            get { return _stack.PreviousPreviousFrame.Member; }
        }

        public Object CurrentInstance
        {
            get { return _stack.CurrentFrame.Instance; }
            set { _stack.CurrentFrame.Instance = value; }
        }

        public Object ParentInstance
        {
            get { return _stack.PreviousFrame.Instance; }
        }

        public Object GrandParentInstance
        {
            get { return (_stack.PreviousPreviousFrame != null) ? _stack.PreviousPreviousFrame.Instance : null; }
        }

        public Object CurrentCollection
        {
            get { return _stack.CurrentFrame.Collection; }
            set { _stack.CurrentFrame.Collection = value; }
        }

        public object ParentCollection
        {
            get { return _stack.PreviousFrame.Collection; }
        }

        public bool CurrentWasAssignedAtCreation
        {
            get { return _stack.CurrentFrame.WasAssignedAtCreation; }
            set { _stack.CurrentFrame.WasAssignedAtCreation = value; }
        }

        public bool CurrentIsObjectFromMember
        {
            get { return _stack.CurrentFrame.IsObjectFromMember; }
            set { _stack.CurrentFrame.IsObjectFromMember = value; }
        }

        public bool ParentIsObjectFromMember
        {
            get { return _stack.PreviousFrame.IsObjectFromMember; }
        }

        public bool GrandParentIsObjectFromMember
        {
            get { return (_stack.PreviousPreviousFrame != null) ? _stack.PreviousPreviousFrame.IsObjectFromMember : false; }
        }

        public bool CurrentIsPropertyValueSet
        {
            // get { return _stack.CurrentFrame.IsPropertyValueSet; }  Currently Unused (FxCop)
            set { _stack.CurrentFrame.IsPropertyValueSet = value; }
        }

        public bool ParentIsPropertyValueSet
        {
            get { return _stack.PreviousFrame.IsPropertyValueSet; }
            set { _stack.PreviousFrame.IsPropertyValueSet = value; }
        }

        public bool CurrentIsTypeConvertedObject
        {
            get { return _stack.CurrentFrame.IsTypeConvertedObject; }
            set { _stack.CurrentFrame.IsTypeConvertedObject = value; }
        }

        public Dictionary<XamlMember, object> CurrentPreconstructionPropertyValues
        {
            get { return _stack.CurrentFrame.PreconstructionPropertyValues; }
        }

        public bool CurrentHasPreconstructionPropertyValuesDictionary
        {
            get { return _stack.CurrentFrame.HasPreconstructionPropertyValuesDictionary; }
        }

        public Dictionary<XamlMember, object> ParentPreconstructionPropertyValues
        {
            get { return _stack.PreviousFrame.PreconstructionPropertyValues; }
        }

        public HashSet<XamlMember> CurrentAssignedProperties
        {
            get { return _stack.CurrentFrame.AssignedProperties; }
        }

        public HashSet<XamlMember> ParentAssignedProperties
        {
            get { return _stack.PreviousFrame.AssignedProperties; }
        }

        public string CurrentInstanceRegisteredName
        {
            get { return _stack.CurrentFrame.InstanceRegisteredName; }
            set { _stack.CurrentFrame.InstanceRegisteredName = value; }
        }

        public string ParentInstanceRegisteredName
        {
            get { return _stack.PreviousFrame.InstanceRegisteredName; }
            set { _stack.PreviousFrame.InstanceRegisteredName = value; }
        }

        public Uri BaseUri { get; set; }

        public int LineNumber { get; set; }
        public int LinePosition { get; set; }

        // Used only for BeginInitHandler, in place of BaseUri.
        public Uri SourceBamlUri
        {
            get { return _settings != null ? _settings.SourceBamlUri : null; }
        }
        
        // This specifically stores the start line number for a start object for consistency
        public int LineNumber_StartObject { get; set; }

        // This specifically stores the start line position for a start object for consistency
        public int LinePosition_StartObject { get; set; }

        public INameScopeDictionary CurrentNameScope
        {
            get
            {
                return LookupNameScopeDictionary((ObjectWriterFrame)_stack.CurrentFrame);
            }
        }

        public INameScopeDictionary ParentNameScope
        {
            get
            {
                return LookupNameScopeDictionary((ObjectWriterFrame)_stack.PreviousFrame);
            }
        }

        public INameScopeDictionary GrandParentNameScope
        {
            get
            {
                return LookupNameScopeDictionary((ObjectWriterFrame)_stack.PreviousPreviousFrame);
            }
        }

        public INameScopeDictionary RootNameScope
        {
            get
            {
                ObjectWriterFrame rootFrame = _stack.GetFrame(SavedDepth + 1);
                return LookupNameScopeDictionary(rootFrame);
            }
        }

        /// <summary>
        /// From x:Arguments or ME positional syntax.
        /// </summary>
        public object[] CurrentCtorArgs
        {
            get { return _stack.CurrentFrame.PositionalCtorArgs; }
            set { _stack.CurrentFrame.PositionalCtorArgs = value; }
        }

        public object CurrentKey
        {
            get { return _stack.CurrentFrame.Key; }
        }

        public bool CurrentIsKeySet
        {
            get { return _stack.CurrentFrame.IsKeySet; }
        }

        public object ParentKey
        {
            get { return _stack.PreviousFrame.Key; }
            set
            {
                _stack.PreviousFrame.Key = value;
            }
        }

        public bool CurrentKeyIsUnconverted
        {
            get { return _stack.CurrentFrame.KeyIsUnconverted; }
            set { _stack.CurrentFrame.KeyIsUnconverted = value; }
        }

        public bool ParentKeyIsUnconverted
        {
            // Write-only property: Getter was dead code, so removed per FxCop; add back if needed.
            set { _stack.PreviousFrame.KeyIsUnconverted = value; }
        }

        public bool ParentShouldConvertChildKeys
        {
            get { return _stack.PreviousFrame.ShouldConvertChildKeys; }
            set { _stack.PreviousPreviousFrame.ShouldConvertChildKeys = value; }
        }

        public bool GrandParentShouldConvertChildKeys
        {
            get { return _stack.PreviousPreviousFrame.ShouldConvertChildKeys; }
            set { _stack.PreviousPreviousFrame.ShouldConvertChildKeys = value; }
        }

        public bool ParentShouldNotConvertChildKeys
        {
            get { return _stack.PreviousFrame.ShouldNotConvertChildKeys; }
            set { _stack.PreviousPreviousFrame.ShouldNotConvertChildKeys = value; }
        }

        public bool GrandParentShouldNotConvertChildKeys
        {
            get { return _stack.PreviousPreviousFrame.ShouldNotConvertChildKeys; }
        }

        public object RootInstance
        {
            get
            {
                //evaluate if _rootInstance should just always look at _rootFrame.Instance instead of caching an instance
                if (_rootInstance == null)
                {
                    ObjectWriterFrame rootFrame = GetTopFrame();
                    _rootInstance = rootFrame.Instance;
                }
                return _rootInstance;
            }
        }

        // Consider replacing GetTopFrame with _rootFrame, _liveRootFrame
        private ObjectWriterFrame GetTopFrame()
        {
            if (_stack.Depth == 0)
            {
                return null;
            }

            XamlFrame frame = _stack.CurrentFrame;
            while (frame.Depth > 1)
            {
                frame = frame.Previous;
            }
            return (ObjectWriterFrame)frame;
        }

        private INameScopeDictionary LookupNameScopeDictionary(ObjectWriterFrame frame)
        {
            if (frame.NameScopeDictionary == null)
            {
                if (frame.XamlType != null && frame.XamlType.IsNameScope)
                {
                    frame.NameScopeDictionary = frame.Instance as INameScopeDictionary ?? new NameScopeDictionary(frame.Instance as INameScope);
                }
                if (frame.NameScopeDictionary == null)
                {
                    if (frame.Depth == 1)
                    {
                        frame.NameScopeDictionary = HuntAroundForARootNameScope(frame);
                    }
                    else if (frame.Depth > 1)
                    {
                        if (frame.Depth == SavedDepth + 1 &&
                            _settings != null && !_settings.RegisterNamesOnExternalNamescope)
                        {
#if TARGETTING35SP1
                            frame.NameScopeDictionary = new NameScopeDictionary(new NameScope());
#else
                            frame.NameScopeDictionary = new NameScope();
#endif
                        }
                        else
                        {
                            var parentFrame = (ObjectWriterFrame)frame.Previous;
                            frame.NameScopeDictionary = LookupNameScopeDictionary(parentFrame);
                        }
                    }
                }
            }
            // We are sure to find a name scope at the root (at least).
            Debug.Assert(frame.NameScopeDictionary != null || frame.Depth == 0);
            return frame.NameScopeDictionary;
        }

        public IEnumerable<INameScopeDictionary> StackWalkOfNameScopes
        {
            get
            {
                var frame = (ObjectWriterFrame)_stack.CurrentFrame;
                INameScopeDictionary previousNameScopeDictionary = null;
                INameScopeDictionary nameScopeDictionary = null;
                while (frame.Depth > 0)
                {
                    nameScopeDictionary = LookupNameScopeDictionary(frame);
                    Debug.Assert(nameScopeDictionary != null);
                    if (frame.NameScopeDictionary != previousNameScopeDictionary)
                    {
                        previousNameScopeDictionary = nameScopeDictionary;
                        yield return nameScopeDictionary;
                    }
                    frame = (ObjectWriterFrame)frame.Previous;
                }
                // return the provided root namescope if it's different from the document root namescope
                if (frame.NameScopeDictionary != null && frame.NameScopeDictionary != previousNameScopeDictionary)
                {
                    yield return frame.NameScopeDictionary;
                }
            }
        }

        public bool IsOnTheLiveStack(object instance)
        {
            var frame = (ObjectWriterFrame)_stack.CurrentFrame;
            while (frame.Depth > SavedDepth)
            {
                if (instance == frame.Instance)
                {
                    return true;
                }
                frame = (ObjectWriterFrame)frame.Previous;
            }
            return false;
        }

        private INameScopeDictionary HuntAroundForARootNameScope(ObjectWriterFrame rootFrame)
        {
            Debug.Assert(rootFrame.Depth == 1);

            object inst = rootFrame.Instance;
            if (inst == null && rootFrame.XamlType.IsNameScope)
            {
                throw new InvalidOperationException(SR.Get(SRID.NameScopeOnRootInstance));
            }

            INameScopeDictionary nameScopeDictionary = null;

            nameScopeDictionary = inst as INameScopeDictionary;

            if (nameScopeDictionary == null)
            {
                INameScope nameScope = inst as INameScope;
                if (nameScope != null)
                {
                    nameScopeDictionary = new NameScopeDictionary(nameScope);
                }
            }
            
            // If the root instance isn't a name scope
            // then perhaps it designated a property as the name scope.
            if (nameScopeDictionary == null)
            {
                XamlType xamlType = rootFrame.XamlType;
                if (xamlType.UnderlyingType != null)
                {
                    // Get the Name Scope Property (from attribute on the class)
                    XamlMember nameScopeProperty = TypeReflector.LookupNameScopeProperty(xamlType);
                    if (nameScopeProperty != null)
                    {
                        // Read the value of the property.  If it is an object we are good.
                        // if it is null create a stock name scope dictionary object and assign it back.
                        INameScope nameScope = (INameScope)_runtime.GetValue(inst, nameScopeProperty, false);
                        if (nameScope == null)
                        {
#if TARGETTING35SP1
                            nameScopeDictionary = new NameScopeDictionary(new NameScope());
#else
                            nameScopeDictionary = new NameScope();
#endif
                            _runtime.SetValue(inst, nameScopeProperty, nameScopeDictionary);
                        }
                        else
                        {
                            nameScopeDictionary = nameScope as INameScopeDictionary;
                            if (nameScopeDictionary == null)
                            {
                                nameScopeDictionary = new NameScopeDictionary(nameScope);
                            }
                        }
                    }
                }
            }

            if (nameScopeDictionary == null && _settings != null 
                && _settings.RegisterNamesOnExternalNamescope)
            {
                ObjectWriterFrame frameZero = (ObjectWriterFrame)rootFrame.Previous;
                nameScopeDictionary = frameZero.NameScopeDictionary;
            }

            // Otherwise we still need a namescope at the root of the parse
            // for our own usage.  For IXamlNameResolver() to use.
            if (nameScopeDictionary == null)
            {
#if TARGETTING35SP1
                nameScopeDictionary = new NameScopeDictionary(new NameScope());
#else
                nameScopeDictionary = new NameScope();
#endif
            }

            rootFrame.NameScopeDictionary = nameScopeDictionary;
            return nameScopeDictionary;
        }

        public XamlSavedContext GetSavedContext(SavedContextType savedContextType)
        {
            // Ensure that we have a root namescope before cloning the stack
            ObjectWriterFrame topFrame = GetTopFrame();
            if (topFrame.NameScopeDictionary == null)
            {
                topFrame.NameScopeDictionary = LookupNameScopeDictionary(topFrame);
            }

            // Clone the stack
            var newStack = new XamlContextStack<ObjectWriterFrame>(_stack, true);            
            XamlSavedContext savedContext = new XamlSavedContext(savedContextType, this, newStack);
            return savedContext;
        }

        public object ResolveName(string name, out bool isFullyInitialized)
        {
            isFullyInitialized = false;
            object value = null;
            foreach (INameScope nameScope in StackWalkOfNameScopes)
            {
                object obj = nameScope.FindName(name);
                if (obj != null)
                {
                    if (IsInitializedCallback != null)
                    {
                        isFullyInitialized = IsInitializedCallback.IsFullyInitialized(obj);
                    }
                    if (NameResolutionComplete || isFullyInitialized || IsInitializedCallback == null)
                    {
                        value = obj;
                    }
                    break;
                }
            }
            return value;
        }

        public IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope()
        {
            List<KeyValuePair<string, object>> allNamesAndValues = new List<KeyValuePair<string, object>>();
            //
            // This could be optimized further by enumerating the collection of namescopes, getting the NamesAndValues
            // from each, calculating the total size required, pre-allocating the final collection, and then
            // inserting the names and values from each name scope into it.
            // However unless we have a lot of namescopes in the graph, which doesn't seem likely, this seems like overkill
            foreach (INameScopeDictionary nameScopeDictionary in StackWalkOfNameScopes)
            {
                foreach (KeyValuePair<string, object> nameValuePair in nameScopeDictionary)
                {
                    if (allNamesAndValues.Exists(pair => pair.Key == nameValuePair.Key))
                    {
                        continue;
                    }
                    allNamesAndValues.Add(nameValuePair);
                }
            }
            return allNamesAndValues;
        }

        internal void AddNameScopeInitializationCompleteSubscriber(EventHandler handler)
        {
            if (_nameScopeInitializationCompleteSubscribers == null)
            {
                _nameScopeInitializationCompleteSubscribers = new List<NameScopeInitializationCompleteSubscriber>();
            }

            var subscriber = new NameScopeInitializationCompleteSubscriber { Handler = handler };
            subscriber.NameScopeDictionaryList.AddRange(StackWalkOfNameScopes);

            _nameScopeInitializationCompleteSubscribers.Add(subscriber);
        }

        internal void RemoveNameScopeInitializationCompleteSubscriber(EventHandler handler)
        {
            var subscriber = _nameScopeInitializationCompleteSubscribers.Find(o => o.Handler == handler);
            if (subscriber != null)
            {
                _nameScopeInitializationCompleteSubscribers.Remove(subscriber);
            }
        }

        internal void RaiseNameScopeInitializationCompleteEvent()
        {
            if (_nameScopeInitializationCompleteSubscribers != null)
            {
                EventArgs e = new EventArgs();
                foreach (var subscriber in _nameScopeInitializationCompleteSubscribers)
                {
                    var resolver = new StackWalkNameResolver(subscriber.NameScopeDictionaryList);
                    subscriber.Handler(resolver, e);
                }
            }
        }

        internal class NameScopeInitializationCompleteSubscriber
        {
            List<INameScopeDictionary> _nameScopeDictionaryList = new List<INameScopeDictionary>();

            public EventHandler Handler
            {
                get; set;
            }

            public List<INameScopeDictionary> NameScopeDictionaryList
            {
                get { return _nameScopeDictionaryList; }
            }
        }

        private class StackWalkNameResolver : IXamlNameResolver
        {
            List<INameScopeDictionary> _nameScopeDictionaryList;

            public StackWalkNameResolver(List<INameScopeDictionary> nameScopeDictionaryList)
            {
                _nameScopeDictionaryList = nameScopeDictionaryList;
            }

            public bool IsFixupTokenAvailable
            {
                get
                {
                    return false;
                }
            }

            public object GetFixupToken(IEnumerable<string> name)
            {
                return null;
            }

            public object GetFixupToken(IEnumerable<string> name, bool canAssignDirectly)
            {
                return null;
            }

            public event EventHandler OnNameScopeInitializationComplete
            {
                // at this point all name scopes have been completed, and we will 
                // not raise any event for subscriptions that come after this.
                add
                {
                }

                remove
                {
                }
            }

            public object Resolve(string name)
            {
                object value = null;
                foreach (INameScopeDictionary nameScope in _nameScopeDictionaryList)
                {
                    object obj = nameScope.FindName(name);
                    if (obj != null)
                    {
                        value = obj;
                        break;
                    }
                }
                return value;
            }

            public object Resolve(string name, out bool isFullyInitialized)
            {
                // This resolver is only used after the parse is complete, including completing
                // name references. So all objects are fully initialized.
                object result = Resolve(name);
                isFullyInitialized = (result != null);
                return result;
            }

            public IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope()
            {
                List<KeyValuePair<string, object>> allNamesAndValues = new List<KeyValuePair<string, object>>();

                foreach (INameScopeDictionary nameScopeDictionary in _nameScopeDictionaryList)
                {
                    foreach (KeyValuePair<string, object> nameValuePair in nameScopeDictionary)
                    {
                        if (allNamesAndValues.Exists(pair => pair.Key == nameValuePair.Key))
                        {
                            continue;
                        }
                        allNamesAndValues.Add(nameValuePair);
                    }
                }
                return allNamesAndValues;
            }
        }
    }
}
