// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using System.Xaml;
using MS.Internal.Xaml.Context;

namespace MS.Internal.Xaml
{
internal class ServiceProviderContext : ITypeDescriptorContext,  // derives from IServiceProvider
                                  IServiceProvider,
                                  IXamlTypeResolver,
                                  IUriContext,
                                  IAmbientProvider,
                                  IXamlSchemaContextProvider,
                                  IRootObjectProvider,
                                  IXamlNamespaceResolver,
                                  IProvideValueTarget,
                                  IXamlNameResolver,
                                  IDestinationTypeProvider,
                                  IXamlLineInfo
    {
        ObjectWriterContext _xamlContext;

        public ServiceProviderContext(ObjectWriterContext context)
        {
            _xamlContext = context;
        }

        #region XamlServiceProviderContext Methods
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IXamlTypeResolver))
            {
                return this;
            }
            else if (serviceType == typeof(IUriContext))
            {
                return this;
            }
            else if (serviceType == typeof(IAmbientProvider))
            {
                return this;
            }
            else if (serviceType == typeof(IXamlSchemaContextProvider))
            {
                return this;
            }
            else if (serviceType == typeof(IProvideValueTarget))
            {
                return this;
            }
            else if (serviceType == typeof(IRootObjectProvider))
            {
                return this;
            }
            else if (serviceType == typeof(IXamlNamespaceResolver))
            {
                return this;
            }
            else if (serviceType == typeof(IXamlNameResolver))
            {
                return this;
            }
            else if (serviceType == typeof(IXamlObjectWriterFactory))
            {
                return new XamlObjectWriterFactory(_xamlContext);
            }
            else if (serviceType == typeof(IDestinationTypeProvider))
            {
                return this;
            }
            else if (serviceType == typeof(IXamlLineInfo))
            {
                return this;
            }

            return null;
        }
        #endregion

        #region ITypeDescriptorContext Methods
        // ITypeDescriptorContext derives from IServiceProvider.
        void ITypeDescriptorContext.OnComponentChanged()
        {
        }

        bool ITypeDescriptorContext.OnComponentChanging()
        {
            return false;
        }

        IContainer ITypeDescriptorContext.Container
        {
            get { return null; }
        }

        object ITypeDescriptorContext.Instance
        {
            get { return null; }
        }

        PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
        {
            get { return null; }
        }
        #endregion

        #region IXamlTypeResolver Members
        Type IXamlTypeResolver.Resolve(string qName)
        {
            return _xamlContext.ServiceProvider_Resolve(qName);
        }
        #endregion

        #region IUriContext Members
        Uri IUriContext.BaseUri
        {
            get { return _xamlContext.BaseUri; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }
        #endregion

        #region IAmbientProvider Members
        AmbientPropertyValue IAmbientProvider.GetFirstAmbientValue(
                                                    IEnumerable<XamlType> ceilingTypes,
                                                    params XamlMember[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            foreach (var property in properties)
            {
                if (property == null)
                {
                    // we don't allow any property to be null
                    throw new ArgumentException(SR.Get(SRID.ValueInArrayIsNull, "properties"));
                }
            }

            return _xamlContext.ServiceProvider_GetFirstAmbientValue(ceilingTypes, properties);
        }

        object IAmbientProvider.GetFirstAmbientValue(params XamlType[] types)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }

            foreach (var type in types)
            {
                if (type == null)
                {
                    // we don't allow any type to be null
                    throw new ArgumentException(SR.Get(SRID.ValueInArrayIsNull, "types"));
                }
            }
            return _xamlContext.ServiceProvider_GetFirstAmbientValue(types);
        }


        IEnumerable<AmbientPropertyValue> IAmbientProvider.GetAllAmbientValues(
                                                    IEnumerable<XamlType> ceilingTypes,
                                                    params XamlMember[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            foreach (var property in properties)
            {
                if (property == null)
                {
                    // we don't allow any property to be null
                    throw new ArgumentException(SR.Get(SRID.ValueInArrayIsNull, "properties"));
                }
            }

            return _xamlContext.ServiceProvider_GetAllAmbientValues(ceilingTypes, properties);
        }

        IEnumerable<object> IAmbientProvider.GetAllAmbientValues(params XamlType[] types)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }

            foreach (var type in types)
            {
                if (type == null)
                {
                    // we don't allow any type to be null
                    throw new ArgumentException(SR.Get(SRID.ValueInArrayIsNull, "types"));
                }
            }

            return _xamlContext.ServiceProvider_GetAllAmbientValues(types);
        }

        IEnumerable<AmbientPropertyValue> IAmbientProvider.GetAllAmbientValues(
                                                    IEnumerable<XamlType> ceilingTypes,
                                                    bool searchLiveStackOnly,
                                                    IEnumerable<XamlType> types,
                                                    params XamlMember[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            foreach (var property in properties)
            {
                if (property == null)
                {
                    // we don't allow any property to be null
                    throw new ArgumentException(SR.Get(SRID.ValueInArrayIsNull, "properties"));
                }
            }

            return _xamlContext.ServiceProvider_GetAllAmbientValues(ceilingTypes, searchLiveStackOnly, types, properties);
        }

        #endregion

        #region IXamlSchemaContextProvider Members
        XamlSchemaContext IXamlSchemaContextProvider.SchemaContext
        {
            get { return _xamlContext.SchemaContext; }
        }
        #endregion

        #region IProvideValueTarget Members
        object IProvideValueTarget.TargetObject
        {
            get { return _xamlContext.ParentInstance; }
        }

        object IProvideValueTarget.TargetProperty
        {
            get { return ContextServices.GetTargetProperty(this._xamlContext); }
        }
        #endregion

        #region IRootObjectProvider Members
        object IRootObjectProvider.RootObject
        {
            get
            {
                return _xamlContext.RootInstance;
            }
        }
        #endregion

        #region IXamlNamespaceResolver Members
        string IXamlNamespaceResolver.GetNamespace(string prefix)
        {
            string xns = _xamlContext.FindNamespaceByPrefix(prefix);
            return xns;
        }

        IEnumerable<NamespaceDeclaration> IXamlNamespaceResolver.GetNamespacePrefixes()
        {
            return _xamlContext.GetNamespacePrefixes();
        }
        #endregion

        #region IXamlNameResolver Members

        bool IXamlNameResolver.IsFixupTokenAvailable
        {
            get { return !_xamlContext.NameResolutionComplete; }
        }

        object IXamlNameResolver.Resolve(string name)
        {
            bool isFullyInitialized;
            return _xamlContext.ResolveName(name, out isFullyInitialized);
        }

        object IXamlNameResolver.Resolve(string name, out bool isFullyInitialized)
        {
            return _xamlContext.ResolveName(name, out isFullyInitialized);
        }

        object IXamlNameResolver.GetFixupToken(IEnumerable<string> names)
        {
            return ((IXamlNameResolver)this).GetFixupToken(names, false);
        }

        object IXamlNameResolver.GetFixupToken(IEnumerable<string> names, bool canAssignDirectly)
        {
            if (_xamlContext.NameResolutionComplete)
            {
                return null;
            }
            var token = new NameFixupToken();
            token.CanAssignDirectly = canAssignDirectly;
            token.NeededNames.AddRange(names);
            if (token.CanAssignDirectly && token.NeededNames.Count != 1)
            {
                throw new ArgumentException(SR.Get(SRID.SimpleFixupsMustHaveOneName), "names");
            }

            // TypeConverter case (aka "Initialization")
            if (_xamlContext.CurrentType == null)
            {
                // If this is OBJECT Initialization
                if (_xamlContext.ParentProperty == XamlLanguage.Initialization)
                {
                    token.FixupType = FixupType.ObjectInitializationValue;

                    // If this is object initialization syntax:
                    //  SO Button>
                    //    SM Background
                    //       SO RefObject
                    //         SM _Initialization
                    //           V "_name"
                    // This TC will return the RefObject and
                    // The fixup is to Button.Background  (the grand parent)
                    //
                    token.Target.Instance = _xamlContext.GrandParentInstance;
                    token.Target.InstanceWasGotten = _xamlContext.GrandParentIsObjectFromMember;
                    token.Target.InstanceType = _xamlContext.GrandParentType;
                    token.Target.Property = _xamlContext.GrandParentProperty;
                }
                else  // This is PROPERTY Initialization
                {
                    token.FixupType = FixupType.PropertyValue;

                    // If this is Property Value syntax:
                    //  SO TextBox
                    //    SM LabelProp   [has a NameRef TypeConverter]
                    //       V "_name"
                    // The fixup is to TextBox.LabelProp (the parent)
                    //
                    token.Target.Instance = _xamlContext.ParentInstance;
                    token.Target.InstanceWasGotten = _xamlContext.ParentIsObjectFromMember;
                    token.Target.InstanceType = _xamlContext.ParentType;
                    token.Target.Property = _xamlContext.ParentProperty;
                }
            }
            else  // MarkupExtensions
            {
                token.FixupType = FixupType.MarkupExtensionRerun;

                token.Target.Instance = _xamlContext.ParentInstance;
                token.Target.InstanceWasGotten = _xamlContext.ParentIsObjectFromMember;
                token.Target.InstanceType = _xamlContext.ParentType;
                token.Target.Property = _xamlContext.ParentProperty;
            }

            // We don't need to save a context stack for simple fixups (Assigned Directly).
            // This makes them ALOT cheaper.
            // [Simple fixups only need the namescope from the stack.]
            //
            if (token.CanAssignDirectly)
            {
                token.NameScopeDictionaryList.AddRange(_xamlContext.StackWalkOfNameScopes);
            }
            else
            {
                token.SavedContext = _xamlContext.GetSavedContext((token.FixupType == FixupType.MarkupExtensionRerun)
                                                                    ? SavedContextType.ReparseMarkupExtension
                                                                    : SavedContextType.ReparseValue);
            }

            return token;
        }


        IEnumerable<KeyValuePair<string, object>> IXamlNameResolver.GetAllNamesAndValuesInScope()
        {
            return _xamlContext.GetAllNamesAndValuesInScope();
        }

        event EventHandler IXamlNameResolver.OnNameScopeInitializationComplete
        {
            add
            {
                _xamlContext.AddNameScopeInitializationCompleteSubscriber(value);
            }
            remove
            {

                _xamlContext.RemoveNameScopeInitializationCompleteSubscriber(value);
            }
        }

        #endregion

        #region IDestinationTypeProvider Members

        public Type GetDestinationType()
        {
            return _xamlContext.GetDestinationType().UnderlyingType;
        }

        #endregion

        #region IXamlLineInfo Members

        public bool HasLineInfo
        {
            get { return _xamlContext.LineNumber != 0 || _xamlContext.LinePosition != 0; }
        }

        public int LineNumber
        {
            get { return _xamlContext.LineNumber; }
        }

        public int LinePosition
        {
            get { return _xamlContext.LinePosition; }
        }

        #endregion

    }
}
