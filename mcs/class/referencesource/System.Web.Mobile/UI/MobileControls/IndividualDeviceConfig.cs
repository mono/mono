//------------------------------------------------------------------------------
// <copyright file="IndividualDeviceConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------


// Comment this out to get a version that doesn't need synchronized 
// access. This can be used for profiling, to compare whether the lock
// or the late writing is more useful.

using System;
using System.Web.Configuration;
using System.Configuration;
using System.Xml;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;
using System.Web.Util;
using System.Threading;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{

    // Data structure for an individual device configuration.
    // Included predicates, page adapter type, and a list of
    // control/controlAdapter pairs. 
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class IndividualDeviceConfig
    {
        internal delegate bool DeviceQualifiesDelegate(HttpContext context);

        private String _name;
        private readonly ControlsConfig _controlsConfig;
        private DeviceQualifiesDelegate  _deviceQualifiesPredicate;
        private Type _pageAdapterType;
        private IWebObjectFactory _pageAdapterFactory;

        // Parent device configuration.

        private IndividualDeviceConfig _parentConfig;
        private String                 _parentConfigName;

        // ControlType --> ControlAdapterType mapping (one of these
        // per individual device config)
        private readonly Hashtable _controlAdapterTypes = new Hashtable();

        // ControlType --> ControlAdapterType mapping cache, used to
        // store mappings that are derived from a complex lookup (one of these
        // per individual device config)
        private readonly Hashtable _controlAdapterLookupCache = new Hashtable();

        // Provide synchronized access to the hashtable, allowing
        // multiple readers but just one writer.  Here we have one per
        // device config.  
        private readonly ReaderWriterLock _controlAdapterTypesLock = new ReaderWriterLock();

        // The highest level to check.

        private static readonly Type _baseControlType = typeof(System.Web.UI.Control);
        
        
        // This constructor takes both a delegate that chooses this
        // device, and a Type to instantiate the appropriate page
        // adapter with.  
        internal IndividualDeviceConfig(ControlsConfig          controlsConfig,
                                      String                  name,
                                      DeviceQualifiesDelegate deviceQualifiesDelegate,
                                      Type                    pageAdapterType,
                                      String                  parentConfigName)
        {
            _controlsConfig = controlsConfig;
            _name = name;
            _deviceQualifiesPredicate = deviceQualifiesDelegate;
            _parentConfigName = parentConfigName;
            _parentConfig = null;
            PageAdapterType = pageAdapterType;
        }

        // This constructor takes just a page adapter for situations
        // where device selection isn't necessary (e.g., the designer).
        internal IndividualDeviceConfig(Type pageAdapterType) : this(null, null, null, pageAdapterType, null)
        {
        }

        // Given a context, see if this device config should handle
        // the given device.  If there is no predicate, return true. 
        internal /*public*/ bool DeviceQualifies(HttpContext context)
        {
            return _deviceQualifiesPredicate == null ?
                true :
                _deviceQualifiesPredicate(context);
        }

        // Register an adapter with the given control.
        internal /*public*/ void AddControl(Type controlType,
                               Type adapterType)
        {
            // Don't need to synchronize, as this is only being called
            // from one thread -- the configuration section handler. 
            _controlAdapterTypes[controlType] = FactoryGenerator.StaticFactoryGenerator.GetFactory(adapterType);
        }

        private Type PageAdapterType
        {
            get
            {
                return _pageAdapterType;
            }
            set {
                _pageAdapterType = value;
                if (value != null) {
                    Debug.Assert(typeof(IPageAdapter).IsAssignableFrom(value));
                    _pageAdapterFactory =
                        (IWebObjectFactory)FactoryGenerator.StaticFactoryGenerator.GetFactory(_pageAdapterType);
                }
            }
        }

        internal DeviceQualifiesDelegate DeviceQualifiesPredicate
        {
            get
            {
                return _deviceQualifiesPredicate;
            }
            set
            {
                _deviceQualifiesPredicate = value;
            }
        }

        protected IWebObjectFactory LookupControl(Type controlType)
        {
            return LookupControl(controlType, false);
        }

        private IWebObjectFactory LookupControl(Type controlType, bool lookInTypeCache)
        {
            IWebObjectFactory factory;

            factory = (IWebObjectFactory)_controlAdapterTypes[controlType];
            if (factory == null && lookInTypeCache)
            {
                // Grab reader lock...
                using (new ReaderWriterLockResource(_controlAdapterTypesLock,
                                                    false))
                {
                    factory = (IWebObjectFactory)_controlAdapterLookupCache[controlType];
                } 
            }

            return factory;
        }

        // Create a new page adapter for the device.
        internal /*public*/ IPageAdapter NewPageAdapter()
        {
            IPageAdapter a = _pageAdapterFactory.CreateInstance() as IPageAdapter;
            
            if (a == null)
            {
                throw new Exception(
                    SR.GetString(SR.IndividualDeviceConfig_TypeMustSupportInterface,
                                 _pageAdapterType.FullName, "IPageAdapter"));
            }

            return a;
        }

        // Given a control's type, create a control adapter for it.

        internal virtual IControlAdapter NewControlAdapter(Type originalControlType)
        {
            IWebObjectFactory factory = GetAdapterFactory(originalControlType);
            
            // Should return non-null, or throw an exception.
            Debug.Assert(factory != null);

            IControlAdapter a = (IControlAdapter) factory.CreateInstance();
            return a;
        }

        // Given a control's type, returns the adapter type to be used.
        // Note that it's legal to not register an adapter type for each
        // control type.  
        //
        // This lookup uses the following steps:
        //
        // (1) Look up the control type directly, to see if an adapter type
        //     has been registered for it.
        // (2) Walk up the control inheritance chain, to see if an adapter type
        //     has been registered for the class. For example, if the passed
        //     control type is a validator, check BaseValidator, Label,
        //     TextControl, and finally MobileControl.
        // (3) If no adapter type has still been found, call the parent configuration,
        //     if any, to look up the adapter type. For example, the CHTML device
        //     configuration would call the HTML device configuration.
        // (4) If an adapter type is found, but is not explicitly registered for
        //     the passed control type, add an entry to the table, so that
        //     subsequent requests do not need to walk the hierarchy.

        protected IWebObjectFactory GetAdapterFactory(Type originalControlType)
        {
            Debug.Assert(_parentConfigName == null);
            
            Type controlType = originalControlType;
            IWebObjectFactory factory = LookupControl(controlType, true); // Look in type cache

            // Walk up hierarchy looking for registered adapters.
            // Stop when we get to the base control.

            while (factory == null && controlType != _baseControlType)
            {
                factory = LookupControl(controlType);
                if (factory == null)
                {
                    controlType = controlType.BaseType;
                }
            }

            // Could not find one in the current hierarchy. So, look it up in
            // the parent config if there is one.

            if (factory == null && _parentConfig != null)
            {
                factory = _parentConfig.GetAdapterFactory(originalControlType);
            }

            if (factory == null)
            {
                throw new Exception(
                    SR.GetString(SR.IndividualDeviceConfig_ControlWithIncorrectPageAdapter,
                                 controlType.FullName, _pageAdapterType.FullName));
                
            } 

            if (controlType != originalControlType)
            {
                // Add to lookup cache, so the next lookup won't require
                // traversing the hierarchy.

                // Grab writer lock...
                using (new ReaderWriterLockResource(_controlAdapterTypesLock,
                                                    true))
                {
                    _controlAdapterLookupCache[originalControlType] = factory;
                }
            }

            return factory;
        }

        internal /*public*/ String Name
        {
            get
            {
                return _name;
            }
        }
        
        internal /*public*/ String ParentConfigName
        {
            get
            {
                return _parentConfigName;
            }
            set
            {
                _parentConfigName = null;
            }
        }

        internal /*public*/ IndividualDeviceConfig ParentConfig
        {
            get
            {
                return _parentConfig;
            }
            set
            {
                _parentConfig = value;
            }
        }

        private enum FixupState { NotFixedUp, FixingUp, FixedUp };
        private FixupState _fixup = FixupState.NotFixedUp;

        internal /*public*/ void FixupInheritance(IndividualDeviceConfig referrer, XmlNode configNode)
        {
            if (_fixup == FixupState.FixedUp)
            {
                return;
            }

            if (_fixup == FixupState.FixingUp)
            {
                Debug.Assert(referrer != null);

                // Circular reference
                throw new Exception(SR.GetString(SR.MobileControlsSectionHandler_CircularReference, 
                                                 referrer.Name));
            }

            _fixup = FixupState.FixingUp;

            if (ParentConfigName != null)
            {
                Debug.Assert(ParentConfigName.Length != 0 && ParentConfig == null);
                    
                ParentConfig = _controlsConfig.GetDeviceConfig(ParentConfigName);

                if (ParentConfig == null)
                {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.MobileControlsSectionHandler_DeviceConfigNotFound,
                                     ParentConfigName),
                        configNode);
                }

                // Make sure parent is fixed up.

                ParentConfig.FixupInheritance(this, configNode);

                if (PageAdapterType == null)
                {
                    PageAdapterType = ParentConfig.PageAdapterType;
                }

                if (DeviceQualifiesPredicate == null)
                {
                    DeviceQualifiesPredicate = ParentConfig.DeviceQualifiesPredicate;
                }

                Debug.Assert(PageAdapterType != null);
                Debug.Assert(DeviceQualifiesPredicate != null);

                // Reset this since we don't need it any longer. 
                ParentConfigName = null;
            }

            _fixup = FixupState.FixedUp;
        }
    }

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ReaderWriterLockResource : IDisposable
    {
        private ReaderWriterLock _lock;
        private bool _writerLock;
        
        internal /*public*/ ReaderWriterLockResource(ReaderWriterLock theLock, bool writerLock)
        {
            _lock = theLock;
            _writerLock = writerLock;
            if (_writerLock)
            {
                _lock.AcquireWriterLock(Timeout.Infinite);
            }
            else
            {
                _lock.AcquireReaderLock(Timeout.Infinite);
            }
        }

        /*public*/ void IDisposable.Dispose()
        {
            if (_writerLock)
            {
                _lock.ReleaseWriterLock();
            }
            else
            {
                _lock.ReleaseReaderLock();
            }
        }
    }
    
}
