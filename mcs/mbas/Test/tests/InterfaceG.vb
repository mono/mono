Imports System

Interface I
	Sub F1()
	Sub F2()
End Interface

Class C1
   Implements I
	Public Sub F() Implements I.F1,I.F2
	End Sub
End Class

Module InterfaceG
	Sub Main()
		Dim C As New C1()
	End Sub
End Module
