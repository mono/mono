//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities;
    using System.Activities.DynamicUpdate;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldNotMatchKeywords, Justification = "Optimizing for XAML naming. VB imperative users will [] qualify (e.g. New [OrElse])")]
    public sealed class OrElse : Activity<bool>
    {
        public OrElse()
            : base()
        {
            this.Implementation =
                () =>
                {
                    if (this.Left != null && this.Right != null)
                    {
                        return new If
                        {
                            Condition = this.Left,
                            Then = new Assign<bool>
                            {
                                To = new OutArgument<bool>(context => this.Result.Get(context)),
                                Value = true,
                            },
                            Else = new Assign<bool>
                            {
                                To = new OutArgument<bool>(context => this.Result.Get(context)),
                                Value = new InArgument<bool>(this.Right)
                            }
                        };
                    }
                    else
                    {
                        return null;
                    }
                };
        }

        [DefaultValue(null)]
        public Activity<bool> Left
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public Activity<bool> Right
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(UpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            metadata.AddImportedChild(this.Left);
            metadata.AddImportedChild(this.Right);

            if (this.Left == null)
            {
                metadata.AddValidationError(SR.BinaryExpressionActivityRequiresArgument("Left", "OrElse", this.DisplayName));
            }

            if (this.Right == null)
            {
                metadata.AddValidationError(SR.BinaryExpressionActivityRequiresArgument("Right", "OrElse", this.DisplayName));
            }
        }
    }
}
