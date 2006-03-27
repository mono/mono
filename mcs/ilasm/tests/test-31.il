//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern corlib { }

.namespace Mono.ILASM.Tests {

           .class public PropertyTest {

                  .method public int32 get_foo ()
                  {
                          ldc.i4.1
                          ret
                  }

                  .method public void set_foo (int32)
                  {
                          ret
                  }

                  .method public void other_foo ()
                  {
                          ret
                  }

                  .method static public int32 get_static_foo ()
                  {
                          ldc.i4.1
                          ret
                  }

                  .method static public void set_static_foo (int32)
                  {
                          ret
                  }

                  .method static public void other_static_foo ()
                  {
                          ret
                  }

                  .property int32 foo (int32) = int32 (0xFFFFFFFF)
                  {
                        .get instance int32 Mono.ILASM.Tests.PropertyTest::get_foo ()
                        .set instance void Mono.ILASM.Tests.PropertyTest::set_foo (int32)
                        .other instance void Mono.ILASM.Tests.PropertyTest::other_foo () 
                  }

                  .property int32 static_foo ()
                  {
                          .get int32 Mono.ILASM.Tests.PropertyTest::get_static_foo ()
                          .set int32 Mono.ILASM.Tests.PropertyTest::set_static_foo (int32)
                          .other void Mono.ILASM.Tests.PropertyTest::other_foo ()
                  }
           }
}
