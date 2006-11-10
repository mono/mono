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
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace System.Xml.Schema
{
			
	/// <summary>
	/// Summary description for XmlSchemaFacet.
	/// </summary>
	public abstract class XmlSchemaFacet : XmlSchemaAnnotated
	{
		[Flags]
		internal protected enum Facet {
			None = 0,
			length = 1 ,
			minLength = 2,
			maxLength = 4,
			pattern = 8,
			enumeration = 16,
			whiteSpace = 32,
			maxInclusive = 64,
			maxExclusive = 128,
			minExclusive = 256,
			minInclusive = 512, 
			totalDigits = 1024,
			fractionDigits = 2048
		};
 
		internal static readonly Facet AllFacets = 
		                        Facet.length | Facet.minLength |  Facet.maxLength |
					Facet.minExclusive | Facet.maxExclusive |
					Facet.minInclusive | Facet.maxInclusive |
					Facet.pattern | Facet.enumeration | Facet.whiteSpace |
					Facet.totalDigits | Facet.fractionDigits;
		
		internal virtual Facet ThisFacet { get { return Facet.None; } }
		
		private bool isFixed;
		private string val;

		protected XmlSchemaFacet()
		{
		}
		
		[System.Xml.Serialization.XmlAttribute("value")]
		public string Value
		{
			get{ return  val; } 
			set{ val = value; }
		}

		[DefaultValue(false)]
		[System.Xml.Serialization.XmlAttribute("fixed")]
		public virtual bool IsFixed 
		{
			get{ return  isFixed; }
			set{ isFixed = value; }
		}
	}
}
