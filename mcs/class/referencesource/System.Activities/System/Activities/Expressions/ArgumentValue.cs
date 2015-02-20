//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Windows.Markup;

    public sealed class ArgumentValue<T> : EnvironmentLocationValue<T>
    {
        RuntimeArgument targetArgument;

        public ArgumentValue()
        {
        }

        public ArgumentValue(string argumentName)
        {
            this.ArgumentName = argumentName;
        }

        public string ArgumentName
        {
            get;
            set;
        }

        public override LocationReference LocationReference
        {
            get { return this.targetArgument; }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            this.targetArgument = null;

            if (string.IsNullOrEmpty(this.ArgumentName))
            {
                metadata.AddValidationError(SR.ArgumentNameRequired);
            }
            else
            {
                this.targetArgument = ActivityUtilities.FindArgument(this.ArgumentName, this);

                if (this.targetArgument == null)
                {
                    metadata.AddValidationError(SR.ArgumentNotFound(this.ArgumentName));
                }
                else if (!TypeHelper.AreTypesCompatible(this.targetArgument.Type, typeof(T)))
                {
                    metadata.AddValidationError(SR.ArgumentTypeMustBeCompatible(this.ArgumentName, this.targetArgument.Type, typeof(T)));
                }
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.ArgumentName))
            {
                return this.ArgumentName;
            }

            return base.ToString();
        }
    }
}
