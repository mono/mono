#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Reflection;
using DbLinq.Util;

namespace DbLinq.Data.Linq.Implementation
{
    /// <summary>
    /// ModificationHandler class handles entities in two ways:
    /// 1. if entity implements IModifed, uses the interface and its IsModifed flag property
    /// 2. otherwise, the handler keeps a dictionary of raw data per entity
    /// </summary>
    internal class MemberModificationHandler : IMemberModificationHandler
    {
        private readonly IDictionary<object, IDictionary<string, object>> rawDataEntities = new Dictionary<object, IDictionary<string, object>>(new ReferenceEqualityComparer<object>());
        private readonly IDictionary<object, IDictionary<string, MemberInfo>> modifiedProperties = new Dictionary<object, IDictionary<string, MemberInfo>>(new ReferenceEqualityComparer<object>());

        private static readonly IDictionary<string, MemberInfo> propertyChangingSentinal = new Dictionary<string, MemberInfo>();

        /// <summary>
        /// Gets the column members.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="metaModel">The meta model.</param>
        /// <returns></returns>
        protected virtual IEnumerable<MemberInfo> GetColumnMembers(Type entityType, MetaModel metaModel)
        {
            foreach (var dataMember in metaModel.GetTable(entityType).RowType.PersistentDataMembers)
            {
                yield return dataMember.Member;
            }
        }

        /// <summary>
        /// Determines whether the specified type is primitive type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the specified type is primitive type; otherwise, <c>false</c>.
        /// </returns>
        protected static bool IsPrimitiveType(Type type)
        {
            if (type.IsValueType)
                return true;
            if (type == typeof(string))
                return true;
            return false;
        }

        /// <summary>
        /// Adds simple (value) properties of an object to a given dictionary
        /// and recurses if a property contains complex data
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rawData"></param>
        /// <param name="prefix"></param>
        /// <param name="metaModel"></param>
        protected void AddRawData(object entity, IDictionary<string, object> rawData, string prefix, MetaModel metaModel)
        {
            if (entity == null)
                return;
            foreach (var memberInfo in GetColumnMembers(entity.GetType(), metaModel))
            {
                var propertyValue = memberInfo.GetMemberValue(entity);
                // if it is a value, it can be stored directly
                var memberType = memberInfo.GetMemberType();
                if (IsPrimitiveType(memberType))
                {
                    rawData[prefix + memberInfo.Name] = propertyValue;
                }
                else if (memberType.IsArray)
                {
                    if (propertyValue != null)
                    {
                        var arrayValue = (Array) propertyValue;
                        for (int arrayIndex = 0; arrayIndex < arrayValue.Length; arrayIndex++)
                        {
                            rawData[string.Format("{0}[{1}]", memberInfo.Name, arrayIndex)] =
                                arrayValue.GetValue(arrayIndex);
                        }
                    }
                }
                else // otherwise, we recurse, and prefix the current property name to sub properties to avoid conflicts
                {
                    AddRawData(propertyValue, rawData, memberInfo.Name + ".", metaModel);
                }
            }
        }

        /// <summary>
        /// Creates a "flat view" from a composite object
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        /// <returns>a pair of {property name, property value}</returns>
        protected IDictionary<string, object> GetEntityRawData(object entity, MetaModel metaModel)
        {
            var rawData = new Dictionary<string, object>();
            AddRawData(entity, rawData, string.Empty, metaModel);
            return rawData;
        }

        /// <summary>
        /// Tells if the object notifies a change
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static bool IsNotifying(object entity)
        {
            return entity is INotifyPropertyChanged
                   || entity is INotifyPropertyChanging;
        }

        /// <summary>
        /// Start to watch an entity. From here, changes will make IsModified() return true
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        public void Register(object entity, MetaModel metaModel)
        {
            Register(entity, entity, metaModel);
        }

        /// <summary>
        /// Start to watch an entity. From here, changes will make IsModified() return true if the entity has changed
        /// If the entity is already registered, there's no error, but the entity is reset to its original state
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityOriginalState"></param>
        /// <param name="metaModel"></param>
        public void Register(object entity, object entityOriginalState, MetaModel metaModel)
        {
            // notifying, we need to wait for changes
            if (IsNotifying(entity))
            {
                RegisterNotification(entity, entityOriginalState, metaModel);
            }
            // raw data, we keep a snapshot of the current state
            else
            {
                if (!rawDataEntities.ContainsKey(entity) && entityOriginalState != null)
                    rawDataEntities[entity] = GetEntityRawData(entityOriginalState, metaModel);
            }
        }

        private void RegisterNotification(object entity, object entityOriginalState, MetaModel metaModel)
        {
            if (modifiedProperties.ContainsKey(entity))
                return;
            modifiedProperties[entity] = null;

            var entityChanged = entity as INotifyPropertyChanged;
            if (entityChanged != null)
            {
                entityChanged.PropertyChanged += OnPropertyChangedEvent;
            }

            var entityChanging = entity as INotifyPropertyChanging;
            if (entityChanging != null)
            {
                entityChanging.PropertyChanging += OnPropertyChangingEvent;
            }

            // then check all properties, and note them as changed if they already did
            if (!ReferenceEquals(entity, entityOriginalState)) // only if we specified another original entity
            {
                foreach (var dataMember in metaModel.GetTable(entity.GetType()).RowType.PersistentDataMembers)
                {
                    var memberInfo = dataMember.Member;
                    if (entityOriginalState == null ||
                        IsPropertyModified(memberInfo.GetMemberValue(entity),
                                           memberInfo.GetMemberValue(entityOriginalState)))
                    {
                        SetPropertyChanged(entity, memberInfo.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Occurs on INotifyPropertyChanged.PropertyChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            SetPropertyChanged(sender, e.PropertyName);
        }

        private void OnPropertyChangingEvent(object entity, PropertyChangingEventArgs e)
        {
            if (modifiedProperties[entity] == null)
                modifiedProperties[entity] = propertyChangingSentinal;
        }

        /// <summary>
        /// Unregisters an entity.
        /// This is useful when it is switched from update to delete list
        /// </summary>
        /// <param name="entity"></param>
        public void Unregister(object entity)
        {
            if (IsNotifying(entity))
                UnregisterNotification(entity);
            else
            {
                if (rawDataEntities.ContainsKey(entity))
                    rawDataEntities.Remove(entity);
            }
        }

		/// <summary>
		/// Unregisters an entity.
		/// This is useful when the DataContext has been disposed
		/// </summary>
		/// <param name="entity"></param>
		public void UnregisterAll()
		{
			//Duplicate the list to not modify modifiedEntities
			var modifiedEntities = new List<object>(modifiedProperties.Keys);
			foreach (var entity in modifiedEntities)
			{
				if (IsNotifying(entity))
					UnregisterNotification(entity);
			}
		}

        private void UnregisterNotification(object entity)
        {
            if (!modifiedProperties.ContainsKey(entity))
                return;
            modifiedProperties.Remove(entity);
            INotifyPropertyChanged npc = entity as INotifyPropertyChanged;
            if (npc != null)
            {
                npc.PropertyChanged -= OnPropertyChangedEvent;
            }
            var changing = entity as INotifyPropertyChanging;
            if (changing != null)
            {
                changing.PropertyChanging -= OnPropertyChangingEvent;
            }
        }

        /// <summary>
        /// This method is called when a notifying object sends an event because of a property change
        /// We may keep track of the precise change in the future
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        private void SetPropertyChanged(object entity, string propertyName)
        {
            PropertyInfo pi = GetProperty(entity, propertyName);
            if (pi == null)
                throw new ArgumentException("Incorrect property changed");

            if (modifiedProperties[entity] == null || 
                    ReferenceEquals(propertyChangingSentinal, modifiedProperties[entity]))
            {
                modifiedProperties[entity] = new Dictionary<string, MemberInfo>();
            }
            modifiedProperties[entity][propertyName] = pi;
        }

        /// <summary>
        /// Returns if the entity was modified since it has been Register()ed for the first time
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        /// <returns></returns>
        public bool IsModified(object entity, MetaModel metaModel)
        {
            // 1. event notifying case (INotify*)
            if (IsNotifying(entity))
                return IsNotifyingModified(entity);

            // 2. raw data
            return IsRawModified(entity, metaModel);
        }

        /// <summary>
        /// Determines whether the specified notifiying entity is modified.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// 	<c>true</c> if the specified notifiying entity is modified; otherwise, <c>false</c>.
        /// </returns>
        private bool IsNotifyingModified(object entity)
        {
            if (!modifiedProperties.ContainsKey(entity) || modifiedProperties[entity] == null)
                return false;
            return ReferenceEquals(propertyChangingSentinal, modifiedProperties[entity]) ||
                modifiedProperties[entity].Count > 0;
        }

        /// <summary>
        /// Determines whether the specified property has changed, by comparing its current and previous value.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>
        /// 	<c>true</c> if the specified property has changed; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPropertyModified(object p1, object p2)
        {
            return !Equals(p1, p2);
        }

        /// <summary>
        /// Determines whether the specified raw entity has changed.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="metaModel">The meta model.</param>
        /// <returns>
        /// 	<c>true</c> if the specified raw entity has changed; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRawModified(object entity, MetaModel metaModel)
        {
            // if not present, maybe it was inserted (or set to dirty)
            // TODO: this will be useless when we will support the differential properties
            if (!rawDataEntities.ContainsKey(entity))
                return true;

            IDictionary<string, object> originalData = rawDataEntities[entity];
            IDictionary<string, object> currentData = GetEntityRawData(entity, metaModel);

            foreach (string key in originalData.Keys)
            {
                object originalValue = originalData[key];
                object currentValue = currentData[key];
                if (IsPropertyModified(originalValue, currentValue))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a list of all modified properties since last Register/ClearModified
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        /// <returns></returns>
        public IList<MemberInfo> GetModifiedProperties(object entity, MetaModel metaModel)
        {
            if (IsNotifying(entity))
                return GetNotifyingModifiedProperties(entity, metaModel);

            return GetRawModifiedProperties(entity, metaModel);
        }

        /// <summary>
        /// Gets all column properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="metaModel">The meta model.</param>
        /// <returns></returns>
        protected IList<MemberInfo> GetAllColumnProperties(object entity, MetaModel metaModel)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            var properties = new List<MemberInfo>(GetColumnMembers(entity.GetType(), metaModel));
            return properties;
        }

        /// <summary>
        /// Gets the self declaring entity modified properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="metaModel">The meta model.</param>
        /// <returns></returns>
        protected IList<MemberInfo> GetSelfDeclaringModifiedProperties(object entity, MetaModel metaModel)
        {
            return GetAllColumnProperties(entity, metaModel);
        }

        /// <summary>
        /// Gets the notifying entity modified properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="metaModel">The meta model.</param>
        /// <returns></returns>
        protected IList<MemberInfo> GetNotifyingModifiedProperties(object entity, MetaModel metaModel)
        {
            IDictionary<string, MemberInfo> properties;
            // if we don't have it, it is fully dirty
            if (!modifiedProperties.TryGetValue(entity, out properties) || 
                    ReferenceEquals(propertyChangingSentinal, modifiedProperties[entity]))
                return GetAllColumnProperties(entity, metaModel);
            return new List<MemberInfo>(properties.Values);
        }

        /// <summary>
        /// Gets modified properties for entity, by using raw compare method.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="metaModel">The meta model.</param>
        /// <returns></returns>
        protected IList<MemberInfo> GetRawModifiedProperties(object entity, MetaModel metaModel)
        {
            var properties = new List<MemberInfo>();

            IDictionary<string, object> originalData;
            // if we don't have this entity we consider all its properties as having been modified
            if (!rawDataEntities.TryGetValue(entity, out originalData))
                return GetAllColumnProperties(entity, metaModel);
            var currentData = GetEntityRawData(entity, metaModel);

            // otherwise, we iterate and find what's changed
            foreach (string key in currentData.Keys)
            {
                var currentValue = currentData[key];
                var originalValue = originalData[key];
                if (IsPropertyModified(originalValue, currentValue))
                    properties.Add(GetProperty(entity, key));
            }

            return properties;
        }

        /// <summary>
        /// Marks the entity as not dirty.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        public void ClearModified(object entity, MetaModel metaModel)
        {
            if (IsNotifying(entity))
                ClearNotifyingModified(entity);
            else
                ClearRawModified(entity, metaModel);
        }

        /// <summary>
        /// Sets the notifying entity as unmodified.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void ClearNotifyingModified(object entity)
        {
            modifiedProperties[entity] = null;
        }

        /// <summary>
        /// Sets the raw entity as unmodified.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="metaModel">The meta model.</param>
        private void ClearRawModified(object entity, MetaModel metaModel)
        {
            rawDataEntities[entity] = GetEntityRawData(entity, metaModel);
        }

        /// <summary>
        /// Gets the property, given a property name.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private static PropertyInfo GetProperty(object entity, string propertyName)
        {
            return entity.GetType().GetProperty(propertyName);
        }
    }
}
