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

using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Reflection;

namespace DbLinq.Data.Linq
{
    /// <summary>
    /// Interface to watch modifications on registered entities
    /// Currently supports:
    /// - IModified (kept for compatibility, not recommended since it does not allow partial updates)
    /// - INotifyPropertyChanging and INotifyPropertyChanged (best choice)
    /// - raw objects (keeps a copy of all entity data)
    /// </summary>
    internal interface IMemberModificationHandler
    {
        /// <summary>
        /// Start to watch an entity. From here, changes will make IsModified() return true
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        void Register(object entity, MetaModel metaModel);

        /// <summary>
        /// Start to watch an entity. From here, changes will make IsModified() return true
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityOriginalState"></param>
        /// <param name="metaModel"></param>
        void Register(object entity, object entityOriginalState, MetaModel metaModel);

        /// <summary>
        /// Returns if the entity was modified since it has been Register()ed for the first time
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <param name="metaModel"></param>
        bool IsModified(object entity, MetaModel metaModel);

        /// <summary>
        /// Marks the entity as not dirty.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        void ClearModified(object entity, MetaModel metaModel);

        /// <summary>
        /// Returns a list of all modified properties since last Register/ClearModified
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <param name="metaModel"></param>
        IList<MemberInfo> GetModifiedProperties(object entity, MetaModel metaModel);

        /// <summary>
        /// Unregisters an entity.
        /// This is useful when it is switched from update to delete list
        /// </summary>
        /// <param name="entity"></param>
        void Unregister(object entity);

		/// <summary>
		/// Unregisters an entity.
		/// This is useful when the DataContext has been disposed
		/// </summary>
		/// <param name="entity"></param>
		void UnregisterAll();
    }
}
