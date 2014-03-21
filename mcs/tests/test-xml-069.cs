// Compiler options: -doc:xml-069.xml

using System;

namespace XmlComments
{
	/// <summary/>
	class Program
	{
		/// <summary/>
		private enum MyEnum
		{
			/// <summary>The first entry</summary>
			One,
		}

		/// <summary>
		/// <see cref="MyEnum.One"/>
		/// <see cref="Program.MyEnum.One"/>
		/// <see cref="XmlComments.Program.MyEnum.One"/>
		/// <see cref="F:XmlComments.Program.MyEnum.One"/>
		/// </summary>
		static void Main(string[] args)
		{
		}
	}
}
