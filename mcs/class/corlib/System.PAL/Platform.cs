// Sean MacIsaac
// Only want to have 1 OperatingSystemObject around.
// We get the correct one from compile time.

namespace System.PAL
{
	internal class Platform
	{
		private static OpSys _os;

		public static OpSys OS
		{

			get
			{
				if (_os == null) {
					_os = new OpSys ();
				}
				return _os;
			}
		}
	}
}
