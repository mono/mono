REM LineNo: 21
REM ExpectedError: BC31408
REM ErrorMessage: 'Private' and 'MustOverride' cannot be combined.

REM LineNo: 24
REM ExpectedError: BC30610
REM ErrorMessage: Class 'C2' must either be declared 'MustInherit' or override the following inherited 'MustOverride' member(s): Public MustOverride Function F2() As Integer.

REM LineNo: 26
REM ExpectedError: BC30284
REM ErrorMessage: function 'F' cannot be declared 'Overrides' because it does not override a function in a base class.

REM LineNo: 32
REM ExpectedError: BC30501
REM ErrorMessage: 'Shared' cannot be combined with 'MustOverride' on a method declaration.

Imports System

'Testing a private mustoverride method
MustInherit Class C1
    Private MustOverride Function F2() As Integer
End Class

Class C2
    Inherits C1
	Private Overrides Function F() As Integer
	End Function
End Class

'Testing a shared mustoverride method 
MustInherit Class C3
              Public Shared MustOverride Sub F()
End Class

Module MustInheritTest
	Sub Main()
		
	End Sub
End Module
