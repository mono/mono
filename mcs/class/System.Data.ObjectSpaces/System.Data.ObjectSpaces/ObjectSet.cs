//
// System.Data.ObjectSpaces.ObjectSet.cs 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.ComponentModel;
using System.Data.ObjectSpaces.Schema;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.ObjectSpaces
{
        public class ObjectSet : CollectionBase, IListSource, IXmlSerializable
        {        
		#region Fields

		ObjectContext context;

		#endregion // Fields

		#region Constructors

                [MonoTODO]                
		public ObjectSet (Type t, ObjectSchema oschema)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public ObjectSet ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public object this [int i] {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		bool IListSource.ContainsListCollection {
			get { throw new NotImplementedException(); }
		}
		 
		internal ObjectContext ObjectContext {
			get { return context; }
		}

		#endregion // Properties

		#region Methods

                [MonoTODO]
		public void Add (object o)
		{
			throw new NotImplementedException ();
		}

                [MonoTODO]
		public void Add (ICollection c)
		{
			throw new NotImplementedException ();
		}

                [MonoTODO]
		public void Add (object o, ObjectState state)
		{
			throw new NotImplementedException ();
		}

                [MonoTODO]
		public void GetRemotingDiffGram (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IList IListSource.GetList ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		XmlSchema IXmlSerializable.GetSchema()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnInsertComplete (int index, object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnRemoveComplete (int index, object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnSetComplete (int index, object oldValue, object newValue)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void StartTracking (Object o, InitialState state)
		{
			throw new NotImplementedException();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
