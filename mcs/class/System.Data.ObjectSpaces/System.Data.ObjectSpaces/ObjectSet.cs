//
// System.Data.ObjectSpaces.ObjectSet.cs 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003-2004
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

		DynamicAssembly da;
		ObjectContext context;
		ObjectSchema os;

		#endregion // Fields

		#region Constructors

		public ObjectSet (Type t, ObjectSchema oschema)
		{
			da = DynamicAssembly.GetDynamicAssembly (t);
			os = oschema;
			context = new CommonObjectContext (oschema);
		}

		public ObjectSet ()
			: this (typeof (object), new ObjectSchema ())
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public object this [int i] {
			get { return InnerList [i]; }
			set { InnerList [i] = value; }
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

		public void Add (object o)
		{
			Type t = o.GetType ();
			if (t != da.UnderlyingType)
				throw new ObjectException (String.Format (Locale.GetText ("Wrong Object type '{0}' added to ObjectSet.  ObjectSet type is '{1}'"), t.FullName, da.UnderlyingType.FullName));
			context.Add (o, ObjectState.Inserted);
			InnerList.Add (o);
		}

		public void Add (ICollection c)
		{
			foreach (object o in c)
				Add (o);
		}

		public void Add (object o, ObjectState state)
		{
			Type t = o.GetType ();
			if (t != da.UnderlyingType)
				throw new ObjectException (String.Format (Locale.GetText ("Wrong Object type '{0}' added to ObjectSet.  ObjectSet type is '{1}'"), t.FullName, da.UnderlyingType.FullName));
			context.Add (o, state);
			InnerList.Add (o);
		}

                [MonoTODO]
		public void GetRemotingDiffGram (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		IList IListSource.GetList ()
		{
			return List;
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
