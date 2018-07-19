// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    Header.cs
**
**
** Purpose: Defines the out-of-band data for a call
**
**
**
===========================================================*/
namespace System.Runtime.Remoting.Messaging{
    using System.Runtime.Remoting;
    using System;
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public class Header
    {
        public Header (String _Name, Object _Value)
        
            : this(_Name, _Value, true) {
        }
        public Header (String _Name, Object _Value, bool _MustUnderstand)
        {
            Name = _Name;
            Value = _Value;
            MustUnderstand = _MustUnderstand;
        }

        public Header (String _Name, Object _Value, bool _MustUnderstand, String _HeaderNamespace)
        {
            Name = _Name;
            Value = _Value;
            MustUnderstand = _MustUnderstand;
            HeaderNamespace = _HeaderNamespace;
        }

        public String    Name;
        public Object    Value;
        public bool   MustUnderstand;

        public String HeaderNamespace;
    }
}
