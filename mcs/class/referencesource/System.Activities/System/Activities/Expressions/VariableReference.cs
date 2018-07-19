//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Linq.Expressions;
    using System.Runtime;

    public sealed class VariableReference<T> : EnvironmentLocationReference<T>
    {
        public VariableReference()
            : base()
        {
        }

        public VariableReference(Variable variable)
            : base()
        {
            this.Variable = variable;
        }

        public Variable Variable
        {
            get;
            set;
        }

        public override LocationReference LocationReference
        {
            get { return this.Variable; }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (this.Variable == null)
            {
                metadata.AddValidationError(SR.VariableMustBeSet);
            }
            else
            {
                if (!(this.Variable is Variable<T>))
                {
                    metadata.AddValidationError(SR.VariableTypeInvalid(this.Variable, typeof(T), this.Variable.Type));
                }

                if (!this.Variable.IsInTree)
                {
                    metadata.AddValidationError(SR.VariableShouldBeOpen(this.Variable.Name));
                }

                if (!metadata.Environment.IsVisible(this.Variable))
                {
                    metadata.AddValidationError(SR.VariableNotVisible(this.Variable.Name));
                }

                if (VariableModifiersHelper.IsReadOnly(Variable.Modifiers))
                {
                    metadata.AddValidationError(SR.VariableIsReadOnly(this.Variable.Name));
                }
            }
        }

        public override string ToString()
        {
            if (Variable != null && !string.IsNullOrEmpty(Variable.Name))
            {
                return Variable.Name;
            }

            return base.ToString();
        }
    }
}
