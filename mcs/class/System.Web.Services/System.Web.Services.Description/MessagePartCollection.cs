// 
// System.Web.Services.Description.MessagePartCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class MessagePartCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal MessagePartCollection (Message message)
			: base (message)
		{
		}	

		#endregion

		#region Properties

		public MessagePart this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (MessagePart) List[index]; 
			}
                        set { List [index] = value; }
		}

		public MessagePart this [string name] {
			get { return this [IndexOf ((MessagePart) Table[name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (MessagePart messagePart) 
		{
			Insert (Count, messagePart);
			return (Count - 1);
		}

		public bool Contains (MessagePart messagePart)
		{
			return List.Contains (messagePart);
		}

		public void CopyTo (MessagePart[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is MessagePart))
				throw new InvalidCastException ();
			return ((MessagePart) value).Name;
		}

		public int IndexOf (MessagePart messagePart)
		{
			return List.IndexOf (messagePart);
		}

		public void Insert (int index, MessagePart messagePart)
		{
			List.Insert (index, messagePart);
		}
	
		public void Remove (MessagePart messagePart)
		{
			List.Remove (messagePart);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((MessagePart) value).SetParent ((Message) parent);
		}
			
		#endregion // Methods
	}
}
