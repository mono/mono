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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    #endregion

    internal enum VertexColor
    {
        White,
        
        Gray,
        
        Black
    }

    internal sealed class BindingGraph
    {
        private BindingObserver observer;

        private Graph graph;

        public BindingGraph(BindingObserver observer)
        {
            this.observer = observer;
            this.graph = new Graph();
        }

        public bool AddCollection(
            object source, 
            string sourceProperty, 
            object collection, 
            string collectionEntitySet)
        {
            Debug.Assert(collection != null, "'collection' can not be null");
            Debug.Assert(
                BindingEntityInfo.IsDataServiceCollection(collection.GetType()), 
                "Argument 'collection' must be an DataServiceCollection<T> of entity type T");

            if (this.graph.ExistsVertex(collection))
            {
                return false;
            }

            Vertex collectionVertex = this.graph.AddVertex(collection);
            collectionVertex.IsCollection = true;
            collectionVertex.EntitySet = collectionEntitySet;

            ICollection collectionItf = collection as ICollection;

            if (source != null)
            {
                collectionVertex.Parent = this.graph.LookupVertex(source);
                collectionVertex.ParentProperty = sourceProperty;
                this.graph.AddEdge(source, collection, sourceProperty);
                
                Type entityType = BindingUtils.GetCollectionEntityType(collection.GetType());
                Debug.Assert(entityType != null, "Collection must at least be inherited from DataServiceCollection<T>");

                if (!typeof(INotifyPropertyChanged).IsAssignableFrom(entityType))
                {
                    throw new InvalidOperationException(Strings.DataBinding_NotifyPropertyChangedNotImpl(entityType));
                }
                
                typeof(BindingGraph)
                    .GetMethod("SetObserver", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(entityType)
                    .Invoke(this, new object[] { collectionItf });
            }
            else
            {
                this.graph.Root = collectionVertex;
            }

            Debug.Assert(
                    collectionVertex.Parent != null || collectionVertex.IsRootCollection, 
                    "If parent is null, then collectionVertex should be a root collection");

            this.AttachCollectionNotification(collection);

            foreach (var item in collectionItf)
            {
                this.AddEntity(
                        source, 
                        sourceProperty, 
                        item,
                        collectionEntitySet, 
                        collection);
            }

            return true;
        }

        public bool AddEntity(
            object source, 
            string sourceProperty, 
            object target, 
            string targetEntitySet, 
            object edgeSource)
        {
            Vertex sourceVertex = this.graph.LookupVertex(edgeSource);
            Debug.Assert(sourceVertex != null, "Must have a valid edge source");
            
            Vertex entityVertex = null;
            bool addedNewEntity = false;

            if (target != null)
            {
                entityVertex = this.graph.LookupVertex(target);

                if (entityVertex == null)
                {
                    entityVertex = this.graph.AddVertex(target);
                    
                    entityVertex.EntitySet = BindingEntityInfo.GetEntitySet(target, targetEntitySet);

                    if (!this.AttachEntityOrComplexObjectNotification(target))
                    {
                        throw new InvalidOperationException(Strings.DataBinding_NotifyPropertyChangedNotImpl(target.GetType()));
                    }
                    
                    addedNewEntity = true;
                }

                if (this.graph.ExistsEdge(edgeSource, target, sourceVertex.IsCollection ? null : sourceProperty))
                {
                    throw new InvalidOperationException(Strings.DataBinding_EntityAlreadyInCollection(target.GetType()));
                }

                this.graph.AddEdge(edgeSource, target, sourceVertex.IsCollection ? null : sourceProperty);
            }

            if (!sourceVertex.IsCollection)
            {
                this.observer.HandleUpdateEntityReference(
                        source, 
                        sourceProperty,
                        sourceVertex.EntitySet, 
                        target,
                        entityVertex == null ? null : entityVertex.EntitySet);
            }
            else
            {
                Debug.Assert(target != null, "Target must be non-null when adding to collections");
                this.observer.HandleAddEntity(
                        source, 
                        sourceProperty,
                        sourceVertex.Parent != null ? sourceVertex.Parent.EntitySet : null,
                        edgeSource as ICollection, 
                        target,
                        entityVertex.EntitySet);
            }

            if (addedNewEntity)
            {
                this.AddFromProperties(target);
            }

            return addedNewEntity;
        }

        public void Remove(object item, object parent, string parentProperty)
        {
            Vertex vertexToRemove = this.graph.LookupVertex(item);
            if (vertexToRemove == null)
            {
                return;
            }

            Debug.Assert(!vertexToRemove.IsRootCollection, "Root collections are never removed");

            Debug.Assert(parent != null, "Parent has to be present.");

            if (parentProperty != null)
            {
                BindingEntityInfo.BindingPropertyInfo bpi = BindingEntityInfo.GetObservableProperties(parent.GetType())
                                                                             .Single(p => p.PropertyInfo.PropertyName == parentProperty);
                Debug.Assert(bpi.PropertyKind == BindingPropertyKind.BindingPropertyKindCollection, "parentProperty must refer to an DataServiceCollection");

                parent = bpi.PropertyInfo.GetValue(parent);
            }

            object source = null;
            string sourceProperty = null;
            string sourceEntitySet = null;
            string targetEntitySet = null;

            this.GetEntityCollectionInfo(
                    parent,
                    out source,
                    out sourceProperty,
                    out sourceEntitySet,
                    out targetEntitySet);

            targetEntitySet = BindingEntityInfo.GetEntitySet(item, targetEntitySet);

            this.observer.HandleDeleteEntity(
                            source,
                            sourceProperty,
                            sourceEntitySet,
                            parent as ICollection,
                            item,
                            targetEntitySet);

            this.graph.RemoveEdge(parent, item, null);
        }

        public void RemoveCollection(object collection)
        {
            Vertex collectionVertex = this.graph.LookupVertex(collection);
            Debug.Assert(collectionVertex != null, "Must be tracking the vertex for the collection");
            
            foreach (Edge collectionEdge in collectionVertex.OutgoingEdges.ToList())
            {
                this.graph.RemoveEdge(collection, collectionEdge.Target.Item, null);
            }

            this.RemoveUnreachableVertices();
        }

        public void RemoveRelation(object source, string relation)
        {
            Edge edge = this.graph
                            .LookupVertex(source)
                            .OutgoingEdges
                            .SingleOrDefault(e => e.Source.Item == source && e.Label == relation);
            if (edge != null)
            {
                this.graph.RemoveEdge(edge.Source.Item, edge.Target.Item, edge.Label);
            }

            this.RemoveUnreachableVertices();
        }

#if DEBUG
        public bool IsTracking(object item)
        {
            return this.graph.ExistsVertex(item);
        }
#endif
        public void RemoveNonTrackedEntities()
        {
            foreach (var entity in this.graph.Select(o => BindingEntityInfo.IsEntityType(o.GetType()) && !this.observer.IsContextTrackingEntity(o)))
            {
                this.graph.ClearEdgesForVertex(this.graph.LookupVertex(entity));
            }
            
            this.RemoveUnreachableVertices();
        }

        public IEnumerable<object> GetCollectionItems(object collection)
        {
            Vertex collectionVertex = this.graph.LookupVertex(collection);
            Debug.Assert(collectionVertex != null, "Must be tracking the vertex for the collection");
            foreach (Edge collectionEdge in collectionVertex.OutgoingEdges.ToList())
            {
                yield return collectionEdge.Target.Item;
            }
        }

        public void Reset()
        {
            this.graph.Reset(this.DetachNotifications);
        }

        public void RemoveUnreachableVertices()
        {
            this.graph.RemoveUnreachableVertices(this.DetachNotifications);
        }

        public void GetEntityCollectionInfo(
            object collection,
            out object source,
            out string sourceProperty,
            out string sourceEntitySet,
            out string targetEntitySet)
        {
            Debug.Assert(collection != null, "Argument 'collection' cannot be null.");
            Debug.Assert(this.graph.ExistsVertex(collection), "Vertex corresponding to 'collection' must exist in the graph.");
            
            this.graph
                .LookupVertex(collection)
                .GetEntityCollectionInfo(
                    out source, 
                    out sourceProperty, 
                    out sourceEntitySet, 
                    out targetEntitySet);
        }

        public void GetAncestorEntityForComplexProperty(
            ref object entity, 
            ref string propertyName, 
            ref object propertyValue)
        {
            Vertex childVertex = this.graph.LookupVertex(entity);
            Debug.Assert(childVertex != null, "Must have a vertex in the graph corresponding to the entity.");
            Debug.Assert(childVertex.IsComplex == true, "Vertex must correspond to a complex object.");
            
            while (childVertex.IsComplex)
            {
                propertyName = childVertex.IncomingEdges[0].Label;
                propertyValue = childVertex.Item;

                Debug.Assert(childVertex.Parent != null, "Complex properties must always have parent vertices.");
                entity = childVertex.Parent.Item;

                childVertex = childVertex.Parent;
            }
        }

        public void AddComplexProperty(object source, string sourceProperty, object target)
        {
            Vertex parentVertex = this.graph.LookupVertex(source);
            Debug.Assert(parentVertex != null, "Must have a valid parent entity for complex properties.");
            Debug.Assert(target != null, "Must have non-null complex object reference.");

            Vertex complexVertex = this.graph.LookupVertex(target);

            if (complexVertex == null)
            {
                complexVertex = this.graph.AddVertex(target);
                complexVertex.Parent = parentVertex;
                complexVertex.IsComplex = true;

                if (!this.AttachEntityOrComplexObjectNotification(target))
                {
                    throw new InvalidOperationException(Strings.DataBinding_NotifyPropertyChangedNotImpl(target.GetType()));
                }
            }
            else
            {
                throw new InvalidOperationException(Strings.DataBinding_ComplexObjectAssociatedWithMultipleEntities(target.GetType()));
            }

            this.graph.AddEdge(source, target, sourceProperty);

            this.AddFromProperties(target);
        }
        
        private void AddFromProperties(object entity)
        {
            foreach (BindingEntityInfo.BindingPropertyInfo bpi in BindingEntityInfo.GetObservableProperties(entity.GetType()))
            {
                object propertyValue = bpi.PropertyInfo.GetValue(entity);

                if (propertyValue != null)
                {
                    switch (bpi.PropertyKind)
                    {
                        case BindingPropertyKind.BindingPropertyKindCollection:
                            this.AddCollection(
                                    entity,
                                    bpi.PropertyInfo.PropertyName,
                                    propertyValue,
                                    null);
                            
                            break;
                            
                        case BindingPropertyKind.BindingPropertyKindEntity:
                            this.AddEntity(
                                    entity,
                                    bpi.PropertyInfo.PropertyName,
                                    propertyValue,
                                    null,
                                    entity);
                            
                            break;
                            
                        default:
                            Debug.Assert(bpi.PropertyKind == BindingPropertyKind.BindingPropertyKindComplex, "Must be complex type if PropertyKind is not entity or collection.");
                            this.AddComplexProperty(
                                    entity, 
                                    bpi.PropertyInfo.PropertyName, 
                                    propertyValue);
                            break;
                    }
                }
            }
        }

        private void AttachCollectionNotification(object target)
        {
            Debug.Assert(target != null, "Argument 'target' cannot be null");

            INotifyCollectionChanged notify = target as INotifyCollectionChanged;
            Debug.Assert(notify != null, "DataServiceCollection must implement INotifyCollectionChanged");

            notify.CollectionChanged -= this.observer.OnCollectionChanged;
            notify.CollectionChanged += this.observer.OnCollectionChanged;
        }

        private bool AttachEntityOrComplexObjectNotification(object target)
        {
            Debug.Assert(target != null, "Argument 'target' cannot be null");

            INotifyPropertyChanged notify = target as INotifyPropertyChanged;
            if (notify != null)
            {
                notify.PropertyChanged -= this.observer.OnPropertyChanged;
                notify.PropertyChanged += this.observer.OnPropertyChanged;
                return true;
            }

            return false;
        }

        private void DetachNotifications(object target)
        {
            Debug.Assert(target != null, "Argument 'target' cannot be null");
            
            this.DetachCollectionNotifications(target);

            INotifyPropertyChanged notifyPropertyChanged = target as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged -= this.observer.OnPropertyChanged;
            }
        }

        private void DetachCollectionNotifications(object target)
        {
            Debug.Assert(target != null, "Argument 'target' cannot be null");

            INotifyCollectionChanged notifyCollectionChanged = target as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
            {
                notifyCollectionChanged.CollectionChanged -= this.observer.OnCollectionChanged;
            }
        }

        private void SetObserver<T>(ICollection collection)
        {
            DataServiceCollection<T> oec = collection as DataServiceCollection<T>;
            oec.Observer = this.observer;
        }

        internal sealed class Graph
        {
            private Dictionary<object, Vertex> vertices;

            private Vertex root;
            
            public Graph()
            {
                this.vertices = new Dictionary<object, Vertex>(ReferenceEqualityComparer<object>.Instance);
            }

            public Vertex Root
            {
                get
                {
                    Debug.Assert(this.root != null, "Must have a non-null root vertex when this call is made.");
                    return this.root;
                }
                
                set
                {
                    Debug.Assert(this.root == null, "Must only initialize root vertex once.");   
                    Debug.Assert(this.ExistsVertex(value.Item), "Must already have the assigned vertex in the graph.");
                    this.root = value;
                }
            }

            public Vertex AddVertex(object item)
            {
                Vertex v = new Vertex(item);
                this.vertices.Add(item, v);
                return v;
            }

            public void ClearEdgesForVertex(Vertex v)
            {
                foreach (Edge e in v.OutgoingEdges.Concat(v.IncomingEdges).ToList())
                {
                    this.RemoveEdge(e.Source.Item, e.Target.Item, e.Label);
                }
            }

            public bool ExistsVertex(object item)
            {
                Vertex v;
                return this.vertices.TryGetValue(item, out v);
            }

            public Vertex LookupVertex(object item)
            {
                Vertex v;
                this.vertices.TryGetValue(item, out v);
                return v;
            }

            public Edge AddEdge(object source, object target, string label)
            {
                Vertex s = this.vertices[source];
                Vertex t = this.vertices[target];
                Edge e = new Edge { Source = s, Target = t, Label = label };
                s.OutgoingEdges.Add(e);
                t.IncomingEdges.Add(e);
                return e;
            }

            public void RemoveEdge(object source, object target, string label)
            {
                Vertex s = this.vertices[source];
                Vertex t = this.vertices[target];
                Edge e = new Edge { Source = s, Target = t, Label = label };
                s.OutgoingEdges.Remove(e);
                t.IncomingEdges.Remove(e);
            }

            public bool ExistsEdge(object source, object target, string label)
            {
                Edge e = new Edge { Source = this.vertices[source], Target = this.vertices[target], Label = label };
                return this.vertices[source].OutgoingEdges.Any(r => r.Equals(e));
            }

            public IList<object> Select(Func<object, bool> filter)
            {
                return this.vertices.Keys.Where(filter).ToList();
            }

            public void Reset(Action<object> action)
            {
                foreach (object obj in this.vertices.Keys)
                {
                    action(obj);
                }

                this.vertices.Clear();
            }

            public void RemoveUnreachableVertices(Action<object> detachAction)
            {
                try
                {
                    foreach (Vertex v in this.UnreachableVertices())
                    {
                        this.ClearEdgesForVertex(v);
                        detachAction(v.Item);
                        this.vertices.Remove(v.Item);
                    }
                }
                finally
                {
                    foreach (Vertex v in this.vertices.Values)
                    {
                        v.Color = VertexColor.White;
                    }
                }
            }
            
            private IEnumerable<Vertex> UnreachableVertices()
            {
                Queue<Vertex> q = new Queue<Vertex>();
                
                this.Root.Color = VertexColor.Gray;
                q.Enqueue(this.Root);
                
                while (q.Count != 0)
                {
                    Vertex current = q.Dequeue();
                    
                    foreach (Edge e in current.OutgoingEdges)
                    {
                        if (e.Target.Color == VertexColor.White)
                        {
                            e.Target.Color = VertexColor.Gray;
                            q.Enqueue(e.Target);
                        }
                    }
                    
                    current.Color = VertexColor.Black;
                }
                
                return this.vertices.Values.Where(v => v.Color == VertexColor.White).ToList();
            }
        }

        internal sealed class Vertex
        {
            private List<Edge> incomingEdges;
            
            private List<Edge> outgoingEdges;

            public Vertex(object item)
            {
                Debug.Assert(item != null, "item must be non-null");
                this.Item = item;
                this.Color = VertexColor.White;
            }

            public object Item
            {
                get;
                private set;
            }

            public string EntitySet
            {
                get;
                set;
            }

            public bool IsCollection
            {
                get;
                set;
            }

            public bool IsComplex
            {
                get;
                set;
            }

            public Vertex Parent
            {
                get;
                set;
            }

            public string ParentProperty
            {
                get;
                set;
            }

            public bool IsRootCollection
            {
                get
                {
                    return this.IsCollection && this.Parent == null;
                }
            }

            public VertexColor Color
            {
                get;
                set;
            }

            public IList<Edge> IncomingEdges
            {
                get
                {
                    if (this.incomingEdges == null)
                    {
                        this.incomingEdges = new List<Edge>();
                    }

                    return this.incomingEdges;
                }
            }

            public IList<Edge> OutgoingEdges
            {
                get
                {
                    if (this.outgoingEdges == null)
                    {
                        this.outgoingEdges = new List<Edge>();
                    }

                    return this.outgoingEdges;
                }
            }

            public void GetEntityCollectionInfo(
                out object source,
                out string sourceProperty,
                out string sourceEntitySet,
                out string targetEntitySet)
            {
                Debug.Assert(this.IsCollection, "Must be a collection to be in this method");

                if (!this.IsRootCollection)
                {
                    Debug.Assert(this.Parent != null, "Parent must be non-null for child collection");
                    
                    source = this.Parent.Item;
                    Debug.Assert(source != null, "Source object must be present for child collection");

                    sourceProperty = this.ParentProperty;
                    Debug.Assert(sourceProperty != null, "Source entity property associated with a child collection must be non-null");

#if DEBUG
                    PropertyInfo propertyInfo = source.GetType().GetProperty(sourceProperty);
                    Debug.Assert(propertyInfo != null, "Unable to get information for the source entity property associated with a child collection");
#endif
                    sourceEntitySet = this.Parent.EntitySet;
                }
                else
                {
                    Debug.Assert(this.Parent == null, "Parent must be null for top level collection");
                    source = null;
                    sourceProperty = null;
                    sourceEntitySet = null;
                }

                targetEntitySet = this.EntitySet;
            }
        }

        internal sealed class Edge : IEquatable<Edge>
        {
            public Vertex Source
            {
                get;
                set;
            }

            public Vertex Target
            {
                get;
                set;
            }

            public string Label
            {
                get;
                set;
            }

            public bool Equals(Edge other)
            {
                return other != null &&
                    Object.ReferenceEquals(this.Source, other.Source) &&
                    Object.ReferenceEquals(this.Target, other.Target) &&
                    this.Label == other.Label;
            }
        }    
    }
}
