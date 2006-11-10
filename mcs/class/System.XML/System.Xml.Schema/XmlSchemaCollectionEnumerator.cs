// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com

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
using System.Collections;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaCollectionEnumerator.
	/// </summary>
	public sealed class XmlSchemaCollectionEnumerator : IEnumerator
	{
		private IEnumerator xenum;

		internal XmlSchemaCollectionEnumerator(ICollection col)
		{
			xenum = col.GetEnumerator ();
		}

		// Properties
		public XmlSchema Current { 
			get {
				return (XmlSchema) xenum.Current; 
			}
		}
		// Methods
		public bool MoveNext()
		{
			return xenum.MoveNext();
		}

		//Explicit Interface implementation
		bool IEnumerator.MoveNext()
		{
			return xenum.MoveNext();
		}
		void IEnumerator.Reset()
		{
			xenum.Reset();
		}
		object IEnumerator.Current
		{
			get { return xenum.Current; }
		}
	}
}
