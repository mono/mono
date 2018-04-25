//------------------------------------------------------------------------------
// <copyright file="State.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Runtime.Collections;
    using System.Windows.Markup;

    /// <summary>
    /// This class represents a State in a StateMachine.
    /// </summary>
    public sealed class State
    {
        InternalState internalState;
        Collection<Transition> transitions;
        NoOp nullTrigger;
        Collection<Variable> variables;

        /// <summary>
        /// Gets or sets DisplayName of the State.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets entry action of the State. It is executed when the StateMachine enters the State. 
        /// It's optional.
        /// </summary>
        [DefaultValue(null)]
        public Activity Entry
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets exit action of the State. It is executed when the StateMachine leaves the State. 
        /// It's optional.
        /// </summary>
        [DependsOn("Entry")]
        [DefaultValue(null)]
        public Activity Exit
        {
            get;
            set;
        }


        /// <summary>
        /// Gets Transitions collection contains all outgoing Transitions from the State.
        /// </summary>
        [DependsOn("Exit")]
        public Collection<Transition> Transitions
        {
            get
            {
                if (this.transitions == null)
                {
                    this.transitions = new ValidatingCollection<Transition>
                    {

                        // disallow null values
                        OnAddValidationCallback = item =>
                        {
                            if (item == null)
                            {
                                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
                            }
                        },
                    };
                }

                return this.transitions;
            }
        }

        /// <summary>
        /// Gets Variables which can be used within the scope of State and its Transitions collection.
        /// </summary>
        [DependsOn("Transitions")]
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
                                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
                            }
                        },
                    };
                }

                return this.variables;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the State is a final State.
        /// </summary>
        [DefaultValue(false)]
        public bool IsFinal
        {
            get;
            set;
        }

        /// <summary>
        /// Gets Internal activity representation of state.
        /// </summary>
        internal InternalState InternalState
        {
            get
            {
                if (this.internalState == null)
                {
                    this.internalState = new InternalState(this);
                }
                return this.internalState;
            }
        }

        /// <summary>
        /// Gets or sets PassNumber is used to detect re-visiting when traversing states in StateMachine. 
        /// </summary>
        internal uint PassNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether state can be reached via transitions.
        /// </summary>
        internal bool Reachable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets StateId is unique within a StateMachine.
        /// </summary>
        internal string StateId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the display name of the parent state machine of the state.
        /// Used for tracking purpose only.
        /// </summary>
        internal string StateMachineName
        {
            get;
            set;
        }

        /// <summary>
        /// Clear internal state. 
        /// </summary>
        internal void ClearInternalState()
        {
            this.internalState = null;
        }

        internal NoOp NullTrigger
        {
            get
            {
                if (this.nullTrigger == null)
                {
                    this.nullTrigger = new NoOp
                    {
                        DisplayName = "Null Trigger"
                    };
                }

                return this.nullTrigger;
            }
        }

        internal sealed class NoOp : CodeActivity
        {
            protected override void Execute(CodeActivityContext context)
            {

            }
        }
    }
}
