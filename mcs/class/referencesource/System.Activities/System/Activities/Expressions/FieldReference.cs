//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;

    public sealed class FieldReference<TOperand, TResult> : CodeActivity<Location<TResult>>
    {
        FieldInfo fieldInfo;

        public FieldReference()
            : base()
        {
        }

        [DefaultValue(null)]
        public string FieldName
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public InArgument<TOperand> Operand
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
            else if (typeof(TOperand).IsValueType)
            {
                metadata.AddValidationError(SR.TargetTypeIsValueType(this.GetType().Name, this.DisplayName));
            }

            if (string.IsNullOrEmpty(this.FieldName))
            {
                metadata.AddValidationError(SR.ActivityPropertyMustBeSet("FieldName", this.DisplayName));
            }
            else
            {
                Type operandType = typeof(TOperand);
                this.fieldInfo = operandType.GetField(this.FieldName);

                if (this.fieldInfo == null)
                {
                    metadata.AddValidationError(SR.MemberNotFound(this.FieldName, typeof(TOperand).Name));
                }
                else
                {
                    if (fieldInfo.IsInitOnly)
                    {
                        metadata.AddValidationError(SR.MemberIsReadOnly(this.FieldName, typeof(TOperand).Name));
                    }
                    isRequired = !this.fieldInfo.IsStatic;
                }
            }
            MemberExpressionHelper.AddOperandArgument(metadata, this.Operand, isRequired);
        }

        protected override Location<TResult> Execute(CodeActivityContext context)
        {
            Fx.Assert(this.fieldInfo != null, "fieldInfo must not be null.");
            return new FieldLocation(this.fieldInfo, this.Operand.Get(context));
        }

        [DataContract]
        internal class FieldLocation : Location<TResult>
        {
            FieldInfo fieldInfo;

            object owner;

            public FieldLocation(FieldInfo fieldInfo, object owner)
                : base()
            {
                this.fieldInfo = fieldInfo;
                this.owner = owner;
            }

            public override TResult Value
            {
                get
                {
                    //if (!this.fieldInfo.IsStatic && this.owner == null)
                    //{
                    //    // The field is non-static, and obj is a null reference 
                    //    if (this.fieldInfo.DeclaringType != null)
                    //    {
                    //        throw FxTrace.Exception.AsError(new ValidationException(SR.NullReferencedMemberAccess(this.fieldInfo.DeclaringType.Name, this.fieldInfo.Name)));
                    //    }
                    //    else
                    //    {
                    //        throw FxTrace.Exception.AsError(new ValidationException(SR.NullReferencedMemberAccess(typeof(FieldInfo), "DeclaringType")));
                    //    }
                    //}
                    return (TResult)this.fieldInfo.GetValue(this.owner);
                }
                set
                {
                    //if (!this.fieldInfo.IsStatic && this.owner == null)
                    //{
                    //    if (this.fieldInfo.DeclaringType != null)
                    //    {
                    //        // The field is non-static, and obj is a null reference 
                    //        throw FxTrace.Exception.AsError(new ValidationException(SR.NullReferencedMemberAccess(this.fieldInfo.DeclaringType.Name, this.fieldInfo.Name)));
                    //    }
                    //    else
                    //    {
                    //        throw FxTrace.Exception.AsError(new ValidationException(SR.NullReferencedMemberAccess(typeof(FieldInfo), "DeclaringType")));
                    //    }
                    //}
                    this.fieldInfo.SetValue(this.owner, value);
                }
            }

            [DataMember(Name = "fieldInfo")]
            internal FieldInfo SerializedFieldInfo
            {
                get { return this.fieldInfo; }
                set { this.fieldInfo = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "owner")]
            internal object SerializedOwner
            {
                get { return this.owner; }
                set { this.owner = value; }
            }
        }
    }
}
