Imports System
Interface I
	Sub S(byVal a As Integer,Optional b As Integer=20)	
End Interface
Class C1
	Implements I
	Sub S() Implements I.S
	End Sub
	Sub S(byRef a As Integer) Implements I.S
	End Sub
	Sub S(byVal a as Integer, Optional b As Integer=30) Implements I.S
	End Sub
End Class
Class C2 	'Class implements the same method more than once
	Implements I
	Public Sub S(byVal a As Integer,Optional b As Integer=20) Implements I.S
	End Sub
	Public Sub H(byVal a As Integer,Optional b As Integer=20) Implements I.S
	End Sub
End Class
Module InterfaceC6
	Sub Main()
		
	End Sub
End Module

