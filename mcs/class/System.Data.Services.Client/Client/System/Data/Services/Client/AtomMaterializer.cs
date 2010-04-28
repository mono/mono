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
    #region Namespaces.

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;

    #endregion Namespaces.

    internal static class AtomMaterializerInvoker
    {
        internal static IEnumerable<T> EnumerateAsElementType<T>(IEnumerable source)
        {
            return AtomMaterializer.EnumerateAsElementType<T>(source);
        }

        internal static List<TTarget> ListAsElementType<T, TTarget>(object materializer, IEnumerable<T> source) where T : TTarget
        {
            Debug.Assert(materializer.GetType() == typeof(AtomMaterializer), "materializer.GetType() == typeof(AtomMaterializer)");
            return AtomMaterializer.ListAsElementType<T, TTarget>((AtomMaterializer)materializer, source);
        }

        internal static bool ProjectionCheckValueForPathIsNull(
            object entry,
            Type expectedType,
            object path)
        {
            Debug.Assert(entry.GetType() == typeof(AtomEntry), "entry.GetType() == typeof(AtomEntry)");
            Debug.Assert(path.GetType() == typeof(ProjectionPath), "path.GetType() == typeof(ProjectionPath)");
            return AtomMaterializer.ProjectionCheckValueForPathIsNull((AtomEntry)entry, expectedType, (ProjectionPath)path);
        }

        internal static IEnumerable ProjectionSelect(
            object materializer,
            object entry,
            Type expectedType,
            Type resultType,
            object path,
            Func<object, object, Type, object> selector)
        {
            Debug.Assert(materializer.GetType() == typeof(AtomMaterializer), "materializer.GetType() == typeof(AtomMaterializer)");
            Debug.Assert(entry.GetType() == typeof(AtomEntry), "entry.GetType() == typeof(AtomEntry)");
            Debug.Assert(path.GetType() == typeof(ProjectionPath), "path.GetType() == typeof(ProjectionPath)");
            return AtomMaterializer.ProjectionSelect((AtomMaterializer)materializer, (AtomEntry)entry, expectedType, resultType, (ProjectionPath)path, selector);
        }

        internal static AtomEntry ProjectionGetEntry(object entry, string name)
        {
            Debug.Assert(entry.GetType() == typeof(AtomEntry), "entry.GetType() == typeof(AtomEntry)");
            return AtomMaterializer.ProjectionGetEntry((AtomEntry)entry, name);
        }

        internal static object ProjectionInitializeEntity(
            object materializer,
            object entry,
            Type expectedType,
            Type resultType,
            string[] properties,
            Func<object, object, Type, object>[] propertyValues)
        {
            Debug.Assert(materializer.GetType() == typeof(AtomMaterializer), "materializer.GetType() == typeof(AtomMaterializer)");
            Debug.Assert(entry.GetType() == typeof(AtomEntry), "entry.GetType() == typeof(AtomEntry)");
            return AtomMaterializer.ProjectionInitializeEntity((AtomMaterializer)materializer, (AtomEntry)entry, expectedType, resultType, properties, propertyValues);
        }

        internal static object ProjectionValueForPath(object materializer, object entry, Type expectedType, object path)
        {
            Debug.Assert(materializer.GetType() == typeof(AtomMaterializer), "materializer.GetType() == typeof(AtomMaterializer)");
            Debug.Assert(entry.GetType() == typeof(AtomEntry), "entry.GetType() == typeof(AtomEntry)");
            Debug.Assert(path.GetType() == typeof(ProjectionPath), "path.GetType() == typeof(ProjectionPath)");
            return AtomMaterializer.ProjectionValueForPath((AtomMaterializer)materializer, (AtomEntry)entry, expectedType, (ProjectionPath)path);
        }

        internal static object DirectMaterializePlan(object materializer, object entry, Type expectedEntryType)
        {
            Debug.Assert(materializer.GetType() == typeof(AtomMaterializer), "materializer.GetType() == typeof(AtomMaterializer)");
            Debug.Assert(entry.GetType() == typeof(AtomEntry), "entry.GetType() == typeof(AtomEntry)");
            return AtomMaterializer.DirectMaterializePlan((AtomMaterializer)materializer, (AtomEntry)entry, expectedEntryType);
        }

        internal static object ShallowMaterializePlan(object materializer, object entry, Type expectedEntryType)
        {
            Debug.Assert(materializer.GetType() == typeof(AtomMaterializer), "materializer.GetType() == typeof(AtomMaterializer)");
            Debug.Assert(entry.GetType() == typeof(AtomEntry), "entry.GetType() == typeof(AtomEntry)");
            return AtomMaterializer.ShallowMaterializePlan((AtomMaterializer)materializer, (AtomEntry)entry, expectedEntryType);
        }
    }

    [DebuggerDisplay("AtomMaterializer {parser}")]
    internal class AtomMaterializer
    {
        #region Private fields.

        private readonly DataServiceContext context;

        private readonly Type expectedType;


        private readonly AtomMaterializerLog log;

        private readonly ProjectionPlan materializeEntryPlan;

        private readonly Action<object, object> materializedObjectCallback;

        private readonly MergeOption mergeOption;

        private readonly Dictionary<IEnumerable, DataServiceQueryContinuation> nextLinkTable;

        private readonly AtomParser parser;

        private object currentValue;

        private bool ignoreMissingProperties;

        private object targetInstance;

        #endregion Private fields.

        #region Constructors.

        internal AtomMaterializer(
            AtomParser parser, 
            DataServiceContext context, 
            Type expectedType, 
            bool ignoreMissingProperties, 
            MergeOption mergeOption, 
            AtomMaterializerLog log, 
            Action<object, object> materializedObjectCallback,
            QueryComponents queryComponents,
            ProjectionPlan plan)
        {
            Debug.Assert(context != null, "context != null");
            Debug.Assert(parser != null, "parser != null");
            Debug.Assert(log != null, "log != null");

            this.context = context;
            this.parser = parser;
            this.expectedType = expectedType;
            this.ignoreMissingProperties = ignoreMissingProperties;
            this.mergeOption = mergeOption;
            this.log = log;
            this.materializedObjectCallback = materializedObjectCallback;
            this.nextLinkTable = new Dictionary<IEnumerable, DataServiceQueryContinuation>(ReferenceEqualityComparer<IEnumerable>.Instance);
            this.materializeEntryPlan = plan ?? CreatePlan(queryComponents);
        }

        #endregion Constructors.

        #region Internal properties.

        internal DataServiceContext Context
        {
            get { return this.context; }
        }

        internal ProjectionPlan MaterializeEntryPlan
        {
            get { return this.materializeEntryPlan; }
        }

        internal object TargetInstance
        {
            get 
            { 
                return this.targetInstance;
            }

            set
            {
                Debug.Assert(value != null, "value != null -- otherwise we have no instance target.");
                this.targetInstance = value;
            }
        }

        internal AtomFeed CurrentFeed
        {
            get
            {
                return this.parser.CurrentFeed;
            }
        }

        internal AtomEntry CurrentEntry
        {
            get
            {
                return this.parser.CurrentEntry;
            }
        }

        internal object CurrentValue
        {
            get
            {
                return this.currentValue;
            }
        }

        internal AtomMaterializerLog Log
        {
            get { return this.log; }
        }

        internal Dictionary<IEnumerable, DataServiceQueryContinuation> NextLinkTable
        {
            get { return this.nextLinkTable; }
        }

        internal bool IsEndOfStream
        {
            get { return this.parser.DataKind == AtomDataKind.Finished; }
        }

        #endregion Internal properties.

        #region Projection support.

        internal static IEnumerable<T> EnumerateAsElementType<T>(IEnumerable source)
        {
            Debug.Assert(source != null, "source != null");

            IEnumerable<T> typedSource = source as IEnumerable<T>;
            if (typedSource != null)
            {
                return typedSource;
            }
            else
            {
                return EnumerateAsElementTypeInternal<T>(source);
            }
        }

        internal static IEnumerable<T> EnumerateAsElementTypeInternal<T>(IEnumerable source)
        {
            Debug.Assert(source != null, "source != null");

            foreach (object item in source)
            {
                yield return (T)item;
            }
        }

        internal static bool IsDataServiceCollection(Type type)
        {
            while (type != null)
            {
                if (type.IsGenericType && WebUtil.IsDataServiceCollectionType(type.GetGenericTypeDefinition()))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        internal static List<TTarget> ListAsElementType<T, TTarget>(AtomMaterializer materializer, IEnumerable<T> source) where T : TTarget
        {
            Debug.Assert(materializer != null, "materializer != null");
            Debug.Assert(source != null, "source != null");

            List<TTarget> typedSource = source as List<TTarget>;
            if (typedSource != null)
            {
                return typedSource;
            }

            List<TTarget> list;
            IList sourceList = source as IList;
            if (sourceList != null)
            {
                list = new List<TTarget>(sourceList.Count);
            }
            else
            {
                list = new List<TTarget>();
            }

            foreach (T item in source)
            {
                list.Add((TTarget)item);
            }

            DataServiceQueryContinuation continuation;
            if (materializer.nextLinkTable.TryGetValue(source, out continuation))
            {
                materializer.nextLinkTable[list] = continuation;
            }

            return list;
        }

        internal static bool ProjectionCheckValueForPathIsNull(
            AtomEntry entry,
            Type expectedType,
            ProjectionPath path)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(path != null, "path != null");

            if (path.Count == 0 || path.Count == 1 && path[0].Member == null)
            {
                return entry.IsNull;
            }

            bool result = false;
            AtomContentProperty atomProperty = null;
            List<AtomContentProperty> properties = entry.DataValues;
            for (int i = 0; i < path.Count; i++)
            {
                var segment = path[i];
                if (segment.Member == null)
                {
                    continue;
                }

                bool segmentIsLeaf = i == path.Count - 1;
                string propertyName = segment.Member;
                ClientType.ClientProperty property = ClientType.Create(expectedType).GetProperty(propertyName, false);
                atomProperty = GetPropertyOrThrow(properties, propertyName);
                ValidatePropertyMatch(property, atomProperty);
                if (atomProperty.Feed != null)
                {
                    Debug.Assert(segmentIsLeaf, "segmentIsLeaf -- otherwise the path generated traverses a feed, which should be disallowed");
                    result = false;
                }
                else
                {
                    Debug.Assert(
                        atomProperty.Entry != null,
                        "atomProperty.Entry != null -- otherwise a primitive property / complex type is being rewritte with a null check; this is only supported for entities and collection");
                    if (segmentIsLeaf)
                    {
                        result = atomProperty.Entry.IsNull;
                    }

                    properties = atomProperty.Entry.DataValues;
                    entry = atomProperty.Entry;
                }

                expectedType = property.PropertyType;
            }

            return result;
        }

        internal static IEnumerable ProjectionSelect(
            AtomMaterializer materializer,
            AtomEntry entry,
            Type expectedType,
            Type resultType,
            ProjectionPath path,
            Func<object, object, Type, object> selector)
        {
            ClientType entryType = entry.ActualType ?? ClientType.Create(expectedType);
            IEnumerable list = (IEnumerable)Util.ActivatorCreateInstance(typeof(List<>).MakeGenericType(resultType));
            AtomContentProperty atomProperty = null;
            ClientType.ClientProperty property = null;
            for (int i = 0; i < path.Count; i++)
            {
                var segment = path[i];
                if (segment.Member == null)
                {
                    continue;
                }

                string propertyName = segment.Member;
                property = entryType.GetProperty(propertyName, false);
                atomProperty = GetPropertyOrThrow(entry, propertyName);

                if (atomProperty.Entry != null)
                {
                    entry = atomProperty.Entry;
                    entryType = ClientType.Create(property.NullablePropertyType, false);
                } 
            }

            ValidatePropertyMatch(property, atomProperty);
            AtomFeed sourceFeed = atomProperty.Feed;
            Debug.Assert(
                sourceFeed != null, 
                "sourceFeed != null -- otherwise ValidatePropertyMatch should have thrown or property isn't a collection (and should be part of this plan)");

            Action<object, object> addMethod = GetAddToCollectionDelegate(list.GetType());
            foreach (var paramEntry in sourceFeed.Entries)
            {
                object projected = selector(materializer, paramEntry, property.CollectionType );
                addMethod(list, projected);
            }

            ProjectionPlan plan = new ProjectionPlan();
            plan.LastSegmentType = property.CollectionType;
            plan.Plan = selector;
            plan.ProjectedType = resultType;

            materializer.FoundNextLinkForCollection(list, sourceFeed.NextLink, plan);

            return list;
        }
        
                internal static AtomEntry ProjectionGetEntry(AtomEntry entry, string name)
        {
            Debug.Assert(entry != null, "entry != null -- ProjectionGetEntry never returns a null entry, and top-level materialization shouldn't pass one in");

            AtomContentProperty property = GetPropertyOrThrow(entry, name);
            if (property.Entry == null)
            {
                throw new InvalidOperationException(Strings.AtomMaterializer_PropertyNotExpectedEntry(name, entry.Identity));
            }

            CheckEntryToAccessNotNull(property.Entry, name);

            return property.Entry;
        }

        internal static object ProjectionInitializeEntity(
            AtomMaterializer materializer,
            AtomEntry entry,
            Type expectedType,
            Type resultType,
            string[] properties,
            Func<object, object, Type, object>[] propertyValues)
        {
            if (entry == null || entry.IsNull)
            {
                throw new NullReferenceException(Strings.AtomMaterializer_EntryToInitializeIsNull(resultType.FullName));
            }

            object result;
            if (!entry.EntityHasBeenResolved)
            {
                AtomMaterializer.ProjectionEnsureEntryAvailableOfType(materializer, entry, resultType);
            }
            else if (!resultType.IsAssignableFrom(entry.ActualType.ElementType))
            {
                string message = Strings.AtomMaterializer_ProjectEntityTypeMismatch(
                    resultType.FullName,
                    entry.ActualType.ElementType.FullName,
                    entry.Identity);
                throw new InvalidOperationException(message);
            }

            result = entry.ResolvedObject;
            
            for (int i = 0; i < properties.Length; i++)
            {
                var property = entry.ActualType.GetProperty(properties[i], materializer.ignoreMissingProperties);
                object value = propertyValues[i](materializer, entry, expectedType);
                if (entry.ShouldUpdateFromPayload && ClientType.Create(property.NullablePropertyType, false).IsEntityType)
                {
                    materializer.Log.SetLink(entry, property.PropertyName, value);
                }

                bool isEntity = property.CollectionType == null || !ClientType.CheckElementTypeIsEntity(property.CollectionType);
                if (entry.ShouldUpdateFromPayload)
                {
                    if (isEntity)
                    {
                        property.SetValue(result, value, property.PropertyName, false);
                    }
                    else
                    {
                        IEnumerable valueAsEnumerable = (IEnumerable)value;
                        DataServiceQueryContinuation continuation = materializer.nextLinkTable[valueAsEnumerable];
                        Uri nextLinkUri = continuation == null ? null : continuation.NextLinkUri;
                        ProjectionPlan plan = continuation == null ? null : continuation.Plan;
                        materializer.MergeLists(entry, property, valueAsEnumerable, nextLinkUri, plan);
                    }
                }
                else if (!isEntity)
                {
                    materializer.FoundNextLinkForUnmodifiedCollection(property.GetValue(entry.ResolvedObject) as IEnumerable);
                }
            }

            return result;
        }

        internal static object ProjectionValueForPath(AtomMaterializer materializer, AtomEntry entry, Type expectedType, ProjectionPath path)
        {
            Debug.Assert(materializer != null, "materializer != null");
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(path != null, "path != null");

            if (path.Count == 0 || path.Count == 1 && path[0].Member == null)
            {
                if (!entry.EntityHasBeenResolved)
                {
                    materializer.Materialize(entry, expectedType, false);
                }

                return entry.ResolvedObject;
            }

            object result = null;
            AtomContentProperty atomProperty = null;
            List<AtomContentProperty> properties = entry.DataValues;
            for (int i = 0; i < path.Count; i++)
            {
                var segment = path[i];
                if (segment.Member == null)
                {
                    continue;
                }

                bool segmentIsLeaf = i == path.Count - 1;
                string propertyName = segment.Member;

                if (segmentIsLeaf)
                {
                    CheckEntryToAccessNotNull(entry, propertyName);
                    if (!entry.EntityPropertyMappingsApplied)
                    {
                        ClientType attributeSourceType = MaterializeAtom.GetEntryClientType(entry.TypeName, materializer.context, expectedType, false);
                        ApplyEntityPropertyMappings(entry, attributeSourceType);
                    }
                }

                ClientType.ClientProperty property = ClientType.Create(expectedType).GetProperty(propertyName, false);
                atomProperty = GetPropertyOrThrow(properties, propertyName);

                ValidatePropertyMatch(property, atomProperty);

                AtomFeed feedValue = atomProperty.Feed;
                if (feedValue != null)
                {
                    Debug.Assert(segmentIsLeaf, "segmentIsLeaf -- otherwise the path generated traverses a feed, which should be disallowed");

                    Type collectionType = ClientType.GetImplementationType(segment.ProjectionType, typeof(ICollection<>));
                    if (collectionType == null)
                    {
                        collectionType = ClientType.GetImplementationType(segment.ProjectionType, typeof(IEnumerable<>));
                    }

                    Debug.Assert(
                        collectionType != null, 
                        "collectionType != null -- otherwise the property should never have been recognized as a collection");
                    
                    Type nestedExpectedType = collectionType.GetGenericArguments()[0];
                    Type feedType = segment.ProjectionType;
                    if (feedType.IsInterface || IsDataServiceCollection(feedType))
                    {
                        feedType = typeof(System.Collections.ObjectModel.Collection<>).MakeGenericType(nestedExpectedType);
                    }

                    IEnumerable list = (IEnumerable)Util.ActivatorCreateInstance(feedType);
                    MaterializeToList(materializer, list, nestedExpectedType, feedValue.Entries);

                    if (IsDataServiceCollection(segment.ProjectionType))
                    {
                        Type dataServiceCollectionType = WebUtil.GetDataServiceCollectionOfT(nestedExpectedType);
                        list = (IEnumerable)Util.ActivatorCreateInstance(
                            dataServiceCollectionType,
                            list,                            
                            TrackingMode.None);                    }

                    ProjectionPlan plan = CreatePlanForShallowMaterialization(nestedExpectedType);
                    materializer.FoundNextLinkForCollection(list, feedValue.NextLink, plan);
                    result = list;
                }
                else if (atomProperty.Entry != null)
                {
                    if (segmentIsLeaf && !atomProperty.Entry.EntityHasBeenResolved && !atomProperty.IsNull)
                    {
                        materializer.Materialize(atomProperty.Entry, property.PropertyType, false);
                    }

                    properties = atomProperty.Entry.DataValues;
                    result = atomProperty.Entry.ResolvedObject;
                    entry = atomProperty.Entry;
                }
                else
                {
                    if (atomProperty.Properties != null)
                    {
                        if (atomProperty.MaterializedValue == null && !atomProperty.IsNull)
                        {
                            ClientType complexType = ClientType.Create(property.PropertyType);
                            object complexInstance = Util.ActivatorCreateInstance(property.PropertyType);
                            MaterializeDataValues(complexType, atomProperty.Properties, materializer.ignoreMissingProperties, materializer.context);
                            ApplyDataValues(complexType, atomProperty.Properties, materializer.ignoreMissingProperties, materializer.context, complexInstance);
                            atomProperty.MaterializedValue = complexInstance;
                        }
                    }
                    else
                    {
                        MaterializeDataValue(property.NullablePropertyType, atomProperty, materializer.context);
                    }

                    properties = atomProperty.Properties;
                    result = atomProperty.MaterializedValue;
                }

                expectedType = property.PropertyType;
            }

            return result;
        }

        internal static void ProjectionEnsureEntryAvailableOfType(AtomMaterializer materializer, AtomEntry entry, Type requiredType)
        {
            Debug.Assert(materializer != null, "materializer != null");
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(
                materializer.targetInstance == null,
                "materializer.targetInstance == null -- projection shouldn't have a target instance set; that's only used for POST replies");

            if (entry.EntityHasBeenResolved)
            {
                if (!requiredType.IsAssignableFrom(entry.ResolvedObject.GetType()))
                {
                    throw new InvalidOperationException(
                        "Expecting type '" + requiredType + "' for '" + entry.Identity + "', but found " +
                        "a previously created instance of type '" + entry.ResolvedObject.GetType());
                }

                return;
            }

            if (entry.Identity == null)
            {
                throw Error.InvalidOperation(Strings.Deserialize_MissingIdElement);
            }

            if (!materializer.TryResolveAsCreated(entry) && 
                !materializer.TryResolveFromContext(entry, requiredType))
            {
                materializer.ResolveByCreatingWithType(entry, requiredType);
            }
            else
            {
                if (!requiredType.IsAssignableFrom(entry.ResolvedObject.GetType()))
                {
                    throw Error.InvalidOperation(Strings.Deserialize_Current(requiredType, entry.ResolvedObject.GetType()));
                }
            }
        }

        internal static object DirectMaterializePlan(AtomMaterializer materializer, AtomEntry entry, Type expectedEntryType)
        {
            materializer.Materialize(entry, expectedEntryType, true);
            return entry.ResolvedObject;
        }

        internal static object ShallowMaterializePlan(AtomMaterializer materializer, AtomEntry entry, Type expectedEntryType)
        {
            materializer.Materialize(entry, expectedEntryType, false);
            return entry.ResolvedObject;
        }

        internal static void ValidatePropertyMatch(ClientType.ClientProperty property, AtomContentProperty atomProperty)
        {
            Debug.Assert(property != null, "property != null");
            Debug.Assert(atomProperty != null, "atomProperty != null");

            if (property.IsKnownType && (atomProperty.Feed != null || atomProperty.Entry != null))
            {
                throw Error.InvalidOperation(Strings.Deserialize_MismatchAtomLinkLocalSimple);
            }

            if (atomProperty.Feed != null && property.CollectionType == null)
            {
                throw Error.InvalidOperation(Strings.Deserialize_MismatchAtomLinkFeedPropertyNotCollection(property.PropertyName));
            }

            if (atomProperty.Entry != null && property.CollectionType != null)
            {
                throw Error.InvalidOperation(Strings.Deserialize_MismatchAtomLinkEntryPropertyIsCollection(property.PropertyName));
            }
        }

        #endregion Projection support.
        
        #region Internal methods.

        internal bool Read()
        {
            this.currentValue = null;

            this.nextLinkTable.Clear();
            while (this.parser.Read())
            {
                Debug.Assert(
                    this.parser.DataKind != AtomDataKind.None,
                    "parser.DataKind != AtomDataKind.None -- otherwise parser.Read() didn't update its state");
                Debug.Assert(
                    this.parser.DataKind != AtomDataKind.Finished,
                    "parser.DataKind != AtomDataKind.Finished -- otherwise parser.Read() shouldn't have returned true");

                switch (this.parser.DataKind)
                {
                    case AtomDataKind.Feed:
                    case AtomDataKind.FeedCount:
                        break;
                    case AtomDataKind.Entry:
                        Debug.Assert(
                            this.parser.CurrentEntry != null,
                            "parser.CurrentEntry != null -- otherwise parser.DataKind shouldn't be Entry");
                        this.CurrentEntry.ResolvedObject = this.TargetInstance;
                        this.currentValue = this.materializeEntryPlan.Run(this, this.CurrentEntry, this.expectedType);
                        return true;
                    case AtomDataKind.PagingLinks:
                        break;
                    default:
                        Debug.Assert(
                            this.parser.DataKind == AtomDataKind.Custom,
                            "parser.DataKind == AtomDataKind.Custom -- otherwise AtomMaterializer.Read switch is missing a case");

                        Type underlyingExpectedType = Nullable.GetUnderlyingType(this.expectedType) ?? this.expectedType;
                        ClientType targetType = ClientType.Create(underlyingExpectedType);
                        if (ClientConvert.IsKnownType(underlyingExpectedType))
                        {
                            string elementText = this.parser.ReadCustomElementString();
                            if (elementText != null)
                            {
                                this.currentValue = ClientConvert.ChangeType(elementText, underlyingExpectedType);
                            }

                            return true;
                        }
                        else if (!targetType.IsEntityType && this.parser.IsDataWebElement)
                        {
                            AtomContentProperty property = this.parser.ReadCurrentPropertyValue();
                            if (property == null || property.IsNull)
                            {
                                this.currentValue = null;
                            }
                            else
                            {
                                this.currentValue = targetType.CreateInstance();
                                MaterializeDataValues(targetType, property.Properties, this.ignoreMissingProperties, this.context);
                                ApplyDataValues(targetType, property.Properties, this.ignoreMissingProperties, this.context, this.currentValue);
                            }

                            return true;
                        }

                        break;
                }
            }

            Debug.Assert(this.parser.DataKind == AtomDataKind.Finished, "parser.DataKind == AtomDataKind.None");
            Debug.Assert(this.parser.CurrentEntry == null, "parser.Current == null");
            return false;
        }

        internal void PropagateContinuation<T>(IEnumerable<T> from, DataServiceCollection<T> to)
        {
            DataServiceQueryContinuation continuation;
            if (this.nextLinkTable.TryGetValue(from, out continuation))
            {
                this.nextLinkTable.Add(to, continuation);
                Util.SetNextLinkForCollection(to, continuation);
            }
        }

        #endregion Internal methods.

        #region Private methods.

        private static void CheckEntryToAccessNotNull(AtomEntry entry, string name)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(name != null, "name != null");

            if (entry.IsNull)
            {
                throw new NullReferenceException(Strings.AtomMaterializer_EntryToAccessIsNull(name));
            }
        }

        private static ProjectionPlan CreatePlan(QueryComponents queryComponents)
        {
            LambdaExpression projection = queryComponents.Projection;
            ProjectionPlan result;
            if (projection == null)
            {
                result = CreatePlanForDirectMaterialization(queryComponents.LastSegmentType);
            }
            else
            {
                result = ProjectionPlanCompiler.CompilePlan(projection, queryComponents.NormalizerRewrites);
                result.LastSegmentType = queryComponents.LastSegmentType;
            }

            return result;
        }

        private static ProjectionPlan CreatePlanForDirectMaterialization(Type lastSegmentType)
        {
            ProjectionPlan result = new ProjectionPlan();
            result.Plan = AtomMaterializerInvoker.DirectMaterializePlan;
            result.ProjectedType = lastSegmentType;
            result.LastSegmentType = lastSegmentType;
            return result;
        }

        private static ProjectionPlan CreatePlanForShallowMaterialization(Type lastSegmentType)
        {
            ProjectionPlan result = new ProjectionPlan();
            result.Plan = AtomMaterializerInvoker.ShallowMaterializePlan;
            result.ProjectedType = lastSegmentType;
            result.LastSegmentType = lastSegmentType;
            return result;
        }

        private static Action<object, object> GetAddToCollectionDelegate(Type listType)
        {
            Debug.Assert(listType != null, "listType != null");
            
            Type listElementType;
            MethodInfo addMethod = ClientType.GetAddToCollectionMethod(listType, out listElementType);
            ParameterExpression list = Expression.Parameter(typeof(object), "list");
            ParameterExpression item = Expression.Parameter(typeof(object), "element");
            Expression body = Expression.Call(Expression.Convert(list, listType), addMethod, Expression.Convert(item, listElementType));
#if ASTORIA_LIGHT
            LambdaExpression lambda = ExpressionHelpers.CreateLambda(body, list, item);
#else
            LambdaExpression lambda = Expression.Lambda(body, list, item);
#endif
            return (Action<object, object>)lambda.Compile();
        }

        private static object GetOrCreateCollectionProperty(object instance, ClientType.ClientProperty property, Type collectionType)
        {
            Debug.Assert(instance != null, "instance != null");
            Debug.Assert(property != null, "property != null");
            Debug.Assert(property.CollectionType != null, "property.CollectionType != null -- otherwise property isn't a collection");

            object result;
            result = property.GetValue(instance);
            if (result == null)
            {
                if (collectionType == null)
                {
                    collectionType = property.PropertyType;
                    if (collectionType.IsInterface)
                    {
                        collectionType = typeof(System.Collections.ObjectModel.Collection<>).MakeGenericType(property.CollectionType);
                    }
                }

                result = Activator.CreateInstance(collectionType);
                property.SetValue(instance, result, property.PropertyName, false );
            }

            Debug.Assert(result != null, "result != null -- otherwise GetOrCreateCollectionProperty didn't fall back to creation");
            return result;
        }

        private static void MaterializeToList(
            AtomMaterializer materializer,
            IEnumerable list,
            Type nestedExpectedType,
            IEnumerable<AtomEntry> entries)
        {
            Debug.Assert(materializer != null, "materializer != null");
            Debug.Assert(list != null, "list != null");

            Action<object, object> addMethod = GetAddToCollectionDelegate(list.GetType());
            foreach (AtomEntry feedEntry in entries)
            {
                if (!feedEntry.EntityHasBeenResolved)
                {
                    materializer.Materialize(feedEntry, nestedExpectedType,  false);
                }

                addMethod(list, feedEntry.ResolvedObject);
            }
        }

         private static bool MaterializeDataValue(Type type, AtomContentProperty atomProperty, DataServiceContext context)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(atomProperty != null, "atomProperty != null");
            Debug.Assert(context != null, "context != null");

            string propertyTypeName = atomProperty.TypeName;
            string propertyValueText = atomProperty.Text;

            ClientType nestedElementType = null;
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            bool knownType = ClientConvert.IsKnownType(underlyingType);
            if (!knownType)
            {
                nestedElementType = MaterializeAtom.GetEntryClientType(propertyTypeName, context, type, true);
                Debug.Assert(nestedElementType != null, "nestedElementType != null -- otherwise ReadTypeAttribute (or someone!) should throw");
                knownType = ClientConvert.IsKnownType(nestedElementType.ElementType);
            }

            if (knownType)
            {
                if (atomProperty.IsNull)
                {
                    if (!ClientType.CanAssignNull(type))
                    {
                        throw new InvalidOperationException(Strings.AtomMaterializer_CannotAssignNull(atomProperty.Name, type.FullName));
                    }

                    atomProperty.MaterializedValue = null;
                    return true;
                }
                else
                {
                    object value = propertyValueText;
                    if (propertyValueText != null)
                    {
                        value = ClientConvert.ChangeType(propertyValueText, (null != nestedElementType ? nestedElementType.ElementType : underlyingType));
                    }

                    atomProperty.MaterializedValue = value;
                    return true;
                }
            }

            return false;
        }

        private static void MaterializeDataValues(
            ClientType actualType, 
            List<AtomContentProperty> values,
            bool ignoreMissingProperties,
            DataServiceContext context)
        {
            Debug.Assert(actualType != null, "actualType != null");
            Debug.Assert(values != null, "values != null");
            Debug.Assert(context != null, "context != null");

            foreach (var atomProperty in values)
            {
                string propertyName = atomProperty.Name;
                
                var property = actualType.GetProperty(propertyName, ignoreMissingProperties); // may throw
                if (property == null)
                {
                    continue;
                }

                if (atomProperty.Feed == null && atomProperty.Entry == null)
                {
                    bool materialized = MaterializeDataValue(property.NullablePropertyType, atomProperty, context);
                    if (!materialized && property.CollectionType != null)
                    {
                        throw Error.NotSupported(Strings.ClientType_CollectionOfNonEntities);
                    }
                }
            }
        }

        private static void ApplyDataValue(ClientType type, AtomContentProperty property, bool ignoreMissingProperties, DataServiceContext context, object instance)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(property != null, "property != null");
            Debug.Assert(context != null, "context != null");
            Debug.Assert(instance != null, "instance != context");

            var prop = type.GetProperty(property.Name, ignoreMissingProperties);
            if (prop == null)
            {
                return;
            }

            if (property.Properties != null)
            {
                if (prop.IsKnownType ||
                    ClientConvert.IsKnownType(MaterializeAtom.GetEntryClientType(property.TypeName, context, prop.PropertyType, true).ElementType))
                {
                    throw Error.InvalidOperation(Strings.Deserialize_ExpectingSimpleValue);
                }

                 bool needToSet = false;
                ClientType complexType = ClientType.Create(prop.PropertyType);
                object complexInstance = prop.GetValue(instance);
                if (complexInstance == null)
                {
                    complexInstance = complexType.CreateInstance();
                    needToSet = true;
                }

                MaterializeDataValues(complexType, property.Properties, ignoreMissingProperties, context);
                ApplyDataValues(complexType, property.Properties, ignoreMissingProperties, context, complexInstance);

                if (needToSet)
                {
                    prop.SetValue(instance, complexInstance, property.Name, true );
                }
            }
            else
            {
                prop.SetValue(instance, property.MaterializedValue, property.Name, true );
            }
        }

        private static void ApplyDataValues(ClientType type, IEnumerable<AtomContentProperty> properties, bool ignoreMissingProperties, DataServiceContext context, object instance)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(properties != null, "properties != null");
            Debug.Assert(context != null, "properties != context");
            Debug.Assert(instance != null, "instance != context");

            foreach (var p in properties)
            {
                ApplyDataValue(type, p, ignoreMissingProperties, context, instance);
            }
        }

        private static void SetValueOnPath(List<AtomContentProperty> values, string path, string value, string typeName)
        {
            Debug.Assert(values != null, "values != null");
            Debug.Assert(path != null, "path != null");

            bool existing = true;
            AtomContentProperty property = null;
            foreach (string step in path.Split('/'))
            {
                if (values == null)
                {
                    Debug.Assert(property != null, "property != null -- if values is null then this isn't the first step");
                    property.EnsureProperties();
                    values = property.Properties;
                }

                property = values.Where(v => v.Name == step).FirstOrDefault();
                if (property == null)
                {
                    AtomContentProperty newProperty = new AtomContentProperty();
                    existing = false;
                    newProperty.Name = step;
                    values.Add(newProperty);
                    property = newProperty;
                }
                else
                {
                    if (property.IsNull)
                    {
                        return;
                    }
                }

                values = property.Properties;
            }

            Debug.Assert(property != null, "property != null -- property path should have at least one segment");
            
            if (existing == false)
            {
                property.TypeName = typeName;
                property.Text = value;
            }
        }

        private static void ApplyEntityPropertyMappings(AtomEntry entry, ClientType entryType)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(entry.Tag is XElement, "entry.Tag is XElement");
            Debug.Assert(entryType != null, "entryType != null -- othewise how would we know to apply property mappings (note that for projections entry.ActualType may be different that entryType)?");
            Debug.Assert(!entry.EntityPropertyMappingsApplied, "!entry.EntityPropertyMappingsApplied -- EPM should happen only once per entry");

            if (entryType.HasEntityPropertyMappings)
            {
                XElement entryElement = entry.Tag as XElement;
                Debug.Assert(entryElement != null, "entryElement != null");
                ApplyEntityPropertyMappings(entry, entryElement, entryType.EpmTargetTree.SyndicationRoot);
                ApplyEntityPropertyMappings(entry, entryElement, entryType.EpmTargetTree.NonSyndicationRoot);
            }

            entry.EntityPropertyMappingsApplied = true;
        }

        private static void ApplyEntityPropertyMappings(AtomEntry entry, XElement entryElement, EpmTargetPathSegment target)
        {
            Debug.Assert(target != null, "target != null");
            Debug.Assert(!target.HasContent, "!target.HasContent");

            Stack<System.Data.Services.Common.EpmTargetPathSegment> segments = new Stack<System.Data.Services.Common.EpmTargetPathSegment>();
            Stack<XElement> elements = new Stack<XElement>();

            segments.Push(target);
            elements.Push(entryElement);

            while (segments.Count > 0)
            {
                System.Data.Services.Common.EpmTargetPathSegment segment = segments.Pop();
                XElement element = elements.Pop();
                if (segment.HasContent)
                {
                    var node = element.Nodes().Where(n => n.NodeType == XmlNodeType.Text || n.NodeType == XmlNodeType.SignificantWhitespace).FirstOrDefault();
                    string elementValue = (node == null) ? null : ((XText)node).Value;
                    Debug.Assert(segment.EpmInfo != null, "segment.EpmInfo != null -- otherwise segment.HasValue should be false");

                    string path = segment.EpmInfo.Attribute.SourcePath;
                    string typeName = (string)element.Attribute(XName.Get(XmlConstants.AtomTypeAttributeName, XmlConstants.DataWebMetadataNamespace));

                    SetValueOnPath(entry.DataValues, path, elementValue, typeName);
                }

                foreach (var item in segment.SubSegments)
                {
                    if (item.IsAttribute)
                    {
                        string localName = item.SegmentName.Substring(1);
                        var attribute = element.Attribute(XName.Get(localName, item.SegmentNamespaceUri));
                        if (attribute != null)
                        {
                            SetValueOnPath(entry.DataValues, item.EpmInfo.Attribute.SourcePath, attribute.Value, null);
                        }
                    }
                    else
                    {
                        var child = element.Element(XName.Get(item.SegmentName, item.SegmentNamespaceUri));
                        if (child != null)
                        {
                            segments.Push(item);
                            elements.Push(child);
                        }
                    }
                }

                Debug.Assert(segments.Count == elements.Count, "segments.Count == elements.Count -- otherwise they're out of sync");
            }
        }

        private static AtomContentProperty GetPropertyOrThrow(List<AtomContentProperty> properties, string propertyName)
        {
            AtomContentProperty atomProperty = null;
            if (properties != null)
            {
                atomProperty = properties.Where(p => p.Name == propertyName).FirstOrDefault();
            }

            if (atomProperty == null)
            {
                throw new InvalidOperationException(Strings.AtomMaterializer_PropertyMissing(propertyName));
            }

            Debug.Assert(atomProperty != null, "atomProperty != null");
            return atomProperty;
        }

        private static AtomContentProperty GetPropertyOrThrow(AtomEntry entry, string propertyName)
        {
            AtomContentProperty atomProperty = null;
            var properties = entry.DataValues;
            if (properties != null)
            {
                atomProperty = properties.Where(p => p.Name == propertyName).FirstOrDefault();
            }

            if (atomProperty == null)
            {
                throw new InvalidOperationException(Strings.AtomMaterializer_PropertyMissingFromEntry(propertyName, entry.Identity));
            }

            Debug.Assert(atomProperty != null, "atomProperty != null");
            return atomProperty;
        }

        private void MergeLists(AtomEntry entry, ClientType.ClientProperty property, IEnumerable list, Uri nextLink, ProjectionPlan plan)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(entry.ResolvedObject != null, "entry.ResolvedObject != null");
            Debug.Assert(property != null, "property != null");
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(plan != null || nextLink == null, "plan != null || nextLink == null");

            if (entry.ShouldUpdateFromPayload && 
                property.NullablePropertyType == list.GetType() && 
                property.GetValue(entry.ResolvedObject) == null)
            {
                property.SetValue(entry.ResolvedObject, list, property.PropertyName, false );
                this.FoundNextLinkForCollection(list, nextLink, plan);

                foreach (object item in list)
                {
                    this.log.AddedLink(entry, property.PropertyName, item);
                }

                return;
            }

            this.ApplyItemsToCollection(entry, property, list, nextLink, plan);
        }

        private bool TryResolveAsTarget(AtomEntry entry)
        {
            if (entry.ResolvedObject == null)
            {
                return false;
            }

            Debug.Assert(
                entry.ResolvedObject == this.TargetInstance,
                "entry.ResolvedObject == this.TargetInstance -- otherwise there we ResolveOrCreateInstance more than once on the same entry");
            Debug.Assert(
                this.mergeOption == MergeOption.OverwriteChanges || this.mergeOption == MergeOption.PreserveChanges,
                "MergeOption.OverwriteChanges and MergeOption.PreserveChanges are the only expected values during SaveChanges");
            entry.ActualType = ClientType.Create(entry.ResolvedObject.GetType());
            this.log.FoundTargetInstance(entry);
            entry.ShouldUpdateFromPayload = this.mergeOption == MergeOption.PreserveChanges ? false : true;
            entry.EntityHasBeenResolved = true;
            return true;
        }

        private bool TryResolveFromContext(AtomEntry entry, Type expectedEntryType)
        {
            bool tracking = this.mergeOption != MergeOption.NoTracking;
            if (tracking)
            {
                EntityStates state;
                entry.ResolvedObject = this.context.TryGetEntity(entry.Identity, entry.ETagText, this.mergeOption, out state);
                if (entry.ResolvedObject != null)
                {
                    if (!expectedEntryType.IsInstanceOfType(entry.ResolvedObject))
                    {
                        throw Error.InvalidOperation(Strings.Deserialize_Current(expectedEntryType, entry.ResolvedObject.GetType()));
                    }

                    entry.ActualType = ClientType.Create(entry.ResolvedObject.GetType());
                    entry.EntityHasBeenResolved = true;

                     entry.ShouldUpdateFromPayload =
                        this.mergeOption == MergeOption.OverwriteChanges ||
                        (this.mergeOption == MergeOption.PreserveChanges && state == EntityStates.Unchanged) ||
                        (this.mergeOption == MergeOption.PreserveChanges && state == EntityStates.Deleted);
                    this.log.FoundExistingInstance(entry);

                    return true;
                }
            }

            return false;
        }

        private void ResolveByCreatingWithType(AtomEntry entry, Type type)
        {
            Debug.Assert(
                entry.ResolvedObject == null,
                "entry.ResolvedObject == null -- otherwise we're about to overwrite - should never be called");
            entry.ActualType = ClientType.Create(type);
            entry.ResolvedObject = Activator.CreateInstance(type);
            entry.CreatedByMaterializer = true;
            entry.ShouldUpdateFromPayload = true;
            entry.EntityHasBeenResolved = true;
            this.log.CreatedInstance(entry);
        }

        private void ResolveByCreating(AtomEntry entry, Type expectedEntryType)
        {
            Debug.Assert(
                entry.ResolvedObject == null,
                "entry.ResolvedObject == null -- otherwise we're about to overwrite - should never be called");

            ClientType actualType = MaterializeAtom.GetEntryClientType(entry.TypeName, this.context, expectedEntryType, true);

            Debug.Assert(actualType != null, "actualType != null -- otherwise ClientType.Create returned a null value");
            this.ResolveByCreatingWithType(entry, actualType.ElementType);
        }

        private bool TryResolveAsCreated(AtomEntry entry)
        {
            AtomEntry existingEntry;
            if (!this.log.TryResolve(entry, out existingEntry))
            {
                return false;
            }

            Debug.Assert(
                existingEntry.ResolvedObject != null, 
                "existingEntry.ResolvedObject != null -- how did it get there otherwise?");
            entry.ActualType = existingEntry.ActualType;
            entry.ResolvedObject = existingEntry.ResolvedObject;
            entry.CreatedByMaterializer = existingEntry.CreatedByMaterializer;
            entry.ShouldUpdateFromPayload = existingEntry.ShouldUpdateFromPayload;
            entry.EntityHasBeenResolved = true;
            return true;
        }

        private void ResolveOrCreateInstance(AtomEntry entry, Type expectedEntryType)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(expectedEntryType != null, "expectedEntryType != null");
            Debug.Assert(entry.EntityHasBeenResolved == false, "entry.EntityHasBeenResolved == false");

            if (!this.TryResolveAsTarget(entry))
            {
                if (entry.Identity == null)
                {
                    throw Error.InvalidOperation(Strings.Deserialize_MissingIdElement);
                }

                if (!this.TryResolveAsCreated(entry))
                {
                    if (!this.TryResolveFromContext(entry, expectedEntryType))
                    {
                        this.ResolveByCreating(entry, expectedEntryType);
                    }
                }
            }

            Debug.Assert(entry.ActualType != null, "entry.ActualType != null");
            Debug.Assert(entry.ResolvedObject != null, "entry.ResolvedObject != null");
            Debug.Assert(entry.EntityHasBeenResolved, "entry.EntityHasBeenResolved");

            return;
        }

        private void ApplyFeedToCollection(
            AtomEntry entry,
            ClientType.ClientProperty property,
            AtomFeed feed,
            bool includeLinks)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(property != null, "property != null");
            Debug.Assert(feed != null, "feed != null");

            ClientType collectionType = ClientType.Create(property.CollectionType);
            foreach (AtomEntry feedEntry in feed.Entries)
            {
                this.Materialize(feedEntry, collectionType.ElementType, includeLinks);
            }

            ProjectionPlan continuationPlan = includeLinks ? CreatePlanForDirectMaterialization(property.CollectionType) : CreatePlanForShallowMaterialization(property.CollectionType);
            this.ApplyItemsToCollection(entry, property, feed.Entries.Select(e => e.ResolvedObject), feed.NextLink, continuationPlan);
        }

        private void ApplyItemsToCollection(
            AtomEntry entry,
            ClientType.ClientProperty property,
            IEnumerable items,
            Uri nextLink,
            ProjectionPlan continuationPlan)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(property != null, "property != null");
            Debug.Assert(items != null, "items != null");

            object collection = entry.ShouldUpdateFromPayload ? GetOrCreateCollectionProperty(entry.ResolvedObject, property, null) : null;
            ClientType collectionType = ClientType.Create(property.CollectionType);
            foreach (object item in items)
            {
                if (!collectionType.ElementType.IsAssignableFrom(item.GetType()))
                {
                    string message = Strings.AtomMaterializer_EntryIntoCollectionMismatch(
                        item.GetType().FullName,
                        collectionType.ElementType.FullName);
                    throw new InvalidOperationException(message);
                }

                if (entry.ShouldUpdateFromPayload)
                {
                    property.SetValue(collection, item, property.PropertyName, true );
                    this.log.AddedLink(entry, property.PropertyName, item);
                }
            }

            if (entry.ShouldUpdateFromPayload)
            {
                this.FoundNextLinkForCollection(collection as IEnumerable, nextLink, continuationPlan);
            }
            else
            {
                this.FoundNextLinkForUnmodifiedCollection(property.GetValue(entry.ResolvedObject) as IEnumerable);
            }

            if (this.mergeOption == MergeOption.OverwriteChanges || this.mergeOption == MergeOption.PreserveChanges)
            {
                var itemsToRemove =
                    from x in this.context.GetLinks(entry.ResolvedObject, property.PropertyName)
                    where MergeOption.OverwriteChanges == this.mergeOption || EntityStates.Added != x.State
                    select x.Target;
                itemsToRemove = itemsToRemove.Except(EnumerateAsElementType<object>(items));
                foreach (var item in itemsToRemove)
                {
                    if (collection != null)
                    {
                        property.RemoveValue(collection, item);
                    }

                    this.log.RemovedLink(entry, property.PropertyName, item);
                }
            }
        }

        private void FoundNextLinkForCollection(IEnumerable collection, Uri link, ProjectionPlan plan)
        {
            Debug.Assert(plan != null || link == null, "plan != null || link == null");

            if (collection != null && !this.nextLinkTable.ContainsKey(collection))
            {
                DataServiceQueryContinuation continuation = DataServiceQueryContinuation.Create(link, plan);
                this.nextLinkTable.Add(collection, continuation);
                Util.SetNextLinkForCollection(collection, continuation);
            }
        }

        private void FoundNextLinkForUnmodifiedCollection(IEnumerable collection)
        {
            if (collection != null && !this.nextLinkTable.ContainsKey(collection))
            {
                this.nextLinkTable.Add(collection, null);
            }
        }

        private void Materialize(AtomEntry entry, Type expectedEntryType, bool includeLinks)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(entry.DataValues != null, "entry.DataValues != null -- otherwise not correctly initialized");
            Debug.Assert(
                entry.ResolvedObject == null || entry.ResolvedObject == this.targetInstance,
                "entry.ResolvedObject == null || entry.ResolvedObject == this.targetInstance -- otherwise getting called twice");
            Debug.Assert(expectedEntryType != null, "expectedType != null");

            this.ResolveOrCreateInstance(entry, expectedEntryType);
            Debug.Assert(entry.ResolvedObject != null, "entry.ResolvedObject != null -- otherwise ResolveOrCreateInstnace didn't do its job");

            this.MaterializeResolvedEntry(entry, includeLinks);
        }

         private void MaterializeResolvedEntry(AtomEntry entry, bool includeLinks)
        {
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(entry.ResolvedObject != null, "entry.ResolvedObject != null -- otherwise not resolved/created!");

            ClientType actualType = entry.ActualType;

            if (!entry.EntityPropertyMappingsApplied)
            {
                ApplyEntityPropertyMappings(entry, entry.ActualType);
            }

            MaterializeDataValues(actualType, entry.DataValues, this.ignoreMissingProperties, this.context);

            foreach (var e in entry.DataValues)
            {
                var prop = actualType.GetProperty(e.Name, this.ignoreMissingProperties);
                if (prop == null)
                {
                    continue;
                }

                if (entry.ShouldUpdateFromPayload == false && e.Entry == null && e.Feed == null)
                {
                    continue;
                }

                if (!includeLinks && (e.Entry != null || e.Feed != null))
                {
                    continue;
                }

                ValidatePropertyMatch(prop, e);

                AtomFeed feedValue = e.Feed;
                if (feedValue != null)
                {
                    Debug.Assert(includeLinks, "includeLinks -- otherwise we shouldn't be materializing this entry");
                    this.ApplyFeedToCollection(entry, prop, feedValue, includeLinks);
                }
                else if (e.Entry != null)
                {
                    if (!e.IsNull)
                    {
                        Debug.Assert(includeLinks, "includeLinks -- otherwise we shouldn't be materializing this entry");
                        this.Materialize(e.Entry, prop.PropertyType, includeLinks);
                    }

                    if (entry.ShouldUpdateFromPayload)
                    {
                        prop.SetValue(entry.ResolvedObject, e.Entry.ResolvedObject, e.Name, true );
                        this.log.SetLink(entry, prop.PropertyName, e.Entry.ResolvedObject);
                    }
                }
                else
                {
                    Debug.Assert(entry.ShouldUpdateFromPayload, "entry.ShouldUpdateFromPayload -- otherwise we're about to set a property we shouldn't");
                    ApplyDataValue(actualType, e, this.ignoreMissingProperties, this.context, entry.ResolvedObject);
                }
            }

            Debug.Assert(entry.ResolvedObject != null, "entry.ResolvedObject != null -- otherwise we didn't do any useful work");
            if (this.materializedObjectCallback != null)
            {
                this.materializedObjectCallback(entry.Tag, entry.ResolvedObject);
            }
        }

        #endregion Private methods.
    }
}
