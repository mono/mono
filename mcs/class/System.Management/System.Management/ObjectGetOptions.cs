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

namespace System.Management
{
	public class ObjectGetOptions : ManagementOptions
	{
		public bool UseAmendedQualifiers
		{
			get
			{
				if ((base.Flags & 0x20000) != 0)
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
				ObjectGetOptions objectGetOption = this;
				if (value)
				{
					flags = base.Flags | 0x20000;
				}
				else
				{
					flags = base.Flags & -131073;
				}
				objectGetOption.Flags = flags;
				base.FireIdentifierChanged();
			}
		}

		public ObjectGetOptions() : this(null, ManagementOptions.InfiniteTimeout, false)
		{
		}

		public ObjectGetOptions(ManagementNamedValueCollection context) : this(context, ManagementOptions.InfiniteTimeout, false)
		{
		}

		public ObjectGetOptions(ManagementNamedValueCollection context, TimeSpan timeout, bool useAmendedQualifiers) : base(context, timeout)
		{
			this.UseAmendedQualifiers = useAmendedQualifiers;
		}

		internal static ObjectGetOptions _Clone(ObjectGetOptions options)
		{
			return ObjectGetOptions._Clone(options, null);
		}

		internal static ObjectGetOptions _Clone(ObjectGetOptions options, IdentifierChangedEventHandler handler)
		{
			ObjectGetOptions objectGetOption;
			if (options == null)
			{
				objectGetOption = new ObjectGetOptions();
			}
			else
			{
				objectGetOption = new ObjectGetOptions(options.context, options.timeout, options.UseAmendedQualifiers);
			}
			if (handler == null)
			{
				if (options != null)
				{
					objectGetOption.IdentifierChanged += new IdentifierChangedEventHandler(options.HandleIdentifierChange);
				}
			}
			else
			{
				objectGetOption.IdentifierChanged += handler;
			}
			return objectGetOption;
		}

		public override object Clone()
		{
			ManagementNamedValueCollection managementNamedValueCollection = null;
			if (base.Context != null)
			{
				managementNamedValueCollection = base.Context.Clone();
			}
			return new ObjectGetOptions(managementNamedValueCollection, base.Timeout, this.UseAmendedQualifiers);
		}
	}
}