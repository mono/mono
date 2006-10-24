//
// TypeAttributes.cs
//
// Author:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2006 Jonathan Pryor
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

namespace Mono.Unix.Native {

	[AttributeUsage (AttributeTargets.Field)]
	internal class blkcnt_tAttribute : MapAttribute {
		
		public blkcnt_tAttribute () : base ("blkcnt_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class blksize_tAttribute : MapAttribute {
		
		public blksize_tAttribute () : base ("blksize_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class dev_tAttribute : MapAttribute {
		
		public dev_tAttribute () : base ("dev_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class gid_tAttribute : MapAttribute {
		
		public gid_tAttribute () : base ("gid_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class fsblkcnt_tAttribute : MapAttribute {
		
		public fsblkcnt_tAttribute () : base ("fsblkcnt_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class fsfilcnt_tAttribute : MapAttribute {
		
		public fsfilcnt_tAttribute () : base ("fsfilcnt_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class ino_tAttribute : MapAttribute {
		
		public ino_tAttribute () : base ("ino_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class nlink_tAttribute : MapAttribute {
		
		public nlink_tAttribute () : base ("nlink_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class off_tAttribute : MapAttribute {
		
		public off_tAttribute () : base ("off_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class pid_tAttribute : MapAttribute {
		
		public pid_tAttribute () : base ("pid_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class suseconds_tAttribute : MapAttribute {
		
		public suseconds_tAttribute () : base ("suseconds_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class uid_tAttribute : MapAttribute {
		
		public uid_tAttribute () : base ("uid_t")
		{
		}
	}

	[AttributeUsage (AttributeTargets.Field)]
	internal class time_tAttribute : MapAttribute {
		
		public time_tAttribute () : base ("time_t")
		{
		}
	}
}

