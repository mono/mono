REM LineNo: 8
REM ExpectedError: BC30479
REM ErrorMessage: Shared 'Sub New' cannot have any parameters.

Imports System

Class A
	Shared Sub New(i as integer)
	End Sub
End Class

Module M
	Sub Main()
	End Sub
End Module
