// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
//  GacMembershipCondition.cs
//
//  Implementation of membership condition for being in the Gac
//

namespace System.Security.Policy {
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Diagnostics.Contracts;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    sealed public class GacMembershipCondition : IMembershipCondition, IConstantMembershipCondition, IReportMatchMembershipCondition
    {
        //------------------------------------------------------
        //
        // PUBLIC CONSTRUCTORS
        //
        //------------------------------------------------------

        public GacMembershipCondition()
        {
        }

        //------------------------------------------------------
        //
        // IMEMBERSHIPCONDITION IMPLEMENTATION
        //
        //------------------------------------------------------

        public bool Check( Evidence evidence )
        {
            object usedEvidence = null;
            return (this as IReportMatchMembershipCondition).Check(evidence, out usedEvidence);
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
        {
            usedEvidence = null;

            if (evidence == null)
                return false;

            return evidence.GetHostEvidence<GacInstalled>() != null;
        }

        public IMembershipCondition Copy()
        {
            return new GacMembershipCondition();
        }

        public SecurityElement ToXml()
        {
            return ToXml( null );
        }

        public void FromXml( SecurityElement e )
        {
            FromXml( e, null );
        }

        public SecurityElement ToXml( PolicyLevel level )
        {
            SecurityElement root = new SecurityElement( "IMembershipCondition" );
            System.Security.Util.XMLUtil.AddClassAttribute( root, this.GetType(), this.GetType().FullName );
            root.AddAttribute( "version", "1" );
            return root;
        }

        public void FromXml( SecurityElement e, PolicyLevel level )
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (!e.Tag.Equals( "IMembershipCondition" ))
                throw new ArgumentException( Environment.GetResourceString( "Argument_MembershipConditionElement" ) );
            Contract.EndContractBlock();
        }

        public override bool Equals( Object o )
        {
            GacMembershipCondition that = (o as GacMembershipCondition);
            if (that != null)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override String ToString()
        {
            return Environment.GetResourceString( "GAC_ToString" );
        }
    }
}
