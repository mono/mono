//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Activities.Presentation.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime;
    using System.Text;

    [Fx.Tag.XamlVisible(false)]
    public class EditingScope : ModelEditingScope
    {
        ModelTreeManager modelTreeManager;
        EditingScope outerScope;
        List<Change> changes;
        List<Change> appliedChanges;
        bool suppressUndo;
        private HashSet<ModelItem> itemsAdded;
        private HashSet<ModelItem> itemsRemoved;

        // this is intended for ImmediateEditingScope only
        private bool? hasEffectiveChanges;

        internal EditingScope(ModelTreeManager modelTreeManager, EditingScope outerScope)
        {
            this.modelTreeManager = modelTreeManager;
            this.changes = new List<Change>();
            this.outerScope = outerScope;
            this.HasModelChanges = false;
            this.itemsAdded = new HashSet<ModelItem>();
            this.itemsRemoved = new HashSet<ModelItem>();
        }

        private EditingScope()
        {
        }

        public bool HasEffectiveChanges
        {
            get
            {
                if (this.hasEffectiveChanges.HasValue)
                {
                    Fx.Assert(this.GetType() == typeof(ImmediateEditingScope), "we should only set this property on an ImmediateEditingScope");

                    return this.hasEffectiveChanges.Value;
                }

                return this.appliedChanges != null && this.appliedChanges.Count > 0;
            }

            internal set
            {
                Fx.Assert(this.GetType() == typeof(ImmediateEditingScope), "we should only set this property on an ImmediateEditingScope");

                this.hasEffectiveChanges = value;
            }
        }

        internal bool HasModelChanges
        {
            get;

            // setter is only expected to be called by EditingScope and ImmediateEditingScope
            set;
        }

        internal bool SuppressUndo
        {
            get
            {
                return this.suppressUndo;
            }
            set
            {
                Fx.Assert(!value || this.outerScope == null, "If we are suppressing undo, then we are not nested within another editingScope, otherwise suppress undo won't work.");
                this.suppressUndo = value;
            }
        }

        internal ReadOnlyCollection<ModelItem> ItemsAdded
        {
            get
            {
                return new ReadOnlyCollection<ModelItem>(this.itemsAdded.ToList());
            }
        }

        internal ReadOnlyCollection<ModelItem> ItemsRemoved
        {
            get
            {
                return new ReadOnlyCollection<ModelItem>(this.itemsRemoved.ToList());
            }
        }

        /// <summary>
        /// Gets or sets whether validation should be suppressed when this EditingScope completes.
        /// </summary>
        internal bool SuppressValidationOnComplete
        {
            get;
            set;
        }

        public List<Change> Changes
        {
            get
            {
                return changes;
            }
        }

        internal void EditingScope_ModelItemsAdded(object sender, ModelItemsAddedEventArgs e)
        {
            this.HandleModelItemsAdded(e.ModelItemsAdded);
        }

        internal void EditingScope_ModelItemsRemoved(object sender, ModelItemsRemovedEventArgs e)
        {
            this.HandleModelItemsRemoved(e.ModelItemsRemoved);
        }

        internal void HandleModelItemsAdded(IEnumerable<ModelItem> modelItems)
        {
            foreach (ModelItem addedItem in modelItems)
            {
                if (this.itemsRemoved.Contains(addedItem))
                {
                    this.itemsRemoved.Remove(addedItem);
                }
                else
                {
                    Fx.Assert(!this.itemsAdded.Contains(addedItem), "One ModelItem should not be added more than once.");
                    this.itemsAdded.Add(addedItem);
                }
            }
        }

        internal void HandleModelItemsRemoved(IEnumerable<ModelItem> modelItems)
        {
            foreach (ModelItem removedItem in modelItems)
            {
                if (this.itemsAdded.Contains(removedItem))
                {
                    this.itemsAdded.Remove(removedItem);
                }
                else
                {
                    Fx.Assert(!itemsRemoved.Contains(removedItem), "One ModelItem should not be removed more than once.");
                    this.itemsRemoved.Add(removedItem);
                }
            }
        }

        protected override void OnComplete()
        {
            Fx.Assert(this.itemsAdded.Count == 0 && this.itemsRemoved.Count == 0, "There should not be items changed before completed.");
            this.modelTreeManager.RegisterModelTreeChangeEvents(this);

            bool modelChangeBegin = false;
            try
            {
                if (this.outerScope == null)
                {
                    appliedChanges = new List<Change>();
                    int startIndex = 0;
                    // pump all changes, applying changes can add more changes to the end of the change list.
                    while (startIndex < this.Changes.Count)
                    {
                        // pickup the new changes
                        List<Change> changesToApply = this.Changes.GetRange(startIndex, this.Changes.Count - startIndex);
                        startIndex = this.Changes.Count;

                        foreach (Change change in changesToApply)
                        {
                            if (change != null)
                            {
                                if (change is ModelChange && !modelChangeBegin)
                                {
                                    this.BeginModelChange();
                                    modelChangeBegin = true;
                                }

                                if (change.Apply())
                                {
                                    appliedChanges.Add(change);
                                }
                            }

                            if (change is ModelChange)
                            {
                                this.HasModelChanges = true;
                            }
                        }
                    }
                }
                else
                {
                    outerScope.Changes.AddRange(this.Changes);
                }
            }
            finally
            {
                if (modelChangeBegin)
                {
                    this.EndModelChange();
                }

                this.modelTreeManager.UnregisterModelTreeChangeEvents(this);
            }

            this.modelTreeManager.OnEditingScopeCompleted(this);
        }

        private void BeginModelChange()
        {
            ValidationService validationService = this.modelTreeManager.Context.Services.GetService<ValidationService>();
            if (validationService != null)
            {
                validationService.DeactivateValidation();
            }
        }

        private void EndModelChange()
        {
            ValidationService validationService = this.modelTreeManager.Context.Services.GetService<ValidationService>();
            if (validationService != null)
            {
                validationService.ActivateValidation();
            }
        }

        protected override bool CanComplete()
        {
            return this.modelTreeManager.CanEditingScopeComplete(this);
        }

        protected override void OnRevert(bool finalizing)
        {
            bool modelChangeBegin = false;
            try
            {
                if (this.appliedChanges != null)
                {
                    List<Change> revertChanges = new List<Change>();
                    foreach (Change change in this.appliedChanges)
                    {
                        revertChanges.Add(change.GetInverse());
                    }
                    revertChanges.Reverse();
                    foreach (Change change in revertChanges)
                    {
                        if (change is ModelChange && !modelChangeBegin)
                        {
                            this.BeginModelChange();
                            modelChangeBegin = true;
                        }

                        change.Apply();
                        this.appliedChanges.RemoveAt(this.appliedChanges.Count - 1);
                    }
                }
            }
            finally
            {
                if (modelChangeBegin)
                {
                    this.EndModelChange();
                }
            }

            this.modelTreeManager.UnregisterModelTreeChangeEvents(this);
            this.modelTreeManager.OnEditingScopeReverted(this);
        }

        protected override bool OnException(Exception e)
        {
            return false;
        }
    }
}
