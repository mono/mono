using System;
using System.Collections;

namespace NUnit.Core
{
	/// <summary>
	/// Summary description for Filter.
	/// </summary>
	public interface IFilter
	{
		bool Pass(TestSuite suite);

		bool Pass(TestCase test); 
	}

	[Serializable]
	public abstract class Filter : IFilter
	{
		private bool exclude;

		public Filter() : this( false ) { }

		public Filter( bool exclude )
		{
			this.exclude = exclude;
		}

		public bool Exclude
		{
			get { return exclude; }
			set { exclude = value; }
		}

		public void Negate()
		{
			exclude = !exclude;
		}

		#region IFilter Members

		public abstract bool Pass(TestSuite suite);

		public abstract bool Pass(TestCase test);

		#endregion
	}
}
