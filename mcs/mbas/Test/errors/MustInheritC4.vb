REM LineNo: 9
REM ExpectedError: BC31408
REM ErrorMessage: 'Private' and 'MustOverride' cannot be combined.

Imports System

'Testing a private mustoverride method
MustInherit Class C1
    Private MustOverride Function F2() As Integer
End Class

Module MustInheritTest
	Sub Main()
		
	End Sub
End Module
