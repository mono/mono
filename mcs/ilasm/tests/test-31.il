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
                        nop
                  }

                  .method public void set_foo (int32)
                  {
                        nop
                  }

                  .method public void other_foo ()
                  {
                        
                  }

                  .property int32 foo (int32) = int32 (0xFFFFFFFF)
                  {
                        .get instance int32 Mono.ILASM.Tests.PropertyTest::get_foo ()
                        .set instance void Mono.ILASM.Tests.PropertyTest::set_foo (int32)
                        .other instance void Mono.ILASM.Tests.PropertyTest::other_foo () 
                  }
           }
}
