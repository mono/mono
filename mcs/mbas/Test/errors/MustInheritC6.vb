REM LineNo: 10
REM ExpectedError: BC31408
REM ErrorMessage: 'Private' and 'MustOverride' cannot be combined.

Imports System

'Testing a mustoverride method with a method body

MustInherit Class C3
    Private MustOverride Function F2() As Integer
End Class

Module MustInheritTest
	Sub Main()
	End Sub
End Module

