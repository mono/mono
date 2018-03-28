//------------------------------------------------------------------------------
// <copyright file="Coordinator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Data.Objects.Internal;

namespace System.Data.Common.Internal.Materialization
{

    /// <summary>
    /// A coordinator is responsible for tracking state and processing result in a root or nested query
    /// result collection. The coordinator exists within a graph, and knows its Parent, (First)Child,
    /// and Next sibling. This allows the Shaper to use the coordinator as a simple state machine when
    /// consuming store reader results.
    /// </summary>
    internal abstract class Coordinator
    {
        #region state

        /// <summary>
        /// The factory used to generate this coordinator instance. Contains delegates used
        /// by the Shaper during result enumeration.
        /// </summary>
        internal readonly CoordinatorFactory CoordinatorFactory;

        /// <summary>
        /// Parent coordinator (the coordinator producing rows containing this collection).
        /// If this is the root, null.
        /// </summary>
        internal readonly Coordinator Parent;

        /// <summary>
        /// First coordinator for nested results below this collection. When reading a new row
        /// for this coordinator, we walk down to the Child.
        /// 
        /// NOTE:: this cannot be readonly because we can't know both the parent and the child
        /// at initialization time; we set the Child in the parent's constructor.
        /// </summary>
        public Coordinator Child
        {
            get { return _child; }
            protected set { _child = value; }
        }
        private Coordinator _child;

        /// <summary>
        /// Next coordinator at this depth. Once we're done consuming results for this reader,
        /// we move on to this.Next.
        /// </summary>
        internal readonly Coordinator Next;

        /// <summary>
        /// Indicates whether data has been read for the collection being aggregated or yielded
        /// by this coordinator.
        /// </summary> 
        public bool IsEntered
        {
            get { return _isEntered; }
            protected set { _isEntered = value; }
        }
        private bool _isEntered;

        /// <summary>
        /// Indicates whether this is the top level coordinator for a query.
        /// </summary>
        internal bool IsRoot
        {
            get { return null == Parent; }
        }

        #endregion

        #region constructor

        protected Coordinator(CoordinatorFactory coordinatorFactory, Coordinator parent, Coordinator next)
        {
            this.CoordinatorFactory = coordinatorFactory;
            this.Parent = parent;
            this.Next = next;
        }

        #endregion

        #region "public" surface area

        /// <summary>
        /// Registers this hierarchy of coordinators in the given shaper.
        /// </summary>
        internal void Initialize(Shaper shaper)
        {
            ResetCollection(shaper);

            // Add this coordinator to the appropriate state slot in the 
            // shaper so that it is available to materialization delegates.
            shaper.State[this.CoordinatorFactory.StateSlot] = this;
            
            if (null != this.Child)
            {
                this.Child.Initialize(shaper);
            }
            if (null != this.Next)
            {
                this.Next.Initialize(shaper);
            }
        }

        /// <summary>
        /// Determines the maximum depth of this subtree.
        /// </summary>
        internal int MaxDistanceToLeaf()
        {
            int maxDistance = 0;
            Coordinator child = this.Child;
            while (null != child)
            {
                maxDistance = Math.Max(maxDistance, child.MaxDistanceToLeaf() + 1);
                child = child.Next;
            }
            return maxDistance;
        }

        /// <summary>
        /// This method is called when the current collection is finished and it's time to move to the next collection.
        /// Recursively initializes children and siblings as well.
        /// </summary>
        internal abstract void ResetCollection(Shaper shaper);

        /// <summary>
        /// Precondition: the current row has data for the coordinator.
        /// Side-effects: updates keys currently stored in state and updates IsEntered if a new value is encountered.
        /// Determines whether the row contains the next element in this collection.
        /// </summary>
        internal bool HasNextElement(Shaper shaper)
        {
            // check if this row contains a new element for this coordinator
            bool result = false;
            
            if (!this.IsEntered || !this.CoordinatorFactory.CheckKeys(shaper))
            {
                // remember initial keys values
                this.CoordinatorFactory.SetKeys(shaper);
                this.IsEntered = true;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Precondition: the current row has data and contains a new element for the coordinator.
        /// Reads the next element in this collection.
        /// </summary>
        internal abstract void ReadNextElement(Shaper shaper);

        #endregion
    }

    /// <summary>
    /// Typed <see cref="Coordinator"/>
    /// </summary>
    internal class Coordinator<T> : Coordinator
    {
        #region state

        internal readonly CoordinatorFactory<T> TypedCoordinatorFactory;

        /// <summary>
        /// Exposes the Current element that has been materialized (and is being populated) by this coordinator.
        /// </summary>
        internal T Current
        {
            get { return _current; }
        }
        private T _current;

        /// <summary>
        /// For ObjectResult, aggregates all elements for in the nested collection handled by this coordinator.
        /// </summary>
        private ICollection<T> _elements;

        /// <summary>
        /// For ObjectResult, aggregates all elements as wrapped entities for in the nested collection handled by this coordinator.
        /// </summary>
        private List<IEntityWrapper> _wrappedElements;

        /// <summary>
        /// Delegate called when the current nested collection has been consumed. This is necessary in Span
        /// scenarios where an EntityCollection RelatedEnd is populated only when all related entities have
        /// been materialized.  This version of the close handler works with wrapped entities.
        /// </summary>
        private Action<Shaper, List<IEntityWrapper>> _handleClose;

        /// <summary>
        /// For nested, object-layer coordinators we want to collect all the elements we find and handle them
        /// when the root coordinator advances.  Otherwise we just want to return them as we find them.
        /// </summary>
        private readonly bool IsUsingElementCollection;

        #endregion

        #region constructors

        internal Coordinator(CoordinatorFactory<T> coordinator, Coordinator parent, Coordinator next)
            : base(coordinator, parent, next)
        {
            this.TypedCoordinatorFactory = coordinator;

            // generate all children
            Coordinator nextChild = null;
            foreach (var nestedCoordinator in coordinator.NestedCoordinators.Reverse())
            {
                // last child processed is first child...
                this.Child = nestedCoordinator.CreateCoordinator(this, nextChild);
                nextChild = this.Child;
            }

            this.IsUsingElementCollection = (!this.IsRoot && typeof(T) != typeof(RecordState));
        }

        #endregion

        #region "public" surface area

        internal override void ResetCollection(Shaper shaper)
        {
            // Check to see if anyone has registered for notification when the current coordinator
            // is reset.
            if (null != _handleClose)
            {
                _handleClose(shaper, _wrappedElements);
                _handleClose = null;
            }

            // Reset is entered for this collection.
            this.IsEntered = false;

            if (IsUsingElementCollection)
            {
                _elements = this.TypedCoordinatorFactory.InitializeCollection(shaper);
                _wrappedElements = new List<IEntityWrapper>();
            }

            if (null != this.Child)
            {
                this.Child.ResetCollection(shaper);
            }
            if (null != this.Next)
            {
                this.Next.ResetCollection(shaper);
            }
        }

        internal override void ReadNextElement(Shaper shaper)
        {
            T element;
            IEntityWrapper wrappedElement = null;
            try
            {
                if (this.TypedCoordinatorFactory.WrappedElement == null)
                {
                    element = this.TypedCoordinatorFactory.Element(shaper);
                }
                else
                {
                    wrappedElement = this.TypedCoordinatorFactory.WrappedElement(shaper);
                    // This cast may throw, in which case it will be immediately caught
                    // and the error handling expression will be used to get the appropriate error message.
                    element = (T)wrappedElement.Entity;
                }
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    // Some errors can occur while a close handler is registered.  This clears
                    // out the handler so that ElementWithErrorHandling will report the correct
                    // error rather than asserting on the missing close handler.
                    ResetCollection(shaper);
                    // call a variation of the "Element" delegate with more detailed
                    // error handling (to produce a better exception message)
                    element = this.TypedCoordinatorFactory.ElementWithErrorHandling(shaper);
                }

                // rethrow
                throw;
            }
            if (IsUsingElementCollection)
            {
                _elements.Add(element);
                if (wrappedElement != null)
                {
                    _wrappedElements.Add(wrappedElement);
                }
            }
            else
            {
                _current = element;
            }
        }

        /// <summary>
        /// Sets the delegate called when this collection is closed.  This close handler works on
        /// a collection of wrapped entities, rather than on the raw entity objects.
        /// </summary>
        internal void RegisterCloseHandler(Action<Shaper, List<IEntityWrapper>> closeHandler)
        {
            Debug.Assert(null == _handleClose, "more than one handler for a collection close 'event'");
            _handleClose = closeHandler;
        }

        /// <summary>
        /// Called when we're disposing the enumerator;         
        /// </summary>
        internal void SetCurrentToDefault()
        {
            _current = default(T);
        }

        #endregion

        #region runtime callable code
        
        // Code in this section is called from the delegates produced by the Translator.  It may  
        // not show up if you search using Find All References

        /// <summary>
        /// Returns a handle to the element aggregator for this nested collection.
        /// </summary>
        private IEnumerable<T> GetElements()
        {
            return _elements;
        }

        #endregion
    }
}
