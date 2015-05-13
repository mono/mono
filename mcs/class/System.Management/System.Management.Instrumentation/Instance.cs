//
// AssemblyRef
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

namespace System.Management.Instrumentation
{
	[InstrumentationClass(InstrumentationType.Instance)]
	public abstract class Instance : IInstance
	{
		private ProvisionFunction publishFunction;

		private ProvisionFunction revokeFunction;

		private bool published;

		[IgnoreMember]
		public bool Published
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.published;
			}
			set
			{
				if (!this.published || value)
				{
					if (!this.published && value)
					{
						this.PublishFunction(this);
						this.published = true;
					}
					return;
				}
				else
				{
					this.RevokeFunction(this);
					this.published = false;
					return;
				}
			}
		}

		private ProvisionFunction PublishFunction
		{
			get
			{
				if (this.publishFunction == null)
				{
					this.publishFunction = Instrumentation.GetPublishFunction(this.GetType());
				}
				return this.publishFunction;
			}
		}

		private ProvisionFunction RevokeFunction
		{
			get
			{
				if (this.revokeFunction == null)
				{
					this.revokeFunction = Instrumentation.GetRevokeFunction(this.GetType());
				}
				return this.revokeFunction;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected Instance()
		{
		}
	}
}