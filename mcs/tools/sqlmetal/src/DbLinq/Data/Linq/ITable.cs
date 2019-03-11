#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System.Collections;
using System.Data.Linq;
using System.Linq;

#if MONO_STRICT
namespace System.Data.Linq
#else
namespace DbLinq.Data.Linq
#endif
{
    public partial interface ITable : IEnumerable, IQueryable
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        DataContext Context { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        bool IsReadOnly { get; }
        /// <summary>
        /// Attaches the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Attach(object entity);
        /// <summary>
        /// Attaches the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="asModified">if set to <c>true</c> [as modified].</param>
        void Attach(object entity, bool asModified);
        /// <summary>
        /// Attaches the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="original">The original.</param>
        void Attach(object entity, object original);
        /// <summary>
        /// Attaches all entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void AttachAll(IEnumerable entities);
        /// <summary>
        /// Attaches all entites.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="asModified">if set to <c>true</c> [as modified].</param>
        void AttachAll(IEnumerable entities, bool asModified);
        /// <summary>
        /// Marks entities as to be deleted.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void DeleteAllOnSubmit(IEnumerable entities);
        /// <summary>
        /// Marks entity as to be deleted.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void DeleteOnSubmit(object entity);
        /// <summary>
        /// Gets the modified members.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        ModifiedMemberInfo[] GetModifiedMembers(object entity);
        /// <summary>
        /// Gets the state of the original entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        object GetOriginalEntityState(object entity);
        /// <summary>
        /// Marks all entities to be inserted.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void InsertAllOnSubmit(IEnumerable entities);
        /// <summary>
        /// Marks entity to be inserted.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void InsertOnSubmit(object entity);
    }
}
