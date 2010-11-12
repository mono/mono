//
// System.Messaging.MessageQueuePermissionAttribute.cs
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Peter Van Isacker
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Security;
using System.Security.Permissions;

namespace System.Messaging {
	
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct |
		AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Event,
		AllowMultiple=true, Inherited=false)]
	[Serializable]
	public class MessageQueuePermissionAttribute: CodeAccessSecurityAttribute {

		private MessageQueuePermissionAccess _permissionAccess;
		private string _machineName;
		private string _label;
		private string _category;
		private string _path;

		public MessageQueuePermissionAttribute (SecurityAction action)
			:base (action)
		{
		}
		
		public string Category {
			get { return _category; }
			set {
				if (value == null)
					throw new InvalidOperationException ("null");
				_category = value;
			}
		}
		
		public string Label {
			get { return _label; }
			set {
				if (value == null)
					throw new InvalidOperationException ("null");
				_label = value;
			}
		}
		
		public string MachineName {
			get { return _machineName; }
			set {
				if (value == null)
					throw new InvalidOperationException ("null");
				MessageQueuePermission.ValidateMachineName (value);
				_machineName = value;
			}
		}
		
		public string Path {
			get { return _path; }
			set {
				if (value == null)
					throw new InvalidOperationException ("null");
				MessageQueuePermission.ValidatePath (value);
				_path = value;
			}
		}
		
		public MessageQueuePermissionAccess PermissionAccess {
			get { return _permissionAccess; }
			set { _permissionAccess = value; }
		}
		
		public override IPermission CreatePermission ()
		{
			if (base.Unrestricted)
				return new MessageQueuePermission (PermissionState.Unrestricted);
			else
				return new MessageQueuePermission (_permissionAccess, _machineName, _label, _category);
		}
	}
}
