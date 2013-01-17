using System;
using System.Runtime.InteropServices;

	[ComImport, GuidAttribute("E5CB7A31-7512-11D2-89CE-0080C792E5D8")]
	public class CorMetaDataDispenserExClass { }

	[ComImport, GuidAttribute("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3"),
	CoClass(typeof(CorMetaDataDispenserExClass))]
	public interface IMetaDataDispenserEx { }

	
public class Test
{
	public static void XXX ()
	{
		IMetaDataDispenserEx o = new IMetaDataDispenserEx();
	}

	public static void Main()
	{
		/* It doesn't work on Mono runtime
		IMetaDataDispenserEx o = new IMetaDataDispenserEx();
		if (o.GetType () != typeof (CorMetaDataDispenserExClass))
			return 1;
		Console.WriteLine ("OK");
		return 0;
		*/
	}
}
