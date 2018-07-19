//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Validation
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public sealed class AddValidationError : NativeActivity
    {
        public AddValidationError()
        {
        }

        public InArgument<string> Message
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public InArgument<bool> IsWarning
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public InArgument<string> PropertyName
        {
            get;
            set;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>();

            RuntimeArgument messageArgument = new RuntimeArgument("Message", typeof(string), ArgumentDirection.In);
            metadata.Bind(this.Message, messageArgument);
            arguments.Add(messageArgument);

            RuntimeArgument isWarningArgument = new RuntimeArgument("IsWarning", typeof(bool), ArgumentDirection.In, false);
            metadata.Bind(this.IsWarning, isWarningArgument);
            arguments.Add(isWarningArgument);
            
            RuntimeArgument propertyNameArgument = new RuntimeArgument("PropertyName", typeof(string), ArgumentDirection.In, false);
            metadata.Bind(this.PropertyName, propertyNameArgument);
            arguments.Add(propertyNameArgument);

            metadata.SetArgumentsCollection(arguments);
        }

        protected override void Execute(NativeActivityContext context)
        {
            bool isWarning = false;
            string propertyName = string.Empty;
            string errorCode = string.Empty;
            
            if (this.IsWarning != null)
            {
                isWarning = this.IsWarning.Get(context);
            }
            
            if (this.PropertyName != null)
            {
                propertyName = this.PropertyName.Get(context);            
            }
            
            Constraint.AddValidationError(context, new ValidationError(this.Message.Get(context), isWarning, propertyName));
        }
    }
}
