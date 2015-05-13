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

namespace System.Management
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public abstract class ManagementOptions : ICloneable
	{
		public readonly static TimeSpan InfiniteTimeout;

		internal int flags;

		internal ManagementNamedValueCollection context;

		internal TimeSpan timeout;

		public ManagementNamedValueCollection Context
		{
			get
			{
				if (this.context != null)
				{
					return this.context;
				}
				else
				{
					ManagementNamedValueCollection managementNamedValueCollection = new ManagementNamedValueCollection();
					ManagementNamedValueCollection managementNamedValueCollection1 = managementNamedValueCollection;
					this.context = managementNamedValueCollection;
					return managementNamedValueCollection1;
				}
			}
			set
			{
				ManagementNamedValueCollection managementNamedValueCollection = this.context;
				if (value == null)
				{
					this.context = new ManagementNamedValueCollection();
				}
				else
				{
					this.context = value.Clone();
				}
				if (managementNamedValueCollection != null)
				{
					managementNamedValueCollection.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
				}
				this.context.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
				this.HandleIdentifierChange(this, null);
			}
		}

		internal int Flags
		{
			get
			{
				return this.flags;
			}
			set
			{
				this.flags = value;
			}
		}

		internal bool SendStatus
		{
			get
			{
				if ((this.Flags & 128) != 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			set
			{
				int flags;
				ManagementOptions managementOption = this;
				if (!value)
				{
					flags = this.Flags & -129;
				}
				else
				{
					flags = this.Flags | 128;
				}
				managementOption.Flags = flags;
			}
		}

		public TimeSpan Timeout
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.timeout;
			}
			set
			{
				if (value.Ticks >= (long)0)
				{
					this.timeout = value;
					this.FireIdentifierChanged();
					return;
				}
				else
				{
					throw new ArgumentOutOfRangeException("value");
				}
			}
		}

		static ManagementOptions()
		{
			ManagementOptions.InfiniteTimeout = TimeSpan.MaxValue;
		}

		internal ManagementOptions() : this(null, ManagementOptions.InfiniteTimeout)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal ManagementOptions(ManagementNamedValueCollection context, TimeSpan timeout) : this(context, timeout, 0)
		{
		}

		internal ManagementOptions(ManagementNamedValueCollection context, TimeSpan timeout, int flags)
		{
			this.flags = flags;
			if (context == null)
			{
				this.context = null;
			}
			else
			{
				this.Context = context;
			}
			this.Timeout = timeout;
		}

		public abstract object Clone();

		internal void FireIdentifierChanged()
		{
			if (this.IdentifierChanged != null)
			{
				this.IdentifierChanged(this, null);
			}
		}

		internal IWbemContext GetContext()
		{
			if (this.context == null)
			{
				return null;
			}
			else
			{
				return this.context.GetContext();
			}
		}

		internal void HandleIdentifierChange(object sender, IdentifierChangedEventArgs args)
		{
			this.FireIdentifierChanged();
		}

		internal event IdentifierChangedEventHandler IdentifierChanged;
	}
}