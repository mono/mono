//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Windows.Markup;

    [ContentProperty("Duration")]
    public sealed class Delay : NativeActivity
    {
        static Func<TimerExtension> getDefaultTimerExtension = new Func<TimerExtension>(GetDefaultTimerExtension);
        Variable<Bookmark> timerBookmark;

        public Delay()
            : base()
        {
            this.timerBookmark = new Variable<Bookmark>();
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<TimeSpan> Duration
        {
            get;
            set;
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument durationArgument = new RuntimeArgument("Duration", typeof(TimeSpan), ArgumentDirection.In, true);
            metadata.Bind(this.Duration, durationArgument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { durationArgument });
            metadata.AddImplementationVariable(this.timerBookmark);
            metadata.AddDefaultExtensionProvider(getDefaultTimerExtension);
        }

        static TimerExtension GetDefaultTimerExtension()
        {
            return new DurableTimerExtension();
        }

        protected override void Execute(NativeActivityContext context)
        {
            TimeSpan duration = this.Duration.Get(context);
            if (duration < TimeSpan.Zero)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("Duration", duration, SR.DurationIsNegative(this.DisplayName));
            }

            if (duration == TimeSpan.Zero)
            {
                return; 
            }
                        
            TimerExtension timerExtension = GetTimerExtension(context);
            Bookmark bookmark = context.CreateBookmark();
            timerExtension.RegisterTimer(duration, bookmark);
            this.timerBookmark.Set(context, bookmark);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            Bookmark timerBookmark = this.timerBookmark.Get(context);
            TimerExtension timerExtension = GetTimerExtension(context);
            timerExtension.CancelTimer(timerBookmark);
            context.RemoveBookmark(timerBookmark);
            context.MarkCanceled();
        }

        protected override void Abort(NativeActivityAbortContext context)
        {
            Bookmark timerBookmark = this.timerBookmark.Get(context);
            // The bookmark could be null in abort when user passed in a negative delay as a duration
            if (timerBookmark != null)
            {
                TimerExtension timerExtension = GetTimerExtension(context);
                timerExtension.CancelTimer(timerBookmark);
            }
            base.Abort(context);
        }

        TimerExtension GetTimerExtension(ActivityContext context)
        {
            TimerExtension timerExtension = context.GetExtension<TimerExtension>();
            Fx.Assert(timerExtension != null, "TimerExtension must exist.");
            return timerExtension;
        }
    }
}
