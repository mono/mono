//------------------------------------------------------------------------------
// <copyright file="SettingsBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using  System.Collections.Specialized;
    using  System.Runtime.Serialization;
    using  System.Configuration.Provider;
    using  System.Collections;
    using System.ComponentModel;
   ////////////////////////////////////////////////////////////
   ////////////////////////////////////////////////////////////
   ////////////////////////////////////////////////////////////

   public abstract class SettingsBase {

       protected SettingsBase()
       {
           _PropertyValues = new SettingsPropertyValueCollection();
       }
       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       public virtual object this[string propertyName]
       {
           get {
               if (IsSynchronized) {
                   lock (this) {
                       return GetPropertyValueByName(propertyName);
                   }
               } else {
                   return GetPropertyValueByName(propertyName);
               }
           }
           set {
               if (IsSynchronized) {
                   lock (this) {
                       SetPropertyValueByName(propertyName, value);
                   }
               } else {
                   SetPropertyValueByName(propertyName, value);
               }
           }
       }

       private object GetPropertyValueByName(string propertyName)
       {
           if (Properties == null || _PropertyValues == null || Properties.Count == 0)
               throw new SettingsPropertyNotFoundException(SR.GetString(SR.SettingsPropertyNotFound, propertyName));
           SettingsProperty pp = Properties[propertyName];
           if (pp == null)
               throw new SettingsPropertyNotFoundException(SR.GetString(SR.SettingsPropertyNotFound, propertyName));
           SettingsPropertyValue p = _PropertyValues[propertyName];
           if (p == null)
           {
               GetPropertiesFromProvider(pp.Provider);
               p = _PropertyValues[propertyName];
               if (p == null)
                   throw new SettingsPropertyNotFoundException(SR.GetString(SR.SettingsPropertyNotFound, propertyName));
           }
           return p.PropertyValue;
       }
       private void SetPropertyValueByName(string propertyName, object propertyValue)
       {
           if (Properties == null || _PropertyValues == null || Properties.Count == 0)
               throw new SettingsPropertyNotFoundException(SR.GetString(SR.SettingsPropertyNotFound, propertyName));

           SettingsProperty pp = Properties[propertyName];
           if (pp == null)
               throw new SettingsPropertyNotFoundException(SR.GetString(SR.SettingsPropertyNotFound, propertyName));

           if (pp.IsReadOnly)
               throw new SettingsPropertyIsReadOnlyException(SR.GetString(SR.SettingsPropertyReadOnly, propertyName));

           if (propertyValue != null && !pp.PropertyType.IsInstanceOfType(propertyValue))
               throw new SettingsPropertyWrongTypeException(SR.GetString(SR.SettingsPropertyWrongType, propertyName));

           SettingsPropertyValue p = _PropertyValues[propertyName];
           if (p == null)
           {
               GetPropertiesFromProvider(pp.Provider);
               p = _PropertyValues[propertyName];
               if (p == null)
                   throw new SettingsPropertyNotFoundException(SR.GetString(SR.SettingsPropertyNotFound, propertyName));
           }

           p.PropertyValue = propertyValue;
       }

       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       public void Initialize(
               SettingsContext                  context,
               SettingsPropertyCollection       properties,
               SettingsProviderCollection       providers)
       {
           _Context = context;
           _Properties = properties;
           _Providers = providers;
       }

       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       public virtual void Save() {
           if (IsSynchronized) {
               lock (this) {
                   SaveCore();
               }
           } else {
               SaveCore();
           }
       }
       private void SaveCore()
       {
           if (Properties == null || _PropertyValues == null || Properties.Count == 0)
               return;

           foreach(SettingsProvider prov in Providers) {
               SettingsPropertyValueCollection ppcv = new SettingsPropertyValueCollection();
               foreach (SettingsPropertyValue pp in PropertyValues)
               {
                   if (pp.Property.Provider == prov) {
                       ppcv.Add(pp);
                   }
               }
               if (ppcv.Count > 0) {
                   prov.SetPropertyValues(Context, ppcv);
               }
           }
           foreach (SettingsPropertyValue pp in PropertyValues)
               pp.IsDirty = false;
       }
       virtual public SettingsPropertyCollection Properties  { get { return _Properties; }}
       virtual public SettingsProviderCollection Providers   { get { return _Providers; }}
       virtual public SettingsPropertyValueCollection PropertyValues { get { return _PropertyValues; } }
       virtual public SettingsContext Context { get { return _Context; } }


       private void GetPropertiesFromProvider(SettingsProvider provider)
       {
           SettingsPropertyCollection ppc = new SettingsPropertyCollection();
           foreach (SettingsProperty pp in Properties)
           {
               if (pp.Provider == provider)
               {
                   ppc.Add(pp);
               }
           }

           if (ppc.Count > 0)
           {
               SettingsPropertyValueCollection ppcv = provider.GetPropertyValues(Context, ppc);
               foreach (SettingsPropertyValue p in ppcv)
               {
                   if (_PropertyValues[p.Name] == null)
                       _PropertyValues.Add(p);
               }
           }
       }

       public static SettingsBase Synchronized(SettingsBase settingsBase)
       {
           settingsBase._IsSynchronized = true;
           return settingsBase;
       }
       ////////////////////////////////////////////////////////////
       ////////////////////////////////////////////////////////////
       private  SettingsPropertyCollection      _Properties       = null;
       private  SettingsProviderCollection      _Providers        = null;
       private  SettingsPropertyValueCollection _PropertyValues   = null;
       private  SettingsContext                 _Context          = null;
       private  bool                            _IsSynchronized   = false;

       [Browsable(false)]
       public bool IsSynchronized { get { return _IsSynchronized; } }
   }
}
