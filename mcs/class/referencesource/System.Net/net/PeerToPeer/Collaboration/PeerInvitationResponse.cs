using System;

namespace System.Net.PeerToPeer.Collaboration
{
    public class PeerInvitationResponse
    {
        private PeerInvitationResponseType m_peerInvResponseType;

        internal PeerInvitationResponse() { }
        internal PeerInvitationResponse(PeerInvitationResponseType reponseType) 
        {
            m_peerInvResponseType = reponseType;
        }

        public PeerInvitationResponseType PeerInvitationResponseType
        {
            get{
                return m_peerInvResponseType;
            }
            internal set{
                m_peerInvResponseType = value;
            }
        }
    }
}
