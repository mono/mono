namespace System.Workflow.ComponentModel
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.ComponentModel.Design.Serialization;
    using System.Reflection;

    //
    [DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    [TypeConverter(typeof(ConditionTypeConverter))]
    [ActivityValidator(typeof(ConditionValidator))]
    [MergableProperty(false)]
    [Browsable(true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class ActivityCondition : DependencyObject
    {
        public abstract bool Evaluate(Activity activity, IServiceProvider provider);
    }
}
