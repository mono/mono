// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: CaseInsensitiveHashCodeProvider
** 
** <OWNER>[....]</OWNER>
**
**
** Purpose: Designed to support hashtables which require 
** case-insensitive behavior while still maintaining case,
** this provides an efficient mechanism for getting the 
** hashcode of the string ignoring case.
**
**
============================================================*/
namespace System.Collections {
//This class does not contain members and does not need to be serializable
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Diagnostics.Contracts;

    [Serializable]  
    [Obsolete("Please use StringComparer instead.")]    
    [System.Runtime.InteropServices.ComVisible(true)]
    public class CaseInsensitiveHashCodeProvider : IHashCodeProvider {
        private TextInfo m_text;
        private static volatile CaseInsensitiveHashCodeProvider m_InvariantCaseInsensitiveHashCodeProvider = null;

        public CaseInsensitiveHashCodeProvider() {
            m_text = CultureInfo.CurrentCulture.TextInfo;
        }

        public CaseInsensitiveHashCodeProvider(CultureInfo culture) {
            if (culture==null) {
                throw new ArgumentNullException("culture");
            }
            Contract.EndContractBlock();
            m_text = culture.TextInfo;
        }

        public static CaseInsensitiveHashCodeProvider Default
        {
            get
            {
                Contract.Ensures(Contract.Result<CaseInsensitiveHashCodeProvider>() != null);

                return new CaseInsensitiveHashCodeProvider(CultureInfo.CurrentCulture);
            }
        }
        
        public static CaseInsensitiveHashCodeProvider DefaultInvariant
        {
            get
            {
                Contract.Ensures(Contract.Result<CaseInsensitiveHashCodeProvider>() != null);

                if (m_InvariantCaseInsensitiveHashCodeProvider == null) {
                    m_InvariantCaseInsensitiveHashCodeProvider = new CaseInsensitiveHashCodeProvider(CultureInfo.InvariantCulture);
                }
                return m_InvariantCaseInsensitiveHashCodeProvider;
            }
        }

        public int GetHashCode(Object obj) {
            if (obj==null) {
                throw new ArgumentNullException("obj");
            }
            Contract.EndContractBlock();

            String s = obj as String;
            if (s==null) {
                return obj.GetHashCode();
            }

            return m_text.GetCaseInsensitiveHashCode(s);
        }
    }
}
