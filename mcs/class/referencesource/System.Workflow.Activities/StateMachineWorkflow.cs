namespace System.Workflow.Activities
{
    using System;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;

    [SRDescription(SR.StateMachineWorkflowActivityDescription)]
    [Designer(typeof(StateMachineWorkflowDesigner), typeof(IRootDesigner))]
    [Designer(typeof(StateMachineWorkflowDesigner), typeof(IDesigner))]
    [ToolboxItem(false)]
    [ToolboxBitmap(typeof(StateMachineWorkflowActivity), "Resources.StateMachineWorkflowActivity.png")]
    [ActivityValidator(typeof(StateActivityValidator))]
    [SRCategory(SR.Standard)]
    [SRDisplayName(SR.StateMachineWorkflow)]
    [System.Runtime.InteropServices.ComVisible(false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class StateMachineWorkflowActivity : StateActivity
    {
        internal const string InitialStateNamePropertyName = "InitialStateName";
        internal const string CompletedStateNamePropertyName = "CompletedStateName";
        public const string SetStateQueueName = "SetStateQueue";

        //metadata properties
        public static readonly DependencyProperty InitialStateNameProperty = DependencyProperty.Register(StateMachineWorkflowActivity.InitialStateNamePropertyName, typeof(string), typeof(StateMachineWorkflowActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty CompletedStateNameProperty = DependencyProperty.Register(StateMachineWorkflowActivity.CompletedStateNamePropertyName, typeof(string), typeof(StateMachineWorkflowActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));

        public StateMachineWorkflowActivity()
        {
        }


        public StateMachineWorkflowActivity(string name)
            : base(name)
        {
        }

        [SRDescription(SR.DynamicUpdateConditionDescr)]
        [SRCategory(SR.Conditions)]
        public ActivityCondition DynamicUpdateCondition
        {
            get
            {
                return WorkflowChanges.GetCondition(this) as ActivityCondition;
            }
            set
            {
                WorkflowChanges.SetCondition(this, value);
            }
        }


        [ValidationOption(ValidationOption.Optional)]
        [SRDescription(SR.InitialStateDescription)]
        [Editor(typeof(StateDropDownEditor), typeof(UITypeEditor))]
        [DefaultValue("")]
        public string InitialStateName
        {
            get
            {
                return (string)base.GetValue(InitialStateNameProperty);
            }
            set
            {
                base.SetValue(InitialStateNameProperty, value);
            }
        }

        [SRDescription(SR.CompletedStateDescription)]
        [Editor(typeof(StateDropDownEditor), typeof(UITypeEditor))]
        [DefaultValue("")]
        public string CompletedStateName
        {
            get
            {
                return (string)base.GetValue(CompletedStateNameProperty);
            }
            set
            {
                base.SetValue(CompletedStateNameProperty, value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CurrentStateName
        {
            get
            {
                StateMachineExecutionState executionState = this.ExecutionState;
                if (executionState == null)
                    return null;

                return executionState.CurrentStateName;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string PreviousStateName
        {
            get
            {
                StateMachineExecutionState executionState = this.ExecutionState;
                if (executionState == null)
                    return null;

                return executionState.PreviousStateName;
            }
        }

        internal StateMachineExecutionState ExecutionState
        {
            get
            {
                return (StateMachineExecutionState)base.GetValue(StateMachineExecutionStateProperty);
            }
        }
    }
}
