// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  ApplicationDirectoryMembershipCondition.cs
// 
// <OWNER>[....]</OWNER>
//
//  Implementation of membership condition for "application directories"
//

namespace System.Security.Policy {   
    using System;
    using SecurityElement = System.Security.SecurityElement;
    using System.Security.Policy;
    using URLString = System.Security.Util.URLString;
    using System.Collections;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    sealed public class ApplicationDirectoryMembershipCondition : IMembershipCondition, IConstantMembershipCondition, IReportMatchMembershipCondition
    {
        //------------------------------------------------------
        //
        // PRIVATE STATE DATA
        //
        //------------------------------------------------------
        
        //------------------------------------------------------
        //
        // PUBLIC CONSTRUCTORS
        //
        //------------------------------------------------------
    
        public ApplicationDirectoryMembershipCondition()
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

            ApplicationDirectory dir = evidence.GetHostEvidence<ApplicationDirectory>();
            Url url = evidence.GetHostEvidence<Url>();

            if (dir != null && url != null)
            {
                // We need to add a wildcard at the end because IsSubsetOf keys off of it.
                String appDir = dir.Directory;
                
                if (appDir != null && appDir.Length > 1)
                {
                    if (appDir[appDir.Length-1] == '/')
                        appDir += "*";
                    else
                        appDir += "/*";
                    
                    URLString appDirString = new URLString(appDir);
                    if (url.GetURLString().IsSubsetOf(appDirString))
                    {
                        usedEvidence = dir;
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        public IMembershipCondition Copy()
        {
            return new ApplicationDirectoryMembershipCondition();
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
            System.Security.Util.XMLUtil.AddClassAttribute( root, this.GetType(), "System.Security.Policy.ApplicationDirectoryMembershipCondition" );
            // If you hit this assert then most likely you are trying to change the name of this class. 
            // This is ok as long as you change the hard coded string above and change the assert below.
            BCLDebug.Assert( this.GetType().FullName.Equals( "System.Security.Policy.ApplicationDirectoryMembershipCondition" ), "Class name changed!" );

            root.AddAttribute( "version", "1" );
            
            return root;
        }
    
        public void FromXml( SecurityElement e, PolicyLevel level )
        {
            if (e == null)
                throw new ArgumentNullException("e");
        
            if (!e.Tag.Equals( "IMembershipCondition" ))
            {
                throw new ArgumentException( Environment.GetResourceString( "Argument_MembershipConditionElement" ) );
            }
        }
        
        public override bool Equals( Object o )
        {
            return (o is ApplicationDirectoryMembershipCondition);
        }
        
        public override int GetHashCode()
        {
            return typeof( ApplicationDirectoryMembershipCondition ).GetHashCode();
        }
        
        public override String ToString()
        {
            return Environment.GetResourceString( "ApplicationDirectory_ToString" );
        }   
    }
}
