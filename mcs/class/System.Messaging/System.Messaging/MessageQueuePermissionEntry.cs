//
// System.Messaging.MessageQueuePermissionEntry.cs
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

namespace System.Messaging {

	[Serializable]
	public class MessageQueuePermissionEntry {

		private MessageQueuePermissionAccess _permissionAccess;
		private string _machineName;
		private string _label;
		private string _category;
		private string _path;

		public MessageQueuePermissionEntry (MessageQueuePermissionAccess permissionAccess, string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			MessageQueuePermission.ValidatePath (path);

			_permissionAccess = permissionAccess;
			_path = path;
		}
		
		public MessageQueuePermissionEntry (MessageQueuePermissionAccess permissionAccess, string machineName, string label, string category)
		{
			if (machineName == null)
				throw new ArgumentNullException ("machineName");
			MessageQueuePermission.ValidateMachineName (machineName);

			_permissionAccess = permissionAccess;
			_machineName = machineName;
			_label = label;
			_category = category;
		}
		
		public string Category {
			get { return _category; }
		}
		
		public string Label {
			get { return _label; }
		}

		public string MachineName {
			get { return _machineName; }
		}
		
		public string Path {
			get { return _path; }
		}
		
		public MessageQueuePermissionAccess PermissionAccess {
			get { return _permissionAccess; }
		}
	}
}
