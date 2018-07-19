//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using SR2 = System.ServiceModel.Activities.SR;

    public class CorrelationQuery
    {
        Collection<MessageQuerySet> selectAdditional;

        public CorrelationQuery()
        {
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.CollectionPropertiesShouldBeReadOnly,
            Justification = "TODO 87762, remove the set")]
        [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.PropertyExternalTypesMustBeKnown,
            Justification = "This property is XAML friendly, no need to add KnownXamlExternal")]
        [DefaultValue(null)]
        public MessageQuerySet Select
        {
            get;
            set;
        }

        public Collection<MessageQuerySet> SelectAdditional
        {
            get
            {
                if (this.selectAdditional == null)
                {
                    this.selectAdditional = new QueryCollection();
                }
                return this.selectAdditional;
            }
        }

        [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.PropertyExternalTypesMustBeKnown,
            Justification = "This property is XAML friendly, no need to add KnownXamlExternal")]
        [DefaultValue(null)]
        public MessageFilter Where
        {
            get;
            set;
        }

        internal bool IsDefaultContextQuery
        {
            get;
            set;
        }

        public override bool Equals(object other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            CorrelationQuery otherQuery = other as CorrelationQuery;
            if (otherQuery == null)
            {
                return false;
            }

            if (this.Where == null)
            {
                return otherQuery.Where == null;
            }

            return this.Where.Equals(otherQuery.Where);
        }

        public override int GetHashCode()
        {
            return (this.Where != null) ? this.Where.GetHashCode() : 0;
        }

        internal static bool IsQueryCollectionSearchable(IEnumerable<CorrelationQuery> queries)
        {
            foreach (CorrelationQuery query in queries)
            {
                if (!(query.Where is CorrelationActionMessageFilter || query.Where is ActionMessageFilter))
                {
                    return false;
                }
            }

            return true;
        }

        internal static CorrelationQuery FindCorrelationQueryForAction(IEnumerable<CorrelationQuery> queries, string action)
        {
            string localAction = action != null ? action : String.Empty;
            foreach (CorrelationQuery query in queries)
            {
                // if the action is wildcard, we have a match all
                if (query.Where is CorrelationActionMessageFilter)
                {
                    if (((CorrelationActionMessageFilter)query.Where).Action == localAction || localAction == MessageHeaders.WildcardAction)
                    {
                        return query;
                    }
                }
                else if (query.Where is ActionMessageFilter)
                {
                    if (((ActionMessageFilter)query.Where).Actions.Contains(localAction) || localAction == MessageHeaders.WildcardAction)
                    {
                        return query;
                    }
                }
            }

            return null;
        }

        internal CorrelationQuery Clone()
        {
            CorrelationQuery cloneQuery = new CorrelationQuery 
            {   
                Select = this.Select,
                IsDefaultContextQuery = this.IsDefaultContextQuery,
                Where = this.Where,
            };
            if (this.selectAdditional != null)
            {
                foreach (MessageQuerySet messageQuerySet in this.selectAdditional)
                {
                    cloneQuery.SelectAdditional.Add(messageQuerySet);
                }
            }
            return cloneQuery;
        }

        class QueryCollection : Collection<MessageQuerySet>
        {
            public QueryCollection()
            {
            }

            protected override void InsertItem(int index, MessageQuerySet item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }

                base.InsertItem(index, item);
            }

            protected override void SetItem(int index, MessageQuerySet item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }

                base.SetItem(index, item);
            }
        }
    }
}
