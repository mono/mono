//------------------------------------------------------------------------------
// <copyright file="Timer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Timers {

    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System;
    using System.Runtime.Versioning;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;

    /// <devdoc>
    ///    <para>Handles recurring events in an application.</para>
    /// </devdoc>
    [
    DefaultProperty("Interval"),
    DefaultEvent("Elapsed"),
    HostProtection(Synchronization=true, ExternalThreading=true)
    ]
    public class Timer : Component, ISupportInitialize {
        private double interval;
        private bool  enabled;
        private bool initializing;
        private bool delayedEnable;                
        private ElapsedEventHandler onIntervalElapsed;
        private bool autoReset;               
        private ISynchronizeInvoke synchronizingObject;  
        private bool disposed;
        private System.Threading.Timer timer;
        private TimerCallback callback;
        private Object cookie;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Timers.Timer'/> class, with the properties
        ///    set to initial values.</para>
        /// </devdoc>
        public Timer()
        : base() {
            interval = 100;
            enabled = false;
            autoReset = true;
            initializing = false;
            delayedEnable = false;
            callback = new TimerCallback(this.MyTimerCallback);
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Timers.Timer'/> class, setting the <see cref='System.Timers.Timer.Interval'/> property to the specified period.
        ///    </para>
        /// </devdoc>
        public Timer(double interval)
        : this() {
            if (interval <= 0)
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "interval", interval));
                        
            double roundedInterval = Math.Ceiling(interval);
            if (roundedInterval > Int32.MaxValue || roundedInterval <= 0) {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "interval", interval));                
            }

            this.interval = (int) roundedInterval;
        }

        /// <devdoc>
        /// <para>Gets or sets a value indicating whether the Timer raises the Tick event each time the specified
        /// Interval has elapsed,
        ///    when Enabled is set to true.</para>
        /// </devdoc>
        [Category("Behavior"),  TimersDescription(SR.TimerAutoReset), DefaultValue(true)]
        public bool AutoReset {
            get {
                return this.autoReset;
            }

            set {
                if (DesignMode)
                     this.autoReset = value;
                else if (this.autoReset != value) {
                     this.autoReset = value;
                    if( timer != null) {
                         UpdateTimer();
                    }
                }
            }
        }

        /// <devdoc>
        /// <para>Gets or sets a value indicating whether the <see cref='System.Timers.Timer'/>
        /// is able
        /// to raise events at a defined interval.</para>
        /// </devdoc>
        //[....] - The default value by design is false, don't change it.
        [Category("Behavior"), TimersDescription(SR.TimerEnabled), DefaultValue(false)]
        public bool Enabled {
            get {
                return this.enabled;
            }

            set {
                if (DesignMode) {
                    this.delayedEnable = value;            
                    this.enabled = value; 
                }                    
                else if (initializing)
                    this.delayedEnable = value;            
                else if (enabled != value) {                                                                                                     
                    if (!value) {                                           
                        if( timer != null) {
                            cookie = null;
                            timer.Dispose();
                            timer = null;
                        }
                        enabled = value;                        
                    }
                    else {                          
                        enabled = value;
                        if( timer == null) {
                            if (disposed) {
                                throw new ObjectDisposedException(GetType().Name);
                            }

                            int i = (int)Math.Ceiling(interval);
                            cookie = new Object();
                            timer = new System.Threading.Timer(callback, cookie, i, autoReset? i:Timeout.Infinite);
                        }
                        else {
                            UpdateTimer();
                        }
                    }                        

                }                                                     
          }
        }


        private void UpdateTimer() {
            int i = (int)Math.Ceiling(interval);
            timer.Change(i, autoReset? i :Timeout.Infinite );
        }
        
        /// <devdoc>
        ///    <para>Gets or
        ///       sets the interval on which
        ///       to raise events.</para>
        /// </devdoc>
        [Category("Behavior"), TimersDescription(SR.TimerInterval), DefaultValue(100d), SettingsBindable(true)]
        public double Interval {
            get {
                return this.interval;
            }

            set {
                if (value <= 0)
                    throw new ArgumentException(SR.GetString(SR.TimerInvalidInterval, value, 0));
                                
                interval = value;                
                if (timer != null) {
                    UpdateTimer();
                }
            }
        }      


        /// <devdoc>
        /// <para>Occurs when the <see cref='System.Timers.Timer.Interval'/> has
        ///    elapsed.</para>
        /// </devdoc>
        [Category("Behavior"), TimersDescription(SR.TimerIntervalElapsed)]
        public event ElapsedEventHandler Elapsed {
            add {
                onIntervalElapsed += value;
            }
            remove {
                onIntervalElapsed -= value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Sets the enable property in design mode to true by default.
        ///    </para>
        /// </devdoc>                              
        /// <internalonly/>
        public override ISite Site {
            set {
                base.Site = value;
                if (this.DesignMode)
                    this.enabled= true;
            }
            
            get {
                return base.Site;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the object used to marshal event-handler calls that are issued when
        ///       an interval has elapsed.</para>
        /// </devdoc>
        [
        Browsable(false),        
        DefaultValue(null), 
        TimersDescription(SR.TimerSynchronizingObject)
        ]
        public ISynchronizeInvoke SynchronizingObject {
            get {
                if (this.synchronizingObject == null && DesignMode) {
                    IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));
                    if (host != null) {
                        object baseComponent = host.RootComponent;
                        if (baseComponent != null && baseComponent is ISynchronizeInvoke)
                            this.synchronizingObject = (ISynchronizeInvoke)baseComponent;
                    }                        
                }
            
                return this.synchronizingObject;
            }
            
            set {
                this.synchronizingObject = value;
            }
        }                  
        
        /// <devdoc>
        ///    <para>
        ///       Notifies
        ///       the object that initialization is beginning and tells it to stand by.
        ///    </para>
        /// </devdoc>
        public void BeginInit() {
            this.Close();
            this.initializing = true;
        }
                
        /// <devdoc>
        ///    <para>Disposes of the resources (other than memory) used by
        ///       the <see cref='System.Timers.Timer'/>.</para>
        /// </devdoc>
        public void Close() {                  
            initializing = false;
            delayedEnable = false;
            enabled = false;
                                        
            if (timer != null ) {
                timer.Dispose();
                timer = null;
            }                                            
        }                                

        /// <internalonly/>
        /// <devdoc>        
        /// </devdoc>
        protected override void Dispose(bool disposing) {            
            Close();                        
            this.disposed = true;
            base.Dispose(disposing);
        }      
         
        /// <devdoc>
        ///    <para>
        ///       Notifies the object that initialization is complete.
        ///    </para>
        /// </devdoc>
        public void EndInit() {
            this.initializing = false;            
            this.Enabled = this.delayedEnable;
        }        

        /// <devdoc>
        /// <para>Starts the timing by setting <see cref='System.Timers.Timer.Enabled'/> to <see langword='true'/>.</para>
        /// </devdoc>
        public void Start() {
            Enabled = true;
        }

        /// <devdoc>
        ///    <para>
        ///       Stops the timing by setting <see cref='System.Timers.Timer.Enabled'/> to <see langword='false'/>.
        ///    </para>
        /// </devdoc>
        public void Stop() {
            Enabled = false;
        }
                        
        private void MyTimerCallback(object state) {
            // System.Threading.Timer will not cancel the work item queued before the timer is stopped.
            // We don't want to handle the callback after a timer is stopped.
            if( state != cookie) { 
                return;
            } 
            
            if (!this.autoReset) {
                enabled = false;
            }

            FILE_TIME filetime = new FILE_TIME();
            GetSystemTimeAsFileTime(ref filetime);
            ElapsedEventArgs elapsedEventArgs = new ElapsedEventArgs(filetime.ftTimeLow, filetime.ftTimeHigh); 
            try {                                            
                // To avoid ---- between remove handler and raising the event
                ElapsedEventHandler intervalElapsed = this.onIntervalElapsed;
                if (intervalElapsed != null) {
                    if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
                        this.SynchronizingObject.BeginInvoke(intervalElapsed, new object[]{this, elapsedEventArgs});
                    else                        
                       intervalElapsed(this,  elapsedEventArgs);                                   
                }
            }
            catch {             
            }            
        }                        

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILE_TIME {
            internal int ftTimeLow;
            internal int ftTimeHigh;
        }

        [ResourceExposure(ResourceScope.None)]
        [DllImport(ExternDll.Kernel32), SuppressUnmanagedCodeSecurityAttribute()]
        internal static extern void GetSystemTimeAsFileTime(ref FILE_TIME lpSystemTimeAsFileTime);		
    }
}

