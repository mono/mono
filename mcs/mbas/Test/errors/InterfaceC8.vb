REM LineNo: 16
REM ExpectedError: BC31035
REM ErrorMessage: Interface 'I' is not implemented by this class.

Imports System
Interface I
	Sub S()
End Interface
Class C1
	Implements I
	Public Sub S() Implements I.S
	End Sub
End Class
Class C2
	Inherits C1
	Public Sub S() Implements I.S
	End Sub
End Class
Module InterfaceC7
	Sub Main()
	End Sub
End Module
