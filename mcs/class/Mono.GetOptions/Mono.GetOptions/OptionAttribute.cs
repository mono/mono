//
// OptionAttribute.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Rafael Teixeira
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

namespace Mono.GetOptions
{

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class OptionAttribute : Attribute
	{
		public string ShortDescription;
		public char ShortForm;
		public string LongForm;
		public string AlternateForm;
		public int MaxOccurs; // negative means there is no limit
		
		public bool VBCStyleBoolean;
		public bool SecondLevelHelp;

		private void SetValues(
			string shortDescription, 
			char shortForm, 
			string longForm, 
			string alternateForm,
			int maxOccurs)
		{
			ShortDescription = shortDescription; 
			ShortForm = shortForm;
			LongForm = longForm;
			MaxOccurs = maxOccurs;
			AlternateForm = alternateForm;
		}

		public OptionAttribute(string shortDescription)
		{
			SetValues(shortDescription, ' ', string.Empty, string.Empty, 1);
		}

		public OptionAttribute(string shortDescription, char shortForm)
		{
			SetValues(shortDescription, shortForm, string.Empty, string.Empty, 1);
		}

		public OptionAttribute(string shortDescription, char shortForm, string longForm)
		{
			SetValues(shortDescription, shortForm, longForm, string.Empty, 1);
		}

		public OptionAttribute(string shortDescription, string longForm)
		{
			SetValues(shortDescription, ' ', longForm, string.Empty, 1); 
		}

		public OptionAttribute(string shortDescription, char shortForm, string longForm, string alternateForm)
		{
			SetValues(shortDescription, shortForm, longForm, alternateForm, 1);
		}

		public OptionAttribute(string shortDescription, string longForm, string alternateForm)
		{
			SetValues(shortDescription, ' ', longForm, alternateForm, 1); 
		}

		public OptionAttribute(int maxOccurs, string shortDescription)
		{
			SetValues(shortDescription, ' ', string.Empty, string.Empty, maxOccurs); 
		}

		public OptionAttribute(int maxOccurs, string shortDescription, char shortForm)
		{
			SetValues(shortDescription, shortForm, string.Empty, string.Empty, maxOccurs);
		}

		public OptionAttribute(int maxOccurs, string shortDescription, char shortForm, string longForm)
		{
			SetValues(shortDescription, shortForm, longForm, string.Empty, maxOccurs); 
		}

		public OptionAttribute(int maxOccurs, string shortDescription, string longForm)
		{
			SetValues(shortDescription, ' ', longForm, string.Empty, maxOccurs); 
		}
		
		public OptionAttribute(int maxOccurs, string shortDescription, char shortForm, string longForm, string alternateForm)
		{
			SetValues(shortDescription, shortForm, longForm, alternateForm, maxOccurs); 
		}

		public OptionAttribute(int maxOccurs, string shortDescription, string longForm, string alternateForm)
		{
			SetValues(shortDescription, ' ', longForm, alternateForm, maxOccurs); 
		}
	}
}
