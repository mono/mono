// 
// System.Web.Services.Description.Message.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Web.Services;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public sealed class Message : DocumentableItem {

		#region Fields

		string name;
		MessagePartCollection parts;
		ServiceDescription serviceDescription;

		#endregion // Fields

		#region Constructors
		
		public Message ()
		{
			name = String.Empty;
			parts = new MessagePartCollection (this);
			serviceDescription = null;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("name", DataType = "NCName")]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[XmlElement ("part")]
		public MessagePartCollection Parts {
			get { return parts; }
		}

		[XmlIgnore]
		public ServiceDescription ServiceDescription {
			get { return serviceDescription; }
		}

		#endregion // Properties

		#region Methods

		public MessagePart FindPartByName (string partName)
		{
			return parts [partName];
		}

		public MessagePart[] FindPartsByName (string[] partNames) 
		{
			ArrayList searchResults = new ArrayList ();

			foreach (string partName in partNames)
				searchResults.Add (FindPartByName (partName));

			int count = searchResults.Count;

			if (count == 0)
				throw new ArgumentException ();

			MessagePart[] returnValue = new MessagePart[count];
			searchResults.CopyTo (returnValue);
			return returnValue;
		}

		internal void SetParent (ServiceDescription serviceDescription)
		{
			this.serviceDescription = serviceDescription;
		}

		#endregion
	}
}
