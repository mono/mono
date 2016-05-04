//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{
    using System;

    using Microsoft.InfoCards.Diagnostics;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;

    //
    // Summary:
    //  This class throws an Argument exception if an attempt is made to assign a non-null
    //  value to the Value property more than once.
    //
    class ThrowOnMultipleAssignment<T>
    {

        string m_errorString;
        T m_value;

        public T Value
        {
            get { return m_value; }
            set
            {
                if (null != m_value && null != value)
                {
                    throw IDT.ThrowHelperArgument(m_errorString);
                }
                else if (null == m_value)
                {
                    m_value = value;
                }
            }
        }

        //
        // Parameters:
        //  errorString  - If Value gets assigned to more than once an argument exception will be thrown with this
        //                 string as the Exception string.
        //
        public ThrowOnMultipleAssignment(string errorString)
        {

            IDT.DebugAssert(!String.IsNullOrEmpty(errorString), "Must have an error string");

            m_errorString = errorString;
        }
    }
}

