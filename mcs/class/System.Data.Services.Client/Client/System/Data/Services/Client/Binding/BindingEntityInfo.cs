//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
#region Namespaces
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
#endregion

    internal enum BindingPropertyKind
    {
        BindingPropertyKindComplex,

        BindingPropertyKindEntity,

        BindingPropertyKindCollection
    }

    internal class BindingEntityInfo
    {
        private static readonly object FalseObject = new object();

        private static readonly object TrueObject = new object();

        private static readonly ReaderWriterLockSlim metadataCacheLock = new ReaderWriterLockSlim();

        private static readonly HashSet<Type> knownNonEntityTypes = new HashSet<Type>(EqualityComparer<Type>.Default);

        private static readonly Dictionary<Type, object> knownObservableCollectionTypes = new Dictionary<Type, object>(EqualityComparer<Type>.Default);

        private static readonly Dictionary<Type, BindingEntityInfoPerType> bindingEntityInfos = new Dictionary<Type, BindingEntityInfoPerType>(EqualityComparer<Type>.Default);

        internal static IList<BindingPropertyInfo> GetObservableProperties(Type entityType)
        {
            return GetBindingEntityInfoFor(entityType).ObservableProperties;
        }

        internal static ClientType GetClientType(Type entityType)
        {
            return GetBindingEntityInfoFor(entityType).ClientType;
        }

        internal static string GetEntitySet(
            object target,
            string targetEntitySet)
        {
            Debug.Assert(target != null, "Argument 'target' cannot be null.");
            Debug.Assert(BindingEntityInfo.IsEntityType(target.GetType()), "Argument 'target' must be an entity type.");

            if (!String.IsNullOrEmpty(targetEntitySet))
            {
                return targetEntitySet;
            }
            else
            {
                return BindingEntityInfo.GetEntitySetAttribute(target.GetType());
            }
        }

        internal static bool IsDataServiceCollection(Type collectionType)
        {
            Debug.Assert(collectionType != null, "Argument 'collectionType' cannot be null.");

            metadataCacheLock.EnterReadLock();
            try
            {
                object resultAsObject;
                if (knownObservableCollectionTypes.TryGetValue(collectionType, out resultAsObject))
                {
                    return resultAsObject == TrueObject;
                }
            }
            finally
            {
                metadataCacheLock.ExitReadLock();
            }

            Type type = collectionType;
            bool result = false;

            while (type != null)
            {
                if (type.IsGenericType)
                {
                    Type[] parms = type.GetGenericArguments();

                    if (parms != null && parms.Length == 1 && IsEntityType(parms[0]))
                    {
                        Type dataServiceCollection = WebUtil.GetDataServiceCollectionOfT(parms);
                        if (dataServiceCollection != null && dataServiceCollection.IsAssignableFrom(type))
                        {
                            result = true;
                            break;
                        }
                    }
                }

                type = type.BaseType;
            }

            metadataCacheLock.EnterWriteLock();
            try
            {
                if (!knownObservableCollectionTypes.ContainsKey(collectionType))
                {
                    knownObservableCollectionTypes[collectionType] = result ? TrueObject : FalseObject;
                }
            }
            finally
            {
                metadataCacheLock.ExitWriteLock();
            }

            return result;
        }

        internal static bool IsEntityType(Type type)
        {
            Debug.Assert(type != null, "Argument 'type' cannot be null.");

            metadataCacheLock.EnterReadLock();
            try
            {
                if (knownNonEntityTypes.Contains(type))
                {
                    return false;
                }
            }
            finally
            {
                metadataCacheLock.ExitReadLock();
            }

            try
            {
                if (BindingEntityInfo.IsDataServiceCollection(type))
                {
                    return false;
                }

                return ClientType.Create(type).IsEntityType;
            }
            catch (InvalidOperationException)
            {
                metadataCacheLock.EnterWriteLock();
                try
                {
                    if (!knownNonEntityTypes.Contains(type))
                    {
                        knownNonEntityTypes.Add(type);
                    }
                }
                finally
                {
                    metadataCacheLock.ExitWriteLock();
                }

                return false;
            }
        }

        internal static object GetPropertyValue(object source, string sourceProperty, out BindingPropertyInfo bindingPropertyInfo)
        {
            Type sourceType = source.GetType();

            bindingPropertyInfo = BindingEntityInfo.GetObservableProperties(sourceType)
                                                   .SingleOrDefault(x => x.PropertyInfo.PropertyName == sourceProperty);

            if (bindingPropertyInfo == null)
            {
                return BindingEntityInfo.GetClientType(sourceType)
                                        .GetProperty(sourceProperty, false)
                                        .GetValue(source);
            }
            else
            {
                return bindingPropertyInfo.PropertyInfo.GetValue(source);
            }
        }

        private static BindingEntityInfoPerType GetBindingEntityInfoFor(Type entityType)
        {
            BindingEntityInfoPerType bindingEntityInfo;

            metadataCacheLock.EnterReadLock();
            try
            {
                if (bindingEntityInfos.TryGetValue(entityType, out bindingEntityInfo))
                {
                    return bindingEntityInfo;
                }
            }
            finally
            {
                metadataCacheLock.ExitReadLock();
            }

            bindingEntityInfo = new BindingEntityInfoPerType();

            object[] attributes = entityType.GetCustomAttributes(typeof(EntitySetAttribute), true);

            bindingEntityInfo.EntitySet = (attributes != null && attributes.Length == 1) ? ((EntitySetAttribute)attributes[0]).EntitySet : null;
            bindingEntityInfo.ClientType = ClientType.Create(entityType);
            
            foreach (ClientType.ClientProperty p in bindingEntityInfo.ClientType.Properties)
            {
                BindingPropertyInfo bpi = null;
            
                Type propertyType = p.PropertyType;
                
                if (p.CollectionType != null)
                {
                    if (BindingEntityInfo.IsDataServiceCollection(propertyType))
                    {
                        bpi = new BindingPropertyInfo { PropertyKind = BindingPropertyKind.BindingPropertyKindCollection };
                    }
                }
                else
                if (BindingEntityInfo.IsEntityType(propertyType))
                {
                    bpi = new BindingPropertyInfo { PropertyKind = BindingPropertyKind.BindingPropertyKindEntity };
                }
                else
                if (BindingEntityInfo.CanBeComplexProperty(p))
                {
                    bpi = new BindingPropertyInfo { PropertyKind = BindingPropertyKind.BindingPropertyKindComplex };
                }
                
                if (bpi != null)
                {
                    bpi.PropertyInfo = p;
                    
                    if (bindingEntityInfo.ClientType.IsEntityType || bpi.PropertyKind == BindingPropertyKind.BindingPropertyKindComplex)
                    {
                        bindingEntityInfo.ObservableProperties.Add(bpi);
                    }
                }
            }

            metadataCacheLock.EnterWriteLock();
            try
            {
                if (!bindingEntityInfos.ContainsKey(entityType))
                {
                    bindingEntityInfos[entityType] = bindingEntityInfo;
                }
            }
            finally
            {
                metadataCacheLock.ExitWriteLock();
            }

            return bindingEntityInfo;
        }

        private static bool CanBeComplexProperty(ClientType.ClientProperty property)
        {
            Debug.Assert(property != null, "property != null");
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(property.PropertyType))
            {
                Debug.Assert(!property.IsKnownType, "Known types do not implement INotifyPropertyChanged.");
                return true;
            }

            return false;
        }

        private static string GetEntitySetAttribute(Type entityType)
        {
            return GetBindingEntityInfoFor(entityType).EntitySet;
        }

        internal class BindingPropertyInfo
        {
            public ClientType.ClientProperty PropertyInfo
            {
                get;
                set;
            }

            public BindingPropertyKind PropertyKind
            {
                get;
                set;
            }
        }

        private sealed class BindingEntityInfoPerType
        {
            private List<BindingPropertyInfo> observableProperties;

            public BindingEntityInfoPerType()
            {
                this.observableProperties = new List<BindingPropertyInfo>();
            }

            public String EntitySet
            {
                get;
                set;
            }

            public ClientType ClientType
            {
                get;
                set;
            }

            public List<BindingPropertyInfo> ObservableProperties
            {
                get
                {
                    return this.observableProperties;
                }
            }
        }

#if ASTORIA_LIGHT
        private sealed class ReaderWriterLockSlim
        {
            private object _lock = new object();

            internal void EnterReadLock()
            {
                Monitor.Enter(_lock);
            }

            internal void EnterWriteLock()
            {
                Monitor.Enter(_lock);
            }

            internal void ExitReadLock()
            {
                Monitor.Exit(_lock);
            }

            internal void ExitWriteLock()
            {
                Monitor.Exit(_lock);
            }
        }
#endif
    }
}
