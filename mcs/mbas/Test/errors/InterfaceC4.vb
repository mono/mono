REM LineNo: 7
REM ExpectedError: BC30270
REM ErrorMessage: 'Public' is not valid on an interface method declaration.

Imports System
Interface I
	Public Sub S()
End Interface
Module InterfaceC4
	Sub Main()
	End Sub
End Module
