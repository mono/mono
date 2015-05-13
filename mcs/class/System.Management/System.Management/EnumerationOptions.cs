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

namespace System.Management
{
	public class EnumerationOptions : ManagementOptions
	{
		private int blockSize;

		public int BlockSize
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.blockSize;
			}
			set
			{
				if (value > 0)
				{
					this.blockSize = value;
					return;
				}
				else
				{
					throw new ArgumentOutOfRangeException("value");
				}
			}
		}

		public bool DirectRead
		{
			get
			{
				if ((base.Flags & 0x200) != 0)
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
				EnumerationOptions enumerationOption = this;
				if (value)
				{
					flags = base.Flags | 0x200;
				}
				else
				{
					flags = base.Flags & -513;
				}
				enumerationOption.Flags = flags;
			}
		}

		public bool EnsureLocatable
		{
			get
			{
				if ((base.Flags & 0x100) != 0)
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
				EnumerationOptions enumerationOption = this;
				if (value)
				{
					flags = base.Flags | 0x100;
				}
				else
				{
					flags = base.Flags & -257;
				}
				enumerationOption.Flags = flags;
			}
		}

		public bool EnumerateDeep
		{
			get
			{
				if ((base.Flags & 1) != 0)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			set
			{
				int flags;
				EnumerationOptions enumerationOption = this;
				if (!value)
				{
					flags = base.Flags | 1;
				}
				else
				{
					flags = base.Flags & -2;
				}
				enumerationOption.Flags = flags;
			}
		}

		public bool PrototypeOnly
		{
			get
			{
				if ((base.Flags & 2) != 0)
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
				EnumerationOptions enumerationOption = this;
				if (value)
				{
					flags = base.Flags | 2;
				}
				else
				{
					flags = base.Flags & -3;
				}
				enumerationOption.Flags = flags;
			}
		}

		public bool ReturnImmediately
		{
			get
			{
				if ((base.Flags & 16) != 0)
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
				EnumerationOptions enumerationOption = this;
				if (!value)
				{
					flags = base.Flags & -17;
				}
				else
				{
					flags = base.Flags | 16;
				}
				enumerationOption.Flags = flags;
			}
		}

		public bool Rewindable
		{
			get
			{
				if ((base.Flags & 32) != 0)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			set
			{
				int flags;
				EnumerationOptions enumerationOption = this;
				if (value)
				{
					flags = base.Flags & -33;
				}
				else
				{
					flags = base.Flags | 32;
				}
				enumerationOption.Flags = flags;
			}
		}

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
				EnumerationOptions enumerationOption = this;
				if (value)
				{
					flags = base.Flags | 0x20000;
				}
				else
				{
					flags = base.Flags & -131073;
				}
				enumerationOption.Flags = flags;
			}
		}

		public EnumerationOptions() : this(null, ManagementOptions.InfiniteTimeout, 1, true, true, false, false, false, false, false)
		{
		}

		public EnumerationOptions(ManagementNamedValueCollection context, TimeSpan timeout, int blockSize, bool rewindable, bool returnImmediatley, bool useAmendedQualifiers, bool ensureLocatable, bool prototypeOnly, bool directRead, bool enumerateDeep) : base(context, timeout)
		{
			this.BlockSize = blockSize;
			this.Rewindable = rewindable;
			this.ReturnImmediately = returnImmediatley;
			this.UseAmendedQualifiers = useAmendedQualifiers;
			this.EnsureLocatable = ensureLocatable;
			this.PrototypeOnly = prototypeOnly;
			this.DirectRead = directRead;
			this.EnumerateDeep = enumerateDeep;
		}

		public override object Clone()
		{
			ManagementNamedValueCollection managementNamedValueCollection = null;
			if (base.Context != null)
			{
				managementNamedValueCollection = base.Context.Clone();
			}
			return new EnumerationOptions(managementNamedValueCollection, base.Timeout, this.blockSize, this.Rewindable, this.ReturnImmediately, this.UseAmendedQualifiers, this.EnsureLocatable, this.PrototypeOnly, this.DirectRead, this.EnumerateDeep);
		}
	}
}