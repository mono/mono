REM LineNo: 18
REM ExpectedError: BC31408
REM ErrorMessage: 'Private' and 'MustOverride' cannot be combined.

REM LineNo: 19
REM ExpectedError: BC30188
REM ErrorMessage: Declaration expected.

REM LineNo: 20
REM ExpectedError: BC30430
REM ErrorMessage: 'End Function' must be preceded by a matching 'Function'.

Imports System

'Testing a mustoverride method with a method body

MustInherit Class C3
    Private MustOverride Function F2() As Integer
        Console.WriteLine("If you see this then there is something wrong!!!")
    End Function
End Class

Module MustInheritTest
	Sub Main()
	End Sub
End Module

