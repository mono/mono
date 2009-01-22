using System;
using System.Reflection;

namespace MonoTests.SystemWeb.Framework {
	/// <summary>
	/// This interface is used to add foreign data to the implementing class
	/// instances (similar to Python common practice) or AOP field injection.
	/// </summary>
	/// <remarks>
	/// This is achieved by convention that every class <b>using</b> this interface
	/// passes it's own type to the indexer property.
	/// </remarks>
	/// <example>
	/// class IForeignDataUsingClass
	/// {
	///	public string getData (IForeignData fd)
	///	{
	///		return fd[this.GetType ()] as string;
	///	}
	/// }
	/// </example>
	public interface IForeignData
	{
		/// <summary>
		/// Gets or sets the foreign data hold by the given instance.
		/// </summary>
		/// <param name="type">Type that wishes to inject a field.</param>
		/// <returns></returns>
		object this [Type type]
		{
			get;
			set;
		}
	}
}
