using System;

namespace System.Xml
{
	/// <summary>
	/// Passed to delegates on document tree changes
	/// </summary>
	public class XmlNodeChangedEventArgs
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


#if NET_1_2
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
