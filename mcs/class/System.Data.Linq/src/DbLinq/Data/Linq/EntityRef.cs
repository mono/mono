//
// EntityRef.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//   Pablo Íñigo Blasco <pibgeus@gmail.com>
//
// Copyright (C) 2008 Novell, Inc.
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
using System.Collections.Generic;
using DbLinq;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    public struct EntityRef<TEntity> where TEntity : class
    {
        private TEntity entity;
        private bool hasLoadedOrAssignedValue;

        public EntityRef(TEntity entity)
        {
            this.entity = entity;
            hasLoadedOrAssignedValue = true;
        }

        [DbLinqToDo]
        public EntityRef(IEnumerable<TEntity> source)
        {
            throw new NotImplementedException();
        }

        public EntityRef(EntityRef<TEntity> entityRef)
        {
            this.entity = entityRef.Entity;
            hasLoadedOrAssignedValue = true;
        }

        public TEntity Entity
        {
            get 
            { 
                return entity; 
            }
            set
            {
                entity = value;
                hasLoadedOrAssignedValue = true;
            }
        }

        public bool HasLoadedOrAssignedValue
        {
            get
            {
                return hasLoadedOrAssignedValue;
            }
        }
    }
}
