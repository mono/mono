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

using System.Collections.Generic;

namespace System.Data.Linq
{
    //TODO: this should be checked: is LINQ working to handle this?
    public struct EntityRef<TEntity> where TEntity : class
    {
        #region .ctor
        [MonoTODO()]
        public EntityRef(TEntity entity)
        {
            this.source = null; //TODO: we should assign it...
            this.entity = entity;
        }

        public EntityRef(IEnumerable<TEntity> source)
        {
            this.source = source;
            entity = default(TEntity);
        }

        public EntityRef(EntityRef<TEntity> entityRef)
        {
            source = entityRef.source;
            entity = entityRef.entity;
        }
        #endregion

        #region Fields
        private IEnumerable<TEntity> source;
        private TEntity entity;
        #endregion

        #region Properties
        public TEntity Entity
        {
            get { return entity; }
            set { entity = value; }
        }

        [MonoTODO()]
        public bool HasLoadedOrAssignedValue
        {
            get { return true; }
        }
        #endregion
    }
}