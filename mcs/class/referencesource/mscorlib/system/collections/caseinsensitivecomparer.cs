// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: CaseInsensitiveComparer
** 
** <OWNER>Microsoft</OWNER>
**
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
[System.Runtime.InteropServices.ComVisible(true)]
    public class CaseInsensitiveComparer : IComparer {
        private CompareInfo m_compareInfo;
        private static volatile CaseInsensitiveComparer m_InvariantCaseInsensitiveComparer;
        
        public CaseInsensitiveComparer() {
            m_compareInfo = CultureInfo.CurrentCulture.CompareInfo;
        }

        public CaseInsensitiveComparer(CultureInfo culture) {
            if (culture==null) {
                throw new ArgumentNullException("culture");
            }
            Contract.EndContractBlock();
            m_compareInfo = culture.CompareInfo;
        }

        public static CaseInsensitiveComparer Default
        { 
            get
            {
                Contract.Ensures(Contract.Result<CaseInsensitiveComparer>() != null);

                return new CaseInsensitiveComparer(CultureInfo.CurrentCulture);
            }
        }
        
        public static CaseInsensitiveComparer DefaultInvariant
        { 
            get
            {
                Contract.Ensures(Contract.Result<CaseInsensitiveComparer>() != null);

                if (m_InvariantCaseInsensitiveComparer == null) {
                    m_InvariantCaseInsensitiveComparer = new CaseInsensitiveComparer(CultureInfo.InvariantCulture);
                }
                return m_InvariantCaseInsensitiveComparer;
            }
        }
    
        // Behaves exactly like Comparer.Default.Compare except that the comparison is case insensitive
        // Compares two Objects by calling CompareTo.  If a == 
        // b,0 is returned.  If a implements 
        // IComparable, a.CompareTo(b) is returned.  If a 
        // doesn't implement IComparable and b does, 
        // -(b.CompareTo(a)) is returned, otherwise an 
        // exception is thrown.
        // 
        public int Compare(Object a, Object b) {
            String sa = a as String;
            String sb = b as String;
            if (sa != null && sb != null)
                return m_compareInfo.Compare(sa, sb, CompareOptions.IgnoreCase);
            else
                return Comparer.Default.Compare(a,b);
        }
    }
}
