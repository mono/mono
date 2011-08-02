//
// System.Web.Configuration.ProcessModelSection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.ComponentModel;
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class ProcessModelSection : ConfigurationSection
	{
		static ConfigurationProperty autoConfigProp;
		static ConfigurationProperty clientConnectedCheckProp;
		static ConfigurationProperty comAuthenticationLevelProp;
		static ConfigurationProperty comImpersonationLevelProp;
		static ConfigurationProperty cpuMaskProp;
		static ConfigurationProperty enableProp;
		static ConfigurationProperty idleTimeoutProp;
		static ConfigurationProperty logLevelProp;
		static ConfigurationProperty maxAppDomainsProp;
		static ConfigurationProperty maxIoThreadsProp;
		static ConfigurationProperty maxWorkerThreadsProp;
		static ConfigurationProperty memoryLimitProp;
		static ConfigurationProperty minIoThreadsProp;
		static ConfigurationProperty minWorkerThreadsProp;
		static ConfigurationProperty passwordProp;
		static ConfigurationProperty pingFrequencyProp;
		static ConfigurationProperty pingTimeoutProp;
		static ConfigurationProperty requestLimitProp;
		static ConfigurationProperty requestQueueLimitProp;
		static ConfigurationProperty responseDeadlockIntervalProp;
		static ConfigurationProperty responseRestartDeadlockIntervalProp;
		static ConfigurationProperty restartQueueLimitProp;
		static ConfigurationProperty serverErrorMessageFileProp;
		static ConfigurationProperty shutdownTimeoutProp;
		static ConfigurationProperty timeoutProp;
		static ConfigurationProperty userNameProp;
		static ConfigurationProperty webGardenProp;
		static ConfigurationPropertyCollection properties;

		static ConfigurationElementProperty elementProperty;

		static ProcessModelSection ()
		{
			autoConfigProp = new ConfigurationProperty ("autoConfig", typeof (bool), false);
			clientConnectedCheckProp = new ConfigurationProperty ("clientConnectedCheck", typeof (TimeSpan), TimeSpan.FromSeconds (5),
									      PropertyHelper.InfiniteTimeSpanConverter,
									      PropertyHelper.DefaultValidator,
									      ConfigurationPropertyOptions.None);
			comAuthenticationLevelProp = new ConfigurationProperty ("comAuthenticationLevel", typeof (ProcessModelComAuthenticationLevel), ProcessModelComAuthenticationLevel.Connect,
										new GenericEnumConverter (typeof (ProcessModelComAuthenticationLevel)),
										PropertyHelper.DefaultValidator,
										ConfigurationPropertyOptions.None);
			comImpersonationLevelProp = new ConfigurationProperty ("comImpersonationLevel", typeof (ProcessModelComImpersonationLevel), ProcessModelComImpersonationLevel.Impersonate,
									       new GenericEnumConverter (typeof (ProcessModelComImpersonationLevel)),
									       PropertyHelper.DefaultValidator,
									       ConfigurationPropertyOptions.None);
			cpuMaskProp = new ConfigurationProperty ("cpuMask", typeof (int), (int) (int.MaxValue & 0xfffffff));
			enableProp = new ConfigurationProperty ("enable", typeof (bool), true);
			idleTimeoutProp = new ConfigurationProperty ("idleTimeout", typeof (TimeSpan), TimeSpan.MaxValue,
								     PropertyHelper.InfiniteTimeSpanConverter,
								     PropertyHelper.DefaultValidator,
								     ConfigurationPropertyOptions.None);
			logLevelProp = new ConfigurationProperty ("logLevel", typeof (ProcessModelLogLevel), ProcessModelLogLevel.Errors,
								  new GenericEnumConverter (typeof (ProcessModelLogLevel)),
								  PropertyHelper.DefaultValidator,
								  ConfigurationPropertyOptions.None);
			maxAppDomainsProp = new ConfigurationProperty ("maxAppDomains", typeof (int), 2000,
								       TypeDescriptor.GetConverter (typeof (int)),
								       PropertyHelper.IntFromOneToMax_1Validator,
								       ConfigurationPropertyOptions.None);
			maxIoThreadsProp = new ConfigurationProperty ("maxIoThreads", typeof (int), 20,
								       TypeDescriptor.GetConverter (typeof (int)),
								       PropertyHelper.IntFromOneToMax_1Validator,
								       ConfigurationPropertyOptions.None);
			maxWorkerThreadsProp = new ConfigurationProperty ("maxWorkerThreads", typeof (int), 20,
									  TypeDescriptor.GetConverter (typeof (int)),
									  PropertyHelper.IntFromOneToMax_1Validator,
									  ConfigurationPropertyOptions.None);
			memoryLimitProp = new ConfigurationProperty ("memoryLimit", typeof (int), 60);
			minIoThreadsProp = new ConfigurationProperty ("minIoThreads", typeof (int), 1,
								      TypeDescriptor.GetConverter (typeof (int)),
								      PropertyHelper.IntFromOneToMax_1Validator,
								      ConfigurationPropertyOptions.None);
			minWorkerThreadsProp = new ConfigurationProperty ("minWorkerThreads", typeof (int), 1,
									  TypeDescriptor.GetConverter (typeof (int)),
									  PropertyHelper.IntFromOneToMax_1Validator,
									  ConfigurationPropertyOptions.None);
			passwordProp = new ConfigurationProperty ("password", typeof (string), "AutoGenerate");
			pingFrequencyProp = new ConfigurationProperty ("pingFrequency", typeof (TimeSpan), TimeSpan.MaxValue,
								       PropertyHelper.InfiniteTimeSpanConverter,
								       PropertyHelper.DefaultValidator,
								       ConfigurationPropertyOptions.None);
			pingTimeoutProp = new ConfigurationProperty ("pingTimeout", typeof (TimeSpan), TimeSpan.MaxValue,
								     PropertyHelper.InfiniteTimeSpanConverter,
								     PropertyHelper.DefaultValidator,
								     ConfigurationPropertyOptions.None);
			requestLimitProp = new ConfigurationProperty ("requestLimit", typeof (int), Int32.MaxValue,
								      PropertyHelper.InfiniteIntConverter,
								      PropertyHelper.IntFromZeroToMaxValidator,
								      ConfigurationPropertyOptions.None);
			requestQueueLimitProp = new ConfigurationProperty ("requestQueueLimit", typeof (int), 5000,
								      PropertyHelper.InfiniteIntConverter,
								      PropertyHelper.IntFromZeroToMaxValidator,
								      ConfigurationPropertyOptions.None);
			responseDeadlockIntervalProp = new ConfigurationProperty ("responseDeadlockInterval", typeof (TimeSpan), TimeSpan.FromMinutes (3),
										  PropertyHelper.InfiniteTimeSpanConverter,
										  PropertyHelper.PositiveTimeSpanValidator,
										  ConfigurationPropertyOptions.None);
			responseRestartDeadlockIntervalProp = new ConfigurationProperty ("responseRestartDeadlockInterval", typeof (TimeSpan), TimeSpan.FromMinutes (3),
											 PropertyHelper.InfiniteTimeSpanConverter,
											 PropertyHelper.DefaultValidator,
											 ConfigurationPropertyOptions.None);
			restartQueueLimitProp = new ConfigurationProperty ("restartQueueLimit", typeof (int), 10,
									   PropertyHelper.InfiniteIntConverter,
									   PropertyHelper.IntFromZeroToMaxValidator,
									   ConfigurationPropertyOptions.None);
			serverErrorMessageFileProp = new ConfigurationProperty ("serverErrorMessageFile", typeof (string), "");
			shutdownTimeoutProp = new ConfigurationProperty ("shutdownTimeout", typeof (TimeSpan), TimeSpan.FromSeconds (5),
									 PropertyHelper.InfiniteTimeSpanConverter,
									 PropertyHelper.PositiveTimeSpanValidator,
									 ConfigurationPropertyOptions.None);
			timeoutProp = new ConfigurationProperty ("timeout", typeof (TimeSpan), TimeSpan.MaxValue,
								 PropertyHelper.InfiniteTimeSpanConverter,
								 PropertyHelper.DefaultValidator,
								 ConfigurationPropertyOptions.None);
			userNameProp = new ConfigurationProperty ("userName", typeof (string), "machine");
			webGardenProp = new ConfigurationProperty ("webGarden", typeof (bool), false);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (autoConfigProp);
			properties.Add (clientConnectedCheckProp);
			properties.Add (comAuthenticationLevelProp);
			properties.Add (comImpersonationLevelProp);
			properties.Add (cpuMaskProp);
			properties.Add (enableProp);
			properties.Add (idleTimeoutProp);
			properties.Add (logLevelProp);
			properties.Add (maxAppDomainsProp);
			properties.Add (maxIoThreadsProp);
			properties.Add (maxWorkerThreadsProp);
			properties.Add (memoryLimitProp);
			properties.Add (minIoThreadsProp);
			properties.Add (minWorkerThreadsProp);
			properties.Add (passwordProp);
			properties.Add (pingFrequencyProp);
			properties.Add (pingTimeoutProp);
			properties.Add (requestLimitProp);
			properties.Add (requestQueueLimitProp);
			properties.Add (responseDeadlockIntervalProp);
			properties.Add (responseRestartDeadlockIntervalProp);
			properties.Add (restartQueueLimitProp);
			properties.Add (serverErrorMessageFileProp);
			properties.Add (shutdownTimeoutProp);
			properties.Add (timeoutProp);
			properties.Add (userNameProp);
			properties.Add (webGardenProp);

			elementProperty = new ConfigurationElementProperty (new CallbackValidator (typeof (ProcessModelSection), ValidateElement));
		}

		static void ValidateElement (object o)
		{
			/* XXX do some sort of element validation here? */
		}

		protected internal override ConfigurationElementProperty ElementProperty {
			get { return elementProperty; }
		}

		[ConfigurationProperty ("autoConfig", DefaultValue = "False")]
		public bool AutoConfig {
			get { return (bool) base [autoConfigProp];}
			set { base[autoConfigProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[ConfigurationProperty ("clientConnectedCheck", DefaultValue = "00:00:05")]
		public TimeSpan ClientConnectedCheck {
			get { return (TimeSpan) base [clientConnectedCheckProp];}
			set { base[clientConnectedCheckProp] = value; }
		}

		[ConfigurationProperty ("comAuthenticationLevel", DefaultValue = "Connect")]
		public ProcessModelComAuthenticationLevel ComAuthenticationLevel {
			get { return (ProcessModelComAuthenticationLevel) base [comAuthenticationLevelProp];}
			set { base[comAuthenticationLevelProp] = value; }
		}

		[ConfigurationProperty ("comImpersonationLevel", DefaultValue = "Impersonate")]
		public ProcessModelComImpersonationLevel ComImpersonationLevel {
			get { return (ProcessModelComImpersonationLevel) base [comImpersonationLevelProp];}
			set { base[comImpersonationLevelProp] = value; }
		}

		[ConfigurationProperty ("cpuMask", DefaultValue = "0xffffffff")]
		public int CpuMask {
			get { return (int) base [cpuMaskProp];}
			set { base[cpuMaskProp] = value; }
		}

		[ConfigurationProperty ("enable", DefaultValue = "True")]
		public bool Enable {
			get { return (bool) base [enableProp];}
			set { base[enableProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[ConfigurationProperty ("idleTimeout", DefaultValue = "10675199.02:48:05.4775807")]
		public TimeSpan IdleTimeout {
			get { return (TimeSpan) base [idleTimeoutProp];}
			set { base[idleTimeoutProp] = value; }
		}

		[ConfigurationProperty ("logLevel", DefaultValue = "Errors")]
		public ProcessModelLogLevel LogLevel {
			get { return (ProcessModelLogLevel) base [logLevelProp];}
			set { base[logLevelProp] = value; }
		}

		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue - 1)]
		[ConfigurationProperty ("maxAppDomains", DefaultValue = "2000")]
		public int MaxAppDomains {
			get { return (int) base [maxAppDomainsProp];}
			set { base[maxAppDomainsProp] = value; }
		}

		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue - 1)]
		[ConfigurationProperty ("maxIoThreads", DefaultValue = "20")]
		public int MaxIOThreads {
			get { return (int) base [maxIoThreadsProp];}
			set { base[maxIoThreadsProp] = value; }
		}

		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue - 1)]
		[ConfigurationProperty ("maxWorkerThreads", DefaultValue = "20")]
		public int MaxWorkerThreads {
			get { return (int) base [maxWorkerThreadsProp];}
			set { base[maxWorkerThreadsProp] = value; }
		}

		[ConfigurationProperty ("memoryLimit", DefaultValue = "60")]
		public int MemoryLimit {
			get { return (int) base [memoryLimitProp];}
			set { base[memoryLimitProp] = value; }
		}

		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue - 1)]
		[ConfigurationProperty ("minIoThreads", DefaultValue = "1")]
		public int MinIOThreads {
			get { return (int) base [minIoThreadsProp];}
			set { base[minIoThreadsProp] = value; }
		}

		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue- 1)]
		[ConfigurationProperty ("minWorkerThreads", DefaultValue = "1")]
		public int MinWorkerThreads {
			get { return (int) base [minWorkerThreadsProp];}
			set { base[minWorkerThreadsProp] = value; }
		}

		[ConfigurationProperty ("password", DefaultValue = "AutoGenerate")]
		public string Password {
			get { return (string) base [passwordProp];}
			set { base[passwordProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[ConfigurationProperty ("pingFrequency", DefaultValue = "10675199.02:48:05.4775807")]
		public TimeSpan PingFrequency {
			get { return (TimeSpan) base [pingFrequencyProp];}
			set { base[pingFrequencyProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[ConfigurationProperty ("pingTimeout", DefaultValue = "10675199.02:48:05.4775807")]
		public TimeSpan PingTimeout {
			get { return (TimeSpan) base [pingTimeoutProp];}
			set { base[pingTimeoutProp] = value; }
		}

		[TypeConverter (typeof (InfiniteIntConverter))]
		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("requestLimit", DefaultValue = "2147483647")]
		public int RequestLimit {
			get { return (int) base [requestLimitProp];}
			set { base[requestLimitProp] = value; }
		}

		[TypeConverter (typeof (InfiniteIntConverter))]
		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("requestQueueLimit", DefaultValue = "5000")]
		public int RequestQueueLimit {
			get { return (int) base [requestQueueLimitProp];}
			set { base[requestQueueLimitProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[TimeSpanValidator (MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
		[ConfigurationProperty ("responseDeadlockInterval", DefaultValue = "00:03:00")]
		public TimeSpan ResponseDeadlockInterval {
			get { return (TimeSpan) base [responseDeadlockIntervalProp];}
			set { base[responseDeadlockIntervalProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[ConfigurationProperty ("responseRestartDeadlockInterval", DefaultValue = "00:03:00")]
		public TimeSpan ResponseRestartDeadlockInterval {
			get { return (TimeSpan) base [responseRestartDeadlockIntervalProp];}
			set { base[responseRestartDeadlockIntervalProp] = value; }
		}

		[TypeConverter (typeof (InfiniteIntConverter))]
		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("restartQueueLimit", DefaultValue = "10")]
		public int RestartQueueLimit {
			get { return (int) base [restartQueueLimitProp];}
			set { base[restartQueueLimitProp] = value; }
		}

		[ConfigurationProperty ("serverErrorMessageFile", DefaultValue = "")]
		public string ServerErrorMessageFile {
			get { return (string) base [serverErrorMessageFileProp];}
			set { base[serverErrorMessageFileProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[TimeSpanValidator (MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
		[ConfigurationProperty ("shutdownTimeout", DefaultValue = "00:00:05")]
		public TimeSpan ShutdownTimeout {
			get { return (TimeSpan) base [shutdownTimeoutProp];}
			set { base[shutdownTimeoutProp] = value; }
		}

		[TypeConverter (typeof (InfiniteTimeSpanConverter))]
		[ConfigurationProperty ("timeout", DefaultValue = "10675199.02:48:05.4775807")]
		public TimeSpan Timeout {
			get { return (TimeSpan) base [timeoutProp];}
			set { base[timeoutProp] = value; }
		}

		[ConfigurationProperty ("userName", DefaultValue = "machine")]
		public string UserName {
			get { return (string) base [userNameProp];}
			set { base[userNameProp] = value; }
		}

		[ConfigurationProperty ("webGarden", DefaultValue = "False")]
		public bool WebGarden {
			get { return (bool) base [webGardenProp];}
			set { base[webGardenProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

