// cs1518-2.cs : Attributes cannot be applied to namespaces.
// Line: 5
using System;

[error_1518(11)]
namespace Mono.Tests
{
	[AttributeUsage(AttributeTargets.All)]
	public class error_1518Attribute : Attribute
	{
		private int x;

		public error_1518Attribute(int x)
		{
			this.x = x;
		}

		public int X
		{
			get
			{
				return x;
			}
		}
	}

	[error_1518(10)]
	public class error_1518Class
	{
		public error_1518Class()
		{
		}
	}
}