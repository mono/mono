// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using XAML3 = System.Windows.Markup;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Text;
using MS.Internal.Xaml.Context;
using MS.Internal.Xaml.Runtime;
using System.Xaml.Schema;
using System.Xaml.MS.Impl;
using MS.Internal.Xaml.Parser;
using System.Windows.Markup;
using System.Globalization;


namespace System.Xaml
{
    public class XamlObjectWriter : XamlWriter, IXamlLineInfoConsumer, IAddLineInfo, ICheckIfInitialized
    {
        object _lastInstance;
        bool _inDispose;
        ObjectWriterContext _context;
        DeferringWriter _deferringWriter;
        private EventHandler<XamlObjectEventArgs> _afterBeginInitHandler;
        private EventHandler<XamlObjectEventArgs> _beforePropertiesHandler;
        private EventHandler<XamlObjectEventArgs> _afterPropertiesHandler;
        private EventHandler<XamlObjectEventArgs> _afterEndInitHandler;
#if !TARGETTING35SP1
        private EventHandler<XAML3.XamlSetValueEventArgs> _xamlSetValueHandler;
#endif
        private object _rootObjectInstance;
        private bool _skipDuplicatePropertyCheck;
        NameFixupGraph _nameFixupGraph;
        private Dictionary<object, List<PendingCollectionAdd>> _pendingCollectionAdds;
        INameScope _rootNamescope;
        bool _skipProvideValueOnRoot;
        bool _nextNodeMustBeEndMember;
        bool _preferUnconvertedDictionaryKeys;
        private Dictionary<object, ObjectWriterContext> _pendingKeyConversionContexts;

#if DEBUG
        private bool _inNameResolution;
#endif

        public XamlObjectWriter(XamlSchemaContext schemaContext)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            Initialize(schemaContext, (XamlSavedContext)null, (XamlObjectWriterSettings)null);
        }

        public XamlObjectWriter(XamlSchemaContext schemaContext, XamlObjectWriterSettings settings)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            Initialize(schemaContext, (XamlSavedContext)null, settings);
        }

        internal XamlObjectWriter(XamlSavedContext savedContext, XamlObjectWriterSettings settings)
        {
            if (savedContext == null)
            {
                throw new ArgumentNullException("savedContext");
            }
            if (savedContext.SchemaContext == null)
            {
                throw new ArgumentException(SR.Get(SRID.SavedContextSchemaContextNull), "savedContext");
            }
            Initialize(savedContext.SchemaContext, savedContext, settings);
        }

        void Initialize(XamlSchemaContext schemaContext, XamlSavedContext savedContext, XamlObjectWriterSettings settings)
        {
            _inDispose = false;
            //ObjectWriter must be passed in a non-null SchemaContext.  We check that here, since the CreateContext method
            //will create one if a null SchemaContext was passed in.
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            if (savedContext != null && schemaContext != savedContext.SchemaContext)
            {
                throw new ArgumentException(SR.Get(SRID.SavedContextSchemaContextMismatch), "schemaContext");
            }

            if (settings != null)
            {
                _afterBeginInitHandler = settings.AfterBeginInitHandler;
                _beforePropertiesHandler = settings.BeforePropertiesHandler;
                _afterPropertiesHandler = settings.AfterPropertiesHandler;
                _afterEndInitHandler = settings.AfterEndInitHandler;
#if !TARGETTING35SP1
                _xamlSetValueHandler = settings.XamlSetValueHandler;
#endif
                _rootObjectInstance = settings.RootObjectInstance;
                _skipDuplicatePropertyCheck = settings.SkipDuplicatePropertyCheck;
                _skipProvideValueOnRoot = settings.SkipProvideValueOnRoot;
                _preferUnconvertedDictionaryKeys = settings.PreferUnconvertedDictionaryKeys;
            }

            INameScope rootNameScope = (settings != null) ? settings.ExternalNameScope : null;

            XamlRuntime runtime = CreateRuntime(settings, schemaContext);

            if (savedContext != null)
            {
                _context = new ObjectWriterContext(savedContext, settings, rootNameScope, runtime);
            }
            else if (schemaContext != null)
            {
                _context = new ObjectWriterContext(schemaContext, settings, rootNameScope, runtime);
                _context.AddNamespacePrefix(KnownStrings.XmlPrefix, XamlLanguage.Xml1998Namespace);
            }
            else
            {
                throw _context.WithLineInfo(new XamlInternalException());
            }
            _context.IsInitializedCallback = this;

            _deferringWriter = new DeferringWriter(_context);
            _rootNamescope = null;
        }

        private XamlRuntime CreateRuntime(XamlObjectWriterSettings settings, XamlSchemaContext schemaContext)
        {
            XamlRuntime result = null;
            XamlRuntimeSettings runtimeSettings = null;
            if (settings != null)
            {
                runtimeSettings = new XamlRuntimeSettings { IgnoreCanConvert = settings.IgnoreCanConvert };
#if !TARGETTING35SP1
                if (settings.AccessLevel != null)
                {
                    result = new PartialTrustTolerantRuntime(runtimeSettings, settings.AccessLevel, schemaContext);
                }
            }
            if (result == null)
            {
#endif
                result = new ClrObjectRuntime(runtimeSettings, true /*isWriter*/);
            }
            result.LineInfo = this;
            return result;
        }

        protected virtual void OnAfterBeginInit(Object value)
        {
            if (_afterBeginInitHandler != null)
            {
                // Currently SourceBamlUri exists only to address pre .NET 4.6 compat issue. It is not
                // null only when we load system resources like themes\generic.xaml. And only for
                // resources located in RD itself (as opposite to nested RDs). Thus we should never see
                // different non-null base and source URIs. In any case base URI is preferred.
                Debug.Assert(_context.SourceBamlUri == null || _context.BaseUri == null || _context.SourceBamlUri == _context.BaseUri,
                    "Source BAML URI and base URI do not agree");
                _afterBeginInitHandler(this, new XamlObjectEventArgs(value, _context.BaseUri ?? _context.SourceBamlUri, _context.LineNumber_StartObject, _context.LinePosition_StartObject));
            }
        }

        protected virtual void OnBeforeProperties(Object value)
        {
            if (_beforePropertiesHandler != null)
            {
                _beforePropertiesHandler(this, new XamlObjectEventArgs(value));
            }
        }

        protected virtual void OnAfterProperties(Object value)
        {
            if (_afterPropertiesHandler != null)
            {
                _afterPropertiesHandler(this, new XamlObjectEventArgs(value));
            }
        }

        protected virtual void OnAfterEndInit(Object value)
        {
            if (_afterEndInitHandler != null)
            {
                _afterEndInitHandler(this, new XamlObjectEventArgs(value));
            }
        }

#if !TARGETTING35SP1
        protected virtual bool OnSetValue(object eventSender, XamlMember member, object value)
        {
            if (_xamlSetValueHandler != null)
            {
                var e = new XamlSetValueEventArgs(member, value);
                _xamlSetValueHandler(eventSender, e);
                return e.Handled;
            }
            return false;
        }
#endif

        private NameFixupGraph NameFixupGraph
        {
            get
            {
                if (_nameFixupGraph == null)
                {
                    _nameFixupGraph = new NameFixupGraph();
                }
                return _nameFixupGraph;
            }
        }

        private bool HasUnresolvedChildren(object parent)
        {
            if (_nameFixupGraph == null)
            {
                return false;
            }
            return _nameFixupGraph.HasUnresolvedChildren(parent);
        }

        private Dictionary<object, List<PendingCollectionAdd>> PendingCollectionAdds
        {
            get
            {
                if (_pendingCollectionAdds == null)
                    _pendingCollectionAdds = new Dictionary<object, List<PendingCollectionAdd>>();
                return _pendingCollectionAdds;
            }
        }

        private Dictionary<object, ObjectWriterContext> PendingKeyConversionContexts
        {
            get
            {
                if (_pendingKeyConversionContexts == null)
                    _pendingKeyConversionContexts = new Dictionary<object, ObjectWriterContext>();
                return _pendingKeyConversionContexts;
            }
        }

        private XamlRuntime Runtime
        {
            get { return _context.Runtime; }
        }

        private void TryCreateParentInstance(ObjectWriterContext ctx)
        {
            if (ctx.ParentInstance == null && ctx.ParentProperty != XamlLanguage.Arguments)
            {
                ctx.LiftScope();
                Logic_CreateAndAssignToParentStart(ctx);
                ctx.UnLiftScope();
            }
        }

        public override void WriteGetObject()
        {
            ThrowIfDisposed();

            // Deferring Checking
            //
            _deferringWriter.WriteGetObject();
            if (_deferringWriter.Handled)
            {
                return;
            }

            // Error Checking
            //
            if (_nextNodeMustBeEndMember)
            {
                string err = SR.Get(SRID.ValueMustBeFollowedByEndMember);
                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }
            XamlMember parentProperty = (_context.CurrentType == null && _context.Depth > 1)
                ? _context.ParentProperty
                : _context.CurrentProperty;  // there is a push frame below making this the parent property.
            if (parentProperty == null)
            {
                XamlType xamlType = (_context.CurrentType == null && _context.Depth > 1)
                    ? _context.ParentType
                    : _context.CurrentType;

                string err = (xamlType != null)
                    ? SR.Get(SRID.NoPropertyInCurrentFrame_GO, xamlType.ToString())
                    : SR.Get(SRID.NoPropertyInCurrentFrame_GO_noType);

                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }

            // Real processing begins here.
            //

            //The first node (T, SO, or EP) after an EndObject should null out _lastInstance.
            _lastInstance = null;

            // A Frame is pushed by either a AddNamespace or a WriteGet/StartObject
            if (_context.CurrentType != null)
            {
                _context.PushScope();
            }

            TryCreateParentInstance(_context);

            _context.CurrentIsObjectFromMember = true;

            object parentInstance = _context.ParentInstance;
            _context.CurrentType = parentProperty.Type;

            object inst = Runtime.GetValue(parentInstance, parentProperty);
            if (inst == null)
            {
                throw _context.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.GetObjectNull, parentInstance.GetType(), parentProperty.Name)));
            }
            _context.CurrentInstance = inst;
            if (parentProperty.Type.IsCollection || parentProperty.Type.IsDictionary)
            {
                _context.CurrentCollection = inst;
            }
        }

        public override void WriteStartObject(XamlType xamlType)
        {
            ThrowIfDisposed();
            if (xamlType == null)
            {
                throw new ArgumentNullException("xamlType");
            }

            // Deferring Checking
            //
            _deferringWriter.WriteStartObject(xamlType);
            if (_deferringWriter.Handled)
            {
                return;
            }

            // This is to store the correct start line number and position
            // of start object as _context.LineNumber & Position might get updated
            // to the next start object if the current object is created through StartMember function
            _context.LineNumber_StartObject = _context.LineNumber;
            _context.LinePosition_StartObject = _context.LinePosition;

            // Error Checking
            //
            if (_nextNodeMustBeEndMember)
            {
                string err = SR.Get(SRID.ValueMustBeFollowedByEndMember);
                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }
            if (xamlType.IsUnknown)
            {
                string err = SR.Get(SRID.CantCreateUnknownType, xamlType.GetQualifiedName());
                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }

            if (_context.CurrentType != null && _context.CurrentProperty == null)
            {
                string err = SR.Get(SRID.NoPropertyInCurrentFrame_SO, xamlType.ToString(),
                                                                    _context.CurrentType.ToString());
                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }

            // Real processing begins here.
            //

            //The first node (T, SO, or EP) after an EndObject should null out _lastInstance.
            _lastInstance = null;

            // A Frame is pushed by either a AddNamespace or a WriteGet/StartObject
            if (_context.CurrentType != null)
            {
                _context.PushScope();
            }

            _context.CurrentType = xamlType;

            // Don't create the Root Instance if we were given one in the settings.
            // This is an important senario when a XamlObject loads a XamlDefinition of itself
            // in it's constructor.  The instance is already created (that is how we got into
            // the constructor), now don't create the first StartObject use the existing instance.
            //
            if ((_context.LiveDepth == 1) && (_rootObjectInstance != null))
            {
                XamlType rootType = GetXamlType(_rootObjectInstance.GetType());
                if (!rootType.CanAssignTo(_context.CurrentType))
                {
                    throw new XamlParseException(SR.Get(SRID.CantAssignRootInstance,
                        rootType.GetQualifiedName(), xamlType.GetQualifiedName()));
                }
                _context.CurrentInstance = _rootObjectInstance;
                if (_context.CurrentType.IsCollection || _context.CurrentType.IsDictionary)
                {
                    _context.CurrentCollection = _rootObjectInstance;
                }
                Logic_BeginInit(_context);
            }

        }

        public override void WriteEndObject()
        {
            ThrowIfDisposed();

            // Deferring Checking
            //
            _deferringWriter.WriteEndObject();
            if (_deferringWriter.Handled)
            {
                if (_deferringWriter.Mode == DeferringMode.TemplateReady)
                {
                    Debug.Assert(_context.CurrentProperty.DeferringLoader != null);
                    XamlNodeList templateList = _deferringWriter.CollectTemplateList();
                    _context.PushScope();
                    _context.CurrentInstance = (XamlReader)templateList.GetReader();
                }
                return;
            }

            // Error Checking
            //
            if (_nextNodeMustBeEndMember)
            {
                string err = SR.Get(SRID.ValueMustBeFollowedByEndMember);
                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }
            if (_context.CurrentType == null)
            {
                string err = SR.Get(SRID.NoTypeInCurrentFrame_EO);
                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }
            if (_context.CurrentProperty != null)
            {
                string err = SR.Get(SRID.OpenPropertyInCurrentFrame_EO, _context.CurrentType.ToString(),
                                                                        _context.CurrentProperty.ToString());
                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }

            // Real processing begins here.
            //
            Debug.Assert(_context.LiveDepth > 0);
            bool hasUnresolvedChildren = HasUnresolvedChildren(_context.CurrentInstance);
            bool isFixupToken = _context.CurrentInstance is NameFixupToken;

            if (!_context.CurrentIsObjectFromMember)
            {
                // We defer creation in WriteObject because we might have args for
                // Create From InitText, or Create with parameters.
                // But, If we got to End Object and still haven't Created it,
                // then create it now.
                if (_context.CurrentInstance == null)
                {
                    Logic_CreateAndAssignToParentStart(_context);
                }

                XamlType xamlType = _context.CurrentType;
                object instance = _context.CurrentInstance;
                OnAfterProperties(instance);

                if (_context.CurrentType.IsMarkupExtension)
                {
                    // Can't call EndInit() or ProvideValue() if all the properties are not in.
                    if (hasUnresolvedChildren)
                    {
                        Logic_DeferProvideValue(_context);
                    }
                    else
                    {
                        ExecutePendingAdds(_context.CurrentType, _context.CurrentInstance);
                        Logic_EndInit(_context);
                        instance = _context.CurrentInstance;
                        Logic_AssignProvidedValue(_context);
                        if (_context.CurrentInstanceRegisteredName != null)
                        {
                            // Names on MEs apply to the ME, not to the provided value
                            if (_nameFixupGraph != null)
                            {
                                TriggerNameResolution(instance, _context.CurrentInstanceRegisteredName);
                            }
                            _context.CurrentInstanceRegisteredName = null;
                        }
                        instance = _context.CurrentInstance;
                        isFixupToken = instance is NameFixupToken;
                        hasUnresolvedChildren = !isFixupToken && HasUnresolvedChildren(instance);
                    }
                }
                else
                {
                    if (_context.LiveDepth > 1)
                    {
                        if (!_context.CurrentWasAssignedAtCreation)
                        {
                            Logic_DoAssignmentToParentProperty(_context);
                        }
                    }

                    // Can't call EndInit() on an object if any of its properties are not resolved.
                    //
                    if (hasUnresolvedChildren)
                    {
                        if (_context.LiveDepth > 1)
                        {
                            Logic_AddDependencyForUnresolvedChildren(_context, null);
                        }
                    }
                    else if (!isFixupToken)
                    {
                        ExecutePendingAdds(_context.CurrentType, _context.CurrentInstance);
                        Logic_EndInit(_context);
                    }
                }
            }
            else // object is retrieved
            {
                if (hasUnresolvedChildren)
                {
                    Debug.Assert(_context.LiveDepth > 1);
                    Logic_AddDependencyForUnresolvedChildren(_context, null);
                }
                else
                {
                    ExecutePendingAdds(_context.CurrentType, _context.CurrentInstance);
                }

                if (_context.ParentIsPropertyValueSet)
                {
                    throw _context.WithLineInfo(new XamlDuplicateMemberException(
                                            _context.ParentProperty,
                                            _context.ParentType));
                }
            }
            _lastInstance = _context.CurrentInstance;
            string name = _context.CurrentInstanceRegisteredName;

            if (_context.LiveDepth == 1)
            {
                _rootNamescope = _context.RootNameScope;
            }

            // We must PopScope() before Forward Reference Resolution.  Because
            // References are only returned to "completed" object.  Completed
            // is determined in part by the object's absence from the builder stack.
            _context.PopScope();

            if (hasUnresolvedChildren)
            {
                _nameFixupGraph.IsOffTheStack(_lastInstance, name, _context.LineNumber, _context.LinePosition);
            }
            else if (isFixupToken)
            {
                if (name != null)
                {
                    NameFixupToken token = (NameFixupToken)_lastInstance;
                    if (token.FixupType == FixupType.ObjectInitializationValue && !token.CanAssignDirectly)
                    {
                        // This is a TypeConverted object, but the converter returned a FixupToken.
                        // We need to put the name on the token's saved context, so that we can register
                        // the name to the actual object when we finally create it.
                        var objectFrame = (ObjectWriterFrame)token.SavedContext.Stack.PreviousFrame;
                        objectFrame.InstanceRegisteredName = name;
                    }
                }
            }
            else if (_nameFixupGraph != null)
            {
                TriggerNameResolution(_lastInstance, name);
            }

            if (_context.LiveDepth == 0 && !_inDispose)
            {
                CompleteNameReferences();
                _context.RaiseNameScopeInitializationCompleteEvent();
            }
        }

        public override void WriteStartMember(XamlMember property)
        {
            ThrowIfDisposed();
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            // Deferring Checking
            //
            _deferringWriter.WriteStartMember(property);
            if (_deferringWriter.Handled)
            {
                return;
            }

            // Error Checking
            //
            string err = null;
            if (_nextNodeMustBeEndMember)
            {
                err = SR.Get(SRID.ValueMustBeFollowedByEndMember);
            }
            else if (property == XamlLanguage.UnknownContent)
            {
                err = SR.Get(SRID.TypeHasNoContentProperty, _context.CurrentType);
            }
            else if (property.IsUnknown)
            {
                err = SR.Get(SRID.CantSetUnknownProperty, property.ToString());
            }
            else if (_context.CurrentProperty != null)
            {
                err = SR.Get(SRID.OpenPropertyInCurrentFrame_SM, _context.CurrentType.ToString(),
                                                                 _context.CurrentProperty.ToString(),
                                                                 property.ToString());
            }
            else if (_context.CurrentType == null)
            {
                err = SR.Get(SRID.NoTypeInCurrentFrame_SM, property.ToString());
            }

            if (err != null)
            {
                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }

            // Real processing starts here.
            //
            Debug.Assert(_context.LiveDepth > 0);
            _context.CurrentProperty = property;

            // Duplicate Property Setting Check.
            //
            Logic_DuplicatePropertyCheck(_context, property, false /*onParent*/);

            // If we haven't created the object yet then consider creating it now.
            // We need an object instance to set property values on.
            if (_context.CurrentInstance == null)
            {
                if (!IsConstructionDirective(_context.CurrentProperty)
                    && !IsDirectiveAllowedOnNullInstance(_context.CurrentProperty, _context.CurrentType))
                {
                    Logic_CreateAndAssignToParentStart(_context);
                }

                // Creates the backing collection for x:PositionParameters
                // Note: this sets the "current collection" but the object instance is still NULL!!
                if (property == XamlLanguage.PositionalParameters)
                {
                    _context.CurrentCollection = new List<PositionalParameterDescriptor>();
                }
            }
            else  // instance != null
            {
                // If we see any construction directives (like x:Arguments, x:Type) when we already have an instance, we throw
                if (IsTextConstructionDirective(property))
                {
                    throw _context.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.LateConstructionDirective, property.Name)));
                }

                if (_context.CurrentIsTypeConvertedObject)
                {
                    // This is an error path where properties are set on a Type converter instance
                    // We allow directives or attachable properties but not "normal" properties.
                    if (!property.IsDirective && !property.IsAttachable)
                    {
                        throw _context.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.SettingPropertiesIsNotAllowed, property.Name)));
                    }
                    // We don't allow attachable properties either, if the the Type Converter returned
                    // a NameFixupToken. We could consider allowing this in the future by storing the
                    // APs on the token.
                    if (property.IsAttachable && _context.CurrentInstance is NameFixupToken)
                    {
                        NameFixupToken token = (NameFixupToken)_context.CurrentInstance;
                        throw _context.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.AttachedPropOnFwdRefTC,
                            property, _context.CurrentType, string.Join(", ", token.NeededNames.ToArray()))));
                    }
                }
            }
            if (property.IsDirective && property != XamlLanguage.Items && property != XamlLanguage.PositionalParameters)
            {
                // Creates the container for x:Arguments  (possible other future directives)  If this was
                // just for x:Arguments it could be moved into the (inst == null) case above.
                // Note: this sets the "current collection" but does not touch the "current instance".
                // Note: there is a special case for x:PosParam above because the collection type is not public.
                // Note: The dictionary check is there because Collection and Dictionary checks go together.
                XamlType propertyXamlType = property.Type;
                if (propertyXamlType.IsCollection || propertyXamlType.IsDictionary)
                {
                    _context.CurrentCollection = Runtime.CreateInstance(property.Type, null);
                }
            }
        }

        public override void WriteEndMember()
        {
            ThrowIfDisposed();

            // Deferring Checking
            //
            _deferringWriter.WriteEndMember();
            if (_deferringWriter.Handled)
            {
                return;
            }

            // Error Checking
            //
            XamlMember property;
            // In the Text value case we will be on the text frame
            // and the property is in the parent.
            if (_context.CurrentType == null)
            {
                property = _context.ParentProperty;
            }
            // In the Object value case we pop'ed and assigned the value already.
            else
            {
                property = _context.CurrentProperty;
            }

            if (property == null)
            {
                string err = (_context.CurrentType != null)
                    ? SR.Get(SRID.NoPropertyInCurrentFrame_EM, _context.CurrentType.ToString())
                    : SR.Get(SRID.NoPropertyInCurrentFrame_EM_noType);

                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }

            // Real processing starts here.
            //
            _nextNodeMustBeEndMember = false;

            //The first node (T, SO, or EP) after an EndObject should null out _lastInstance.
            _lastInstance = null;

            if (property == XamlLanguage.Arguments)
            {
                _context.CurrentCtorArgs = ((List<object>)_context.CurrentCollection).ToArray();
            }
            else if (property == XamlLanguage.Initialization)
            {
                Logic_CreateFromInitializationValue(_context);
            }
            else if (property == XamlLanguage.Items)
            {
                _context.CurrentCollection = null;
            }
            else if (property == XamlLanguage.PositionalParameters)
            {
                Logic_ConvertPositionalParamsToArgs(_context);
            }
            else if (property == XamlLanguage.Class)
            {
                object value = null;
                if (_context.CurrentType == null)
                {
                    value = _context.CurrentInstance;
                    _context.PopScope();
                }
                Logic_ValidateXClass(_context, value);
            }
            else if (_context.CurrentType == null)
            {
                // CurrentType == null means we are in a Value Frame (under a property)
                // [rather than the result of some S0 sub-tree].
                // For example <Foo.Color>Red</Foo.Color> or <Foo Color="Red"> results in:
                // SM Color; V "Red"; EM;  // and the type converter for Color will be invoked
                // But: <Foo.Color><x:String>Red</x:String></Foo.Color> results in:
                // SM Color; SO String; SM _init; V "Red"; EM; EO; EM
                // Currently the String object is assigned into the Color Property in EndObject.
                // Converting XML TEXT like "Red" is completly different than <x:String>Red<x:String>
                // Conversion today will take more than TEXT as input.
                // Template Conversion is also done here
                //
                object value = _context.CurrentInstance;

                bool shouldSetValue = true;
                if (value != null)
                {
                    MarkupExtension me = value as MarkupExtension;
                    if (me != null)
                    {
                        _context.CurrentInstance = me;
                        XamlType valueXamlType = GetXamlType(value.GetType());
                        if (!property.Type.IsMarkupExtension || !valueXamlType.CanAssignTo(property.Type))
                        {
                            Logic_AssignProvidedValue(_context);
                            shouldSetValue = false;
                        }
                        // If the MarkupExtension's provided value is not assigned to the property, the MarkupExtension will be assigned directly.
                    }
                    else  // normal TC case.
                    {
                        // If the value is a string or is not directly assignable, convert it.
                        XamlType valueXamlType = GetXamlType(value.GetType());
                        if (valueXamlType == XamlLanguage.String || !valueXamlType.CanAssignTo(property.Type))
                        {
                            if (property.IsDirective && property == XamlLanguage.Key && !Logic_ShouldConvertKey(_context))
                            {
                                shouldSetValue = true;
                                _context.ParentKeyIsUnconverted = true;
                            }
                            else
                            {
                                shouldSetValue = Logic_CreatePropertyValueFromValue(_context);
                            }
                        }
                    }
                }
                _lastInstance = _context.CurrentInstance;
                if (shouldSetValue)
                {
                    Logic_DoAssignmentToParentProperty(_context);
                }
                _context.PopScope();  // Value Node Scope
            }

            // Clear Current Property because the value for this property
            // has been set and the frame popped off.
            _context.CurrentProperty = null;
            _context.CurrentIsPropertyValueSet = false;
        }

        public override void WriteValue(object value)
        {
            ThrowIfDisposed();

            // Deferring Checking
            //
            _deferringWriter.WriteValue(value);
            if (_deferringWriter.Handled)
            {
                // Handles the case of SM Template; V NodeList; EM
                if (_deferringWriter.Mode == DeferringMode.TemplateReady)
                {
                    Debug.Assert(_context.CurrentProperty.DeferringLoader != null);
                    XamlNodeList templateList = _deferringWriter.CollectTemplateList();
                    _context.PushScope();
                    _context.CurrentInstance = (XamlReader)templateList.GetReader();
                }
                return;
            }

            // Error Checking
            //
            XamlMember currentProperty = _context.CurrentProperty;
            if (currentProperty == null)
            {
                string err = (_context.CurrentType != null)
                    ? SR.Get(SRID.NoPropertyInCurrentFrame_V, value, _context.CurrentType.ToString())
                    : SR.Get(SRID.NoPropertyInCurrentFrame_V_noType, value);

                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }

            //The first node (T, SO, or EP) after an EndObject should null out _lastInstance.
            _lastInstance = null;

            _context.PushScope();
            _context.CurrentInstance = value;

            // current property is now parentproperty. (pushScope above)
            // Name change to reduce confusion.
            XamlMember parentProperty = currentProperty;
            currentProperty = null;

            // There are two cases.
            // 1) Text on a regular property.
            //    Processed on EndMember.  (not here)
            // 2) Text on a directive with a list type.
            //    Processed Item by item right here.
            _nextNodeMustBeEndMember = true;
            if (parentProperty.IsDirective)
            {
                XamlType parentXamlType = parentProperty.Type;
                if (parentXamlType.IsCollection || parentXamlType.IsDictionary)
                {
                    _nextNodeMustBeEndMember = false;
                    if (parentProperty == XamlLanguage.PositionalParameters)
                    {
                        _context.CurrentType = XamlLanguage.PositionalParameterDescriptor;
                        _context.CurrentInstance = new PositionalParameterDescriptor(value, true);
                        Logic_DoAssignmentToParentCollection(_context);
                        _context.PopScope();
                    }
                    else
                    {
                        _context.CurrentInstance = value;
                        Logic_DoAssignmentToParentCollection(_context);
                        _context.PopScope();
                    }
                }
            }
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            ThrowIfDisposed();
            if (namespaceDeclaration == null)
            {
                throw new ArgumentNullException("namespaceDeclaration");
            }
            if(namespaceDeclaration.Prefix == null)
            {
                throw new ArgumentException(SR.Get(SRID.NamespaceDeclarationPrefixCannotBeNull));
            }
            if(namespaceDeclaration.Namespace == null)
            {
                throw new ArgumentException(SR.Get(SRID.NamespaceDeclarationNamespaceCannotBeNull));
            }

            // Deferring Checking
            //
            _deferringWriter.WriteNamespace(namespaceDeclaration);
            if (_deferringWriter.Handled)
            {
                return;
            }

            // Error Checking
            //
            if (_nextNodeMustBeEndMember)
            {
                string err = SR.Get(SRID.ValueMustBeFollowedByEndMember);
                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }
            if (_context.CurrentType != null && _context.CurrentProperty == null)
            {
                string err = SR.Get(SRID.NoPropertyInCurrentFrame_NS, namespaceDeclaration.Prefix,
                                                                      namespaceDeclaration.Namespace,
                                                                      _context.CurrentType.ToString());
                throw _context.WithLineInfo(new XamlObjectWriterException(err));
            }

            // A Frame is pushed by either a AddNamespace or a WriteObject
            if (_context.CurrentType != null)
            {
                _context.PushScope();
            }
            _context.AddNamespacePrefix(namespaceDeclaration.Prefix, namespaceDeclaration.Namespace);
        }

        public void Clear()
        {
            ThrowIfDisposed();
            while (_context.LiveDepth > 0)
            {
                _context.PopScope();
            }
            _rootNamescope = null;
            _nextNodeMustBeEndMember = false;
            _deferringWriter.Clear();
            _context.PushScope();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                _inDispose = true;
                if (disposing && !IsDisposed)
                {
                    bool seenStartObject = _context.LiveDepth > 1 || _context.CurrentType != null;
                    if (seenStartObject)
                    {
                        while (_context.LiveDepth > 0)
                        {
                            if (_context.CurrentProperty != null)
                            {
                                WriteEndMember();
                            }
                            WriteEndObject();
                        }
                    }
                    _deferringWriter.Close();
                    _deferringWriter = null;

                    // null references to external objects that might be
                    // held alive if this object is not released by its
                    // caller after the Dispose call.
                    _context = null;
                    _afterBeginInitHandler = null;
                    _beforePropertiesHandler = null;
                    _afterPropertiesHandler = null;
                    _afterEndInitHandler = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
                _inDispose = false;
            }
        }

        private void ThrowIfDisposed()
        {
            if(IsDisposed)
            {
                throw new ObjectDisposedException("XamlObjectWriter");
            }
        }

        public INameScope RootNameScope
        {
            get
            {
                if (_rootNamescope != null)
                {
                    return _rootNamescope;
                }
                else
                {
                    return _context.RootNameScope;
                }
            }
        }

        //Result should return the _lastInstance when called after an EO.  Otherwise, it should return null.
        //Currently we null out _lastInstance in the nodes that can come after EO (T, SO, EP). Can an NS come,
        // what should we do?
        public virtual object Result
        {
            get
            {
                return _lastInstance;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                ThrowIfDisposed();
                return _context.SchemaContext;
            }
        }

        #region IXamlLineInfoConsumer Members

        public void SetLineInfo(int lineNumber, int linePosition)
        {
            ThrowIfDisposed();
            _context.LineNumber = lineNumber;
            _context.LinePosition = linePosition;

            // Deferring Checking
            //
            _deferringWriter.SetLineInfo(lineNumber, linePosition);
        }

        public bool ShouldProvideLineInfo
        {
            get
            {
                ThrowIfDisposed();
                return true;
            }
        }

        #endregion

        // ----------------  Private Exception / Line Number ------

        XamlException IAddLineInfo.WithLineInfo(XamlException ex)
        {
            return _context.WithLineInfo(ex);
        }

        // -----------------  Private methods -------------------

        /// <summary>
        /// Gets the Key alias property when x:Key is not present.
        /// </summary>
        private object GetKeyFromInstance(object instance, XamlType instanceType, IAddLineInfo lineInfo)
        {
            XamlMember keyProperty = instanceType.GetAliasedProperty(XamlLanguage.Key);
            if (keyProperty == null || instance == null)
            {
                throw lineInfo.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.MissingKey, instanceType.Name)));
            }
            object key = Runtime.GetValue(instance, keyProperty);
            return key;
        }

        private XamlType GetXamlType(Type clrType)
        {
            XamlType result = SchemaContext.GetXamlType(clrType);
            if (result == null)
            {
                throw new InvalidOperationException(SR.Get(SRID.ObjectWriterTypeNotAllowed,
                    SchemaContext.GetType(), clrType));
            }
            return result;
        }

        // These are the all the directives that affect Construction of object.
        bool IsConstructionDirective(XamlMember xamlMember)
        {
            return xamlMember == XamlLanguage.Arguments
                || xamlMember == XamlLanguage.Base
                || xamlMember == XamlLanguage.FactoryMethod
                || xamlMember == XamlLanguage.Initialization
                || xamlMember == XamlLanguage.PositionalParameters
                || xamlMember == XamlLanguage.TypeArguments;
        }

        // BAML sometimes sends the x:base directive later than it should
        // so these are the Ctor Directives we worry about 'Users' messing up.
        bool IsTextConstructionDirective(XamlMember xamlMember)
        {
            return xamlMember == XamlLanguage.Arguments
                || xamlMember == XamlLanguage.FactoryMethod
                || xamlMember == XamlLanguage.PositionalParameters
                || xamlMember == XamlLanguage.TypeArguments;
        }

        // Non Ctor directives that are also allowed before object creation.
        // These are compat issues with BAML recording x:Key x:Uid earlier than
        // it current XAML standards.
        bool IsDirectiveAllowedOnNullInstance(XamlMember xamlMember, XamlType xamlType)
        {
            if (xamlMember == XamlLanguage.Key)
            {
                return true;
            }
            if (xamlMember == XamlLanguage.Uid)
            {
                if (null == xamlType.GetAliasedProperty(XamlLanguage.Uid))
                {
                    return true;
                }
            }
            return false;
        }

        private void Logic_CreateAndAssignToParentStart(ObjectWriterContext ctx)
        {
            object inst;
            object factoryMethodName;
            XamlType currentType = ctx.CurrentType;

            if (ctx.CurrentIsObjectFromMember)
            {
                throw ctx.WithLineInfo(new XamlInternalException(SR.Get(SRID.ConstructImplicitType)));
            }

            // If the Constructor Arguments came from ME {} syntax and there are
            // any MEs on the parameter list, they will be un-evaluated because
            // of the rule in AssignProvideValue that skips PV() if the
            // parent instance is null (which is always is for Ctor params).
            // So Unpack and call Provide Value on items in the arguement vector here.
            if (currentType.IsMarkupExtension && ctx.CurrentCtorArgs != null)
            {
                object[] args = ctx.CurrentCtorArgs;
                for (int i = 0; i < args.Length; i++)
                {
                    MarkupExtension me = args[i] as MarkupExtension;
                    if (me != null)
                    {
                        args[i] = Logic_PushAndPopAProvideValueStackFrame(ctx, XamlLanguage.PositionalParameters, me, false);
                    }
                }
            }

            // If No Factory Method
            if (!ctx.CurrentHasPreconstructionPropertyValuesDictionary ||
                !ctx.CurrentPreconstructionPropertyValues.TryGetValue(XamlLanguage.FactoryMethod, out factoryMethodName))
            {
                inst = Runtime.CreateInstance(currentType, ctx.CurrentCtorArgs);
            }
            else  // with Factory Method
            {
                XamlPropertyName propertyName = XamlPropertyName.Parse((string)factoryMethodName);
                if (propertyName == null)
                {
                    string errMsg = string.Format(TypeConverterHelper.InvariantEnglishUS, SR.Get(SRID.InvalidExpression), factoryMethodName);
                    throw ctx.WithLineInfo(new XamlInternalException(errMsg));
                }

                XamlType ownerType;
                if (propertyName.Owner == null)
                {
                    ownerType = currentType;
                }
                else
                {
                    ownerType = ctx.GetXamlType(propertyName.Owner);
                    if (ownerType == null)
                    {
                        XamlTypeName ownerTypeName = ctx.GetXamlTypeName(propertyName.Owner);
                        throw ctx.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.CannotResolveTypeForFactoryMethod, ownerTypeName, propertyName.Name)));
                    }
                }

                inst = Runtime.CreateWithFactoryMethod(ownerType, propertyName.Name, ctx.CurrentCtorArgs);

                Debug.Assert(inst != null); // runtime throws before it returns null.

                XamlType instType = GetXamlType(inst.GetType());
                if (!instType.CanAssignTo(currentType))
                {
                    throw ctx.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.NotAssignableFrom, currentType.GetQualifiedName(), instType.GetQualifiedName())));
                }
            }
            ctx.CurrentCtorArgs = null;
            ctx.CurrentInstance = inst;
            if (currentType.IsCollection || currentType.IsDictionary)
            {
                ctx.CurrentCollection = inst;
            }

            Logic_BeginInit(ctx);

            // If UsableDuringInit, Assign to parent
            // We don't want to assign MEs to the parent since we need to call ProvideValue on them
            //   which is handled in WriteEndObject
            if (ctx.LiveDepth > 1 && !(inst is MarkupExtension))
            {
                if (ctx.LiveDepth > 1)
                {
                    Logic_CheckAssignmentToParentStart(ctx);
                }
            }

            OnBeforeProperties(inst);

            Logic_ApplyCurrentPreconstructionPropertyValues(ctx);
        }

        private void Logic_ConvertPositionalParamsToArgs(ObjectWriterContext ctx)
        {
            XamlType meType = ctx.CurrentType;
            if (!meType.IsMarkupExtension)
            {
                throw ctx.WithLineInfo(new XamlInternalException(SR.Get(SRID.NonMEWithPositionalParameters)));
            }

            var rawArgs = (List<PositionalParameterDescriptor>)ctx.CurrentCollection;
            object[] argInstances = new object[rawArgs.Count];
            IEnumerable<XamlType> paramTypes = meType.GetPositionalParameters(rawArgs.Count);

            if (null == paramTypes)
            {
                // A constructor with the specified number of arguments doesn't exist
                string msg = string.Format(TypeConverterHelper.InvariantEnglishUS, SR.Get(SRID.NoSuchConstructor), rawArgs.Count, meType.Name);
                throw ctx.WithLineInfo(new XamlObjectWriterException(msg));
            }

            int i = 0;
            foreach (XamlType pparamType in paramTypes)
            {
                if (i < rawArgs.Count)
                {
                    object inst;
                    PositionalParameterDescriptor pparam = rawArgs[i];
                    if (pparam.WasText)
                    {
                        XamlValueConverter<TypeConverter> ts = pparamType.TypeConverter;
                        object value = pparam.Value;
                        inst = Logic_CreateFromValue(ctx, ts, value, null, pparamType.Name);
                    }
                    else
                    {
                        inst = rawArgs[i].Value;
                    }
                    argInstances[i++] = inst;
                }
                else
                {
                    throw ctx.WithLineInfo(new XamlInternalException(SR.Get(SRID.PositionalParamsWrongLength)));
                }
                ctx.CurrentCtorArgs = argInstances;
            }
        }

        private void Logic_CreateFromInitializationValue(ObjectWriterContext ctx)
        {
            XamlType xamlType = ctx.ParentType;
            XamlValueConverter<TypeConverter> ts = xamlType.TypeConverter;
            object value = ctx.CurrentInstance;
            object inst = null;

            if (xamlType.IsUnknown)
            {
                string err = SR.Get(SRID.CantCreateUnknownType, xamlType.GetQualifiedName());
                throw ctx.WithLineInfo(new XamlObjectWriterException(err));
            }
            if (ts == null)
            {
                throw ctx.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.InitializationSyntaxWithoutTypeConverter, xamlType.GetQualifiedName())));
            }
            inst = Logic_CreateFromValue(ctx, ts, value, null, xamlType.Name);

            // Pop off the Text Frame.
            ctx.PopScope();

            ctx.CurrentInstance = inst;
            ctx.CurrentIsTypeConvertedObject = true;

            if (!(inst is NameFixupToken))
            {
                if (xamlType.IsCollection || xamlType.IsDictionary)
                {
                    ctx.CurrentCollection = inst;
                }
                Logic_ApplyCurrentPreconstructionPropertyValues(ctx, true);
            }
        }

        private object Logic_CreateFromValue(ObjectWriterContext ctx, XamlValueConverter<TypeConverter> typeConverter,
            object value, XamlMember property, string targetName)
        {
            return Logic_CreateFromValue(ctx, typeConverter, value, property, targetName, this);
        }

        private object Logic_CreateFromValue(ObjectWriterContext ctx, XamlValueConverter<TypeConverter> typeConverter,
            object value, XamlMember property, string targetName, IAddLineInfo lineInfo)
        {
            try
            {
                object result = Runtime.CreateFromValue(ctx.ServiceProviderContext, typeConverter,
                                                            value, property);
                return result;
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                string err = SR.Get(SRID.TypeConverterFailed, targetName, value);
                throw lineInfo.WithLineInfo(new XamlObjectWriterException(err, ex));
            }
        }

        private bool Logic_CreatePropertyValueFromValue(ObjectWriterContext ctx)
        {
            XamlMember property = ctx.ParentProperty;
            XamlType propertyType = property.Type;

            object value = ctx.CurrentInstance;
            XamlReader deferredContent = value as XamlReader;
            if (deferredContent != null)
            {
                // property.DeferringLoader looks at the property AND the type of the property.
                XamlValueConverter<XamlDeferringLoader> deferringLoader = property.DeferringLoader;
                if (deferringLoader != null)
                {
                    ctx.CurrentInstance = Runtime.DeferredLoad(
                        ctx.ServiceProviderContext, deferringLoader, deferredContent);
                    return true;
                }
            }

            // property.TypeConverter looks at the property AND the type of the property.
            XamlValueConverter<TypeConverter> converter = property.TypeConverter;
            object inst = null;

#if !TARGETTING35SP1
            XamlType declaringType = null;
            if (property.IsAttachable)
            {
                declaringType = property.DeclaringType;
            }
            else
            {
                declaringType = ctx.ParentType;
            }

            if (property != null && !property.IsUnknown && declaringType != null)
            {
                XamlType grandParentXamlType = ctx.GrandParentType;
                if (property.IsDirective &&
                    property == XamlLanguage.Key &&
                    grandParentXamlType != null &&
                    grandParentXamlType.IsDictionary)
                {
                    converter = grandParentXamlType.KeyType.TypeConverter;
                }
                if (converter!= null && converter.ConverterType != null && converter != BuiltInValueConverter.String)
                {
                    TypeConverter typeConverter = Runtime.GetConverterInstance(converter);
                    if (typeConverter != null)
                    {
                        if (declaringType.SetTypeConverterHandler != null)
                        {
                            var eventArgs = new XamlSetTypeConverterEventArgs(property, typeConverter, value, ctx.ServiceProviderContext,
                                    TypeConverterHelper.InvariantEnglishUS,
                                    ctx.ParentInstance);

                            eventArgs.CurrentType = declaringType;

                            declaringType.SetTypeConverterHandler(ctx.ParentInstance, eventArgs);
                            if (eventArgs.Handled == true)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
#endif

            if (converter != null)
            {
                inst = Logic_CreateFromValue(ctx, converter, value, property, property.Name);
            }
            else
            {
                // the value may be assignable as the property value directly,
                // so pass it through to be set to the property value later.
                inst = value;
            }
            ctx.CurrentInstance = inst;
            return true;
        }

        // For backcompat with 3.x parser, if the compat flag PreferUnconvertedKeys is set,
        // we don't convert keys if the dictionary implements non-generic IDictionary, unless:
        // - we've already failed calling IDictionary.Add on this same instance, or
        // - it's a built-in BCL dictionary that we know requires conversion.
        //
        // We track this state via the following flags:
        // - KeyIsUnconverted on a dictionary item frame indicates that the key was not converted.
        // - ShouldConvertChildKeys on a dictionary frame indicates that the dictionary requires
        //   key conversion.
        // - ShouldNotConvertChildKeys on a dictionary frame indicates that the add of the first
        //   key succeeded without conversion, so we shouldn't convert any of the remaining keys.
        // If neither of the Should flags is set, that means this is the first dictionary item,
        // so we'll try without converting the key, and fall back to conversion if it fails.
        private bool Logic_ShouldConvertKey(ObjectWriterContext ctx)
        {
            Debug.Assert(ctx.ParentProperty == XamlLanguage.Key);
            if (!_preferUnconvertedDictionaryKeys || ctx.GrandParentShouldConvertChildKeys)
            {
                return true;
            }
            if (ctx.GrandParentShouldNotConvertChildKeys)
            {
                return false;
            }
            XamlType dictionaryType = ctx.GrandParentType;
            if (dictionaryType != null && dictionaryType.IsDictionary &&
                typeof(System.Collections.IDictionary).IsAssignableFrom(dictionaryType.UnderlyingType) &&
                !IsBuiltInGenericDictionary(dictionaryType.UnderlyingType))
            {
                return false;
            }
            ctx.GrandParentShouldConvertChildKeys = true;
            return true;
        }

        private static bool IsBuiltInGenericDictionary(Type type)
        {
            if (type == null || !type.IsGenericType)
            {
                return false;
            }
            Type openGeneric = type.GetGenericTypeDefinition();
            return openGeneric == typeof(Dictionary<,>) ||
                openGeneric == typeof(SortedDictionary<,>) ||
                openGeneric == typeof(SortedList<,>) ||
                openGeneric == typeof(ConcurrentDictionary<,>);
        }

        private void Logic_BeginInit(ObjectWriterContext ctx)
        {
            object inst = ctx.CurrentInstance;
            XamlType xamlType = ctx.CurrentType;

            Runtime.InitializationGuard(xamlType, inst, true);
            if (ctx.BaseUri != null)
            {
                Runtime.SetUriBase(xamlType, inst, ctx.BaseUri);
            }

            // This is back compat with WPF 3.5.
            // This activates a second path to setting the object's
            // _contentLoaded in the generated code.
            if (inst == ctx.RootInstance)
            {
                Logic_SetConnectionId(ctx, 0, inst);
            }

            OnAfterBeginInit(inst);
        }

        private void Logic_EndInit(ObjectWriterContext ctx)
        {
            XamlType xamlType = ctx.CurrentType;
            object inst = ctx.CurrentInstance;

            Runtime.InitializationGuard(xamlType, inst, false);

            OnAfterEndInit(inst);
        }

        // Called when a MarkupExtension has unresolved children and so we can't call ProvideValue on it
        private void Logic_DeferProvideValue(ObjectWriterContext ctx)
        {
            var deferredMarkupExtensionContext = ctx.GetSavedContext(SavedContextType.ReparseMarkupExtension);
            if (ctx.LiveDepth > 2 && ctx.ParentProperty == XamlLanguage.Key &&
                ctx.GrandParentType.IsDictionary)
            {
                NameFixupToken token = GetTokenForUnresolvedChildren(ctx.CurrentInstance,
                    XamlLanguage.Key, deferredMarkupExtensionContext);
                Logic_PendKeyFixupToken(ctx, token);
            }
            else
            {
                Logic_AddDependencyForUnresolvedChildren(ctx, deferredMarkupExtensionContext);
            }
        }

        private void Logic_DuplicatePropertyCheck(ObjectWriterContext ctx, XamlMember property, bool onParent)
        {
            if (!_skipDuplicatePropertyCheck)
            {
                HashSet<XamlMember> setProperties = onParent ? ctx.ParentAssignedProperties : ctx.CurrentAssignedProperties;
                if (setProperties.Contains(property))
                {
                    // In some cases, the XamlXmlWriter emits duplicate xml:space. We can ignore these
                    // since ObjectWriter doesn't actually process xml:space.
                    if (property != XamlLanguage.Space)
                    {
                        XamlType xamlType = onParent ? ctx.ParentType : ctx.CurrentType;
                        throw ctx.WithLineInfo(new XamlDuplicateMemberException(property, xamlType));
                    }
                }
                else
                {
                    setProperties.Add(property);
                }
            }
        }

        private void Logic_ApplyCurrentPreconstructionPropertyValues(ObjectWriterContext ctx)
        {
            Logic_ApplyCurrentPreconstructionPropertyValues(ctx, false);
        }

        private void Logic_ApplyCurrentPreconstructionPropertyValues(ObjectWriterContext ctx, bool skipDirectives)
        {
            if (ctx.CurrentHasPreconstructionPropertyValuesDictionary)
            {
                Dictionary<XamlMember, object> propertyValues = ctx.CurrentPreconstructionPropertyValues;

                object value = null;
                foreach (XamlMember prop in propertyValues.Keys)
                {
                    if (skipDirectives && prop.IsDirective)
                    {
                        continue;
                    }

                    value = propertyValues[prop];

                    // If the saved pre-construction property value is an ME
                    // then we need to evaluate it now.  This in somewhat involved.
                    // We need to create a stack state that IProvideValueTarget and IRecieveMarkupExtension
                    // will understand.
                    // We did evaluate MEs on directives when the prop/value was saved,
                    // so don't call ProvideValue() now on directives here.
                    // (x:Key and x:Name need their own "saved spot" outside of PreconstructionPropertyValues)

                    MarkupExtension me = value as MarkupExtension;
                    if (me != null && !prop.IsDirective)
                    {
                        Logic_PushAndPopAProvideValueStackFrame(ctx, prop, me, true);
                    }
                    else
                    {
                        Logic_ApplyPropertyValue(ctx, prop, value, false /*onParent*/);
                    }
                }
            }
        }

        private object Logic_PushAndPopAProvideValueStackFrame(ObjectWriterContext ctx, XamlMember prop, MarkupExtension me, bool useIRME)
        {
            XamlMember savedProp = ctx.CurrentProperty;
            ctx.CurrentProperty = prop;

            ctx.PushScope();
            ctx.CurrentInstance = me;

            object retVal = null;
            if (useIRME)
            {
                Logic_AssignProvidedValue(ctx);
                // retVal = null;
            }
            else
            {
                retVal = Runtime.CallProvideValue(me, ctx.ServiceProviderContext);
            }

            ctx.PopScope();
            ctx.CurrentProperty = savedProp;
            return retVal;
        }

        private void Logic_ApplyPropertyValue(ObjectWriterContext ctx, XamlMember prop,
                                                object value, bool onParent)
        {
            object inst = onParent ? ctx.ParentInstance : ctx.CurrentInstance;
            if (value is XData)
            {
                XData xData = value as XData;
                if (prop.Type.IsXData)
                {
                    Runtime.SetXmlInstance(inst, prop, xData);
                    return;
                }
                else
                {
                    value = xData.Text;
                }
            }

            SetValue(inst, prop, value);

            if (prop.IsDirective)
            {
                XamlType xamlType = onParent ? ctx.ParentType : ctx.CurrentType;
                XamlMember propertyForDirective = xamlType.GetAliasedProperty(prop as XamlDirective);
                if (prop != XamlLanguage.Key && propertyForDirective != null)
                {
                    // handle aliases for x:Uid x:Lang etc.
                    Logic_DuplicatePropertyCheck(ctx, propertyForDirective, onParent);
                    object convertedValue = Logic_CreateFromValue(ctx, propertyForDirective.TypeConverter, value, propertyForDirective, propertyForDirective.Name);
                    SetValue(inst, propertyForDirective, convertedValue);
                }

                if (prop == XamlLanguage.Name)
                {
                    // register a named object
                    if (inst == ctx.CurrentInstance)
                    {
                        Logic_RegisterName_OnCurrent(ctx, (string)value);
                    }
                    else
                    {
                        Debug.Assert(inst == ctx.ParentInstance);
                        Logic_RegisterName_OnParent(ctx, (string)value);
                    }
                }
                else if (prop == XamlLanguage.ConnectionId)
                {
                    Logic_SetConnectionId(ctx, (int)value, inst);
                }
                else if (prop == XamlLanguage.Base)
                {
                    Logic_CheckBaseUri(ctx, (string)value);
                    ctx.BaseUri = new Uri((string)value);
                    if (ctx.ParentInstance != null)
                    {
                        Runtime.SetUriBase(ctx.ParentType, ctx.ParentInstance, ctx.BaseUri);
                    }
                }
            }
        }

        private void Logic_CheckBaseUri(ObjectWriterContext ctx, string value)
        {
            // Make sure BaseUri is not already set and that we can only set BaseUri on the root element
            // Depth > 2 because on the root element, SO/SM takes 1 slot, V is slot 2
            if ((ctx.BaseUri != null) || (ctx.Depth > 2))
            {
                throw new XamlObjectWriterException(SR.Get(SRID.CannotSetBaseUri));
            }
        }

        /// <summary>
        /// Process the call to a MarkupExtension.ProvideValue() and assign the result
        /// </summary>
        private void Logic_AssignProvidedValue(ObjectWriterContext ctx)
        {
            bool handled = Logic_ProvideValue(ctx);
            if (!handled && ctx.ParentProperty != null)
            {
                Logic_DoAssignmentToParentProperty(ctx);
            }
        }

        // Returns true if the assignment was handled by a SetMarkupExtensionHandler
        private bool Logic_ProvideValue(ObjectWriterContext ctx)
        {
            object inst = ctx.CurrentInstance;
            MarkupExtension me = (MarkupExtension)inst;
            object parentInstance = ctx.ParentInstance;
            XamlMember parentProperty = ctx.ParentProperty;

#if !TARGETTING35SP1
            if (parentProperty != null && parentProperty.IsUnknown == false)
            {
                XamlType declaringType = null;

                if (parentProperty.IsAttachable)
                {
                    declaringType = parentProperty.DeclaringType;
                }
                else
                {
                    declaringType = ctx.ParentType;
                }

                if (declaringType != null)
                {
                    if (declaringType.SetMarkupExtensionHandler != null)
                    {
                        var eventArgs = new XamlSetMarkupExtensionEventArgs(parentProperty, me, ctx.ServiceProviderContext, parentInstance);
                        eventArgs.CurrentType = declaringType;
                        declaringType.SetMarkupExtensionHandler(parentInstance, eventArgs);
                        if (eventArgs.Handled == true)
                        {
                            return true;
                        }
                    }
                }
            }
#endif
            // The Service Provider Interface IProvideValueTarget requires that we can supply ProvideValue with the
            // instance of the left-hand side of the property assignment. Markup extensions that are assigned to
            // directives are allowed to have a null left-hand instance.

            // For a MarkupExtension (ME), either the parent instance is not null, the parent property is a directive,
            // or it is the live root instance. ME.ProvideValue must be invoked in each case, except where a ME is the
            // live root instance and _skipProvideValueOnRoot is true. This allows live root instances of templates to
            // remain as MEs where necessary.
            Debug.Assert(parentInstance != null || parentProperty != null && parentProperty.IsDirective || ctx.LiveDepth == 1);
            object value = me;
            if (ctx.LiveDepth != 1 || !_skipProvideValueOnRoot)
            {
                value = Runtime.CallProvideValue(me, ctx.ServiceProviderContext);
            }

            // Checking that the ME isn't the Root of the XAML Document.
            if (ctx.ParentProperty != null)
            {
                if (value != null)
                {
                    if (!(value is NameFixupToken))
                    {
                        ctx.CurrentType = GetXamlType(value.GetType());
                    }
                }
                else if (ctx.ParentProperty == XamlLanguage.Items)
                {
                    ctx.CurrentType = ctx.ParentType.ItemType;
                }
                else
                {
                    ctx.CurrentType = ctx.ParentProperty.Type;
                }
                ctx.CurrentInstance = value;
            }
            else
            {
                ctx.CurrentInstance = value;
            }
            return false;
        }

        private void Logic_PendCurrentFixupToken_SetValue(ObjectWriterContext ctx, NameFixupToken token)
        {
            token.LineNumber = ctx.LineNumber;
            token.LinePosition = ctx.LinePosition;
            token.Runtime = Runtime;
            NameFixupGraph.AddDependency(token);
        }

        private void Logic_CheckAssignmentToParentStart(ObjectWriterContext ctx)
        {
            Debug.Assert(ctx.LiveDepth > 1);

            // First defeat the TopDown'ness if we are in a dictionary
            // because the key property can come later, we must wait
            // until End Object to compute the key for the Dictionary.Add(Key, val)
            bool inDictionary = ((ctx.ParentProperty == XamlLanguage.Items) && ctx.ParentType.IsDictionary);
            XamlType type = ctx.CurrentType;

            if (type.IsUsableDuringInitialization && !inDictionary)
            {
                ctx.CurrentWasAssignedAtCreation = true;
                Logic_DoAssignmentToParentProperty(ctx);
            }
            else
            {
                ctx.CurrentWasAssignedAtCreation = false;
            }
        }

        private void Logic_DoAssignmentToParentCollection(ObjectWriterContext ctx)
        {
            object parentCollection = ctx.ParentCollection;
            XamlType collectionType = ctx.ParentType;
            XamlType currentType = ctx.CurrentType;
            object value = ctx.CurrentInstance;

            if (!collectionType.IsDictionary)
            {
                if (!Logic_PendAssignmentToParentCollection(ctx, null, false))
                {
                    // If Value is a Markup Extention then check the collection item type
                    // if it can hold the ME then don't call ProvideValue().
                    MarkupExtension me = value as MarkupExtension;
                    if(me != null && !Logic_WillParentCollectionAdd(ctx, value.GetType(), true))
                    {
                        // We don't need to call Logic_ProvideValue() with the extra handler
                        // interfaces, because this is collection not a scalar property.
                        value = Runtime.CallProvideValue(me, ctx.ServiceProviderContext);
                    }
                    Runtime.Add(parentCollection, collectionType, value, currentType);
                }
            }
            else
            {
                if (currentType == null)
                {
                    currentType = value == null ? collectionType.ItemType : GetXamlType(value.GetType());
                }
                object key = ctx.CurrentKey;
                bool keyIsSet = ctx.CurrentIsKeySet;
                if (!Logic_PendAssignmentToParentCollection(ctx, key, keyIsSet))
                {
                    if (!keyIsSet)
                    {
                        key = GetKeyFromInstance(value, currentType, this);
                    }
                    Logic_AddToParentDictionary(ctx, key, value);
                }
            }
        }

        /// <summary>
        ///  Returns true when the item type of the collection is assignable from
        ///  the given type. Normally we wouldn't call this (Let the CLR check and
        ///  throw if the item doesn't fit)  but for Markup Extensions we check this
        ///  Before calling ProvideValue();
        /// </summary>
        /// <param name="ctx">The context</param>
        /// <param name="type">The type of the new item</param>
        /// <param name="excludeObjectType">return false if collection Item Type is Object</param>
        /// <returns></returns>
        private bool Logic_WillParentCollectionAdd(ObjectWriterContext ctx, Type type, bool excludeObjectType)
        {
            Debug.Assert(ctx.ParentType.IsCollection, "Logic_WillParentCollectionAdd called on a non-collection");

            // The Parent Property is x:Items which is always List<Object>
            // To get the real item type we need to go to the Type.
            XamlType itemType = ctx.ParentType.ItemType;

            if (excludeObjectType && itemType == XamlLanguage.Object)
            {
                return false;
            }

            if (itemType.UnderlyingType.IsAssignableFrom(type))
            {
                return true;
            }
            return false;
        }

        private void Logic_AddToParentDictionary(ObjectWriterContext ctx, object key, object value)
        {
            if (ctx.CurrentKeyIsUnconverted && !ctx.ParentShouldNotConvertChildKeys)
            {
                if (!ctx.ParentShouldConvertChildKeys)
                {
                    // If we didn't convert the key earlier (see Logic_ShouldConvertKey for details),
                    // we try to add unconverted key, and if it fails, we convert it, and try again.
                    try
                    {
                        Runtime.AddToDictionary(ctx.ParentCollection, ctx.ParentType, value, ctx.CurrentType, key);
                        // We've succesfully added the key without converting it; we should apply the same
                        // policy to the other keys for this dictionary instance.
                        ctx.ParentShouldNotConvertChildKeys = true;
                        return;
                    }
                    catch (XamlObjectWriterException ex)
                    {
                        // A dictionary that is passed in the wrong key type should throw ArgumentException.
                        // However a naive implementer might throw InvalidCastException instead, so
                        // catch that too.
                        if (!(ex.InnerException is ArgumentException) &&
                            !(ex.InnerException is InvalidCastException))
                        {
                            throw;
                        }
                        if (Debugger.IsLogging())
                        {
                            Debug.WriteLine(SR.Get(SRID.DictionaryFirstChanceException, ctx.ParentType, key, ctx.ParentType.KeyType));
                        }
                    }

                    // Adding an unconverted key failed, we should make sure that we convert all future
                    // keys on this dictionary instance.
                    ctx.ParentShouldConvertChildKeys = true;
                }
                // Else, this is a deferred add, where the uncoverted key was saved earlier,
                // before the parent's ShouldConvertChildKeys flag was set. So we skip the
                // Add up above, and go directly to the conversion below.

                // Reproduce the stack as it would have been at the point when the key was originally
                // read, and run the type converter now.
                Debug.Assert(ctx.CurrentProperty == null);
                ctx.CurrentProperty = XamlLanguage.Key;
                ctx.PushScope();
                ctx.CurrentInstance = key;
                Logic_CreatePropertyValueFromValue(ctx);
                key = ctx.CurrentInstance;
                ctx.PopScope();
                ctx.CurrentProperty = null;
            }

            Runtime.AddToDictionary(ctx.ParentCollection, ctx.ParentType, value, ctx.CurrentType, key);
        }

        // If any adds to the current collection are pending forward-reference resolution,
        // we need to queue up all the subsequent adds, so that the reference is added in order.
        // Returns TRUE if the item was pended; FALSE if it should be added directly.
        // Consider perf optimization to avoid a global lookup on every item
        private bool Logic_PendAssignmentToParentCollection(ObjectWriterContext ctx, object key, bool keyIsSet)
        {
            object parentCollection = ctx.ParentCollection;
            object value = ctx.CurrentInstance;
            NameFixupToken keyToken = key as NameFixupToken;
            NameFixupToken valueToken = value as NameFixupToken;

            List<PendingCollectionAdd> pendingCollection = null;
            if (_pendingCollectionAdds != null)
            {
                PendingCollectionAdds.TryGetValue(parentCollection, out pendingCollection);
            }
            if (pendingCollection == null &&
                (keyToken != null || valueToken != null ||
                HasUnresolvedChildren(key) || HasUnresolvedChildren(value)))
            {
                // We pend the add not only if the key or value are unresolved, but also if they
                // have any unresolved children. This avoids problems with unresolved implicit keys,
                // the key's hashcode changing as its properties resolve, or incomplete data being
                // passed into an Add method that does validation.
                pendingCollection = new List<PendingCollectionAdd>();
                PendingCollectionAdds.Add(parentCollection, pendingCollection);
            }
            if (keyToken != null)
            {
                // Set KeyHolder to null since the item is no longer on the stack,
                // so we won't be getting its key from the saved KeyHolder anymore
                keyToken.Target.KeyHolder = null;
                keyToken.Target.TemporaryCollectionIndex = pendingCollection.Count;
            }
            if (valueToken != null)
            {
                Logic_PendCurrentFixupToken_SetValue(ctx, valueToken);
                valueToken.Target.TemporaryCollectionIndex = pendingCollection.Count;
            }
            if (pendingCollection != null)
            {
                PendingCollectionAdd pendingAdd = new PendingCollectionAdd
                {
                    Key = key,
                    KeyIsSet = keyIsSet,
                    KeyIsUnconverted = ctx.CurrentKeyIsUnconverted,
                    Item = value,
                    ItemType = ctx.CurrentType,
                    LineNumber = ctx.LineNumber,
                    LinePosition = ctx.LinePosition
                };
                pendingCollection.Add(pendingAdd);
                if (pendingAdd.KeyIsUnconverted)
                {
                    if (!PendingKeyConversionContexts.ContainsKey(parentCollection))
                    {
                        // Snap a SavedContext so we can convert the key later, if needed
                        XamlSavedContext savedContext = ctx.GetSavedContext(SavedContextType.ReparseMarkupExtension);
                        PendingKeyConversionContexts.Add(parentCollection, new ObjectWriterContext(savedContext, null, null, Runtime));
                    }
                }
                return true;
            }
            return false;
        }

        private void Logic_DoAssignmentToParentProperty(ObjectWriterContext ctx)
        {
            XamlMember parentProperty = ctx.ParentProperty;
            object value = ctx.CurrentInstance;

            // First look to handle adds to collections.
            // Collections are always in a directive of collection or dictionary type.
            // oh btw PositionalParameters are the "other" collection property.  :-)
            XamlType ppXamlType = parentProperty.Type;
            if (parentProperty.IsDirective && (ppXamlType.IsCollection || ppXamlType.IsDictionary))
            {
                if (value is NameFixupToken && parentProperty != XamlLanguage.Items)
                {
                    NameFixupToken token = value as NameFixupToken;
                    string names = String.Join(",", token.NeededNames.ToArray());
                    string msg = SR.Get(SRID.ForwardRefDirectives, names);
                    throw ctx.WithLineInfo(new XamlObjectWriterException(msg));
                }
                if (parentProperty == XamlLanguage.PositionalParameters)
                {
                    ctx.CurrentType = XamlLanguage.PositionalParameterDescriptor;
                    ctx.CurrentInstance = new PositionalParameterDescriptor(value, false);
                }
                Logic_DoAssignmentToParentCollection(ctx);
            }
            else
            {
                object parentInstance = ctx.ParentInstance;
                if (parentInstance != null)
                {
                    // This checks for multi-values to single valued properties.
                    // <Button.Background>
                    //    <Brush>Red</Brush>
                    //    <Brush>Blue</Brush>
                    //    <Brush>Green</Brush>
                    // </Button.Background>
                    if (ctx.ParentIsPropertyValueSet)
                    {
                        throw ctx.WithLineInfo(new XamlDuplicateMemberException(
                                                ctx.ParentProperty,
                                                ctx.ParentType));
                    }
                    ctx.ParentIsPropertyValueSet = true;
                    if (value is NameFixupToken)
                    {
                        var token = (NameFixupToken)value;
                        if (parentProperty.IsDirective)
                        {
                            // Only the key directive may be assigned a reference.
                            if (parentProperty != XamlLanguage.Key)
                            {
                                string names = String.Join(",", token.NeededNames.ToArray());
                                string msg = SR.Get(SRID.ForwardRefDirectives, names);
                                throw ctx.WithLineInfo(new XamlObjectWriterException(msg));
                            }
                            Logic_PendKeyFixupToken(ctx, token);
                        }
                        else
                        {
                            Logic_PendCurrentFixupToken_SetValue(ctx, token);
                        }
                    }
                    else
                    {
                        XamlType parentType = ctx.ParentType;

                        if (!ctx.CurrentIsObjectFromMember)
                        {
                            Logic_ApplyPropertyValue(ctx, parentProperty, value, true /*onParent*/);

                            // registered a named object
                            if (parentProperty == parentType.GetAliasedProperty(XamlLanguage.Name))
                            {
                                Logic_RegisterName_OnParent(ctx, (string)value);
                            }

                            if (parentProperty == XamlLanguage.Key)
                            {
                                ctx.ParentKey = value;
                            }
                            // The other aliases of Uid, Lang, don't have special processing.
                        }
                    }
                }
                else  // when parentInstance == null
                {
                    if (parentProperty.IsDirective)
                    {
                        // Base Uri must be set on the Context ASAP.  It needs to be set before the
                        // object is constructed.
                        if (parentProperty == XamlLanguage.Base)
                        {
                            Logic_CheckBaseUri(ctx, (string)value);
                            ctx.BaseUri = new Uri((string)value);
                        }
                        else
                        {

                            if (value is NameFixupToken)
                            {
                                // Only the key directive may be assigned a reference.
                                if (parentProperty != XamlLanguage.Key)
                                {
                                    NameFixupToken token = (NameFixupToken)value;
                                    string names = String.Join(",", token.NeededNames.ToArray());
                                    string msg = SR.Get(SRID.ForwardRefDirectives, names);
                                    throw ctx.WithLineInfo(new XamlObjectWriterException(msg));
                                }
                                Logic_PendKeyFixupToken(ctx, (NameFixupToken)value);
                            }
                            else if (parentProperty == XamlLanguage.Key)
                            {
                                ctx.ParentKey = value;
                            }
                            else
                            {
                                ctx.ParentPreconstructionPropertyValues.Add(parentProperty, value);
                            }
                        }
                    }
                    else
                    {
                        throw new XamlInternalException(SR.Get(SRID.BadStateObjectWriter));
                    }
                }
            }
        }

        private void Logic_PendKeyFixupToken(ObjectWriterContext ctx, NameFixupToken token)
        {
            Debug.Assert(token.Target.Property == XamlLanguage.Key);

            // Keys aren't fixups on the item itself, they're fixups on the KeyHolder that contains it
            // Any changes to the Target must also update the actual ObjectWriterFrame itself as well.
            token.Target.Instance = ctx.GrandParentInstance;
            token.Target.InstanceType = ctx.GrandParentType;
            token.Target.InstanceWasGotten = ctx.GrandParentIsObjectFromMember;
            FixupTargetKeyHolder ftkh = new FixupTargetKeyHolder(token);
            token.Target.KeyHolder = ftkh;
            ctx.ParentKey = ftkh;

            // if the grandparent instance is null, we're not in a dictionary, so the key will never be used
            // so just throw it away
            if (token.Target.Instance != null)
            {
                Logic_PendCurrentFixupToken_SetValue(ctx, token);
            }
        }

        private void Logic_RegisterName_OnCurrent(ObjectWriterContext ctx, string name)
        {
            bool isRoot = ctx.LiveDepth == 1;
            RegisterName(ctx, name, ctx.CurrentInstance, ctx.CurrentType,
                              ctx.CurrentNameScope, ctx.ParentNameScope, isRoot);
            ctx.CurrentInstanceRegisteredName = name;
        }

        private void Logic_RegisterName_OnParent(ObjectWriterContext ctx, string name)
        {
            RegisterName(ctx, name, ctx.ParentInstance, ctx.ParentType,
                               ctx.ParentNameScope, ctx.GrandParentNameScope, false);
            ctx.ParentInstanceRegisteredName = name;
        }

        private void RegisterName(ObjectWriterContext ctx, string name,
                                object inst, XamlType xamlType,
                                INameScope nameScope, INameScope parentNameScope, bool isRoot)
        {
            INameScope underlyingNameScope = nameScope;
            NameScopeDictionary nameScopeDict = nameScope as NameScopeDictionary;
            if (nameScopeDict != null)
            {
                underlyingNameScope = nameScopeDict.UnderlyingNameScope;
            }

            // Don't register a named object on itself.  Unless this is the root.
            if (Object.ReferenceEquals(underlyingNameScope, inst) && !isRoot)
            {
                // If nameScope was the instance AND it wasn't the root...
                // Then use the parent name scope
                nameScope = parentNameScope;
            }

            // Don't register a name for a FixupToken, that will be done when the converter is rerun
            if (!(inst is NameFixupToken))
            {
                try
                {
                    nameScope.RegisterName(name, inst);
                }
                catch (Exception ex)
                {
                    if (CriticalExceptions.IsCriticalException(ex))
                    {
                        throw;
                    }
                    throw ctx.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.NameScopeException, ex.Message), ex));
                }
            }
        }

        private void Logic_SetConnectionId(ObjectWriterContext ctx, int connectionId, object instance)
        {
            object rootInstance = ctx.RootInstance;
            Runtime.SetConnectionId(rootInstance, connectionId, instance);
        }

        private void SetValue(Object inst, XamlMember property, object value)
        {
#if !TARGETTING35SP1
            if (!property.IsDirective)
            {
                if (OnSetValue(inst, property, value))
                {
                    return;
                }
            }
#endif

            Runtime.SetValue(inst, property, value);
        }

        private void Logic_ValidateXClass(ObjectWriterContext ctx, object value)
        {
            if (ctx.Depth > 1)
            {
                throw ctx.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.DirectiveNotAtRoot, XamlLanguage.Class)));
            }
            string className = value as string;
            if (className == null)
            {
                throw ctx.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.DirectiveMustBeString, XamlLanguage.Class)));
            }
            object curInstance = ctx.CurrentInstance;
            Type rootInstanceType = (curInstance != null) ? curInstance.GetType() : ctx.CurrentType.UnderlyingType;
            if (rootInstanceType.FullName != className)
            {
                string rootNamespace = SchemaContext.GetRootNamespace(rootInstanceType.Assembly);
                if (!string.IsNullOrEmpty(rootNamespace))
                {
                    className = rootNamespace + "." + className;
                }
                if (rootInstanceType.FullName != className)
                {
                    throw ctx.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.XClassMustMatchRootInstance, className, rootInstanceType.FullName)));
                }
            }
        }

        // =========== NameFixupToken Processing ======================
        private void Logic_AddDependencyForUnresolvedChildren(ObjectWriterContext ctx,
            XamlSavedContext deferredMarkupExtensionContext)
        {
            object childThatHasUnresolvedChildren = ctx.CurrentInstance;
            XamlMember property = ctx.ParentProperty;

            if (property != null && property.IsDirective && ctx.ParentInstance == null && property != XamlLanguage.Key)
            {
                // The parent instance is null, so we're in a creation directives. Forward refs
                // aren't allowed here.
                List<string> names = new List<string>();
                _nameFixupGraph.GetDependentNames(childThatHasUnresolvedChildren, names);
                string namesString = string.Join(", ", names.ToArray());
                throw ctx.WithLineInfo(new XamlObjectWriterException(SR.Get(SRID.TransitiveForwardRefDirectives,
                    childThatHasUnresolvedChildren.GetType(), property, namesString)));
            }

            NameFixupToken token = GetTokenForUnresolvedChildren(
                childThatHasUnresolvedChildren, property, deferredMarkupExtensionContext);
            token.Target.Instance = ctx.ParentInstance;
            token.Target.InstanceType = ctx.ParentType;
            token.Target.InstanceWasGotten = ctx.ParentIsObjectFromMember;
            Logic_PendCurrentFixupToken_SetValue(ctx, token);
        }

        private NameFixupToken GetTokenForUnresolvedChildren(object childThatHasUnresolvedChildren,
            XamlMember property, XamlSavedContext deferredMarkupExtensionContext)
        {
            NameFixupToken token = new NameFixupToken();
            if (deferredMarkupExtensionContext != null)
            {
                token.FixupType = FixupType.MarkupExtensionFirstRun;
                token.SavedContext = deferredMarkupExtensionContext;
            }
            else
            {
                token.FixupType = FixupType.UnresolvedChildren;
            }
            token.ReferencedObject = childThatHasUnresolvedChildren;
            token.Target.Property = property;
            return token;
        }

        private void CompleteNameReferences()
        {
            if (_nameFixupGraph == null)
            {
                return;
            }

            // Step 1. Resolve any pending simple fixups
            List<NameFixupToken> unresolvedRefs = null;
            IEnumerable<NameFixupToken> simpleFixups = _nameFixupGraph.GetRemainingSimpleFixups();
            foreach (NameFixupToken token in simpleFixups)
            {
                object namedObject = token.ResolveName(token.NeededNames[0]);
                if (namedObject == null)
                {
                    if (unresolvedRefs == null)
                    {
                        unresolvedRefs = new List<NameFixupToken>();
                    }
                    unresolvedRefs.Add(token);
                }
                // Only process if we haven't found any unresolved references. If we have,
                // we're going to throw, so no point in processing.
                else if (unresolvedRefs == null)
                {
                    token.ReferencedObject = namedObject;
                    token.NeededNames.RemoveAt(0);
                    ProcessNameFixup(token, true);

                    // We've resolved the reference, but the resolved object must have unresolved
                    // children, or else the reference wouldn't have remained unresolved until now.
                    // So we add an UnresolvedChildren dependency, so that we will run EndInit/ProvideValue
                    // as appropriate in Step 3.
                    _nameFixupGraph.AddEndOfParseDependency(token.ReferencedObject, token.Target);
                }
            }
            if (unresolvedRefs != null)
            {
                ThrowUnresolvedRefs(unresolvedRefs);
            }

            // Step 2. Run any remaining reparses
            IEnumerable<NameFixupToken> reparses = _nameFixupGraph.GetRemainingReparses();
            foreach (NameFixupToken token in reparses)
            {
                ProcessNameFixup(token, true);
                // Add an UnresolvedChildren dependency, for the same reason as in Step 1 above
                _nameFixupGraph.AddEndOfParseDependency(token.TargetContext.CurrentInstance, token.Target);
            }

            // Step 3. Run all pending ProvideValues and EndInits
            IEnumerable<NameFixupToken> objectDependencies = _nameFixupGraph.GetRemainingObjectDependencies();
            foreach (NameFixupToken token in objectDependencies)
            {
                ProcessNameFixup(token, true);
                if (token.Target.Instance != null &&
                    !_nameFixupGraph.HasUnresolvedChildren(token.Target.Instance))
                {
                    CompleteDeferredInitialization(token.Target);
                }
            }
        }

        // Throw an exception with all the unresolved simple fixups
        private void ThrowUnresolvedRefs(IEnumerable<NameFixupToken> unresolvedRefs)
        {
            StringBuilder exceptionMessage = new StringBuilder();
            bool first = true;
            foreach (NameFixupToken token in unresolvedRefs)
            {
                if (!first)
                {
                    exceptionMessage.AppendLine();
                }
                exceptionMessage.Append(SR.Get(SRID.UnresolvedForwardReferences, token.NeededNames[0]));
                if (token.LineNumber != 0)
                {
                    if (token.LinePosition != 0)
                    {
                        exceptionMessage.Append(SR.Get(SRID.LineNumberAndPosition, string.Empty, token.LineNumber, token.LinePosition));
                    }
                    exceptionMessage.Append(SR.Get(SRID.LineNumberOnly, string.Empty, token.LineNumber));
                }
                first = false;
            }
            throw new XamlObjectWriterException(exceptionMessage.ToString());
        }

        private void TriggerNameResolution(object instance, string name)
        {
#if DEBUG
            Debug.Assert(!_inNameResolution);
            _inNameResolution = true;
#endif
            Debug.Assert(_nameFixupGraph != null);
            _nameFixupGraph.ResolveDependenciesTo(instance, name);
            while (_nameFixupGraph.HasResolvedTokensPendingProcessing)
            {
                NameFixupToken token = _nameFixupGraph.GetNextResolvedTokenPendingProcessing();
                ProcessNameFixup(token, false);

                // If this was a named type-converted object, and it's already off the stack, then
                // it lost its chance to trigger name resolution in WriteEndObject. So resolve any
                // dependencies to its name now.
                if (token.FixupType == FixupType.ObjectInitializationValue &&
                    !token.CanAssignDirectly &&
                    token.TargetContext.CurrentInstanceRegisteredName != null &&
                    !_context.IsOnTheLiveStack(token.TargetContext.CurrentInstance))
                {
                    string convertedName = token.TargetContext.CurrentInstanceRegisteredName;
                    object convertedInstance = token.TargetContext.CurrentInstance;
                    _nameFixupGraph.ResolveDependenciesTo(convertedInstance, convertedName);
                }

                // If this was the last pending fixup for the parent object, then resolve any
                // transitive dependencies
                bool isComplete = !token.Target.InstanceIsOnTheStack &&
                    !_nameFixupGraph.HasUnresolvedOrPendingChildren(token.Target.Instance);
                if (isComplete)
                {
                    CompleteDeferredInitialization(token.Target);

                    object completedInstance = token.Target.Instance;
                    string completedName = token.Target.InstanceName;
                    _nameFixupGraph.ResolveDependenciesTo(completedInstance, completedName);
                }
            }
#if DEBUG
            _inNameResolution = false;
#endif
        }

        bool ICheckIfInitialized.IsFullyInitialized(object instance)
        {
            if (instance == null)
            {
                return true;
            }
            if (_context.LiveDepth > 0)
            {
                // An object is fully initialized if it's off the stack, and has no uninitialized children
                if (_context.IsOnTheLiveStack(instance))
                {
                    return false;
                }
                return _nameFixupGraph == null || !_nameFixupGraph.HasUnresolvedOrPendingChildren(instance);
            }
            else
            {
                // At the end of the parse, we start running reparses on partially initialized objects,
                // and remove those dependencies. But we still want to be able to inform MEs/TCs that
                // the named objects they're getting aren't actually fully initialized. So we save a list
                // of incompletely initialized objects at the point we started completing references.
                return _nameFixupGraph == null || !_nameFixupGraph.WasUninitializedAtEndOfParse(instance);
            }
        }

        private void CompleteDeferredInitialization(FixupTarget target)
        {
            ExecutePendingAdds(target.InstanceType, target.Instance);

            if (!target.InstanceWasGotten)
            {
                IAddLineInfo oldLineInfo = Runtime.LineInfo;
                Runtime.LineInfo = target;
                try
                {
                    Runtime.InitializationGuard(target.InstanceType, target.Instance, false);
                }
                finally
                {
                    Runtime.LineInfo = oldLineInfo;
                }

                OnAfterEndInit(target.Instance);
            }
        }

        // Processes a fixup token by assigning the resolved name or rerunning converters,
        // and calling EndInit if the token's target object is finally fully initialized.
        private void ProcessNameFixup(NameFixupToken token, bool nameResolutionIsComplete)
        {
            Debug.Assert(token.NeededNames.Count == 0 || nameResolutionIsComplete);
            IAddLineInfo oldLineInfo = Runtime.LineInfo;
            try
            {
                Runtime.LineInfo = token;
                if (token.CanAssignDirectly)
                {
                    ProcessNameFixup_Simple(token);
                }
                else if (token.FixupType != FixupType.UnresolvedChildren)
                {
                    ProcessNameFixup_Reparse(token, nameResolutionIsComplete);
                }
            }
            finally
            {
                Runtime.LineInfo = oldLineInfo;
            }
        }

        private void ProcessNameFixup_Simple(NameFixupToken token)
        {
            object value = token.ReferencedObject;
            if (token.Target.Property == XamlLanguage.Key)
            {
                ProcessNameFixup_UpdatePendingAddKey(token, value);
            }
            else if (token.Target.Property == XamlLanguage.Items)
            {
                ProcessNameFixup_UpdatePendingAddItem(token, value);
            }
            else
            {
                SetValue(token.Target.Instance, token.Target.Property, value);
            }
        }

        private void ProcessNameFixup_Reparse(NameFixupToken token, bool nameResolutionIsComplete)
        {
            object value = null;

            var owc = token.TargetContext;
            owc.NameResolutionComplete = nameResolutionIsComplete;
            owc.IsInitializedCallback = this;
            switch (token.FixupType)
            {
                case FixupType.MarkupExtensionFirstRun:
                    bool handled = Logic_ProvideValue(owc);
                    if (handled)
                    {
                        return;
                    }
                    break;
                case FixupType.MarkupExtensionRerun:
                    // Logic_ProvideValue already ran the first time, no need to rerun it
                    value = Runtime.CallProvideValue((MarkupExtension)owc.CurrentInstance, owc.ServiceProviderContext);
                    owc.CurrentInstance = value;
                    break;
                case FixupType.PropertyValue:
                    value = Logic_CreateFromValue(owc, owc.ParentProperty.TypeConverter, owc.CurrentInstance,
                                                    owc.ParentProperty, owc.ParentProperty.Name, token);
                    token.TargetContext.CurrentInstance = value;
                    break;
                case FixupType.ObjectInitializationValue:
                    Logic_CreateFromInitializationValue(owc);
                    if (token.TargetContext.CurrentInstanceRegisteredName != null)
                    {
                        // We couldn't actually register before, because the instance hadn't been
                        // created by the TypeConverter. So register it now.
                        Logic_RegisterName_OnCurrent(token.TargetContext, token.TargetContext.CurrentInstanceRegisteredName);
                    }
                    break;
            }

#if DEBUG
            if(token.Target.Property != token.TargetContext.ParentProperty)
            {
                throw new XamlInternalException("Token's Target Property '{0}' != '{1}' the Token's Context parent Property");
            }
#endif
            if (token.Target.Property == XamlLanguage.Key)
            {
                ProcessNameFixup_UpdatePendingAddKey(token, owc.CurrentInstance);
            }
            else if (token.Target.Property == XamlLanguage.Items)
            {
                ProcessNameFixup_UpdatePendingAddItem(token, owc.CurrentInstance);
            }
            else if (token.Target.Property != null)
            {
                Logic_DoAssignmentToParentProperty(owc);
            }
            else
            {
                // This is a deferred ProvideValue at the root
                Debug.Assert(token.FixupType == FixupType.MarkupExtensionFirstRun && _lastInstance == token.ReferencedObject);
                _lastInstance = owc.CurrentInstance;
            }

            NameFixupToken newToken = owc.CurrentInstance as NameFixupToken;
            if (newToken != null)
            {
                // Line Info should be the same as the original token, not wherever we happen to be currently.
                // Also several properties on Target (IsOnTheStack, EndInstanceLineInfo, and potentially others)
                // may have been updated on the old token, copy those same updates to the new token.
                newToken.Target = token.Target;
                newToken.LineNumber = token.LineNumber;
                newToken.LinePosition = token.LinePosition;

                // Logic_DoAssignmentToParentProperty will add the dependency for the new token, but
                // ProcessNameFixup_UpdatePendingAddKey/Item doesn't. So do that here.
                if (token.Target.Property == XamlLanguage.Key || token.Target.Property == XamlLanguage.Items)
                {
                    _nameFixupGraph.AddDependency(newToken);
                }
            }
        }

        private void ProcessNameFixup_UpdatePendingAddKey(NameFixupToken token, object key)
        {
            if (token.Target.KeyHolder != null)
            {
                // The KeyHolder item is still on the stack, so update the KeyHolder
                Debug.Assert(token.Target.KeyHolder.Key == token);
                token.Target.KeyHolder.Key = key;
            }
            // if the index is less than 0, the target's not a dictionary, so throw away the key
            else if (token.Target.TemporaryCollectionIndex >= 0)
            {
                // The dictionary item is no longer on the stack, so update _pendingCollectionAdds
                List<PendingCollectionAdd> pendingCollection = PendingCollectionAdds[token.Target.Instance];
                PendingCollectionAdd pendingAdd = pendingCollection[token.Target.TemporaryCollectionIndex];
                Debug.Assert(pendingAdd.Key == token);
                pendingAdd.Key = key;
                pendingAdd.KeyIsSet = true;
            }
        }

        private void ProcessNameFixup_UpdatePendingAddItem(NameFixupToken token, object item)
        {
            List<PendingCollectionAdd> pendingCollection = PendingCollectionAdds[token.Target.Instance];
            PendingCollectionAdd pendingAdd = pendingCollection[token.Target.TemporaryCollectionIndex];
            Debug.Assert(pendingAdd.Item == token);
            pendingAdd.Item = item;
            if (!(item is NameFixupToken))
            {
                pendingAdd.ItemType = (item != null) ? GetXamlType(item.GetType()) : null;
            }
        }

        private void ExecutePendingAdds(XamlType instanceType, object instance)
        {
            List<PendingCollectionAdd> pendingCollection;
            if (_pendingCollectionAdds != null && PendingCollectionAdds.TryGetValue(instance, out pendingCollection))
            {
                foreach (PendingCollectionAdd pendingAdd in pendingCollection)
                {
                    XamlType itemType = pendingAdd.ItemType ?? instanceType.ItemType;

                    IAddLineInfo oldLineInfo = Runtime.LineInfo;
                    Runtime.LineInfo = pendingAdd;
                    try
                    {
                        if (instanceType.IsDictionary)
                        {
                            if (!pendingAdd.KeyIsSet)
                            {
                                pendingAdd.Key = GetKeyFromInstance(pendingAdd.Item, itemType, pendingAdd);
                                pendingAdd.KeyIsSet = true;
                            }
                            if (pendingAdd.KeyIsUnconverted)
                            {
                                // If the Add of the unconverted key fails, we will need to convert the key.
                                ObjectWriterContext ctx = PendingKeyConversionContexts[instance];
                                ctx.PopScope();   // The saved context will have some dictionary item on top.
                                ctx.PushScope();  // Pop it and replace it with the current item.
                                ctx.CurrentType = itemType;
                                ctx.CurrentInstance = pendingAdd.Item;
                                ctx.CurrentKeyIsUnconverted = pendingAdd.KeyIsUnconverted;
                                Logic_AddToParentDictionary(ctx, pendingAdd.Key, pendingAdd.Item);
                            }
                            else
                            {
                                Runtime.AddToDictionary(instance, instanceType, pendingAdd.Item, itemType, pendingAdd.Key);
                            }
                        }
                        else
                        {
                            Runtime.Add(instance, instanceType, pendingAdd.Item, pendingAdd.ItemType);
                        }
                    }
                    finally
                    {
                        Runtime.LineInfo = oldLineInfo;
                    }
                }
                PendingCollectionAdds.Remove(instance);
                if (_pendingKeyConversionContexts != null && _pendingKeyConversionContexts.ContainsKey(instance))
                {
                    _pendingKeyConversionContexts.Remove(instance);
                }
            }
        }

        private class PendingCollectionAdd : IAddLineInfo
        {
            public object Key { get; set; }
            public bool KeyIsSet { get; set; }         // Need this because key could validly be null
            public bool KeyIsUnconverted { get; set; }
            public object Item { get; set; }
            public XamlType ItemType { get; set; }     // Need this because Add() overload resolution
                                                       // is based on the SO type, which could be different
                                                       // from the actual type in case of TCs and FactoryMethods
            public int LineNumber { get; set; }
            public int LinePosition { get; set; }

            XamlException IAddLineInfo.WithLineInfo(XamlException ex)
            {
                if (LineNumber > 0)
                {
                    ex.SetLineInfo(LineNumber, LinePosition);
                }
                return ex;
            }
        }
    }
}
