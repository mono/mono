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
	[TypeConverter(typeof(ManagementScopeConverter))]
	public class ManagementScope : ICloneable
	{
		private ManagementPath validatedPath;

		private IWbemServices wbemServices;

		private ConnectionOptions options;

		internal bool IsDefaulted;

		public bool IsConnected
		{
			get
			{
				return null != this.wbemServices;
			}
		}

		public ConnectionOptions Options
		{
			get
			{
				if (this.options != null)
				{
					return this.options;
				}
				else
				{
					ConnectionOptions connectionOption = ConnectionOptions._Clone(null);
					ConnectionOptions connectionOption1 = connectionOption;
					this.options = connectionOption;
					return connectionOption1;
				}
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				else
				{
					if (this.options != null)
					{
						this.options.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					}
					this.options = ConnectionOptions._Clone(value, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
					this.HandleIdentifierChange(this, null);
					return;
				}
			}
		}

		public ManagementPath Path
		{
			get
			{
				if (this.prvpath != null)
				{
					return this.prvpath;
				}
				else
				{
					ManagementPath managementPath = ManagementPath._Clone(null);
					ManagementPath managementPath1 = managementPath;
					this.prvpath = managementPath;
					return managementPath1;
				}
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				else
				{
					if (this.prvpath != null)
					{
						this.prvpath.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
					}
					this.IsDefaulted = false;
					this.prvpath = ManagementPath._Clone(value, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
					this.HandleIdentifierChange(this, null);
					return;
				}
			}
		}

		private ManagementPath prvpath
		{
			get
			{
				return this.validatedPath;
			}
			set
			{
				if (value != null)
				{
					string path = value.Path;
					if (!ManagementPath.IsValidNamespaceSyntax(path))
					{
						ManagementException.ThrowWithExtendedInfo(ManagementStatus.InvalidNamespace);
					}
				}
				this.validatedPath = value;
			}
		}

		public ManagementScope() : this(new ManagementPath(ManagementPath.DefaultPath.Path))
		{
			this.IsDefaulted = true;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementScope(ManagementPath path) : this(path, (ConnectionOptions)null)
		{
		}

		public ManagementScope(string path) : this(new ManagementPath(path), (ConnectionOptions)null)
		{
		}

		public ManagementScope(string path, ConnectionOptions options) : this(new ManagementPath(path), options)
		{
		}

		public ManagementScope(ManagementPath path, ConnectionOptions options)
		{
			if (path == null)
			{
				this.prvpath = ManagementPath._Clone(null);
			}
			else
			{
				this.prvpath = ManagementPath._Clone(path, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
			}
			if (options == null)
			{
				this.options = null;
			}
			else
			{
				this.options = ConnectionOptions._Clone(options, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
			}
			this.IsDefaulted = false;
		}

		internal ManagementScope(ManagementPath path, IWbemServices wbemServices, ConnectionOptions options)
		{
			if (path != null)
			{
				this.Path = path;
			}
			if (options != null)
			{
				this.Options = options;
			}
			this.wbemServices = wbemServices;
		}

		internal ManagementScope(ManagementPath path, ManagementScope scope) : this(path, (scope != null ? scope.options : null))
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal static ManagementScope _Clone(ManagementScope scope)
		{
			return ManagementScope._Clone(scope, null);
		}

		internal static ManagementScope _Clone(ManagementScope scope, IdentifierChangedEventHandler handler)
		{
			ManagementScope managementScope = new ManagementScope(null, null, null);
			if (handler == null)
			{
				if (scope != null)
				{
					managementScope.IdentifierChanged = new IdentifierChangedEventHandler(scope.HandleIdentifierChange);
				}
			}
			else
			{
				managementScope.IdentifierChanged = handler;
			}
			if (scope != null)
			{
				if (scope.prvpath != null)
				{
					managementScope.prvpath = ManagementPath._Clone(scope.prvpath, new IdentifierChangedEventHandler(managementScope.HandleIdentifierChange));
					managementScope.IsDefaulted = scope.IsDefaulted;
				}
				else
				{
					managementScope.prvpath = ManagementPath._Clone(ManagementPath.DefaultPath, new IdentifierChangedEventHandler(managementScope.HandleIdentifierChange));
					managementScope.IsDefaulted = true;
				}
				managementScope.wbemServices = scope.wbemServices;
				if (scope.options != null)
				{
					managementScope.options = ConnectionOptions._Clone(scope.options, new IdentifierChangedEventHandler(managementScope.HandleIdentifierChange));
				}
			}
			else
			{
				managementScope.prvpath = ManagementPath._Clone(ManagementPath.DefaultPath, new IdentifierChangedEventHandler(managementScope.HandleIdentifierChange));
				managementScope.IsDefaulted = true;
				managementScope.wbemServices = null;
				managementScope.options = null;
			}
			return managementScope;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagementScope Clone()
		{
			return ManagementScope._Clone(this);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Connect()
		{
			this.Initialize();
		}

		private void FireIdentifierChanged()
		{
			if (this.IdentifierChanged != null)
			{
				this.IdentifierChanged(this, null);
			}
		}

		internal IWbemServices GetIWbemServices()
		{
			IWbemServices wbemService = this.wbemServices;
			IntPtr unknownForObject = Marshal.GetIUnknownForObject(this.wbemServices);
			object objectForIUnknown = Marshal.GetObjectForIUnknown(unknownForObject);
			Marshal.Release(unknownForObject);
			if (!object.ReferenceEquals(objectForIUnknown, this.wbemServices))
			{
				SecurityHandler securityHandler = this.GetSecurityHandler();
				securityHandler.SecureIUnknown(objectForIUnknown);
				wbemService = (IWbemServices)objectForIUnknown;
				securityHandler.Secure(wbemService);
			}
			return wbemService;
		}

		internal SecuredConnectHandler GetSecuredConnectHandler()
		{
			return new SecuredConnectHandler(this);
		}

		internal SecuredIEnumWbemClassObjectHandler GetSecuredIEnumWbemClassObjectHandler(IEnumWbemClassObject pEnumWbemClassObject)
		{
			return new SecuredIEnumWbemClassObjectHandler(this, pEnumWbemClassObject);
		}

		internal SecuredIWbemServicesHandler GetSecuredIWbemServicesHandler(IWbemServices pWbemServiecs)
		{
			return new SecuredIWbemServicesHandler(this, pWbemServiecs);
		}

		internal SecurityHandler GetSecurityHandler()
		{
			return new SecurityHandler(this);
		}

		private void HandleIdentifierChange(object sender, IdentifierChangedEventArgs args)
		{
			this.wbemServices = null;
			this.FireIdentifierChanged();
		}

		internal void Initialize()
		{
			if (this.prvpath != null)
			{
				if (!this.IsConnected)
				{
					lock (this)
					{
						if (!this.IsConnected)
						{
							if (MTAHelper.IsNoContextMTA())
							{
								this.InitializeGuts(this);
							}
							else
							{
								ThreadDispatch threadDispatch = new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethodWithParam(this.InitializeGuts));
								threadDispatch.Parameter = this;
								threadDispatch.Start();
							}
						}
					}
				}
				return;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private void InitializeGuts(object o)
		{
			bool flag = false;
			ManagementScope connectionOption = (ManagementScope)o;
			if (this.options == null)
			{
				connectionOption.Options = new ConnectionOptions();
			}
			string namespacePath = connectionOption.prvpath.GetNamespacePath(8);
			if (namespacePath == null || namespacePath.Length == 0)
			{
				namespacePath = connectionOption.prvpath.SetNamespacePath(ManagementPath.DefaultPath.Path, out flag);
			}
			int num = 0;
			connectionOption.wbemServices = null;
			if (Environment.OSVersion.Platform == PlatformID.Win32NT && (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1 || Environment.OSVersion.Version.Major >= 6))
			{
				ConnectionOptions flags = connectionOption.options;
				flags.Flags = flags.Flags | 128;
			}
			try
			{
				connectionOption.options.GetPassword();
				num = this.GetSecuredConnectHandler().ConnectNSecureIWbemServices(namespacePath, ref connectionOption.wbemServices);
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				ManagementException.ThrowWithExtendedInfo(cOMException);
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

		/*
		[DllImport("rpcrt4.dll", CharSet=CharSet.None)]
		private static extern int RpcMgmtEnableIdleCleanup();
		*/

		private static int RpcMgmtEnableIdleCleanup ()
		{
			return 0;
		}


		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		object System.ICloneable.Clone()
		{
			return this.Clone();
		}

		internal event IdentifierChangedEventHandler IdentifierChanged;
	}
}