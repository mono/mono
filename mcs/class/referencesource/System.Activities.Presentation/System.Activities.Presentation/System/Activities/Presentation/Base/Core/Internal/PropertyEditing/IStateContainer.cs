//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.State 
{

    // <summary>
    // Internal interface we use to mark classes that manage state that we want to
    // marshal across control instances as well as AppDomains.
    // </summary>
    internal interface IStateContainer 
    {

        // <summary>
        // Retrieves the state stored in the object implementing this interface
        // </summary>
        // <returns>AppDomain-friendly state object</returns>
        object RetrieveState();

        // <summary>
        // Restores its state based on the specified state object
        // </summary>
        // <param name="state">State to apply</param>
        void RestoreState(object state);
    }
}
