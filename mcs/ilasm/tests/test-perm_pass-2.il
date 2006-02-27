// Test for new(2.0) syntax for specifying permissionset
// (Uses string instead of a bytearray or namevalue pairs)

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'test-perm_pass-2'
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module 'test-perm_pass-2.dll'

.class private auto ansi beforefieldinit test
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

  .method public hidebysig static void  foo() cil managed
  {
    .permissionset linkcheck
              "<PermissionSet class=\"System.Security.PermissionSe"
    + "t\"\nversion=\"1\">\n<IPermission class=\"System.Security.Permissi"
    + "ons.SecurityPermission, mscorlib, Version=2.0.0.0, Culture=n"
    + "eutral, PublicKeyToken=b77a5c561934e089\"\nversion=\"1\"\nUnrestr"
    + "icted=\"true\"/>\n</PermissionSet>\n"
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  }

}
