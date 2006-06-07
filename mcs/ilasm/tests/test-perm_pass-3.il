.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'test-perm_pass-3'
{
  .permissionset reqopt
            "<PermissionSet class=\"System.Security.PermissionSe"
  + "t\"\n               version=\"1\">\n   <IPermission class=\"System"
  + ".Security.Permissions.SecurityPermission, mscorlib, Version="
  + "2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"\n "
  + "               version=\"1\"\n                Flags=\"SkipVerifi"
  + "cation\"/>\n</PermissionSet>\n"
  
  .permissionset assert = {
			[mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute = {
				property enum [mscorlib]System.Security.Permissions.IsolatedStorageContainment 'UserQuota' = int32 (5)
			}
  }

  .ver 0:0:0:0
}

.module 'test-perm_pass-3.dll'
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003
.corflags 0x00000001

.class public abstract foo
{
  .permissionset assert = {
	                [mscorlib]System.Security.Permissions.ReflectionPermissionAttribute = {
				property bool 'MemberAccess' = bool(true)
			}
  }

  .method public hidebysig newslot abstract virtual 
          instance void  abc() cil managed
  {
    .permissionset demand =
                {
			[mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute = {
				property enum [mscorlib]System.Security.Permissions.IsolatedStorageContainment 'UserQuota' = int32 (50)
			},

	                [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute = {
				property enum [mscorlib]System.Security.Permissions.IsolatedStorageContainment 'UsageAllowed' = int32 (123)
			},

	                [mscorlib]System.Security.Permissions.ReflectionPermissionAttribute = {
				property char 'MemberAccess' = char (16)
			}
		}

    
  }

}
