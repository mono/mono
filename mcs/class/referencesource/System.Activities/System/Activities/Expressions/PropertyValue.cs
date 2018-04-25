//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities;
    using System.Activities.Validation;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;

    public sealed class PropertyValue<TOperand, TResult> : CodeActivity<TResult>
    {
        Func<TOperand, TResult> operationFunction;
        bool isOperationFunctionStatic;

        public InArgument<TOperand> Operand
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public string PropertyName
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            bool isRequired = false;
            if (typeof(TOperand).IsEnum)
            {
                metadata.AddValidationError(SR.TargetTypeCannotBeEnum(this.GetType().Name, this.DisplayName));
            }

            if (string.IsNullOrEmpty(this.PropertyName))
            {
                metadata.AddValidationError(SR.ActivityPropertyMustBeSet("PropertyName", this.DisplayName));
            }
            else
            {
                PropertyInfo propertyInfo = null;
                Type operandType = typeof(TOperand);
                propertyInfo = operandType.GetProperty(this.PropertyName);

                if (propertyInfo == null)
                {
                    metadata.AddValidationError(SR.MemberNotFound(this.PropertyName, typeof(TOperand).Name));
                }
                else
                {
                    Fx.Assert(propertyInfo.GetAccessors().Length > 0, "Property should have at least 1 accessor.");

                    this.isOperationFunctionStatic = propertyInfo.GetAccessors()[0].IsStatic;
                    isRequired = !this.isOperationFunctionStatic;

                    ValidationError validationError;
                    if (!MemberExpressionHelper.TryGenerateLinqDelegate(this.PropertyName, false, this.isOperationFunctionStatic, out this.operationFunction, out validationError))
                    {
                        metadata.AddValidationError(validationError);
                    }

                    MethodInfo getMethod = propertyInfo.GetGetMethod();
                    MethodInfo setMethod = propertyInfo.GetSetMethod();

                    if ((getMethod != null && !getMethod.IsStatic) || (setMethod != null && !setMethod.IsStatic))
                    {
                        isRequired = true;
                    }
                }
            }
            MemberExpressionHelper.AddOperandArgument(metadata, this.Operand, isRequired);
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            TOperand operandValue = this.Operand.Get(context);

            if (!this.isOperationFunctionStatic && operandValue == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MemberCannotBeNull("Operand", this.GetType().Name, this.DisplayName)));
            }

            TResult result = this.operationFunction(operandValue);
            return result;
        }
    }
}
