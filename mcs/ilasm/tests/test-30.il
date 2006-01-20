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

           .class public EventTest {

                  .method public void Subscribe ()
                  {
                        ldstr "Mono.ILASM.Tests.EventTest::Subscribe ()"
                        call void [mscorlib]System.Console::WriteLine (string)
                  }

                  .method public void Unsubscribe ()
                  {
                        ldstr "Mono.ILASM.Tests.EventTest::Unsubscribe ()"
                        call void [mscorlib]System.Console::WriteLine (string)
                  }

                  .method public void Raise ()
                  {
                        ldstr "Mono.ILASM.Tests.EventTest::Raise ()"
                        call void [mscorlib]System.Console::WriteLine (string)
                  }

                  .event [mscorlib]System.Delegate LeEvent
                  {
                        .addon instance void Mono.ILASM.Tests.EventTest::Subscribe ()
                        .removeon instance void Mono.ILASM.Tests.EventTest::Unsubscribe ()
                        .fire instance void Mono.ILASM.Tests.EventTest::Raise ()
                  }
           }
}
