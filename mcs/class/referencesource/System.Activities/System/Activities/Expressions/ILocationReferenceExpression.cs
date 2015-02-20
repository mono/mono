// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Expressions
{
    // this is an internal interface for EnvironmentLocationReference/EnvironmentLocationValue/LocationReferenceValue to implement
    // to avoid creating instances of those generic types via expensive Activator.CreateInstance.
    interface ILocationReferenceExpression
    {
        ActivityWithResult CreateNewInstance(LocationReference locationReference);
    }
}
