//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Dispatcher;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldHaveCorrectSuffix,
        Justification = "Arch approved name")]
    [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.MarkISerializableTypesWithSerializable,
        Justification = "TODO 87908, We can consider not deriving from Dictionary")]
    public class MessageQuerySet : Dictionary<string, MessageQuery>
    {
        public MessageQuerySet()
        {
        }

        public MessageQuerySet(MessageQueryTable<string> queryTable)
        {
            if (queryTable == null)
            {
                throw FxTrace.Exception.ArgumentNull("queryTable");
            }

            InvertDictionary<MessageQuery, string>(queryTable, this);
        }

        [DefaultValue(null)]
        public string Name
        {
            get;
            set;
        }

        public MessageQueryTable<string> GetMessageQueryTable()
        {
            MessageQueryTable<string> result = new MessageQueryTable<string>();
            InvertDictionary<string, MessageQuery>(this, result);
            return result;
        }


        static void InvertDictionary<TKey, TValue>(IDictionary<TKey, TValue> source, IDictionary<TValue, TKey> destination)
        {
            foreach (KeyValuePair<TKey, TValue> vkpair in source)
            {
                destination.Add(vkpair.Value, vkpair.Key);
            }
        }
    }
}
