// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  ApplicationDirectory.cs
// 
// <OWNER>[....]</OWNER>
//
//  ApplicationDirectory is an evidence type representing the directory the assembly
//  was loaded from.
//

namespace System.Security.Policy {
    
    using System;
    using System.IO;
    using System.Security.Util;
    using System.Collections;
    using System.Diagnostics.Contracts;
    
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class ApplicationDirectory : EvidenceBase
    {
        private URLString m_appDirectory;
    
        public ApplicationDirectory( String name )
        {
            if (name == null)
                throw new ArgumentNullException( "name" );
            Contract.EndContractBlock();
        
            m_appDirectory = new URLString( name );
        }

        private ApplicationDirectory(URLString appDirectory)
        {
            Contract.Assert(appDirectory != null);
            m_appDirectory = appDirectory;
        }

        public String Directory
        {
            get
            {
                return m_appDirectory.ToString();
            }
        }
        
        public override bool Equals(Object o)
        {
            ApplicationDirectory other = o as ApplicationDirectory;
            if (other == null)
            {
                return false;
            }

            return m_appDirectory.Equals(other.m_appDirectory);
        }
    
        public override int GetHashCode()
        {
            return this.m_appDirectory.GetHashCode();
        }

        public override EvidenceBase Clone()
        {
            return new ApplicationDirectory(m_appDirectory);
        }

        public Object Copy()
        {
            return Clone();
        }
    
        internal SecurityElement ToXml()
        {
            SecurityElement root = new SecurityElement( "System.Security.Policy.ApplicationDirectory" );
            // If you hit this assert then most likely you are trying to change the name of this class. 
            // This is ok as long as you change the hard coded string above and change the assert below.
            Contract.Assert( this.GetType().FullName.Equals( "System.Security.Policy.ApplicationDirectory" ), "Class name changed!" );

            root.AddAttribute( "version", "1" );
            
            if (m_appDirectory != null)
                root.AddChild( new SecurityElement( "Directory", m_appDirectory.ToString() ) );
            
            return root;
        }
        
        public override String ToString()
        {
            return ToXml().ToString();
        }
    }
}
