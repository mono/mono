//
// OptionAttribute.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Rafael Teixeira
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
