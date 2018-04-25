//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Collections.ObjectModel;
    using System.Reflection;

    static class IndexerHelper
    {
        public static void OnGetArguments<TItem>(Collection<InArgument> indices, OutArgument<Location<TItem>> result, CodeActivityMetadata metadata)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                RuntimeArgument indexArgument = new RuntimeArgument("Index" + i, indices[i].ArgumentType, ArgumentDirection.In, true);
                metadata.Bind(indices[i], indexArgument);
                metadata.AddArgument(indexArgument);
            }

            RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(Location<TItem>), ArgumentDirection.Out);
            metadata.Bind(result, resultArgument);
            metadata.AddArgument(resultArgument);
        }
        public static void CacheMethod<TOperand, TItem>(Collection<InArgument> indices, ref MethodInfo getMethod, ref MethodInfo setMethod)
        {
            Type[] getTypes = new Type[indices.Count];
            for (int i = 0; i < indices.Count; i++)
            {
                getTypes[i] = indices[i].ArgumentType;
            }

            getMethod = typeof(TOperand).GetMethod("get_Item", getTypes);
            if (getMethod != null && !getMethod.IsSpecialName)
            {
                getMethod = null;
            }

            Type[] setTypes = new Type[indices.Count + 1];
            for (int i = 0; i < indices.Count; i++)
            {
                setTypes[i] = indices[i].ArgumentType;
            }
            setTypes[setTypes.Length - 1] = typeof(TItem);
            setMethod = typeof(TOperand).GetMethod("set_Item", setTypes);
            if (setMethod != null && !setMethod.IsSpecialName)
            {
                setMethod = null;
            }
        }

    }

}
