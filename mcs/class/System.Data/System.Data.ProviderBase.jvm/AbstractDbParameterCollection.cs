//
// System.Data.Common.AbstractDbParameterCollection
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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

		protected internal void OnSchemaChanging()
        {
            if (_parent != null) {
                _parent.OnSchemaChanging();
            }
        }

		#endregion // Methods
	}
}
