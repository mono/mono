REM LineNo: 8
REM ExpectedError: BC30501
REM ErrorMessage: 'Shared' cannot be combined with 'MustOverride' on a method declaration.

Imports System

MustInherit Class C3
              Public Shared MustOverride Sub F()
End Class

Module MustInheritTest
	Sub Main()
		
	End Sub
End Module
