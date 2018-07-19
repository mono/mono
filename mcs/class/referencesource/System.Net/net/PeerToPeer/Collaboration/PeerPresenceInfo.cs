//------------------------------------------------------------------------------
// <copyright file="PeerPresenceInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.PeerToPeer.Collaboration
{
    using System;

    /// <summary>
    /// Encapsulates the presence information for a collab peer
    /// </summary>
    public class PeerPresenceInfo
    {
        private PeerPresenceStatus m_peerPresenceStatus;
        private string m_descriptiveText;

        public PeerPresenceInfo() {}

        public PeerPresenceInfo(PeerPresenceStatus presenceStatus, string description) {
            m_peerPresenceStatus = presenceStatus;
            m_descriptiveText = description;
        }

        public PeerPresenceStatus PresenceStatus
        {
            get{
                return m_peerPresenceStatus;
            }
            set
            {
                m_peerPresenceStatus = value;
            }
        }
        public string DescriptiveText
        {
            get{
                return m_descriptiveText;
            }
            set
            {
                m_descriptiveText = value;
            }
        }
    }

}
