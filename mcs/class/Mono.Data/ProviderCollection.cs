//
// Mono.Data.ProviderCollection
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//  
//
// Copyright (C) Brian Ritchie, 2002
// 
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
namespace Mono.Data 
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	
	
	/// <summary>
	///     <para>
	///       A collection that stores <see cref='.Provider'/> objects.
	///    </para>
	/// </summary>
	/// <seealso cref='.ProviderCollection'/>
	[Serializable()]
	public class ProviderCollection : DictionaryBase  
	{
		
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.ProviderCollection'/>.
		///    </para>
		/// </summary>
		public ProviderCollection() 
		{
		}
		
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.ProviderCollection'/> based on another <see cref='.ProviderCollection'/>.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A <see cref='.ProviderCollection'/> from which the contents are copied
		/// </param>
		public ProviderCollection(ProviderCollection value) 
		{
			this.AddRange(value);
		}
		
		/// <summary>
		///     <para>
		///       Initializes a new instance of <see cref='.ProviderCollection'/> containing any array of <see cref='.Provider'/> objects.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///       A array of <see cref='.Provider'/> objects with which to intialize the collection
		/// </param>
		public ProviderCollection(Provider[] value) 
		{
			this.AddRange(value);
		}
		
		/// <summary>
		/// <para>Represents the entry at the specified index of the <see cref='.Provider'/>.</para>
		/// </summary>
		/// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
		/// <value>
		///    <para> The entry at the specified index of the collection.</para>
		/// </value>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
		public Provider this[string Name] 
		{
			get 
			{
				return ((Provider)(Dictionary[Name]));
			}
			set 
			{
				Dictionary[Name] = value;
			}
		}

		public Provider FindByCommandType(Type CommandType)
		{
			foreach (Provider p in this)
			{
				if (p.CommandType==CommandType)
					return p;
			}
			throw new IndexOutOfRangeException();
		}

		public Provider FindByDataAdapterType(Type DataAdapterType)
		{
			foreach (Provider p in this)
			{
				if (p.DataAdapterType==DataAdapterType)
					return p;
			}
			throw new IndexOutOfRangeException();
		}

		public Provider FindByConnectionType(Type ConnectionType)
		{
			foreach (Provider p in this)
			{
				if (p.ConnectionType==ConnectionType)
					return p;
			}
			throw new IndexOutOfRangeException();
		}

		/// <summary>
		///    <para>Adds a <see cref='.Provider'/> with the specified value to the 
		///    <see cref='.ProviderCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.Provider'/> to add.</param>
		/// <returns>
		///    <para>The index at which the new element was inserted.</para>
		/// </returns>
		/// <seealso cref='.ProviderCollection.AddRange'/>
		public void Add(Provider value) 
		{
			Dictionary.Add(value.Name, value);
		}
		
		/// <summary>
		/// <para>Copies the elements of an array to the end of the <see cref='.ProviderCollection'/>.</para>
		/// </summary>
		/// <param name='value'>
		///    An array of type <see cref='.Provider'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='.ProviderCollection.Add'/>
		public void AddRange(Provider[] value) 
		{
			for (int i = 0; (i < value.Length); i = (i + 1)) 
			{
				this.Add(value[i]);
			}
		}
		
		/// <summary>
		///     <para>
		///       Adds the contents of another <see cref='.ProviderCollection'/> to the end of the collection.
		///    </para>
		/// </summary>
		/// <param name='value'>
		///    A <see cref='.ProviderCollection'/> containing the objects to add to the collection.
		/// </param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <seealso cref='.ProviderCollection.Add'/>
		public void AddRange(ProviderCollection value) 
		{
			foreach (Provider p in value)
			{
				this.Add(p);
			}
		}
		
		/// <summary>
		/// <para>Gets a value indicating whether the 
		///    <see cref='.ProviderCollection'/> contains the specified <see cref='.Provider'/>.</para>
		/// </summary>
		/// <param name='value'>The <see cref='.Provider'/> to locate.</param>
		/// <returns>
		/// <para><see langword='true'/> if the <see cref='.Provider'/> is contained in the collection; 
		///   otherwise, <see langword='false'/>.</para>
		/// </returns>
		/// <seealso cref='.ProviderCollection.IndexOf'/>
		public bool Contains(Provider value) 
		{
			return Dictionary.Contains(value);
		}
		
		/// <summary>
		/// <para>Copies the <see cref='.ProviderCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
		///    specified index.</para>
		/// </summary>
		/// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='.ProviderCollection'/> .</para></param>
		/// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
		/// <returns>
		///   <para>None.</para>
		/// </returns>
		/// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='.ProviderCollection'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
		/// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
		/// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
		/// <seealso cref='System.Array'/>
		public void CopyTo(Provider[] array, int index) 
		{
			Dictionary.CopyTo(array, index);
		}
		
		/// <summary>
		///    <para>Returns an enumerator that can iterate through 
		///       the <see cref='.ProviderCollection'/> .</para>
		/// </summary>
		/// <returns><para>None.</para></returns>
		/// <seealso cref='System.Collections.IEnumerator'/>
		public new ProviderEnumerator GetEnumerator() 
		{
			return new ProviderEnumerator(this);
		}
		
		/// <summary>
		///    <para> Removes a specific <see cref='.Provider'/> from the 
		///    <see cref='.ProviderCollection'/> .</para>
		/// </summary>
		/// <param name='value'>The <see cref='.Provider'/> to remove from the <see cref='.ProviderCollection'/> .</param>
		/// <returns><para>None.</para></returns>
		/// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
		public void Remove(Provider value) 
		{
			Dictionary.Remove(value);
		}
		
		public class ProviderEnumerator : object, IEnumerator 
		{
			
			private IEnumerator baseEnumerator;
			
			private IEnumerable temp;
			
			public ProviderEnumerator(ProviderCollection mappings) 
			{
				this.temp = ((IEnumerable)(mappings));
				this.baseEnumerator = temp.GetEnumerator();
			}
			
			public Provider Current 
			{
				get 
				{
					return ((Provider) ((DictionaryEntry) (baseEnumerator.Current)).Value);
				}
			}
			
			object IEnumerator.Current 
			{
				get 
				{
					return baseEnumerator.Current;
				}
			}
			
			public bool MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
			
			bool IEnumerator.MoveNext() 
			{
				return baseEnumerator.MoveNext();
			}
			
			public void Reset() 
			{
				baseEnumerator.Reset();
			}
			
			void IEnumerator.Reset() 
			{
				baseEnumerator.Reset();
			}
		}
	}
}
