using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;

namespace System.Messaging
{
	/// <summary>
	/// Summary description for MessageQueue.
	/// </summary>
	public class MessageQueue : Component, IEnumerable 
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;

		private void InitializeQueue(string path, bool sharedModeDenyReceive)
		{
			// TODO: make it up
		}

		public MessageQueue(IContainer container)
		{
			container.Add(this);
			InitializeComponent();
		}

		public MessageQueue()
		{
			InitializeComponent();
		}

		public MessageQueue(string path)
		{
			InitializeComponent();
			InitializeQueue(path, false);
		}

		public MessageQueue(string path, bool sharedModeDenyReceive)
		{
			InitializeComponent();
			InitializeQueue(path, sharedModeDenyReceive);
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new Container();
		}
		#endregion

		#region IEnumerable

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException("Not yet!!!");
		}

		#endregion

	}
}
