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
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Management
{
	public class ConnectionOptions : ManagementOptions
	{
		private string locale;

		private string username;

		private SecureString securePassword;

		private string authority;

		private ImpersonationLevel impersonation;

		private AuthenticationLevel authentication;

		private bool enablePrivileges;

		internal const string DEFAULTLOCALE = null;

		internal const string DEFAULTAUTHORITY = null;

		internal const ImpersonationLevel DEFAULTIMPERSONATION = ImpersonationLevel.Impersonate;

		internal const AuthenticationLevel DEFAULTAUTHENTICATION = AuthenticationLevel.Unchanged;

		internal const bool DEFAULTENABLEPRIVILEGES = false;

		public AuthenticationLevel Authentication
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.authentication;
			}
			set
			{
				if (this.authentication != value)
				{
					this.authentication = value;
					base.FireIdentifierChanged();
				}
			}
		}

		public string Authority
		{
			get
			{
				if (this.authority != null)
				{
					return this.authority;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				if (this.authority != value)
				{
					this.authority = value;
					base.FireIdentifierChanged();
				}
			}
		}

		public bool EnablePrivileges
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.enablePrivileges;
			}
			set
			{
				if (this.enablePrivileges != value)
				{
					this.enablePrivileges = value;
					base.FireIdentifierChanged();
				}
			}
		}

		public ImpersonationLevel Impersonation
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.impersonation;
			}
			set
			{
				if (this.impersonation != value)
				{
					this.impersonation = value;
					base.FireIdentifierChanged();
				}
			}
		}

		public string Locale
		{
			get
			{
				if (this.locale != null)
				{
					return this.locale;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				if (this.locale != value)
				{
					this.locale = value;
					base.FireIdentifierChanged();
				}
			}
		}

		public string Password
		{
			set
			{
				if (value == null)
				{
					if (this.securePassword != null)
					{
						this.securePassword.Dispose();
						this.securePassword = null;
						base.FireIdentifierChanged();
					}
					return;
				}
				else
				{
					if (this.securePassword != null)
					{
						SecureString secureString = new SecureString();
						for (int i = 0; i < value.Length; i++)
						{
							secureString.AppendChar(value[i]);
						}
						this.securePassword.Clear();
						this.securePassword = secureString.Copy();
						base.FireIdentifierChanged();
						secureString.Dispose();
						return;
					}
					else
					{
						this.securePassword = new SecureString();
						for (int j = 0; j < value.Length; j++)
						{
							this.securePassword.AppendChar(value[j]);
						}
						return;
					}
				}
			}
		}

		public SecureString SecurePassword
		{
			set
			{
				if (value == null)
				{
					if (this.securePassword != null)
					{
						this.securePassword.Dispose();
						this.securePassword = null;
						base.FireIdentifierChanged();
					}
					return;
				}
				else
				{
					if (this.securePassword != null)
					{
						this.securePassword.Clear();
						this.securePassword = value.Copy();
						base.FireIdentifierChanged();
						return;
					}
					else
					{
						this.securePassword = value.Copy();
						return;
					}
				}
			}
		}

		public string Username
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.username;
			}
			set
			{
				if (this.username != value)
				{
					this.username = value;
					base.FireIdentifierChanged();
				}
			}
		}

		public ConnectionOptions() : this((string)null, (string)null, (string)null, (string)null, (ImpersonationLevel)3, (AuthenticationLevel)(-1), false, (ManagementNamedValueCollection)null, ManagementOptions.InfiniteTimeout)
		{
		}

		public ConnectionOptions(string locale, string username, string password, string authority, ImpersonationLevel impersonation, AuthenticationLevel authentication, bool enablePrivileges, ManagementNamedValueCollection context, TimeSpan timeout) : base(context, timeout)
		{
			if (locale != null)
			{
				this.locale = locale;
			}
			this.username = username;
			this.enablePrivileges = enablePrivileges;
			if (password != null)
			{
				this.securePassword = new SecureString();
				for (int i = 0; i < password.Length; i++)
				{
					this.securePassword.AppendChar(password[i]);
				}
			}
			if (authority != null)
			{
				this.authority = authority;
			}
			if (impersonation != ImpersonationLevel.Default)
			{
				this.impersonation = impersonation;
			}
			if (authentication != AuthenticationLevel.Default)
			{
				this.authentication = authentication;
			}
		}

		public ConnectionOptions(string locale, string username, SecureString password, string authority, ImpersonationLevel impersonation, AuthenticationLevel authentication, bool enablePrivileges, ManagementNamedValueCollection context, TimeSpan timeout) : base(context, timeout)
		{
			if (locale != null)
			{
				this.locale = locale;
			}
			this.username = username;
			this.enablePrivileges = enablePrivileges;
			if (password != null)
			{
				this.securePassword = password.Copy();
			}
			if (authority != null)
			{
				this.authority = authority;
			}
			if (impersonation != ImpersonationLevel.Default)
			{
				this.impersonation = impersonation;
			}
			if (authentication != AuthenticationLevel.Default)
			{
				this.authentication = authentication;
			}
		}

		internal ConnectionOptions(ManagementNamedValueCollection context, TimeSpan timeout, int flags) : base(context, timeout, flags)
		{
		}

		internal ConnectionOptions(ManagementNamedValueCollection context) : base(context, ManagementOptions.InfiniteTimeout)
		{
		}

		internal static ConnectionOptions _Clone(ConnectionOptions options)
		{
			return ConnectionOptions._Clone(options, null);
		}

		internal static ConnectionOptions _Clone(ConnectionOptions options, IdentifierChangedEventHandler handler)
		{
			ConnectionOptions connectionOption;
			if (options == null)
			{
				connectionOption = new ConnectionOptions();
			}
			else
			{
				connectionOption = new ConnectionOptions(options.Context, options.Timeout, options.Flags);
				connectionOption.locale = options.locale;
				connectionOption.username = options.username;
				connectionOption.enablePrivileges = options.enablePrivileges;
				if (options.securePassword == null)
				{
					connectionOption.securePassword = null;
				}
				else
				{
					connectionOption.securePassword = options.securePassword.Copy();
				}
				if (options.authority != null)
				{
					connectionOption.authority = options.authority;
				}
				if (options.impersonation != ImpersonationLevel.Default)
				{
					connectionOption.impersonation = options.impersonation;
				}
				if (options.authentication != AuthenticationLevel.Default)
				{
					connectionOption.authentication = options.authentication;
				}
			}
			if (handler == null)
			{
				if (options != null)
				{
					connectionOption.IdentifierChanged += new IdentifierChangedEventHandler(options.HandleIdentifierChange);
				}
			}
			else
			{
				connectionOption.IdentifierChanged += handler;
			}
			return connectionOption;
		}

		public override object Clone()
		{
			ManagementNamedValueCollection managementNamedValueCollection = null;
			if (base.Context != null)
			{
				managementNamedValueCollection = base.Context.Clone();
			}
			return new ConnectionOptions(this.locale, this.username, this.GetSecurePassword(), this.authority, this.impersonation, this.authentication, this.enablePrivileges, managementNamedValueCollection, base.Timeout);
		}

		internal IntPtr GetPassword()
		{
			IntPtr bSTR;
			if (this.securePassword == null)
			{
				return IntPtr.Zero;
			}
			else
			{
				try
				{
					bSTR = Marshal.SecureStringToBSTR(this.securePassword);
				}
				catch (OutOfMemoryException outOfMemoryException)
				{
					bSTR = IntPtr.Zero;
				}
				return bSTR;
			}
		}

		internal SecureString GetSecurePassword()
		{
			if (this.securePassword == null)
			{
				return null;
			}
			else
			{
				return this.securePassword.Copy();
			}
		}
	}
}