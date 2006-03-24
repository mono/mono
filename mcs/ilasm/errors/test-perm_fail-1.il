.assembly extern mscorlib
{
}
.assembly 'test-perm_fail-1'
{
}

  .class interface public auto ansi abstract foo
  {

    .method public virtual  hidebysig  newslot  abstract 
           instance default void abc ()  cil managed 
    {
	//Invalid enum value 
	.permission demand [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UsageAllowed"=[mscorlib]System.Security.Permissions.IsolatedStorageContainment (1))

    }
    
  }
