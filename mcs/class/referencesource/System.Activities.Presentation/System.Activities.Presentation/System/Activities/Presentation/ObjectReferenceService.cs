// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation
{
    using System.Activities.Debugger;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Validation;
    using System.Activities.Presentation.Xaml;
    using System.Collections.Generic;
    using System.Runtime;

    /// <summary>
    /// This interface is used by visual studio integration to acquire a AppDomain serialization friendly reference to an object.
    /// </summary>
    public sealed class ObjectReferenceService
    {
        private Dictionary<Guid, object> objectReferenceIds;
        private Dictionary<Guid, int> objectReferenceCount;
        private HashSet<Guid> subscribedForSourceLocationChanges;
        private EditingContext context;
        private ModelSearchServiceImpl modelSearchService;
        private ModelTreeManager modelTreeManager;

        /// <summary>
        /// This interface is used by visual studio integration to acquire a AppDomain serialization friendly reference to an object.
        /// </summary>
        /// <param name="context">The EditingContext of the current WorkflowDesigner.</param>
        public ObjectReferenceService(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }

            this.context = context;
            this.context.Services.Subscribe<ModelSearchService>(new SubscribeServiceCallback<ModelSearchService>(this.OnModelSearchServiceAvailable));
            this.context.Services.Subscribe<ModelTreeManager>(new SubscribeServiceCallback<ModelTreeManager>(this.OnModelTreeManagerAvailable));
        }

        /// <summary>
        /// Fire an event when the SourceLocation of an ObjectReference might be updated because of a Save operation.
        /// </summary>
        public event EventHandler<SourceLocationUpdatedEventArgs> SourceLocationUpdated;

        private Dictionary<Guid, object> ObjectReferenceIds
        {
            get
            {
                if (this.objectReferenceIds == null)
                {
                    this.objectReferenceIds = new Dictionary<Guid, object>();
                }

                return this.objectReferenceIds;
            }
        }

        private Dictionary<Guid, int> ObjectReferenceCount
        {
            get
            {
                if (this.objectReferenceCount == null)
                {
                    this.objectReferenceCount = new Dictionary<Guid, int>();
                }

                return this.objectReferenceCount;
            }
        }

        private HashSet<Guid> SubscribedForSourceLocationChanges
        {
            get
            {
                if (this.subscribedForSourceLocationChanges == null)
                {
                    this.subscribedForSourceLocationChanges = new HashSet<Guid>();
                }

                return this.subscribedForSourceLocationChanges;
            }
        }

        /// <summary>
        /// Acquire a reference by the SourceLocation of the object.
        /// Notice this method will automatically register the object to listen to SourceLocationUpdated, if available.
        /// </summary>
        /// <param name="startLine">The start line of the object.</param>
        /// <param name="startColumn">The start column of the object.</param>
        /// <param name="endLine">The end line of the object.</param>
        /// <param name="endColumn">The end column of the object.</param>
        /// <returns>The object reference.</returns>
        public Guid AcquireObjectReference(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (this.modelSearchService != null)
            {
                ModelItem modelItem = this.modelSearchService.FindModelItem(startLine, startColumn, endLine, endColumn);
                if (modelItem != null)
                {
                    object searchObject = modelItem.GetCurrentValue();
                    Guid result = this.AcquireObjectReference(searchObject);
                    this.SubscribedForSourceLocationChanges.Add(result);
                    return result;
                }
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Acquire a reference of an object by its actual reference.
        /// </summary>
        /// <param name="obj">The object which we need to acquire a reference for.</param>
        /// <returns>The object reference.</returns>
        public Guid AcquireObjectReference(object obj)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("obj"));
            }

            bool found = false;
            Guid objectReferenceId = Guid.NewGuid();
            foreach (KeyValuePair<Guid, object> kvp in this.ObjectReferenceIds)
            {
                if (object.ReferenceEquals(kvp.Value, obj))
                {
                    objectReferenceId = kvp.Key;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                this.ObjectReferenceIds.Add(objectReferenceId, obj);
            }

            this.IncreaseReferenceCount(objectReferenceId);
            return objectReferenceId;
        }

        /// <summary>
        /// Release the activity reference - this allow the designer infrastructure to release the actual reference to the activity - thus avoiding memory leak.
        /// </summary>
        /// <param name="objectReferenceId">The activity reference.</param>
        public void ReleaseObjectReference(Guid objectReferenceId)
        {
            if (this.DecreaseReferenceCount(objectReferenceId))
            {
                this.ObjectReferenceIds.Remove(objectReferenceId);
                if (this.SubscribedForSourceLocationChanges.Contains(objectReferenceId))
                {
                    this.SubscribedForSourceLocationChanges.Remove(objectReferenceId);
                }
            }
        }

        /// <summary>
        /// Obtain the actual reference to the object by its ObjectReference - this method should be called within the designer AppDomain only.
        /// </summary>
        /// <param name="objectReferenceId">The activity reference.</param>
        /// <param name="obj">The de-referenced activity, if the reference is available, or null otherwise.</param>
        /// <returns>True if the activity reference can be successfully de-referenced.</returns>
        public bool TryGetObject(Guid objectReferenceId, out object obj)
        {
            return this.ObjectReferenceIds.TryGetValue(objectReferenceId, out obj);
        }

        internal void OnSaveCompleted()
        {
            if (this.SourceLocationUpdated != null)
            {
                if (this.modelSearchService != null)
                {
                    foreach (Guid subscribedObjectReference in this.SubscribedForSourceLocationChanges)
                    {
                        object subscribedObject = this.ObjectReferenceIds[subscribedObjectReference];
                        SourceLocation updatedSourceLocation = this.modelSearchService.FindSourceLocation(this.modelTreeManager.GetModelItem(subscribedObject));
                        if (updatedSourceLocation != null)
                        {
                            this.SourceLocationUpdated(null, new SourceLocationUpdatedEventArgs(subscribedObjectReference, updatedSourceLocation));
                        }
                    }
                }
            }
        }

        private void OnModelSearchServiceAvailable(ModelSearchService modelSearchService)
        {
            if (modelSearchService != null)
            {
                this.modelSearchService = modelSearchService as ModelSearchServiceImpl;
            }
        }

        private void OnModelTreeManagerAvailable(ModelTreeManager modelTreeManager)
        {
            if (modelTreeManager != null)
            {
                this.modelTreeManager = modelTreeManager as ModelTreeManager;
            }
        }

        private void IncreaseReferenceCount(Guid objectReferenceId)
        {
            int referenceCount;
            if (!this.ObjectReferenceCount.TryGetValue(objectReferenceId, out referenceCount))
            {
                referenceCount = 0;
            }

            referenceCount++;
            this.ObjectReferenceCount[objectReferenceId] = referenceCount;
        }

        private bool DecreaseReferenceCount(Guid objectReferenceId)
        {
            Fx.Assert(this.ObjectReferenceCount.ContainsKey(objectReferenceId), "DecreaseReferenceCount should not be called when there is no reference.");
            if (this.ObjectReferenceCount.ContainsKey(objectReferenceId))
            {
                int referenceCount = this.ObjectReferenceCount[objectReferenceId] - 1;
                if (referenceCount == 0)
                {
                    this.ObjectReferenceCount.Remove(objectReferenceId);
                    return true;
                }
                else
                {
                    this.ObjectReferenceCount[objectReferenceId] = referenceCount;
                    return false;
                }
            }

            return true;
        }
    }
}
