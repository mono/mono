//------------------------------------------------------------------------------
// <copyright file="PeerApplicationLaunchInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.PeerToPeer.Collaboration
{
    using System;

    /// <summary>
    /// Represents launch info that collab gives back for the running application.
    /// If this application has been started up via collab invitation then it gives 
    /// back the details of that invitation.
    /// </summary>
    public class PeerApplicationLaunchInfo
    {
        private PeerContact m_peerContact;
        private PeerEndPoint m_peerEndPoint;
        private PeerApplication m_peerApplication;
        private byte[] m_inviteData;
        private string m_message;

        internal PeerApplicationLaunchInfo() { }

        public PeerContact PeerContact
        {
            get{
                return m_peerContact;
            }
            internal set{
                m_peerContact = value;
            }
        }

        public PeerEndPoint PeerEndPoint
        {
            get{
                return m_peerEndPoint;
            }
            internal set{
                m_peerEndPoint = value;
            }
        }

        public PeerApplication PeerApplication
        {
            get{
                return m_peerApplication;
            }
            internal set{
                m_peerApplication = value;
            }
        }

        public byte[] Data
        {
            get{
                return m_inviteData;
            }
            internal set{
                m_inviteData = value;
            }
        }

        public string Message
        {
            get{
                return m_message;
            }
            internal set{
                m_message = value;
            }
        }
    }
}
