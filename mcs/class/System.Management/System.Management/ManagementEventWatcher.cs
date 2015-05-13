//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.Management
{
	[ToolboxItem(false)]
	public class ManagementEventWatcher : Component
	{
		private ManagementScope scope;

		private EventQuery query;

		private EventWatcherOptions options;

		private IEnumWbemClassObject enumWbem;

		private IWbemClassObjectFreeThreaded[] cachedObjects;

		private uint cachedCount;

		private uint cacheIndex;

		private SinkForEventQuery sink;

		private WmiDelegateInvoker delegateInvoker;

		public EventWatcherOptions Options
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.options;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				else
				{
					EventWatcherOptions eventWatcherOption = this.options;
					this.options = (EventWatcherOptions)value.Clone();
					if (eventWatcherOption != null)
					{
						eventWatcherOption.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					}
					this.cachedObjects = new IWbemClassObjectFreeThreaded[this.options.BlockSize];
					this.options.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					this.HandleIdentifierChange(this, null);
					return;
				}
			}
		}

		public EventQuery Query
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.query;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				else
				{
					ManagementQuery managementQuery = this.query;
					this.query = (EventQuery)value.Clone();
					if (managementQuery != null)
					{
						managementQuery.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					}
					this.query.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					this.HandleIdentifierChange(this, null);
					return;
				}
			}
		}

		public ManagementScope Scope
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.scope;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				else
				{
					ManagementScope managementScope = this.scope;
					this.scope = value.Clone();
					if (managementScope != null)
					{
						managementScope.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					}
					this.scope.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					this.HandleIdentifierChange(this, null);
					return;
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementEventWatcher() : this((ManagementScope)null, (EventQuery)null, (EventWatcherOptions)null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementEventWatcher(EventQuery query) : this(null, query, null)
		{
		}

		public ManagementEventWatcher(string query) : this(null, new EventQuery(query), null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementEventWatcher(ManagementScope scope, EventQuery query) : this(scope, query, null)
		{
		}

		public ManagementEventWatcher(string scope, string query) : this(new ManagementScope(scope), new EventQuery(query), null)
		{
		}

		public ManagementEventWatcher(string scope, string query, EventWatcherOptions options) : this(new ManagementScope(scope), new EventQuery(query), options)
		{
		}

		public ManagementEventWatcher(ManagementScope scope, EventQuery query, EventWatcherOptions options)
		{
			if (scope == null)
			{
				this.scope = ManagementScope._Clone(null, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
			}
			else
			{
				this.scope = ManagementScope._Clone(scope, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
			}
			if (query == null)
			{
				this.query = new EventQuery();
			}
			else
			{
				this.query = (EventQuery)query.Clone();
			}
			this.query.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
			if (options == null)
			{
				this.options = new EventWatcherOptions();
			}
			else
			{
				this.options = (EventWatcherOptions)options.Clone();
			}
			this.options.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
			this.enumWbem = null;
			this.cachedCount = 0;
			this.cacheIndex = 0;
			this.sink = null;
			this.delegateInvoker = new WmiDelegateInvoker(this);
		}

		~ManagementEventWatcher()
		{
			try
			{
				this.Stop();
				if (this.scope != null)
				{
					this.scope.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
				}
				if (this.options != null)
				{
					this.options.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
				}
				if (this.query != null)
				{
					this.query.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
				}
			}
			finally
			{
				//base.Finalize();
			}
		}

		internal void FireEventArrived(EventArrivedEventArgs args)
		{
			try
			{
				this.delegateInvoker.FireEventToDelegates(this.EventArrived, args);
			}
			catch
			{
			}
		}

		internal void FireStopped(StoppedEventArgs args)
		{
			try
			{
				this.delegateInvoker.FireEventToDelegates(this.Stopped, args);
			}
			catch
			{
			}
		}

		private void HandleIdentifierChange(object sender, IdentifierChangedEventArgs e)
		{
			this.Stop();
		}

		private void Initialize()
		{
			if (this.query != null)
			{
				if (this.options == null)
				{
					this.Options = new EventWatcherOptions();
				}
				lock (this)
				{
					if (this.scope == null)
					{
						this.Scope = new ManagementScope();
					}
					if (this.cachedObjects == null)
					{
						this.cachedObjects = new IWbemClassObjectFreeThreaded[this.options.BlockSize];
					}
				}
				lock (this.scope)
				{
					this.scope.Initialize();
				}
				return;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		public void Start()
		{
			this.Initialize();
			this.Stop();
			SecurityHandler securityHandler = this.Scope.GetSecurityHandler();
			IWbemServices wbemServices = this.scope.GetIWbemServices();
			try
			{
				this.sink = new SinkForEventQuery(this, this.options.Context, wbemServices);
				if (this.sink.Status < 0)
				{
					Marshal.ThrowExceptionForHR(this.sink.Status);
				}
				int num = this.scope.GetSecuredIWbemServicesHandler(wbemServices).ExecNotificationQueryAsync_(this.query.QueryLanguage, this.query.QueryString, 0, this.options.GetContext(), this.sink.Stub);
				if (num < 0)
				{
					if (this.sink != null)
					{
						this.sink.ReleaseStub();
						this.sink = null;
					}
					if (((long)num & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(num);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
					}
				}
			}
			finally
			{
				securityHandler.Reset();
			}
		}

		public void Stop()
		{
			if (this.enumWbem != null)
			{
				if (Marshal.IsComObject (this.enumWbem)) {
					Marshal.ReleaseComObject(this.enumWbem);
				}
				this.enumWbem = null;
				this.FireStopped(new StoppedEventArgs(this.options.Context, 0x40006));
			}
			if (this.sink != null)
			{
				this.sink.Cancel();
				this.sink = null;
			}
		}

		public ManagementBaseObject WaitForNextEvent()
		{
			unsafe
			{
				int totalMilliseconds;
				ManagementBaseObject managementBaseObject = null;
				this.Initialize();
				lock (this)
				{
					SecurityHandler securityHandler = this.Scope.GetSecurityHandler();
					int num = 0;
					try
					{
						if (this.enumWbem == null)
						{
							num = this.scope.GetSecuredIWbemServicesHandler(this.Scope.GetIWbemServices()).ExecNotificationQuery_(this.query.QueryLanguage, this.query.QueryString, this.options.Flags, this.options.GetContext(), ref this.enumWbem);
						}
						if (num >= 0)
						{
							if (this.cachedCount - this.cacheIndex == 0)
							{
								IWbemClassObject_DoNotMarshal[] wbemClassObjectDoNotMarshalArray = new IWbemClassObject_DoNotMarshal[this.options.BlockSize];
								if (ManagementOptions.InfiniteTimeout == this.options.Timeout)
								{
									totalMilliseconds = -1;
								}
								else
								{
									TimeSpan timeout = this.options.Timeout;
									totalMilliseconds = (int)timeout.TotalMilliseconds;
								}
								int num1 = totalMilliseconds;
								num = this.scope.GetSecuredIEnumWbemClassObjectHandler(this.enumWbem).Next_(num1, this.options.BlockSize, wbemClassObjectDoNotMarshalArray, ref this.cachedCount);
								this.cacheIndex = 0;
								if (num >= 0)
								{
									if (this.cachedCount == 0)
									{
										ManagementException.ThrowWithExtendedInfo(ManagementStatus.Timedout);
									}
									for (int i = 0; (long)i < (long)this.cachedCount; i++)
									{
										this.cachedObjects[i] = new IWbemClassObjectFreeThreaded(Marshal.GetIUnknownForObject(wbemClassObjectDoNotMarshalArray[i].NativeObject));
									}
								}
							}
							if (num >= 0)
							{
								ManagementEventWatcher managementEventWatcher = this;
								managementEventWatcher.cacheIndex = managementEventWatcher.cacheIndex + 1;
							}
						}
					}
					finally
					{
						securityHandler.Reset();
					}
					if (num < 0)
					{
						if (((long)num & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(num);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
						}
					}
				}
				return managementBaseObject;
			}
		}

		public event EventArrivedEventHandler EventArrived;
		public event StoppedEventHandler Stopped;
	}
}