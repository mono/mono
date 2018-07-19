//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Collections;
    using System.Windows.Markup;
    using System.Collections.ObjectModel;

    [ContentProperty("Action")]
    public sealed class InvokeAction : NativeActivity
    {
        IList<Argument> actionArguments;

        public InvokeAction()
        {
            this.actionArguments = new ValidatingCollection<Argument>
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

        [DefaultValue(null)]
        public ActivityAction Action
        {
            get;
            set;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(this.Action);
        }
        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action);
        }

    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T> Argument
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(this.Action);

            RuntimeArgument runtimeArgument = new RuntimeArgument("Argument", typeof(T), ArgumentDirection.In, true);
            metadata.Bind(this.Argument, runtimeArgument);

            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { runtimeArgument });
        }
        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null) // no-op
            {
                return;
            }

            context.ScheduleAction<T>(Action, Argument.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null) // no-op
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context), 
                Argument4.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T7> Argument7
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6, T7> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context), Argument7.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T7> Argument7
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T8> Argument8
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context), Argument7.Get(context),
                Argument8.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T7> Argument7
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T8> Argument8
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T9> Argument9
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context), Argument7.Get(context),
                Argument8.Get(context), Argument9.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T7> Argument7
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T8> Argument8
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T9> Argument9
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T10> Argument10
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context), Argument7.Get(context),
                Argument8.Get(context), Argument9.Get(context), Argument10.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T7> Argument7
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T8> Argument8
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T9> Argument9
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T10> Argument10
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T11> Argument11
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context), Argument7.Get(context),
                Argument8.Get(context), Argument9.Get(context), Argument10.Get(context), Argument11.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T7> Argument7
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T8> Argument8
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T9> Argument9
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T10> Argument10
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T11> Argument11
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T12> Argument12
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context), Argument7.Get(context),
                Argument8.Get(context), Argument9.Get(context), Argument10.Get(context), Argument11.Get(context),
                Argument12.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T7> Argument7
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T8> Argument8
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T9> Argument9
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T10> Argument10
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T11> Argument11
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T12> Argument12
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T13> Argument13
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context), Argument7.Get(context),
                Argument8.Get(context), Argument9.Get(context), Argument10.Get(context), Argument11.Get(context),
                Argument12.Get(context), Argument13.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T7> Argument7
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T8> Argument8
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T9> Argument9
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T10> Argument10
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T11> Argument11
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T12> Argument12
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T13> Argument13
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T14> Argument14
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context), Argument7.Get(context),
                Argument8.Get(context), Argument9.Get(context), Argument10.Get(context), Argument11.Get(context),
                Argument12.Get(context), Argument13.Get(context), Argument14.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T7> Argument7
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T8> Argument8
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T9> Argument9
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T10> Argument10
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T11> Argument11
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T12> Argument12
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T13> Argument13
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T14> Argument14
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T15> Argument15
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context), Argument7.Get(context),
                Argument8.Get(context), Argument9.Get(context), Argument10.Get(context), Argument11.Get(context),
                Argument12.Get(context), Argument13.Get(context), Argument14.Get(context), Argument15.Get(context));
        }
    }

    [ContentProperty("Action")]
    public sealed class InvokeAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : NativeActivity
    {
        public InvokeAction()
        {
        }

        [RequiredArgument]
        public InArgument<T1> Argument1
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T2> Argument2
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T3> Argument3
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T4> Argument4
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T5> Argument5
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T6> Argument6
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T7> Argument7
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T8> Argument8
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T9> Argument9
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T10> Argument10
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T11> Argument11
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T12> Argument12
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T13> Argument13
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T14> Argument14
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T15> Argument15
        {
            get;
            set;
        }

        [RequiredArgument]
        public InArgument<T16> Argument16
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Action
        {
            get;
            set;
        }

        protected override void OnCreateDynamicUpdateMap(NativeActivityUpdateMapMetadata metadata, Activity originalActivity)
        {
            metadata.AllowUpdateInsideThisActivity();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Action == null || Action.Handler == null)
            {
                return;
            }

            context.ScheduleAction(Action, Argument1.Get(context), Argument2.Get(context), Argument3.Get(context),
                Argument4.Get(context), Argument5.Get(context), Argument6.Get(context), Argument7.Get(context),
                Argument8.Get(context), Argument9.Get(context), Argument10.Get(context), Argument11.Get(context),
                Argument12.Get(context), Argument13.Get(context), Argument14.Get(context), Argument15.Get(context),
                Argument16.Get(context));
        }
    }
}


