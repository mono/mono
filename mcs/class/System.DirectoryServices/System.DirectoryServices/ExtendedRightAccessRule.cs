/******************************************************************************
* The MIT License
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace System.DirectoryServices
{
	public sealed class ExtendedRightAccessRule : ActiveDirectoryAccessRule
	{
		public ExtendedRightAccessRule (IdentityReference identity, AccessControlType type) : base(identity, 256, type, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
		{
		}

		public ExtendedRightAccessRule (IdentityReference identity, AccessControlType type, Guid extendedRightType) : base(identity, 256, type, extendedRightType, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
		{
		}

		public ExtendedRightAccessRule (IdentityReference identity, AccessControlType type, ActiveDirectorySecurityInheritance inheritanceType) : base(identity, 256, type, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
		{
		}

		public ExtendedRightAccessRule (IdentityReference identity, AccessControlType type, Guid extendedRightType, ActiveDirectorySecurityInheritance inheritanceType) : base(identity, 256, type, extendedRightType, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
		{
		}

		public ExtendedRightAccessRule (IdentityReference identity, AccessControlType type, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : base(identity, 256, type, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, inheritedObjectType)
		{
		}

		public ExtendedRightAccessRule (IdentityReference identity, AccessControlType type, Guid extendedRightType, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : base(identity, 256, type, extendedRightType, false, InheritanceFlags.None, PropagationFlags.None, inheritedObjectType)
		{
		}
	}
}
