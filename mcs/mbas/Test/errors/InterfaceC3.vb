REM LineNo: 12
REM ExpectedError: BC30602
REM ErrorMessage: Interface members must be methods, properties, events, or type definitions.

REM LineNo: 13
REM ExpectedError: BC30429
REM ErrorMessage: 'End Sub' must be preceded by a matching 'Sub'.

Imports System
Interface I
	Sub S()
	   Dim a As Integer
	End Sub
End Interface
Module InterfaceC3
	Sub Main()
		
	End Sub
End Module
