using System;

namespace Mono.GetOptions
{

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class OptionAttribute : Attribute
	{
		public string ShortDescription;
		public char ShortForm;
		public string LongForm;
		public int MaxOccurs; // negative means there is no limit

		private void SetValues(
			string shortDescription, 
			char shortForm, 
			string longForm, 
			int maxOccurs)
		{
			ShortDescription = shortDescription; 
			ShortForm = shortForm;
			LongForm = longForm;
			MaxOccurs = maxOccurs;
		}

		public OptionAttribute(string shortDescription)
		{
			SetValues(shortDescription, ' ', string.Empty, 1);
		}

		public OptionAttribute(string shortDescription, char shortForm)
		{
			SetValues(shortDescription, shortForm, string.Empty, 1);
		}

		public OptionAttribute(string shortDescription, char shortForm, string longForm)
		{
			SetValues(shortDescription, shortForm, longForm, 1);
		}

		public OptionAttribute(string shortDescription, string longForm)
		{
			SetValues(shortDescription, ' ', longForm, 1); 
		}

		public OptionAttribute(int maxOccurs, string shortDescription)
		{
			SetValues(shortDescription, ' ', string.Empty, maxOccurs); 
		}

		public OptionAttribute(int maxOccurs, string shortDescription, char shortForm)
		{
			SetValues(shortDescription, shortForm, string.Empty, maxOccurs);
		}

		public OptionAttribute(int maxOccurs, string shortDescription, char shortForm, string longForm)
		{
			SetValues(shortDescription, shortForm, longForm, maxOccurs); 
		}

		public OptionAttribute(int maxOccurs, string shortDescription, string longForm)
		{
			SetValues(shortDescription, ' ', longForm, maxOccurs); 
		}
	}
}
