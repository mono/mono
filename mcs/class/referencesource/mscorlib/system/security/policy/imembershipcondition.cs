// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  IMembershipCondition.cs
// 
// <OWNER>ShawnFa</OWNER>
//
//  Interface that all MembershipConditions must implement
//

namespace System.Security.Policy {
    
    using System;
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IMembershipCondition : ISecurityEncodable, ISecurityPolicyEncodable
    {
        bool Check( Evidence evidence );
    
        IMembershipCondition Copy();
        
        String ToString();

        bool Equals( Object obj );
        
    }

    /// <summary>
    ///     Interface for membership conditions that support reporting which evidence objects were used to
    ///     calculate their grant set.
    /// </summary>
    internal interface IReportMatchMembershipCondition : IMembershipCondition
    {
        bool Check(Evidence evidence, out object usedEvidence);
    }
}
