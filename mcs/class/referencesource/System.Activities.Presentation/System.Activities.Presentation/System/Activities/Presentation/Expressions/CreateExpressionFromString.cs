// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Expressions
{
    /// <summary>
    /// This is the delegate supplied by custom expression editor to create expression activity from string text 
    /// </summary>
    /// <param name="expressionText">String text used to create the expression</param>
    /// <param name="useLocationExpression">Should create location expression or not</param>
    /// <param name="expressionType">Return type of the expression</param>
    /// <returns>the created expression activity</returns>
    public delegate ActivityWithResult CreateExpressionFromStringCallback(string expressionText, bool useLocationExpression, Type expressionType);
}
