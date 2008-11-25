using System;

namespace NUnit.Framework
{
	/// <summary>
	/// Summary description for SetCultureAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method|AttributeTargets.Assembly, AllowMultiple=true)]
	public class SetCultureAttribute : PropertyAttribute
	{
		/// <summary>
		/// Construct given the name of a culture
		/// </summary>
		/// <param name="culture"></param>
		public SetCultureAttribute( string culture ) : base( "_SETCULTURE", culture ) { }
	}
}
