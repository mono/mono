// Test for Syntactically valid .permission usage
// The combination of security action and the permissions might not make sense

.assembly extern mscorlib
{
}
.assembly 'test-perm_pass-1'
{
    .permissionset reqopt =  (
  3C 00 50 00 65 00 72 00 6D 00 69 00 73 00 73 00   // <.P.e.r.m.i.s.s.
  69 00 6F 00 6E 00 53 00 65 00 74 00 20 00 63 00   // i.o.n.S.e.t. .c.
  6C 00 61 00 73 00 73 00 3D 00 22 00 53 00 79 00   // l.a.s.s.=.".S.y.
  73 00 74 00 65 00 6D 00 2E 00 53 00 65 00 63 00   // s.t.e.m...S.e.c.
  75 00 72 00 69 00 74 00 79 00 2E 00 50 00 65 00   // u.r.i.t.y...P.e.
  72 00 6D 00 69 00 73 00 73 00 69 00 6F 00 6E 00   // r.m.i.s.s.i.o.n.
  53 00 65 00 74 00 22 00 0A 00 20 00 20 00 20 00   // S.e.t."... . . .
  20 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   //  . . . . . . . .
  20 00 20 00 20 00 20 00 76 00 65 00 72 00 73 00   //  . . . .v.e.r.s.
  69 00 6F 00 6E 00 3D 00 22 00 31 00 22 00 3E 00   // i.o.n.=.".1.".>.
  0A 00 20 00 20 00 20 00 3C 00 49 00 50 00 65 00   // .. . . .<.I.P.e.
  72 00 6D 00 69 00 73 00 73 00 69 00 6F 00 6E 00   // r.m.i.s.s.i.o.n.
  20 00 63 00 6C 00 61 00 73 00 73 00 3D 00 22 00   //  .c.l.a.s.s.=.".
  53 00 79 00 73 00 74 00 65 00 6D 00 2E 00 53 00   // S.y.s.t.e.m...S.
  65 00 63 00 75 00 72 00 69 00 74 00 79 00 2E 00   // e.c.u.r.i.t.y...
  50 00 65 00 72 00 6D 00 69 00 73 00 73 00 69 00   // P.e.r.m.i.s.s.i.
  6F 00 6E 00 73 00 2E 00 53 00 65 00 63 00 75 00   // o.n.s...S.e.c.u.
  72 00 69 00 74 00 79 00 50 00 65 00 72 00 6D 00   // r.i.t.y.P.e.r.m.
  69 00 73 00 73 00 69 00 6F 00 6E 00 2C 00 20 00   // i.s.s.i.o.n.,. .
  6D 00 73 00 63 00 6F 00 72 00 6C 00 69 00 62 00   // m.s.c.o.r.l.i.b.
  2C 00 20 00 56 00 65 00 72 00 73 00 69 00 6F 00   // ,. .V.e.r.s.i.o.
  6E 00 3D 00 32 00 2E 00 30 00 2E 00 30 00 2E 00   // n.=.2...0...0...
  30 00 2C 00 20 00 43 00 75 00 6C 00 74 00 75 00   // 0.,. .C.u.l.t.u.
  72 00 65 00 3D 00 6E 00 65 00 75 00 74 00 72 00   // r.e.=.n.e.u.t.r.
  61 00 6C 00 2C 00 20 00 50 00 75 00 62 00 6C 00   // a.l.,. .P.u.b.l.
  69 00 63 00 4B 00 65 00 79 00 54 00 6F 00 6B 00   // i.c.K.e.y.T.o.k.
  65 00 6E 00 3D 00 62 00 37 00 37 00 61 00 35 00   // e.n.=.b.7.7.a.5.
  63 00 35 00 36 00 31 00 39 00 33 00 34 00 65 00   // c.5.6.1.9.3.4.e.
  30 00 38 00 39 00 22 00 0A 00 20 00 20 00 20 00   // 0.8.9."... . . .
  20 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   //  . . . . . . . .
  20 00 20 00 20 00 20 00 20 00 76 00 65 00 72 00   //  . . . . .v.e.r.
  73 00 69 00 6F 00 6E 00 3D 00 22 00 31 00 22 00   // s.i.o.n.=.".1.".
  0A 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   // .. . . . . . . .
  20 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   //  . . . . . . . .
  20 00 46 00 6C 00 61 00 67 00 73 00 3D 00 22 00   //  .F.l.a.g.s.=.".
  53 00 6B 00 69 00 70 00 56 00 65 00 72 00 69 00   // S.k.i.p.V.e.r.i.
  66 00 69 00 63 00 61 00 74 00 69 00 6F 00 6E 00   // f.i.c.a.t.i.o.n.
  22 00 2F 00 3E 00 0A 00 3C 00 2F 00 50 00 65 00   // "./.>...<./.P.e.
  72 00 6D 00 69 00 73 00 73 00 69 00 6F 00 6E 00   // r.m.i.s.s.i.o.n.
  53 00 65 00 74 00 3E 00 0A 00                   ) // S.e.t.>...

.permissionset demand =  (
  3C 00 50 00 65 00 72 00 6D 00 69 00 73 00 73 00   // <.P.e.r.m.i.s.s.
  69 00 6F 00 6E 00 53 00 65 00 74 00 20 00 63 00   // i.o.n.S.e.t. .c.
  6C 00 61 00 73 00 73 00 3D 00 22 00 53 00 79 00   // l.a.s.s.=.".S.y.
  73 00 74 00 65 00 6D 00 2E 00 53 00 65 00 63 00   // s.t.e.m...S.e.c.
  75 00 72 00 69 00 74 00 79 00 2E 00 50 00 65 00   // u.r.i.t.y...P.e.
  72 00 6D 00 69 00 73 00 73 00 69 00 6F 00 6E 00   // r.m.i.s.s.i.o.n.
  53 00 65 00 74 00 22 00 0A 00 20 00 20 00 20 00   // S.e.t."... . . .
  20 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   //  . . . . . . . .
  20 00 20 00 20 00 20 00 76 00 65 00 72 00 73 00   //  . . . .v.e.r.s.
  69 00 6F 00 6E 00 3D 00 22 00 31 00 22 00 3E 00   // i.o.n.=.".1.".>.
  0A 00 20 00 20 00 20 00 3C 00 49 00 50 00 65 00   // .. . . .<.I.P.e.
  72 00 6D 00 69 00 73 00 73 00 69 00 6F 00 6E 00   // r.m.i.s.s.i.o.n.
  20 00 63 00 6C 00 61 00 73 00 73 00 3D 00 22 00   //  .c.l.a.s.s.=.".
  53 00 79 00 73 00 74 00 65 00 6D 00 2E 00 53 00   // S.y.s.t.e.m...S.
  65 00 63 00 75 00 72 00 69 00 74 00 79 00 2E 00   // e.c.u.r.i.t.y...
  50 00 65 00 72 00 6D 00 69 00 73 00 73 00 69 00   // P.e.r.m.i.s.s.i.
  6F 00 6E 00 73 00 2E 00 53 00 65 00 63 00 75 00   // o.n.s...S.e.c.u.
  72 00 69 00 74 00 79 00 50 00 65 00 72 00 6D 00   // r.i.t.y.P.e.r.m.
  69 00 73 00 73 00 69 00 6F 00 6E 00 2C 00 20 00   // i.s.s.i.o.n.,. .
  6D 00 73 00 63 00 6F 00 72 00 6C 00 69 00 62 00   // m.s.c.o.r.l.i.b.
  2C 00 20 00 56 00 65 00 72 00 73 00 69 00 6F 00   // ,. .V.e.r.s.i.o.
  6E 00 3D 00 32 00 2E 00 30 00 2E 00 30 00 2E 00   // n.=.2...0...0...
  30 00 2C 00 20 00 43 00 75 00 6C 00 74 00 75 00   // 0.,. .C.u.l.t.u.
  72 00 65 00 3D 00 6E 00 65 00 75 00 74 00 72 00   // r.e.=.n.e.u.t.r.
  61 00 6C 00 2C 00 20 00 50 00 75 00 62 00 6C 00   // a.l.,. .P.u.b.l.
  69 00 63 00 4B 00 65 00 79 00 54 00 6F 00 6B 00   // i.c.K.e.y.T.o.k.
  65 00 6E 00 3D 00 62 00 37 00 37 00 61 00 35 00   // e.n.=.b.7.7.a.5.
  63 00 35 00 36 00 31 00 39 00 33 00 34 00 65 00   // c.5.6.1.9.3.4.e.
  30 00 38 00 39 00 22 00 0A 00 20 00 20 00 20 00   // 0.8.9."... . . .
  20 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   //  . . . . . . . .
  20 00 20 00 20 00 20 00 20 00 76 00 65 00 72 00   //  . . . . .v.e.r.
  73 00 69 00 6F 00 6E 00 3D 00 22 00 31 00 22 00   // s.i.o.n.=.".1.".
  0A 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   // .. . . . . . . .
  20 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   //  . . . . . . . .
  20 00 46 00 6C 00 61 00 67 00 73 00 3D 00 22 00   //  .F.l.a.g.s.=.".
  53 00 6B 00 69 00 70 00 56 00 65 00 72 00 69 00   // S.k.i.p.V.e.r.i.
  66 00 69 00 63 00 61 00 74 00 69 00 6F 00 6E 00   // f.i.c.a.t.i.o.n.
  22 00 2F 00 3E 00 0A 00 3C 00 2F 00 50 00 65 00   // "./.>...<./.P.e.
  72 00 6D 00 69 00 73 00 73 00 69 00 6F 00 6E 00   // r.m.i.s.s.i.o.n.
  53 00 65 00 74 00 3E 00 0A 00                   ) // S.e.t.>...

}

  .class interface public auto ansi abstract foo
  {

    .method public virtual  hidebysig  newslot  abstract 
           instance default void abc ()  cil managed 
    {
	//.permission demand [mscorlib]System.Security.Permissions.FileIOPermissionAttribute ("Write"="/tmp", "Read"="/tmp")
	.permission demand [mscorlib]System.Security.Permissions.ReflectionPermissionAttribute ("MemberAccess"=true)
	.permission deny [mscorlib]System.Security.Permissions.ReflectionPermissionAttribute

	//IsolatedStorageContainment - enum
	.permission demand [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UsageAllowed"=[mscorlib]System.Security.Permissions.IsolatedStorageContainment (16))
	.permission deny [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UsageAllowed"=[mscorlib]System.Security.Permissions.IsolatedStorageContainment (int32:16))
	.permission assert [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UsageAllowed"=[mscorlib]System.Security.Permissions.IsolatedStorageContainment (int16:16))
	.permission permitonly [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UsageAllowed"=[mscorlib]System.Security.Permissions.IsolatedStorageContainment (int8:16))

	// UserQuota - long
	// This is the *correct* way to specify the value for UserQuota ...
	.permission linkcheck [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UserQuota"=70000)
	
	// ... but the following are also valid
	.permission demand [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UserQuota"=[mscorlib]System.Security.Permissions.IsolatedStorageContainment (70000))
	.permission deny [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UserQuota"=[mscorlib]System.Security.Permissions.IsolatedStorageContainment (int32:70000))
	.permission assert [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UserQuota"=[mscorlib]System.Security.Permissions.IsolatedStorageContainment (int16:70000))
	.permission permitonly [mscorlib]System.Security.Permissions.IsolatedStorageFilePermissionAttribute ("UserQuota"=[mscorlib]System.Security.Permissions.IsolatedStorageContainment (int8:1000))

        .permissionset deny =  (
        3C 00 50 00 65 00 72 00 6D 00 69 00 73 00 73 00   // <.P.e.r.m.i.s.s.
        69 00 6F 00 6E 00 53 00 65 00 74 00 20 00 63 00   // i.o.n.S.e.t. .c.
        6C 00 61 00 73 00 73 00 3D 00 22 00 53 00 79 00   // l.a.s.s.=.".S.y.
        73 00 74 00 65 00 6D 00 2E 00 53 00 65 00 63 00   // s.t.e.m...S.e.c.
        75 00 72 00 69 00 74 00 79 00 2E 00 50 00 65 00   // u.r.i.t.y...P.e.
        72 00 6D 00 69 00 73 00 73 00 69 00 6F 00 6E 00   // r.m.i.s.s.i.o.n.
        53 00 65 00 74 00 22 00 0A 00 20 00 20 00 20 00   // S.e.t."... . . .
        20 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   //  . . . . . . . .
        20 00 20 00 20 00 20 00 76 00 65 00 72 00 73 00   //  . . . .v.e.r.s.
        69 00 6F 00 6E 00 3D 00 22 00 31 00 22 00 3E 00   // i.o.n.=.".1.".>.
        0A 00 20 00 20 00 20 00 3C 00 49 00 50 00 65 00   // .. . . .<.I.P.e.
        72 00 6D 00 69 00 73 00 73 00 69 00 6F 00 6E 00   // r.m.i.s.s.i.o.n.
        20 00 63 00 6C 00 61 00 73 00 73 00 3D 00 22 00   //  .c.l.a.s.s.=.".
        53 00 79 00 73 00 74 00 65 00 6D 00 2E 00 53 00   // S.y.s.t.e.m...S.
        65 00 63 00 75 00 72 00 69 00 74 00 79 00 2E 00   // e.c.u.r.i.t.y...
        50 00 65 00 72 00 6D 00 69 00 73 00 73 00 69 00   // P.e.r.m.i.s.s.i.
        6F 00 6E 00 73 00 2E 00 53 00 65 00 63 00 75 00   // o.n.s...S.e.c.u.
        72 00 69 00 74 00 79 00 50 00 65 00 72 00 6D 00   // r.i.t.y.P.e.r.m.
        69 00 73 00 73 00 69 00 6F 00 6E 00 2C 00 20 00   // i.s.s.i.o.n.,. .
        6D 00 73 00 63 00 6F 00 72 00 6C 00 69 00 62 00   // m.s.c.o.r.l.i.b.
        2C 00 20 00 56 00 65 00 72 00 73 00 69 00 6F 00   // ,. .V.e.r.s.i.o.
        6E 00 3D 00 31 00 2E 00 30 00 2E 00 35 00 30 00   // n.=.1...0...5.0.
        30 00 30 00 2E 00 30 00 2C 00 20 00 43 00 75 00   // 0.0...0.,. .C.u.
        6C 00 74 00 75 00 72 00 65 00 3D 00 6E 00 65 00   // l.t.u.r.e.=.n.e.
        75 00 74 00 72 00 61 00 6C 00 2C 00 20 00 50 00   // u.t.r.a.l.,. .P.
        75 00 62 00 6C 00 69 00 63 00 4B 00 65 00 79 00   // u.b.l.i.c.K.e.y.
        54 00 6F 00 6B 00 65 00 6E 00 3D 00 62 00 37 00   // T.o.k.e.n.=.b.7.
        37 00 61 00 35 00 63 00 35 00 36 00 31 00 39 00   // 7.a.5.c.5.6.1.9.
        33 00 34 00 65 00 30 00 38 00 39 00 22 00 0A 00   // 3.4.e.0.8.9."...
        20 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   //  . . . . . . . .
        20 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   //  . . . . . . . .
        76 00 65 00 72 00 73 00 69 00 6F 00 6E 00 3D 00   // v.e.r.s.i.o.n.=.
        22 00 31 00 22 00 0A 00 20 00 20 00 20 00 20 00   // ".1."... . . . .
        20 00 20 00 20 00 20 00 20 00 20 00 20 00 20 00   //  . . . . . . . .
        20 00 20 00 20 00 20 00 46 00 6C 00 61 00 67 00   //  . . . .F.l.a.g.
        73 00 3D 00 22 00 55 00 6E 00 6D 00 61 00 6E 00   // s.=.".U.n.m.a.n.
        61 00 67 00 65 00 64 00 43 00 6F 00 64 00 65 00   // a.g.e.d.C.o.d.e.
        22 00 2F 00 3E 00 0A 00 3C 00 2F 00 50 00 65 00   // "./.>...<./.P.e.
        72 00 6D 00 69 00 73 00 73 00 69 00 6F 00 6E 00   // r.m.i.s.s.i.o.n.
        53 00 65 00 74 00 3E 00 0A 00                   ) // S.e.t.>...

    }
    
  }
