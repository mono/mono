//------------------------------------------------------------------------------
// <copyright file="CollabEnumTypes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.PeerToPeer.Collaboration
{
    //
    // Defines all the enums used by peer collaboration
    //

    public enum PeerPresenceStatus
    {
        Offline = 0,
        OutToLunch,
        Away,
        BeRightBack,
        Idle,
        Busy,
        OnThePhone,
        Online
    }

    public enum PeerScope
    {
        None = 0,
        NearMe,
        Internet,
        All = NearMe | Internet
    }

    public enum PeerApplicationRegistrationType
    {
        CurrentUser = 0,
        AllUsers
    }

    public enum PeerInvitationResponseType
    {
        Declined = 0,
        Accepted,
        Expired
    }

    public enum PeerChangeType
    {
        Added = 0,
        Deleted,
        Updated
    }
    
    public enum SubscriptionType
    {
        Blocked = 0,
        Allowed
    }

    internal enum PeerCollabEventType
    {
        WatchListChanged = 1,
        EndPointChanged = 2,
        EndPointPresenceChanged = 3,
        EndPointApplicationChanged = 4,
        EndPointObjectChanged = 5,
        MyEndPointChanged = 6,
        MyPresenceChanged = 7,
        MyApplicationChanged = 8,
        MyObjectChanged = 9,
        PeopleNearMeChanged = 10,
        RequestStatusChanged = 11
    }
}
