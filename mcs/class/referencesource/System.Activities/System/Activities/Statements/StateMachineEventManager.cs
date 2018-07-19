//------------------------------------------------------------------------------
// <copyright file="StateMachineEventManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    
    /// <summary>
    /// StateMachineEventManager is used to manage triggered events globally.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "This type is actually used in LINQ expression and FxCop didn't detect that.")]
    [DataContract]
    class StateMachineEventManager
    {
        // To avoid out of memory, set a fixed length of event queue.
        const int MaxQueueLength = 1 << 25;

        // queue is used to store triggered events
        Queue<TriggerCompletedEvent> queue;
        
        // If a state is running, its condition evaluation bookmark will be added in to activityBookmarks.
        // If a state is completed, its bookmark will be removed.
        Collection<Bookmark> activeBookmarks;     

        /// <summary>
        /// Constructor to do initialization.
        /// </summary>
        public StateMachineEventManager()
        {
            this.queue = new Queue<TriggerCompletedEvent>();
            this.activeBookmarks = new Collection<Bookmark>();
        }

        /// <summary>
        /// Gets or sets the trigger index of current being processed event.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public TriggerCompletedEvent CurrentBeingProcessedEvent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the CurrentConditionIndex denotes the index of condition is being evaluated.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int CurrentConditionIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether StateMachine is on the way of transition.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool OnTransition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the EventManager queue.
        /// </summary>
        public IEnumerable<TriggerCompletedEvent> Queue
        {
            get
            {
                return this.queue;
            }
        }

        /// <summary>
        /// Gets a value indicating whether StateMachineManger is ready to process an event immediately.
        /// </summary>
        bool CanProcessEventImmediately
        {
            get
            {
                return this.CurrentBeingProcessedEvent == null && !this.OnTransition && this.queue.Count == 0;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "queue")]
        internal Queue<TriggerCompletedEvent> SerializedQueue
        {
            get { return this.queue; }
            set { this.queue = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "activeBookmarks")]
        internal Collection<Bookmark> SerializedActiveBookmarks
        {
            get { return this.activeBookmarks; }
            set { this.activeBookmarks = value; }
        }

        /// <summary>
        /// When StateMachine enters a state, condition evaluation bookmark of that state would be added to activeBookmarks collection.
        /// </summary>
        /// <param name="bookmark">Bookmark reference.</param>
        public void AddActiveBookmark(Bookmark bookmark)
        {
            this.activeBookmarks.Add(bookmark);
        }

        /// <summary>
        /// Gets next completed events queue.
        /// </summary>
        /// <returns>Top TriggerCompletedEvent item in the queue.</returns>
        public TriggerCompletedEvent GetNextCompletedEvent()
        {
            while (this.queue.Any())
            {
                TriggerCompletedEvent completedEvent = this.queue.Dequeue();
                if (this.activeBookmarks.Contains(completedEvent.Bookmark))
                {
                    this.CurrentBeingProcessedEvent = completedEvent;
                    return completedEvent;
                }
            }

            return null;
        }

        /// <summary>
        /// This method is used to denote whether a given bookmark is referred by currently processed event.
        /// </summary>
        /// <param name="bookmark">Bookmark reference.</param>
        /// <returns>True is the bookmark references to the event being processed.</returns>
        public bool IsReferredByBeingProcessedEvent(Bookmark bookmark)
        {
            return this.CurrentBeingProcessedEvent != null && this.CurrentBeingProcessedEvent.Bookmark == bookmark;
        }

        /// <summary>
        /// Register a completed event and returns whether the event could be processed immediately.
        /// </summary>
        /// <param name="completedEvent">TriggerCompletedEvent reference.</param>
        /// <param name="canBeProcessedImmediately">True if the Condition can be evaluated.</param>
        public void RegisterCompletedEvent(TriggerCompletedEvent completedEvent, out bool canBeProcessedImmediately)
        {
            canBeProcessedImmediately = this.CanProcessEventImmediately;
            this.queue.Enqueue(completedEvent);
            return;
        }

        /// <summary>
        /// When StateMachine leaves a state, condition evaluation bookmark of that state would be removed from activeBookmarks collection.
        /// </summary>
        /// <param name="bookmark">Bookmark reference.</param>
        public void RemoveActiveBookmark(Bookmark bookmark)
        {
            this.activeBookmarks.Remove(bookmark);
        }
    }
}
