using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Workflow.ComponentModel;

namespace System.Workflow.Activities.Rules
{
    #region class RuleSetCollection

    public sealed class RuleSetCollection : KeyedCollection<string, RuleSet>, IWorkflowChangeDiff
    {
        #region members and constructors

        private bool _runtimeInitialized;
        [NonSerialized]
        private object syncLock = new object();

        public RuleSetCollection()
        {
        }

        #endregion

        #region keyed collection members

        protected override string GetKeyForItem(RuleSet item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, RuleSet item)
        {
            if (this._runtimeInitialized)
                throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

            if (item.Name != null && item.Name.Length >= 0 && this.Contains(item.Name))
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.RuleSetExists, item.Name);
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

        protected override void SetItem(int index, RuleSet item)
        {
            if (this._runtimeInitialized)
                throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

            base.SetItem(index, item);
        }

        new public void Add(RuleSet item)
        {
            if (this._runtimeInitialized)
                throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

            if (null == item)
            {
                throw new ArgumentNullException("item");
            }

            if (null == item.Name)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.InvalidRuleSetName, "item.Name");
                throw new ArgumentException(message);
            }

            base.Add(item);
        }

        #endregion

        #region runtime initializing

        internal void OnRuntimeInitialized()
        {
            lock (this.syncLock)
            {
                if (this._runtimeInitialized)
                    return;

                foreach (RuleSet ruleSet in this)
                {
                    ruleSet.OnRuntimeInitialized();
                }
                _runtimeInitialized = true;
            }
        }

        internal bool RuntimeMode
        {
            set { this._runtimeInitialized = value; }
            get { return this._runtimeInitialized; }
        }

        internal string GenerateRuleSetName()
        {
            string nameBase = Messages.NewRuleSetName;
            string newName;
            int i = 1;
            do
            {
                newName = nameBase + i.ToString(CultureInfo.InvariantCulture);
                i++;
            } while (this.Contains(newName));

            return newName;
        }

        #endregion

        #region IWorkflowChangeDiff Members

        public IList<WorkflowChangeAction> Diff(object originalDefinition, object changedDefinition)
        {
            List<WorkflowChangeAction> listChanges = new List<WorkflowChangeAction>();

            RuleSetCollection originalRuleSets = (RuleSetCollection)originalDefinition;
            RuleSetCollection changedRuleSets = (RuleSetCollection)changedDefinition;

            if (null != changedRuleSets)
            {
                foreach (RuleSet changedRuleSet in changedRuleSets)
                {
                    if ((originalRuleSets != null) && (originalRuleSets.Contains(changedRuleSet.Name)))
                    {
                        RuleSet originalRuleSet = originalRuleSets[changedRuleSet.Name];
                        if (!originalRuleSet.Equals(changedRuleSet))
                        {
                            listChanges.Add(new UpdatedRuleSetAction(originalRuleSet, changedRuleSet));
                        }
                    }
                    else
                    {
                        listChanges.Add(new AddedRuleSetAction(changedRuleSet));
                    }
                }
            }
            if (null != originalRuleSets)
            {
                foreach (RuleSet originalRuleSet in originalRuleSets)
                {
                    if ((changedRuleSets == null) || (!changedRuleSets.Contains(originalRuleSet.Name)))
                    {
                        listChanges.Add(new RemovedRuleSetAction(originalRuleSet));
                    }
                }
            }
            return listChanges;
        }
        #endregion
    }
    #endregion
}
