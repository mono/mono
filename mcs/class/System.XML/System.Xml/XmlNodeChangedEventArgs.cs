
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

namespace System.Xml
{
	/// <summary>
	/// Passed to delegates on document tree changes
	/// </summary>
#if NET_2_0
	public class XmlNodeChangedEventArgs : EventArgs
#else
	public class XmlNodeChangedEventArgs
#endif
	{
		// Private data members
		XmlNode _oldParent;
		XmlNode _newParent;
		XmlNodeChangedAction _action;
		XmlNode _node;

		// public properties
		public XmlNodeChangedAction Action 
		{ 
			get
			{
				return _action;
			}
		} 

		public XmlNode Node 
		{ 
			get
			{
				return _node;
			}
		} 


		public XmlNode OldParent 
		{ 
			get
			{
				return _oldParent;
			}
		} 


		public XmlNode NewParent 
		{ 
			get
			{
				return _newParent;
			}
		} 


#if NET_2_0
		public string OldValue
		{
			get
			{
				throw new NotImplementedException ();
			}
		}
		

		public string NewValue
		{
			get
			{
				throw new NotImplementedException ();
			}
		}
#endif


		// Public Methods
		// Internal Methods
		internal XmlNodeChangedEventArgs(
			XmlNodeChangedAction action, 
			XmlNode node, 
			XmlNode oldParent,
			XmlNode newParent)
		{
			_node = node;
			_oldParent = oldParent;
			_newParent = newParent;
			_action = action;
		}

	}
}
