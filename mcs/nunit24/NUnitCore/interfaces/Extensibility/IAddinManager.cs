using System;

namespace NUnit.Core.Extensibility
{
	public interface IAddinManager
	{
		Addin[] Addins { get; }

		TestFramework[] Frameworks { get; }
	}

}
