REM LineNo: 24
REM ExpectedError: BC31034
REM ErrorMessage: Interface 'I' is already implemented by base class 'C1'.

REM LineNo: 25
REM ExpectedError: BC40004
REM ErrorMessage: sub 'S' conflicts with sub 'S' in the base class 'C1' and so should be declared 'Shadows'.

REM LineNo: 25
REM ExpectedError: BC31037
REM ErrorMessage: 'I.S' is already implemented by base class 'C1' and cannot be implemented again.

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
	Implements I
	Public Sub S() Implements I.S
	End Sub
End Class
Module InterfaceC7
	Sub Main()
	End Sub
End Module
