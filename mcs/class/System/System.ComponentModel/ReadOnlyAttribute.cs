//
// ReadOnlyAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace System.ComponentModel {
	[AttributeUsage (AttributeTargets.All)]
	sealed public class ReadOnlyAttribute : Attribute {
		bool read_only;
		
		public static readonly ReadOnlyAttribute No;
		public static readonly ReadOnlyAttribute Yes;
		public static readonly ReadOnlyAttribute Default;

		static ReadOnlyAttribute ()
		{
			No = new ReadOnlyAttribute (false);
			Yes = new ReadOnlyAttribute (false);
			Default = new ReadOnlyAttribute (false);
		}
		
		public ReadOnlyAttribute (bool read_only)
		{
			this.read_only = read_only;
		}

		public bool IsReadOnly {
			get {
				return read_only;
			}
		}
	};
}
