// 
// System.Web.Services.Description.MessageCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class MessageCollection : ServiceDescriptionBaseCollection {

		#region Constructors
		
		internal MessageCollection (ServiceDescription serviceDescription)
			: base (serviceDescription)
		{
		}

		#endregion

		#region Properties

		public Message this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (Message) List [index]; 
			}
                        set { List [index] = value; }
		}

		public Message this [string name] {
			get { return this [IndexOf ((Message) Table [name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (Message message) 
		{
			Insert (Count, message);
			return (Count - 1);
		}

		public bool Contains (Message message)
		{
			return List.Contains (message);
		}

		public void CopyTo (Message[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is Message))
				throw new InvalidCastException ();

			return ((Message) value).Name;
		}

		public int IndexOf (Message message)
		{
			return List.IndexOf (message);
		}

		public void Insert (int index, Message message)
		{
			List.Insert (index, message);
		}
	
		public void Remove (Message message)
		{
			List.Remove (message);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((Message) value).SetParent ((ServiceDescription) parent);
		}
			
		#endregion // Methods
	}
}
