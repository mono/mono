namespace System.IO.Pipes
{
	[Flags]
	public enum PipeAccessRights
	{
		// FIXME: values are not verified at all
		ReadData = 1,
		WriteData = 2,
		ReadAttributes = 4,
		WriteAttributes = 8,
		ReadExtendedAttributes = 16,
		WriteExtendedAttributes = 32,
		CreateNewInstance = 64,
		Delete = 128,
		ReadPermissions = 256,
		ChangePermissions = 512,
		TakeOwnership = 1024,
		Synchronize = 2048,
		FullControl = ReadWrite | AccessSystemSecurity,
		Read = ReadData | ReadAttributes | ReadExtendedAttributes | ReadPermissions,
		Write = WriteData | WriteAttributes | WriteExtendedAttributes | ChangePermissions,
		ReadWrite = Read | Write,
		AccessSystemSecurity = ReadPermissions | ChangePermissions | TakeOwnership
	}
}
