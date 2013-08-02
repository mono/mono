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
	public class ActiveDirectoryAccessRule : ObjectAccessRule
	{
		public ActiveDirectoryRights ActiveDirectoryRights
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySecurityInheritance InheritanceType
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectoryAccessRule(IdentityReference identity, ActiveDirectoryRights adRights, AccessControlType type) : this(identity, (int)adRights, type, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
		{
		}

		public ActiveDirectoryAccessRule(IdentityReference identity, ActiveDirectoryRights adRights, AccessControlType type, Guid objectType) : this(identity, (int)adRights, type, objectType, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
		{
		}

		public ActiveDirectoryAccessRule(IdentityReference identity, ActiveDirectoryRights adRights, AccessControlType type, ActiveDirectorySecurityInheritance inheritanceType) : this(identity, (int)adRights, type, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
		{
		}

		public ActiveDirectoryAccessRule(IdentityReference identity, ActiveDirectoryRights adRights, AccessControlType type, Guid objectType, ActiveDirectorySecurityInheritance inheritanceType) : this(identity, (int)adRights, type, objectType, false, InheritanceFlags.None, PropagationFlags.None, Guid.Empty)
		{
		}

		public ActiveDirectoryAccessRule(IdentityReference identity, ActiveDirectoryRights adRights, AccessControlType type, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : this(identity, (int)adRights, type, Guid.Empty, false, InheritanceFlags.None, PropagationFlags.None, inheritedObjectType)
		{
		}

		public ActiveDirectoryAccessRule(IdentityReference identity, ActiveDirectoryRights adRights, AccessControlType type, Guid objectType, ActiveDirectorySecurityInheritance inheritanceType, Guid inheritedObjectType) : this(identity, (int)adRights, type, objectType, false, InheritanceFlags.None, PropagationFlags.None, inheritedObjectType)
		{
		}

		internal ActiveDirectoryAccessRule(IdentityReference identity, int accessMask, AccessControlType type, Guid objectType, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, Guid inheritedObjectType) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, objectType, inheritedObjectType, type)
		{
		}
	}
}
