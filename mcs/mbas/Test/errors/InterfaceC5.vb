REM LineNo: 11
REM ExpectedError: BC30149
REM ErrorMessage: 'C1' must implement 'Sub S2()' for interface 'I'.

Imports System
Interface I
	Sub S1()
	Sub S2()
End Interface
Class C1 	'Class C1 does not implement all methods of the interface
	Implements I
	Public Sub S1() Implements I.S1
	End Sub
End Class
Module InterfaceC5
	Sub Main()
	End Sub
End Module
