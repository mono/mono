.assembly extern mscorlib
{
}
.assembly 'test-perm_fail-3'
{
}

  .class interface public auto ansi abstract foo
  {

    .method public virtual  hidebysig  newslot  abstract 
           instance default void abc ()  cil managed 
    {
	//reqmin allowed only for .assembly
	.permission reqmin [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UsageAllowed"=[mscorlib]System.Security.Permissions.IsolatedStorageContainment (16))
    
    }
  }
