// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Gerry Shaw (gerry_shaw@yahoo.com)

namespace SourceForge.NAnt {

    using System.Collections;
    using System.Collections.Specialized;

    public class PropertyDictionary : DictionaryBase {

        /// <summary>
        /// Maintains a list of the property names that are readonly.
        /// </summary>
        StringCollection _readOnlyProperties = new StringCollection();

        /// <summary>
        /// Adds a property that cannot be changed.
        /// </summary>
        /// <remarks>
        /// Properties added with this method can never be changed.  Note that
        /// they are removed if the <c>Clear</c> method is called.
        /// </remarks>
        /// <param name="name">Name of property</param>
        /// <param name="value">Value of property</param>
        public void AddReadOnly(string name, string value) {
            if (!_readOnlyProperties.Contains(name)) {
                _readOnlyProperties.Add(name);
                Dictionary.Add(name, value);
            }
        }

        /// <summary>
        /// Adds a property to the collection.
        /// </summary>
        /// <param name="name">Name of property</param>
        /// <param name="value">Value of property</param>
        public void Add(string name, string value) {
            if (!_readOnlyProperties.Contains(name)) {
                Dictionary.Add(name, value);
            }
        }

        public string this[string name] {
            get { return (string) Dictionary[(object) name]; }
            set {
                if (!_readOnlyProperties.Contains(name)) {
                    Dictionary[name] = value;
                }
            }
        }

        protected override void OnClear() {
            _readOnlyProperties.Clear();
        }
    }
}