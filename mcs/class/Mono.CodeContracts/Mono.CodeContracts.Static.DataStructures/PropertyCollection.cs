// 
// PropertyCollection.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

using System.Collections.Generic;

using Mono.CodeContracts.Static.Analysis.Numerical;

namespace Mono.CodeContracts.Static.DataStructures {
	class PropertyCollection : IPropertyCollection {
		private readonly Dictionary<object, object> dictionary = new Dictionary<object, object> ();

		#region Implementation of IPropertyCollection
		public bool Contains (TypedKey key)
		{
			return this.dictionary.ContainsKey (key);
		}

	    public void Add<T> (TypedKey key, T value)
		{
			this.dictionary.Add (key, value);
		}

	    public bool TryGetValue<T> (TypedKey key, out T value)
	    {
	        object result;

	        if (!this.dictionary.TryGetValue (key, out result))
	            return false.Without (out value);

	        value = (T) result;
	        return true;
	    }

	    public void Remove(TypedKey key)
	    {
	        this.dictionary.Remove (key);
	    }

	    #endregion
	}
}
