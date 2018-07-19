//------------------------------------------------------------------------------
// <copyright file="ProcessModelSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.ComponentModel;
    using System.Web.Util;
    using System.Security.Permissions;

    /*         <!--
        processModel Attributes:
          enable="[true|false]" - Enable processModel
          timeout="[Infinite | HH:MM:SS] - Total life of process, once expired process is shutdown and a new process is created
          idleTimeout="[Infinite | HH:MM:SS]" - Total idle life of process, once expired process is automatically shutdown
          shutdownTimeout="[Infinite | HH:MM:SS]" - Time process is given to shutdown gracefully before being killed
          requestLimit="[Infinite | number]" - Total number of requests to serve before process is shutdown
          requestQueueLimit="[Infinite | number]" - Number of queued requests allowed before requests are rejected
          restartQueueLimit="[Infinite | number]" - Number of requests kept in queue while process is restarting
          memoryLimit="[number]" - Represents percentage of physical memory process is allowed to use before process is recycled
          webGarden="[true|false]" - Determines whether a process should be affinitized with a particular CPU
          cpuMask="[bit mask]" - Controls number of available CPUs available for ASP.NET processes (webGarden must be set to true)
          userName="[user]" - Windows user to run the process as.
                      Special users: "SYSTEM": run as localsystem (high privilege admin) account.
                                     "machine": run as low privilege user account named "ASPNET".
                      Other users: If domain is not specified, current machine name is assumed to be the domain name.
          password="[AutoGenerate | password]" - Password of windows user. For special users (SYSTEM and machine), specify "AutoGenerate".
          logLevel="[All|None|Errors]" - Event types logged to the event log
          clientConnectedCheck="[HH:MM:SS]" - Time a request is left in the queue before ASP.NET does a client connected check
          comAuthenticationLevel="[Default|None|Connect|Call|Pkt|PktIntegrity|PktPrivacy]" - Level of authentication for DCOM security
          comImpersonationLevel="[Default|Anonymous|Identify|Impersonate|Delegate]" - Authentication level for COM security
          responseDeadlockInterval="[Infinite | HH:MM:SS]" - For deadlock detection, timeout for responses when there are executing requests.
          maxWorkerThreads="[number]" - Maximum number of worker threads per CPU in the thread pool
          maxIoThreads="[number]" - Maximum number of IO threads per CPU in the thread pool
          serverErrorMessageFile="[filename]" - Customization for "Server Unavailable" message
          maxAppDomains="[number]" - Maximum allowed number of app domain in one process

          When ASP.NET is running under IIS 6 in native mode, the IIS 6 process model is
          used and most settings in this section are ignored.  Please use the IIS administrative
          UI to configure things like process identity and cycling for the IIS
          worker process for the desired application
        -->
        <processModel
            enable="true"
            timeout="Infinite"
            idleTimeout="Infinite"
            shutdownTimeout="00:00:05"
            requestLimit="Infinite"
            requestQueueLimit="5000"
            restartQueueLimit="10"
            memoryLimit="60"
            webGarden="false"
            cpuMask="0xffffffff"
            userName="machine"
            password="AutoGenerate"
            logLevel="Errors"
            clientConnectedCheck="00:00:05"
            comAuthenticationLevel="Connect"
            comImpersonationLevel="Impersonate"
            responseDeadlockInterval="00:03:00"
            maxWorkerThreads="20"
            maxIoThreads="20"
            maxAppDomains="2000"
        />
    */
    public sealed class ProcessModelSection : ConfigurationSection {
        private const int DefaultMaxThreadsPerCPU = 100;

        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty(new CallbackValidator(typeof(ProcessModelSection), Validate));
        internal static TimeSpan DefaultClientConnectedCheck = new TimeSpan(0, 0, 5);

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propEnable =
            new ConfigurationProperty("enable", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propTimeout =
            new ConfigurationProperty("timeout",
                                        typeof(TimeSpan),
                                        TimeSpan.MaxValue,
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propIdleTimeout =
            new ConfigurationProperty("idleTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.MaxValue,
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propShutdownTimeout =
            new ConfigurationProperty("shutdownTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromSeconds(5),
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestLimit =
            new ConfigurationProperty("requestLimit",
                                        typeof(int),
                                        int.MaxValue,
                                        new InfiniteIntConverter(),
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestQueueLimit =
            new ConfigurationProperty("requestQueueLimit",
                                        typeof(int),
                                        5000,
                                        new InfiniteIntConverter(),
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRestartQueueLimit =
            new ConfigurationProperty("restartQueueLimit",
                                        typeof(int),
                                        10,
                                        new InfiniteIntConverter(),
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMemoryLimit =
            new ConfigurationProperty("memoryLimit", typeof(int), 60, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propWebGarden =
            new ConfigurationProperty("webGarden", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCpuMask =
            new ConfigurationProperty("cpuMask", typeof(string), "0xffffffff", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUserName =
            new ConfigurationProperty("userName", typeof(string), "machine", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPassword =
            new ConfigurationProperty("password", typeof(string), "AutoGenerate", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propLogLevel =
            new ConfigurationProperty("logLevel", typeof(ProcessModelLogLevel), ProcessModelLogLevel.Errors, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propClientConnectedCheck =
            new ConfigurationProperty("clientConnectedCheck",
                                        typeof(TimeSpan),
                                        DefaultClientConnectedCheck,
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propComAuthenticationLevel =
            new ConfigurationProperty("comAuthenticationLevel", typeof(ProcessModelComAuthenticationLevel), ProcessModelComAuthenticationLevel.Connect, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propComImpersonationLevel =
            new ConfigurationProperty("comImpersonationLevel", typeof(ProcessModelComImpersonationLevel), ProcessModelComImpersonationLevel.Impersonate, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propResponseDeadlockInterval =
            new ConfigurationProperty("responseDeadlockInterval",
                                        typeof(TimeSpan),
                                        TimeSpan.FromMinutes(3),
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propResponseRestartDeadlockInterval =
            new ConfigurationProperty("responseRestartDeadlockInterval",
                                        typeof(TimeSpan),
                                        TimeSpan.FromMinutes(3),
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        // NOTE the AutoConfig default value is different then the value shipped in Machine.config
        // This is because the Whidbey value is supposed to be true, but if the user removes the value
        // it should act like pre whidbey behavior which did not have autoconfig.
        private static readonly ConfigurationProperty _propAutoConfig =
            new ConfigurationProperty("autoConfig", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxWorkerThreads =
            new ConfigurationProperty("maxWorkerThreads",
                                        typeof(int),
                                        DefaultMaxThreadsPerCPU,
                                        null,
                                        new IntegerValidator(1, int.MaxValue - 1),
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxIOThreads =
            new ConfigurationProperty("maxIoThreads",
                                        typeof(int),
                                        DefaultMaxThreadsPerCPU,
                                        null,
                                        new IntegerValidator(1, int.MaxValue - 1),
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinWorkerThreads =
            new ConfigurationProperty("minWorkerThreads",
                                        typeof(int),
                                        1,
                                        null,
                                        new IntegerValidator(1, int.MaxValue - 1),
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinIOThreads =
            new ConfigurationProperty("minIoThreads",
                                        typeof(int),
                                        1,
                                        null,
                                        new IntegerValidator(1, int.MaxValue - 1),
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propServerErrorMessageFile =
            new ConfigurationProperty("serverErrorMessageFile", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPingFrequency =
            new ConfigurationProperty("pingFrequency",
                                        typeof(TimeSpan),
                                        TimeSpan.MaxValue,
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPingTimeout =
            new ConfigurationProperty("pingTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.MaxValue,
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxAppDomains =
            new ConfigurationProperty("maxAppDomains",
                                        typeof(int),
                                        2000,
                                        null,
                                        new IntegerValidator(1, int.MaxValue - 1),
                                        ConfigurationPropertyOptions.None);

        private static int cpuCount;
        internal const string sectionName = "system.web/processModel";

        static ProcessModelSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propEnable);
            _properties.Add(_propTimeout);
            _properties.Add(_propIdleTimeout);
            _properties.Add(_propShutdownTimeout);
            _properties.Add(_propRequestLimit);
            _properties.Add(_propRequestQueueLimit);
            _properties.Add(_propRestartQueueLimit);
            _properties.Add(_propMemoryLimit);
            _properties.Add(_propWebGarden);
            _properties.Add(_propCpuMask);
            _properties.Add(_propUserName);
            _properties.Add(_propPassword);
            _properties.Add(_propLogLevel);
            _properties.Add(_propClientConnectedCheck);
            _properties.Add(_propComAuthenticationLevel);
            _properties.Add(_propComImpersonationLevel);
            _properties.Add(_propResponseDeadlockInterval);
            _properties.Add(_propResponseRestartDeadlockInterval);
            _properties.Add(_propAutoConfig);
            _properties.Add(_propMaxWorkerThreads);
            _properties.Add(_propMaxIOThreads);
            _properties.Add(_propMinWorkerThreads);
            _properties.Add(_propMinIOThreads);
            _properties.Add(_propServerErrorMessageFile);
            _properties.Add(_propPingFrequency);
            _properties.Add(_propPingTimeout);
            _properties.Add(_propMaxAppDomains);
            cpuCount = SystemInfo.GetNumProcessCPUs();
        }

        public ProcessModelSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("enable", DefaultValue = true)]
        public bool Enable {
            get {
                return (bool)base[_propEnable];
            }
            set {
                base[_propEnable] = value;
            }
        }

        [ConfigurationProperty("timeout", DefaultValue = TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        public TimeSpan Timeout {
            get {
                return (TimeSpan)base[_propTimeout];
            }
            set {
                base[_propTimeout] = value;
            }
        }

        [ConfigurationProperty("idleTimeout", DefaultValue = TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        public TimeSpan IdleTimeout {
            get {
                return (TimeSpan)base[_propIdleTimeout];
            }
            set {
                base[_propIdleTimeout] = value;
            }
        }

        [ConfigurationProperty("shutdownTimeout", DefaultValue = "00:00:05")]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan ShutdownTimeout {
            get {
                return (TimeSpan)base[_propShutdownTimeout];
            }
            set {
                base[_propShutdownTimeout] = value;
            }
        }

        [ConfigurationProperty("requestLimit", DefaultValue = int.MaxValue)]
        [TypeConverter(typeof(InfiniteIntConverter))]
        [IntegerValidator(MinValue = 0)]
        public int RequestLimit {
            get {
                return (int)base[_propRequestLimit];
            }
            set {
                base[_propRequestLimit] = value;
            }
        }

        [ConfigurationProperty("requestQueueLimit", DefaultValue = 5000)]
        [TypeConverter(typeof(InfiniteIntConverter))]
        [IntegerValidator(MinValue = 0)]
        public int RequestQueueLimit {
            get {
                return (int)base[_propRequestQueueLimit];
            }
            set {
                base[_propRequestQueueLimit] = value;
            }
        }

        [ConfigurationProperty("restartQueueLimit", DefaultValue = 10)]
        [TypeConverter(typeof(InfiniteIntConverter))]
        [IntegerValidator(MinValue = 0)]
        public int RestartQueueLimit {
            get {
                return (int)base[_propRestartQueueLimit];
            }
            set {
                base[_propRestartQueueLimit] = value;
            }
        }

        [ConfigurationProperty("memoryLimit", DefaultValue = 60)]
        public int MemoryLimit {
            get {
                return (int)base[_propMemoryLimit];
            }
            set {
                base[_propMemoryLimit] = value;
            }
        }

        [ConfigurationProperty("webGarden", DefaultValue = false)]
        public bool WebGarden {
            get {
                return (bool)base[_propWebGarden];
            }
            set {
                base[_propWebGarden] = value;
            }
        }

        [ConfigurationProperty("cpuMask", DefaultValue = "0xffffffff")]
        public int CpuMask {
            get {
                return (int)Convert.ToInt32((string)base[_propCpuMask], 16);
            }
            set {
                base[_propCpuMask] = "0x" + Convert.ToString(value, 16);
            }
        }

        [ConfigurationProperty("userName", DefaultValue = "machine")]
        public string UserName {
            get {
                return (string)base[_propUserName];
            }
            set {
                base[_propUserName] = value;
            }
        }

        [ConfigurationProperty("password", DefaultValue = "AutoGenerate")]
        public string Password {
            get {
                return (string)base[_propPassword];
            }
            set {
                base[_propPassword] = value;
            }
        }

        [ConfigurationProperty("logLevel", DefaultValue = ProcessModelLogLevel.Errors)]
        public ProcessModelLogLevel LogLevel {
            get {
                return (ProcessModelLogLevel)base[_propLogLevel];
            }
            set {
                base[_propLogLevel] = value;
            }
        }

        [ConfigurationProperty("clientConnectedCheck", DefaultValue = "00:00:05")]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        public TimeSpan ClientConnectedCheck {
            get {
                return (TimeSpan)base[_propClientConnectedCheck];
            }
            set {
                base[_propClientConnectedCheck] = value;
            }
        }

        [ConfigurationProperty("comAuthenticationLevel", DefaultValue = ProcessModelComAuthenticationLevel.Connect)]
        public ProcessModelComAuthenticationLevel ComAuthenticationLevel {
            get {
                return (ProcessModelComAuthenticationLevel)base[_propComAuthenticationLevel];
            }
            set {
                base[_propComAuthenticationLevel] = value;
            }
        }

        [ConfigurationProperty("comImpersonationLevel", DefaultValue = ProcessModelComImpersonationLevel.Impersonate)]
        public ProcessModelComImpersonationLevel ComImpersonationLevel {
            get {
                return (ProcessModelComImpersonationLevel)base[_propComImpersonationLevel];
            }
            set {
                base[_propComImpersonationLevel] = value;
            }
        }

        [ConfigurationProperty("responseDeadlockInterval", DefaultValue = "00:03:00")]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan ResponseDeadlockInterval {
            get {
                return (TimeSpan)base[_propResponseDeadlockInterval];
            }
            set {
                base[_propResponseDeadlockInterval] = value;
            }
        }

        [ConfigurationProperty("responseRestartDeadlockInterval", DefaultValue = "00:03:00")]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]

        public TimeSpan ResponseRestartDeadlockInterval {
            get {
                return (TimeSpan)base[_propResponseRestartDeadlockInterval];
            }
            set {
                base[_propResponseRestartDeadlockInterval] = value;
            }
        }

        // NOTE the AutoConfig default value is different then the value shipped in Machine.config
        // This is because the Whidbey value is supposed to be true, but if the user removes the value
        // it should act like pre whidbey behavior which did not have autoconfig.
        [ConfigurationProperty("autoConfig", DefaultValue = false)]
        public bool AutoConfig {
            get {
                return (bool)base[_propAutoConfig];
            }
            set {
                base[_propAutoConfig] = value;
            }
        }

        [ConfigurationProperty("maxWorkerThreads", DefaultValue = 20)]
        [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue - 1)]
        public int MaxWorkerThreads {
            get {
                return (int)base[_propMaxWorkerThreads];
            }
            set {
                base[_propMaxWorkerThreads] = value;
            }
        }

        [ConfigurationProperty("maxIoThreads", DefaultValue = 20)]
        [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue - 1)]
        public int MaxIOThreads {
            get {
                return (int)base[_propMaxIOThreads];
            }
            set {
                base[_propMaxIOThreads] = value;
            }
        }

        [ConfigurationProperty("minWorkerThreads", DefaultValue = 1)]
        [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue - 1)]
        public int MinWorkerThreads {
            get {
                return (int)base[_propMinWorkerThreads];
            }
            set {
                base[_propMinWorkerThreads] = value;
            }
        }

        [ConfigurationProperty("minIoThreads", DefaultValue = 1)]
        [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue - 1)]
        public int MinIOThreads {
            get {
                return (int)base[_propMinIOThreads];
            }
            set {
                base[_propMinIOThreads] = value;
            }
        }

        [ConfigurationProperty("serverErrorMessageFile", DefaultValue = "")]
        public string ServerErrorMessageFile {
            get {
                return (string)base[_propServerErrorMessageFile];
            }
            set {
                base[_propServerErrorMessageFile] = value;
            }
        }

        [ConfigurationProperty("pingFrequency", DefaultValue = TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        public TimeSpan PingFrequency {
            get {
                return (TimeSpan)base[_propPingFrequency];
            }
            set {
                base[_propPingFrequency] = value;
            }
        }

        [ConfigurationProperty("pingTimeout", DefaultValue = TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        public TimeSpan PingTimeout {
            get {
                return (TimeSpan)base[_propPingTimeout];
            }
            set {
                base[_propPingTimeout] = value;
            }
        }

        [ConfigurationProperty("maxAppDomains", DefaultValue = 2000)]
        [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue - 1)]
        public int MaxAppDomains {
            get {
                return (int)base[_propMaxAppDomains];
            }
            set {
                base[_propMaxAppDomains] = value;
            }
        }

        internal int CpuCount {
            get {
                return cpuCount;
            }
        }

        internal int DefaultMaxWorkerThreadsForAutoConfig {
            get {
                return DefaultMaxThreadsPerCPU * cpuCount;
            }
        }

        internal int DefaultMaxIoThreadsForAutoConfig {
            get {
                return DefaultMaxThreadsPerCPU * cpuCount;
            }
        }

        internal int MaxWorkerThreadsTimesCpuCount {
            get {
                return MaxWorkerThreads * cpuCount;
            }
        }

        internal int MaxIoThreadsTimesCpuCount {
            get {
                return MaxIOThreads * cpuCount;
            }
        }

        internal int MinWorkerThreadsTimesCpuCount {
            get {
                return MinWorkerThreads * cpuCount;
            }
        }

        internal int MinIoThreadsTimesCpuCount {
            get {
                return MinIOThreads * cpuCount;
            }
        }

        protected override ConfigurationElementProperty ElementProperty {
            get {
                return s_elemProperty;
            }
        }
        private static void Validate(object value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            ProcessModelSection elem = (ProcessModelSection)value;

            int val = -1;

            try {
                val = elem.CpuMask;
            }
            catch {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_non_zero_hexadecimal_attribute, "cpuMask"),
                    elem.ElementInformation.Properties["cpuMask"].Source,
                    elem.ElementInformation.Properties["cpuMask"].LineNumber);
            }

            if (val == 0) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_non_zero_hexadecimal_attribute, "cpuMask"),
                    elem.ElementInformation.Properties["cpuMask"].Source,
                    elem.ElementInformation.Properties["cpuMask"].LineNumber);
            }
        }
    }
}
