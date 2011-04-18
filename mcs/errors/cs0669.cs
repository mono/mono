// CS0669: `CorMetaDataDispenserExClass': A class with the ComImport attribute cannot have a user-defined constructor
// Line: 10

using System;
using System.Runtime.InteropServices;

[ComImport, GuidAttribute("E5CB7A31-7512-11D2-89CE-0080C792E5D8")]
public class CorMetaDataDispenserExClass
{
	public CorMetaDataDispenserExClass (int i) {}
}
