//
// System.Web.Configuration.HttpRuntimeSection
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

#if NET_2_0

using System;
using System.ComponentModel;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class HttpRuntimeSection : ConfigurationSection
	{
		static ConfigurationProperty apartmentThreadingProp;
		static ConfigurationProperty appRequestQueueLimitProp;
		static ConfigurationProperty delayNotificationTimeoutProp;
		static ConfigurationProperty enableProp;
		static ConfigurationProperty enableHeaderCheckingProp;
		static ConfigurationProperty enableKernelOutputCacheProp;
		static ConfigurationProperty enableVersionHeaderProp;
		static ConfigurationProperty executionTimeoutProp;
		static ConfigurationProperty maxRequestLengthProp;
		static ConfigurationProperty maxWaitChangeNotificationProp;
		static ConfigurationProperty minFreeThreadsProp;
		static ConfigurationProperty minLocalRequestFreeThreadsProp;
		static ConfigurationProperty requestLengthDiskThresholdProp;
		static ConfigurationProperty requireRootedSaveAsPathProp;
		static ConfigurationProperty sendCacheControlHeaderProp;
		static ConfigurationProperty shutdownTimeoutProp;
		static ConfigurationProperty useFullyQualifiedRedirectUrlProp;
		static ConfigurationProperty waitChangeNotificationProp;
#if NET_4_0
		static ConfigurationProperty requestPathInvalidCharactersProp;
		static ConfigurationProperty requestValidationTypeProp;
		static ConfigurationProperty requestValidationModeProp;
		static ConfigurationProperty maxQueryStringLengthProp;
		static ConfigurationProperty maxUrlLengthProp;
		static ConfigurationProperty encoderTypeProp;
		static ConfigurationProperty relaxedUrlToFileSystemMappingProp;
#endif
		static ConfigurationPropertyCollection properties;

		static HttpRuntimeSection ()
		{
			apartmentThreadingProp = new ConfigurationProperty ("apartmentThreading", typeof (bool), false);
			appRequestQueueLimitProp = new ConfigurationProperty ("appRequestQueueLimit", typeof (int), 5000,
									      TypeDescriptor.GetConverter (typeof (int)),
									      new IntegerValidator (1, Int32.MaxValue),
									      ConfigurationPropertyOptions.None);
			delayNotificationTimeoutProp = new ConfigurationProperty ("delayNotificationTimeout", typeof (TimeSpan), TimeSpan.FromSeconds (5),
										  PropertyHelper.TimeSpanSecondsConverter,
										  PropertyHelper.DefaultValidator,
										  ConfigurationPropertyOptions.None);
			enableProp = new ConfigurationProperty ("enable", typeof (bool), true);
			enableHeaderCheckingProp = new ConfigurationProperty ("enableHeaderChecking", typeof (bool), true);
			enableKernelOutputCacheProp = new ConfigurationProperty ("enableKernelOutputCache", typeof (bool), true);
			enableVersionHeaderProp = new ConfigurationProperty ("enableVersionHeader", typeof (bool), true);
			executionTimeoutProp = new ConfigurationProperty ("executionTimeout", typeof (TimeSpan), TimeSpan.FromSeconds (110),
									  PropertyHelper.TimeSpanSecondsConverter,
									  PropertyHelper.PositiveTimeSpanValidator,
									  ConfigurationPropertyOptions.None);
			maxRequestLengthProp = new ConfigurationProperty ("maxRequestLength", typeof (int), 4096,
									  TypeDescriptor.GetConverter (typeof (int)),
									  PropertyHelper.IntFromZeroToMaxValidator,
									  ConfigurationPropertyOptions.None);
			maxWaitChangeNotificationProp = new ConfigurationProperty ("maxWaitChangeNotification", typeof (int), 0,
										   TypeDescriptor.GetConverter (typeof (int)),
										   PropertyHelper.IntFromZeroToMaxValidator,
										   ConfigurationPropertyOptions.None);
			minFreeThreadsProp = new ConfigurationProperty ("minFreeThreads", typeof (int), 8,
									TypeDescriptor.GetConverter (typeof (int)),
									PropertyHelper.IntFromZeroToMaxValidator,
									ConfigurationPropertyOptions.None);
			minLocalRequestFreeThreadsProp = new ConfigurationProperty ("minLocalRequestFreeThreads", typeof (int), 4,
										    TypeDescriptor.GetConverter (typeof (int)),
										    PropertyHelper.IntFromZeroToMaxValidator,
										    ConfigurationPropertyOptions.None);
			requestLengthDiskThresholdProp = new ConfigurationProperty ("requestLengthDiskThreshold", typeof (int), 80,
										    TypeDescriptor.GetConverter (typeof (int)),
										    new IntegerValidator (1, Int32.MaxValue),
										    ConfigurationPropertyOptions.None);
			requireRootedSaveAsPathProp = new ConfigurationProperty ("requireRootedSaveAsPath", typeof (bool), true);
			sendCacheControlHeaderProp = new ConfigurationProperty ("sendCacheControlHeader", typeof (bool), true);
			shutdownTimeoutProp = new ConfigurationProperty ("shutdownTimeout", typeof (TimeSpan), TimeSpan.FromSeconds (90),
									 PropertyHelper.TimeSpanSecondsConverter,
									 PropertyHelper.DefaultValidator,
									 ConfigurationPropertyOptions.None);
			useFullyQualifiedRedirectUrlProp = new ConfigurationProperty ("useFullyQualifiedRedirectUrl", typeof (bool), false);
			waitChangeNotificationProp = new ConfigurationProperty ("waitChangeNotification", typeof (int), 0,
										TypeDescriptor.GetConverter (typeof (int)),
										PropertyHelper.IntFromZeroToMaxValidator,
										ConfigurationPropertyOptions.None);
#if NET_4_0
			requestPathInvalidCharactersProp = new ConfigurationProperty ("requestPathInvalidCharacters", typeof (string), ",*,%,&,:,\\,?");
			requestValidationTypeProp = new ConfigurationProperty ("requestValidationType", typeof (string),"System.Web.Util.RequestValidator",
									       TypeDescriptor.GetConverter (typeof (string)),
									       PropertyHelper.NonEmptyStringValidator,
									       ConfigurationPropertyOptions.None);
			requestValidationModeProp = new ConfigurationProperty ("requestValidationMode", typeof (Version), new Version (4, 0),
									       PropertyHelper.VersionConverter,
									       PropertyHelper.DefaultValidator,
									       ConfigurationPropertyOptions.None);
			maxQueryStringLengthProp = new ConfigurationProperty ("maxQueryStringLength", typeof (int), 2048,
									      TypeDescriptor.GetConverter (typeof (int)),
									      PropertyHelper.IntFromZeroToMaxValidator,
									      ConfigurationPropertyOptions.None);
			maxUrlLengthProp = new ConfigurationProperty ("maxUrlLength", typeof (int), 260,
								      TypeDescriptor.GetConverter (typeof (int)),
								      PropertyHelper.IntFromZeroToMaxValidator,
								      ConfigurationPropertyOptions.None);
			encoderTypeProp = new ConfigurationProperty ("encoderType", typeof (string), "System.Web.Util.HttpEncoder",
								     TypeDescriptor.GetConverter (typeof (string)),
								     PropertyHelper.NonEmptyStringValidator,
								     ConfigurationPropertyOptions.None);
			relaxedUrlToFileSystemMappingProp = new ConfigurationProperty ("relaxedUrlToFileSystemMapping", typeof (bool), false);
#endif
			
			properties = new ConfigurationPropertyCollection();
			properties.Add (apartmentThreadingProp);
			properties.Add (appRequestQueueLimitProp);
			properties.Add (delayNotificationTimeoutProp);
			properties.Add (enableProp);
			properties.Add (enableHeaderCheckingProp);
			properties.Add (enableKernelOutputCacheProp);
			properties.Add (enableVersionHeaderProp);
			properties.Add (executionTimeoutProp);
			properties.Add (maxRequestLengthProp);
			properties.Add (maxWaitChangeNotificationProp);
			properties.Add (minFreeThreadsProp);
			properties.Add (minLocalRequestFreeThreadsProp);
			properties.Add (requestLengthDiskThresholdProp);
			properties.Add (requireRootedSaveAsPathProp);
			properties.Add (sendCacheControlHeaderProp);
			properties.Add (shutdownTimeoutProp);
			properties.Add (useFullyQualifiedRedirectUrlProp);
			properties.Add (waitChangeNotificationProp);
#if NET_4_0
			properties.Add (requestPathInvalidCharactersProp);
			properties.Add (requestValidationTypeProp);
			properties.Add (requestValidationModeProp);
			properties.Add (maxQueryStringLengthProp);
			properties.Add (maxUrlLengthProp);
			properties.Add (encoderTypeProp);
			properties.Add (relaxedUrlToFileSystemMappingProp);
#endif
		}

		public HttpRuntimeSection()
		{
		}

		[ConfigurationProperty ("apartmentThreading", DefaultValue = "False")]
		public bool ApartmentThreading {
			get { return (bool) base[apartmentThreadingProp]; }
			set { base[apartmentThreadingProp] = value; }
		}

		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("appRequestQueueLimit", DefaultValue = "5000")]
		public int AppRequestQueueLimit {
			get { return (int) base[appRequestQueueLimitProp]; }
			set { base[appRequestQueueLimitProp] = value; }
		}

		[TypeConverter (typeof (TimeSpanSecondsConverter))]
		[ConfigurationProperty ("delayNotificationTimeout", DefaultValue = "00:00:05")]
		public TimeSpan DelayNotificationTimeout {
			get { return (TimeSpan) base[delayNotificationTimeoutProp]; }
			set { base[delayNotificationTimeoutProp] = value; }
		}

		[ConfigurationProperty ("enable", DefaultValue = "True")]
		public bool Enable {
			get { return (bool) base[enableProp]; }
			set { base[enableProp] = value; }
		}

		[ConfigurationProperty ("enableHeaderChecking", DefaultValue = "True")]
		public bool EnableHeaderChecking {
			get { return (bool) base[enableHeaderCheckingProp]; }
			set { base[enableHeaderCheckingProp] = value; }
		}

		[ConfigurationProperty ("enableKernelOutputCache", DefaultValue = "True")]
		public bool EnableKernelOutputCache {
			get { return (bool) base[enableKernelOutputCacheProp]; }
			set { base[enableKernelOutputCacheProp] = value; }
		}

		[ConfigurationProperty ("enableVersionHeader", DefaultValue = "True")]
		public bool EnableVersionHeader {
			get { return (bool) base[enableVersionHeaderProp]; }
			set { base[enableVersionHeaderProp] = value; }
		}

		[TypeConverter (typeof (TimeSpanSecondsConverter))]
		[TimeSpanValidator (MinValueString = "00:00:00")]
		[ConfigurationProperty ("executionTimeout", DefaultValue = "00:01:50")]
		public TimeSpan ExecutionTimeout {
			get { return (TimeSpan) base[executionTimeoutProp]; }
			set { base[executionTimeoutProp] = value; }
		}

		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("maxRequestLength", DefaultValue = "4096")]
		public int MaxRequestLength {
			get { return (int) base[maxRequestLengthProp]; }
			set { base[maxRequestLengthProp] = value; }
		}

		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("maxWaitChangeNotification", DefaultValue = "0")]
		public int MaxWaitChangeNotification {
			get { return (int) base[maxWaitChangeNotificationProp]; }
			set { base[maxWaitChangeNotificationProp] = value; }
		}

		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("minFreeThreads", DefaultValue = "8")]
		public int MinFreeThreads {
			get { return (int) base[minFreeThreadsProp]; }
			set { base[minFreeThreadsProp] = value; }
		}

		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("minLocalRequestFreeThreads", DefaultValue = "4")]
		public int MinLocalRequestFreeThreads {
			get { return (int) base[minLocalRequestFreeThreadsProp]; }
			set { base[minLocalRequestFreeThreadsProp] = value; }
		}

		[IntegerValidator (MinValue = 1, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("requestLengthDiskThreshold", DefaultValue = "80")]
		public int RequestLengthDiskThreshold {
			get { return (int) base[requestLengthDiskThresholdProp]; }
			set { base[requestLengthDiskThresholdProp] = value; }
		}

		[ConfigurationProperty ("requireRootedSaveAsPath", DefaultValue = "True")]
		public bool RequireRootedSaveAsPath {
			get { return (bool) base[requireRootedSaveAsPathProp]; }
			set { base[requireRootedSaveAsPathProp] = value; }
		}

		[ConfigurationProperty ("sendCacheControlHeader", DefaultValue = "True")]
		public bool SendCacheControlHeader {
			get { return (bool) base[sendCacheControlHeaderProp]; }
			set { base[sendCacheControlHeaderProp] = value; }
		}

		[TypeConverter (typeof (TimeSpanSecondsConverter))]
		[ConfigurationProperty ("shutdownTimeout", DefaultValue = "00:01:30")]
		public TimeSpan ShutdownTimeout {
			get { return (TimeSpan) base[shutdownTimeoutProp]; }
			set { base[shutdownTimeoutProp] = value; }
		}

		[ConfigurationProperty ("useFullyQualifiedRedirectUrl", DefaultValue = "False")]
		public bool UseFullyQualifiedRedirectUrl {
			get { return (bool) base[useFullyQualifiedRedirectUrlProp]; }
			set { base[useFullyQualifiedRedirectUrlProp] = value; }
		}

		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("waitChangeNotification", DefaultValue = "0")]
		public int WaitChangeNotification {
			get { return (int) base[waitChangeNotificationProp]; }
			set { base[waitChangeNotificationProp] = value; }
		}

#if NET_4_0
		[ConfigurationProperty ("requestPathInvalidCharacters", DefaultValue = ",*,%,&,:,\\,?")]
		public string RequestPathInvalidCharacters {
			get { return (string) base [requestPathInvalidCharactersProp]; }
			set { base [requestPathInvalidCharactersProp] = value; }
		}

		[ConfigurationProperty ("requestValidationType", DefaultValue = "System.Web.Util.RequestValidator")]
		[StringValidator (MinLength = 1)]
		public string RequestValidationType {
			get { return (string) base [requestValidationTypeProp]; }
			set { base [requestValidationTypeProp] = value; }
		}

		[ConfigurationProperty ("requestValidationMode", DefaultValue = "4.0")]
		[TypeConverter ("System.Web.Configuration.VersionConverter")]
		public Version RequestValidationMode {
			get { return (Version) base [requestValidationModeProp]; }
			set { base [requestValidationModeProp] = value; }
		}

		[IntegerValidator (MinValue = 0)]
		[ConfigurationProperty ("maxQueryStringLength", DefaultValue = "2048")]
		public int MaxQueryStringLength {
			get { return (int) base [maxQueryStringLengthProp]; }
			set { base [maxQueryStringLengthProp] = value; }
		}

		[IntegerValidator (MinValue = 0)]
		[ConfigurationProperty ("maxUrlLength", DefaultValue = "260")]
		public int MaxUrlLength {
			get { return (int) base [maxUrlLengthProp]; }
			set { base [maxUrlLengthProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("encoderType", DefaultValue = "System.Web.Util.HttpEncoder")]
		public string EncoderType {
			get { return (string) base [encoderTypeProp]; }
			set { base [encoderTypeProp] = value; }
		}

		[ConfigurationProperty ("relaxedUrlToFileSystemMapping", DefaultValue = "False")]
		public bool RelaxedUrlToFileSystemMapping {
			get { return (bool) base [relaxedUrlToFileSystemMappingProp]; }
			set { base [relaxedUrlToFileSystemMappingProp] = value; }
		}
#endif
		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
