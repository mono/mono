namespace System.Web.Hosting {

    /// <devdoc>
    ///    <para>Defines the interface required of a replaceable application monitor by ASP.Net.</para>
    /// </devdoc>
    public interface IApplicationMonitor : IDisposable {
        /// <devdoc>
        ///    <para>Start monitoring and firing notifications.</para>
        /// </devdoc>
        void Start();

        /// <devdoc>
        ///    <para>Stop monitoring and firing notifications.</para>
        /// </devdoc>
        void Stop();
    }

    /// <devdoc>
    ///    <para>A group of repleacable monitor objects used by ASP.Net subsystems to maintain
    ///       application health.</para>
    /// </devdoc>
    public sealed class ApplicationMonitors {

        private class AppMonitorRegisteredObject : IRegisteredObject {
            ApplicationMonitors _appMonitors;

            public AppMonitorRegisteredObject(ApplicationMonitors appMonitors) {
                _appMonitors = appMonitors;
            }

            public void Stop(bool immediate) {
                if (_appMonitors != null) {
                    IApplicationMonitor pbMonitor = _appMonitors.MemoryMonitor;
                    if (pbMonitor != null) {
                        pbMonitor.Stop();
                        pbMonitor.Dispose();
                    }
                }

                HostingEnvironment.UnregisterObject(this);
            }
        }

        private IApplicationMonitor _memoryMonitor;
        public IApplicationMonitor MemoryMonitor {
            get { return _memoryMonitor; }

            set {
                // Allow setting to null
                if (_memoryMonitor != null && _memoryMonitor != value) {
                    _memoryMonitor.Stop();
                    _memoryMonitor.Dispose();
                }

                _memoryMonitor = value;

                if (_memoryMonitor != null) {
                    _memoryMonitor.Start();
                }
            }
        }

        internal ApplicationMonitors() {
#if !FEATURE_PAL
            _memoryMonitor = new AspNetMemoryMonitor();
            _memoryMonitor.Start();
#else
            _memoryMonitor = null;
#endif

            AppMonitorRegisteredObject myRegisteredObject = new AppMonitorRegisteredObject(this);
            HostingEnvironment.RegisterObject(myRegisteredObject);
        }
    }
}

