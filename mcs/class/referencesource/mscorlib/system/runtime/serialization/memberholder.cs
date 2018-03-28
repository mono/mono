// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: MemberHolder
**
**
** Purpose: This is a lightweight class designed to hold the members 
**          and StreamingContext for a particular class.
**
**
============================================================*/
namespace System.Runtime.Serialization {

    using System.Runtime.Remoting;
    using System;
    using System.Reflection;
    [Serializable]
    internal class MemberHolder {
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal MemberInfo[] members = null;
#pragma warning restore 0414
        internal Type memberType;
        internal StreamingContext context;
        
        internal MemberHolder(Type type, StreamingContext ctx) {
            memberType = type;
            context = ctx;
        }
    
        public override int GetHashCode() {
            return memberType.GetHashCode();
        }
    
        public override bool Equals(Object obj) {
            if (!(obj is MemberHolder)) {
                return false;
            }
            
            MemberHolder temp = (MemberHolder)obj;
    
            if (Object.ReferenceEquals(temp.memberType, memberType) && temp.context.State == context.State) {
                return true;
            }
            
            return false;
        }
    }
}
