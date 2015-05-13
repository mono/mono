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
	public class ManagementObjectSearcher : Component
	{
		private ManagementScope scope;

		private ObjectQuery query;

		private EnumerationOptions options;

		public EnumerationOptions Options
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
					this.options = (EnumerationOptions)value.Clone();
					return;
				}
			}
		}

		public ObjectQuery Query
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
					this.query = (ObjectQuery)value.Clone();
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
					this.scope = value.Clone();
					return;
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectSearcher() : this((ManagementScope)null, (ObjectQuery)null, (EnumerationOptions)null)
		{
		}

		public ManagementObjectSearcher(string queryString) : this(null, new ObjectQuery(queryString), null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectSearcher(ObjectQuery query) : this(null, query, null)
		{
		}

		public ManagementObjectSearcher(string scope, string queryString) : this(new ManagementScope(scope), new ObjectQuery(queryString), null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementObjectSearcher(ManagementScope scope, ObjectQuery query) : this(scope, query, null)
		{
		}

		public ManagementObjectSearcher(string scope, string queryString, EnumerationOptions options) : this(new ManagementScope(scope), new ObjectQuery(queryString), options)
		{
		}

		public ManagementObjectSearcher(ManagementScope scope, ObjectQuery query, EnumerationOptions options)
		{
			this.scope = ManagementScope._Clone(scope);
			if (query == null)
			{
				this.query = new ObjectQuery();
			}
			else
			{
				this.query = (ObjectQuery)query.Clone();
			}
			if (options == null)
			{
				this.options = new EnumerationOptions();
				return;
			}
			else
			{
				this.options = (EnumerationOptions)options.Clone();
				return;
			}
		}

		public ManagementObjectCollection Get()
		{
			this.Initialize();
			IEnumWbemClassObject enumWbemClassObject = null;
			SecurityHandler securityHandler = this.scope.GetSecurityHandler();
			EnumerationOptions enumerationOption = (EnumerationOptions)this.options.Clone();
			int num = 0;
			try
			{
				try
				{
					if (!(this.query.GetType() == typeof(SelectQuery)) || ((SelectQuery)this.query).Condition != null || ((SelectQuery)this.query).SelectedProperties != null || !this.options.EnumerateDeep)
					{
						enumerationOption.EnumerateDeep = true;
						num = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).ExecQuery_(this.query.QueryLanguage, this.query.QueryString, enumerationOption.Flags, enumerationOption.GetContext(), ref enumWbemClassObject);
					}
					else
					{
						enumerationOption.EnsureLocatable = false;
						enumerationOption.PrototypeOnly = false;
						if (((SelectQuery)this.query).IsSchemaQuery)
						{
							num = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).CreateClassEnum_(((SelectQuery)this.query).ClassName, enumerationOption.Flags, enumerationOption.GetContext(), ref enumWbemClassObject);
						}
						else
						{
							num = this.scope.GetSecuredIWbemServicesHandler(this.scope.GetIWbemServices()).CreateInstanceEnum_(((SelectQuery)this.query).ClassName, enumerationOption.Flags, enumerationOption.GetContext(), ref enumWbemClassObject);
						}
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					ManagementException.ThrowWithExtendedInfo(cOMException);
				}
			}
			finally
			{
				securityHandler.Reset();
			}
			if (((long)num & (long)-4096) != (long)-2147217408)
			{
				if (((long)num & (long)-2147483648) != (long)0)
				{
					Marshal.ThrowExceptionForHR(num);
				}
			}
			else
			{
				ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
			}
			return new ManagementObjectCollection(this.scope, this.options, enumWbemClassObject);
		}

		public void Get(ManagementOperationObserver watcher)
		{
			if (watcher != null)
			{
				this.Initialize();
				IWbemServices wbemServices = this.scope.GetIWbemServices();
				EnumerationOptions enumerationOption = (EnumerationOptions)this.options.Clone();
				enumerationOption.ReturnImmediately = false;
				if (watcher.HaveListenersForProgress)
				{
					enumerationOption.SendStatus = true;
				}
				WmiEventSink newSink = watcher.GetNewSink(this.scope, enumerationOption.Context);
				SecurityHandler securityHandler = this.scope.GetSecurityHandler();
				int num = 0;
				try
				{
					try
					{
						if (!(this.query.GetType() == typeof(SelectQuery)) || ((SelectQuery)this.query).Condition != null || ((SelectQuery)this.query).SelectedProperties != null || !this.options.EnumerateDeep)
						{
							enumerationOption.EnumerateDeep = true;
							num = this.scope.GetSecuredIWbemServicesHandler(wbemServices).ExecQueryAsync_(this.query.QueryLanguage, this.query.QueryString, enumerationOption.Flags, enumerationOption.GetContext(), newSink.Stub);
						}
						else
						{
							enumerationOption.EnsureLocatable = false;
							enumerationOption.PrototypeOnly = false;
							if (((SelectQuery)this.query).IsSchemaQuery)
							{
								num = this.scope.GetSecuredIWbemServicesHandler(wbemServices).CreateClassEnumAsync_(((SelectQuery)this.query).ClassName, enumerationOption.Flags, enumerationOption.GetContext(), newSink.Stub);
							}
							else
							{
								num = this.scope.GetSecuredIWbemServicesHandler(wbemServices).CreateInstanceEnumAsync_(((SelectQuery)this.query).ClassName, enumerationOption.Flags, enumerationOption.GetContext(), newSink.Stub);
							}
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						watcher.RemoveSink(newSink);
						ManagementException.ThrowWithExtendedInfo(cOMException);
					}
				}
				finally
				{
					securityHandler.Reset();
				}
				if (((long)num & (long)-4096) != (long)-2147217408)
				{
					if (((long)num & (long)-2147483648) != (long)0)
					{
						Marshal.ThrowExceptionForHR(num);
					}
					return;
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("watcher");
			}
		}

		private void Initialize()
		{
			if (this.query != null)
			{
				lock (this)
				{
					if (this.scope == null)
					{
						this.scope = ManagementScope._Clone(null);
					}
				}
				lock (this.scope)
				{
					if (!this.scope.IsConnected)
					{
						this.scope.Initialize();
					}
				}
				return;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}
	}
}