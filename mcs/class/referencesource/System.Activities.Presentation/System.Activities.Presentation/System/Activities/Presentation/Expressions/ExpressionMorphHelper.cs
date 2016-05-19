//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Expressions
{
    public abstract class ExpressionMorphHelper
    {     
        //By default expression cannot infer the type.
        public virtual bool TryInferReturnType(ActivityWithResult expression, EditingContext context, out Type returnType)
        {
            returnType = null;
            return false;
        }        

        public abstract bool TryMorphExpression(ActivityWithResult expression, bool isLocationExpression, Type newType, 
            EditingContext context, out ActivityWithResult newExpression);
    }
}
