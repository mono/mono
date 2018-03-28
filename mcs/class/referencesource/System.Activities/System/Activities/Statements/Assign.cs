//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;

    public sealed class Assign : CodeActivity
    {
        public Assign()
            : base()
        {
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public OutArgument To
        {
            get;
            set;
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument Value
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>();
            
            Type valueType = TypeHelper.ObjectType;

            if (this.Value != null)
            {
                valueType = this.Value.ArgumentType;
            }

            RuntimeArgument valueArgument = new RuntimeArgument("Value", valueType, ArgumentDirection.In, true);
            metadata.Bind(this.Value, valueArgument);

            Type toType = TypeHelper.ObjectType;

            if (this.To != null)
            {
                toType = this.To.ArgumentType;
            }

            RuntimeArgument toArgument = new RuntimeArgument("To", toType, ArgumentDirection.Out, true);
            metadata.Bind(this.To, toArgument);

            arguments.Add(valueArgument);
            arguments.Add(toArgument);

            metadata.SetArgumentsCollection(arguments);

            if (this.Value != null && this.To != null)
            {
                if (!TypeHelper.AreTypesCompatible(this.Value.ArgumentType, this.To.ArgumentType))
                {
                    metadata.AddValidationError(SR.TypeMismatchForAssign(
                                this.Value.ArgumentType,
                                this.To.ArgumentType,
                                this.DisplayName));
                }
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            this.To.Set(context, this.Value.Get(context));
        }
    }

    public sealed class Assign<T> : CodeActivity
    {
        public Assign()
            : base()
        {
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public OutArgument<T> To
        {
            get;
            set;
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<T> Value
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>();

            RuntimeArgument valueArgument = new RuntimeArgument("Value", typeof(T), ArgumentDirection.In, true);
            metadata.Bind(this.Value, valueArgument);

            RuntimeArgument toArgument = new RuntimeArgument("To", typeof(T), ArgumentDirection.Out, true);
            metadata.Bind(this.To, toArgument);

            arguments.Add(valueArgument);
            arguments.Add(toArgument);

            metadata.SetArgumentsCollection(arguments);
        }

        protected override void Execute(CodeActivityContext context)
        {
            context.SetValue(this.To, this.Value.Get(context));
        }
    }
}
