//
// System.Data.Common.AbstractDbParameterCollection
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//

using System;
using System.Data.Common;
using System.Collections;

namespace System.Data.ProviderBase
{
	public abstract class AbstractDbParameterCollection : DbParameterBaseCollection
	{
		#region Fields

		private AbstractDbCommand _parent;

		#endregion // Fields

		#region Constructors

		public AbstractDbParameterCollection(DbCommand parent)
        {
			_parent = (AbstractDbCommand)parent;
        }

		#endregion // Constructors

		#region Properties

		#endregion // Properties

		#region Methods

		public override int Add (object value)
        {
            OnSchemaChanging();
			return base.Add(value);
        }

		public override void Clear()
        {
            OnSchemaChanging();
			base.Clear();
        }

		public override void Insert(int index, object value)
        {
            OnSchemaChanging();
			base.Insert(index,value);
        }

		public override void Remove(object value)
        {
            OnSchemaChanging();
			base.Remove(value);
        }

		public override void RemoveAt(int index)
        {
            OnSchemaChanging();
			base.RemoveAt(index);
        }

		public override void RemoveAt(string parameterName)
        {
            OnSchemaChanging();
			base.RemoveAt(parameterName);
        }

		internal void OnSchemaChanging()
        {
            if (_parent != null) {
                _parent.OnSchemaChanging();
            }
        }

		#endregion // Methods
	}
}
