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
//
// Authors:
//        Antonello Provenzano  <antonello@deveel.com>
//

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Linq
{
    public sealed class Table<T> : IQueryable<T>, IEnumerable<T>, ITable, IQueryable, IEnumerable, IListSource
    {
        #region .ctor
        public Table(DataContext context, MetaTable metaTable)
        {
            this.context = context;
            this.metaTable = metaTable;
        }
        #endregion

        #region Fields
        private DataContext context;
        private MetaTable metaTable;
        #endregion

        #region Properties
        public DataContext Context
        {
            get { return context; }
        }

        //TODO:
        public bool IsReadOnly
        {
            get { return false; }
        }

        //TODO: check this... is this right?
        public string Name {
            get { return metaTable.TableName; }
        }

        //TODO: check this... is this right?
        public Type RowType
        {
            get { return typeof(T); }
        }
        #endregion

        #region IListSource Implementations
        bool IListSource.ContainsListCollection 
        {
            get { return false; }
        }

        IList IListSource.GetList()
        {
            //TODO:
            throw new NotImplementedException();
        }
        #endregion

        #region IQueryable Implementations
        //TODO: check this... is this right?
        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }

        //TODO:
        Expression IQueryable.Expression
        {
            get { return null; }
        }
        
	public IQueryProvider Provider {
		get {
			throw new NotImplementedException ();
		}
	}        
        #endregion

        #region ICollection Implementations
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region Public Methods
        public void Add(T item)
        {
            //TODO:
            throw new NotImplementedException();
        }

        public void Attach(T item)
        {
            //TODO:
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            //TODO:
            throw new NotImplementedException();
        }

        public void Remove(T item)
        {
            //TODO:
            throw new NotImplementedException();
        }

        public void RemoveAll(IEnumerable<T> items)
        {
            //TODO:
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
