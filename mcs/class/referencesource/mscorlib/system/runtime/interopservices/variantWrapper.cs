// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: VariantWrapper.
**
**
** Purpose: Wrapper that is converted to a variant with VT_BYREF | VT_VARIANT.
**
**
=============================================================================*/

namespace System.Runtime.InteropServices {
   
    using System;

    [Serializable]

    public sealed class VariantWrapper
    {
        public VariantWrapper(Object obj)
        {
            m_WrappedObject = obj;
        }

        public Object WrappedObject 
        {
            get 
            {
                return m_WrappedObject;
            }
        }

        private Object m_WrappedObject;
    }
}
