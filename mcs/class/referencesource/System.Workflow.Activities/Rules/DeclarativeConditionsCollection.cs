// ---------------------------------------------------------------------------
// Copyright (C) 2005 Microsoft Corporation - All Rights Reserved
// ---------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using System.Globalization;
using System.IO;
using System.Workflow.ComponentModel.Serialization;

namespace System.Workflow.Activities.Rules
{
    #region RuleConditionCollection Class
    [Serializable]
    public sealed class RuleConditionCollection : KeyedCollection<string, RuleCondition>, IWorkflowChangeDiff
    {
        private bool _runtimeInitialized;
        [NonSerialized]
        private object _runtimeInitializationLock = new object();

        public RuleConditionCollection()
        {
        }

        protected override string GetKeyForItem(RuleCondition item)
        {
            return item.Name;
        }

        /// <summary>
        /// Mark the DeclarativeConditionDefinitionCollection as Runtime Initialized to prevent direct runtime updates.
        /// </summary>
        internal void OnRuntimeInitialized()
        {
            lock (_runtimeInitializationLock)
            {
                if (_runtimeInitialized)
                    return;

                foreach (RuleCondition condition in this)
                {
                    condition.OnRuntimeInitialized();
                }
                _runtimeInitialized = true;
            }
        }

        protected override void InsertItem(int index, RuleCondition item)
        {
            if (this._runtimeInitialized)
                throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

            if (item.Name != null && item.Name.Length >= 0 && this.Contains(item.Name))
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.ConditionExists, item.Name);
                throw new ArgumentException(message);
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (this._runtimeInitialized)
                throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, RuleCondition item)
        {
            if (this._runtimeInitialized)
                throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

            base.SetItem(index, item);
        }

        internal bool RuntimeMode
        {
            set { this._runtimeInitialized = value; }
            get { return this._runtimeInitialized; }
        }

        new public void Add(RuleCondition item)
        {
            if (this._runtimeInitialized)
                throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

            if (null == item)
            {
                throw new ArgumentNullException("item");
            }

            if (null == item.Name)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.InvalidConditionName, "item.Name");
                throw new ArgumentException(message);
            }

            base.Add(item);
        }

        public IList<WorkflowChangeAction> Diff(object originalDefinition, object changedDefinition)
        {
            List<WorkflowChangeAction> listChanges = new List<WorkflowChangeAction>();

            RuleConditionCollection originalConditions = (RuleConditionCollection)originalDefinition;
            RuleConditionCollection changedConditions = (RuleConditionCollection)changedDefinition;

            if (null != changedConditions)
            {
                foreach (RuleCondition cCondition in changedConditions)
                {
                    if (null != originalConditions)
                    {
                        if (originalConditions.Contains(cCondition.Name))
                        {
                            RuleCondition oCondition = originalConditions[cCondition.Name];
                            if (!oCondition.Equals(cCondition))
                            {
                                listChanges.Add(new UpdatedConditionAction(oCondition, cCondition));
                            }
                        }
                        else
                        {
                            listChanges.Add(new AddedConditionAction(cCondition));
                        }
                    }
                    else
                    {
                        listChanges.Add(new AddedConditionAction(cCondition));
                    }
                }
            }

            if (null != originalConditions)
            {
                foreach (RuleCondition oCondition in originalConditions)
                {
                    if (null != changedConditions)
                    {
                        if (!changedConditions.Contains(oCondition.Name))
                        {
                            listChanges.Add(new RemovedConditionAction(oCondition));
                        }
                    }
                    else
                    {
                        listChanges.Add(new RemovedConditionAction(oCondition));
                    }
                }
            }
            return listChanges;
        }
    }
    #endregion
}

