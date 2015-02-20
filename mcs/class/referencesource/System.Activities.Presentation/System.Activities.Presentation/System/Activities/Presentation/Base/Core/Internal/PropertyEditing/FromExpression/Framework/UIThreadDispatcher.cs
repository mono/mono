// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework
{
    using System;
    using System.Diagnostics;
    using System.Windows.Threading;
    using System.Runtime;

    // <summary>
    // A class to execute a method on the UI thread. This must be constructed on the UI thread,
    // usually by calling UIThreadDispatcher.InitializeDispatcher(). Derive from this and
    // install your own on UIThreadDispatcher if you want to.
    // </summary>
    internal class UIThreadDispatcher
    {

        static UIThreadDispatcher dispatcher;
        // fields
        private Dispatcher uiThreadDispatcher;

        // Singleton Management

        public UIThreadDispatcher()
        {
            this.uiThreadDispatcher = Dispatcher.CurrentDispatcher;
        }

        public static UIThreadDispatcher Instance
        {
            get
            {
                Fx.Assert(dispatcher != null, "Instance getter called before Instance is initialized");

                return dispatcher;
            }
            set
            {
                dispatcher = value;
            }
        }


        public static void InitializeInstance()
        {
            Instance = new UIThreadDispatcher();
        }

        public virtual void BeginInvoke(DispatcherPriority priority, Delegate method)
        {
            if (!this.uiThreadDispatcher.HasShutdownStarted)
            {
                this.uiThreadDispatcher.BeginInvoke(priority, method);
            }
        }

        public virtual void BeginInvoke(DispatcherPriority priority, Delegate method, object arg)
        {
            if (!this.uiThreadDispatcher.HasShutdownStarted)
            {
                this.uiThreadDispatcher.BeginInvoke(priority, method, arg);
            }
        }

        public virtual void BeginInvoke(DispatcherPriority priority, Delegate method, object arg, params object[] args)
        {
            if (!this.uiThreadDispatcher.HasShutdownStarted)
            {
                this.uiThreadDispatcher.BeginInvoke(priority, method, arg, args);
            }
        }

        public virtual void Invoke(DispatcherPriority priority, Delegate method)
        {
            if (!this.uiThreadDispatcher.HasShutdownStarted)
            {
                this.uiThreadDispatcher.Invoke(priority, method);
            }
        }

        public virtual void Invoke(DispatcherPriority priority, Delegate method, object arg)
        {
            if (!this.uiThreadDispatcher.HasShutdownStarted)
            {
                this.uiThreadDispatcher.Invoke(priority, method, arg);
            }
        }

        public virtual void Invoke(DispatcherPriority priority, Delegate method, object arg, params object[] args)
        {
            if (!this.uiThreadDispatcher.HasShutdownStarted)
            {
                this.uiThreadDispatcher.Invoke(priority, method, arg, args);
            }
        }
    }
}

