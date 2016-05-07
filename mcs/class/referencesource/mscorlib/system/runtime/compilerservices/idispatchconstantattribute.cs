// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices
{
[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited=false)]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class IDispatchConstantAttribute : CustomConstantAttribute
    {
        public IDispatchConstantAttribute()
        {
        }

        public override Object Value
        {
            get 
            {
                return new DispatchWrapper(null);
            }
        }
    }
}
