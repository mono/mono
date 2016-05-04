//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Expressions
{
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    [Fx.Tag.XamlVisible(false)]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExpressionMorphHelperAttribute : Attribute
    {
        Type helperType;

        public ExpressionMorphHelperAttribute(Type expressionMorphHelperType)
        {
            if (typeof(ExpressionMorphHelper).IsAssignableFrom(expressionMorphHelperType))
            {
                this.helperType = expressionMorphHelperType;
            }
            else
            {                
                throw FxTrace.Exception.AsError(new ArgumentException(string.Format(CultureInfo.CurrentUICulture, 
                    SR.InvalidExpressionMorphHelperType, expressionMorphHelperType.FullName, typeof(ExpressionMorphHelper).FullName)));
            }
        }
        
        public Type ExpressionMorphHelperType
        {
            get
            {
                return this.helperType;
            }
        }
    }
}
