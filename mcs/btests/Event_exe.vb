Imports System
Imports NSEvent

NameSpace NSEvent
Class C1
	Inherits C

        Sub call_S()
                S()
        End Sub
                                                                                
        Sub EH(i as Integer, y as string) Handles MyBase.E
                Console.WriteLine("event-H called")
        End Sub
End Class

End NameSpace

Module M
        Sub Main()
                dim y as new C1 ()
                y.call_S()
        End Sub
End Module
