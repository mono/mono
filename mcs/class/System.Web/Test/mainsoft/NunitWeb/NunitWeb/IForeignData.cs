using System;
using System.Reflection;

namespace MonoTests.SystemWeb.Framework {
	/// <summary>
	/// Summary description for IForeignData.
	/// </summary>
	public interface IForeignData
	{
		object this [Type type]
		{
			get;
			set;
		}
	}
}
