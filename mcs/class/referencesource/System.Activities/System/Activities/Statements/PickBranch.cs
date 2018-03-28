//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.Collections;
    using System.Windows.Markup;

    [ContentProperty("Action")]
    public sealed class PickBranch
    {
        Collection<Variable> variables;
        string displayName;

        public PickBranch()
        {
            displayName = "PickBranch";
        }

        public Collection<Variable> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    this.variables = new ValidatingCollection<Variable>
                    {
                        // disallow null values
                        OnAddValidationCallback = item =>
                        {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                }
                return this.variables;
            }
        }

        [DefaultValue(null)]
        [DependsOn("Variables")]
        public Activity Trigger { get; set; }

        [DefaultValue(null)]
        [DependsOn("Trigger")]
        public Activity Action { get; set; }

        // 
        [DefaultValue("PickBranch")]
        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
            set
            {
                this.displayName = value;
            }
        }
    }
}
