namespace NUnit.Runner 
{
	using System.Reflection;
	/// <summary>
	/// This class defines the current version of NUnit
	/// </summary>
	public class Version 
	{
		private Version() 
		{
			// don't instantiate
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static string id() 
		{
			return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			//return "1.10";
		}
	}
}
