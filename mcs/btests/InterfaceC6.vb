REM LineNo: 34
REM ExpectedError: BC30149
REM ErrorMessage: 'C1' must implement 'Sub S(a As Integer, [b As Integer = 20])' for interface 'I'.

REM LineNo: 35
REM ExpectedError: BC30401
REM ErrorMessage: 'S' cannot implement 'S' because there is no matching sub on interface 'I'.

REM LineNo: 37
REM ExpectedError: BC30300
REM ErrorMessage: 'Public Sub S(ByRef a As Integer)' and 'Public Sub S(a As Integer, [b As Integer = 30])' cannot overload each other because they differ only by optional parameters.

REM LineNo: 37
REM ExpectedError: BC30345
REM ErrorMessage: 'Public Sub S(ByRef a As Integer)' and 'Public Sub S(a As Integer, [b As Integer = 30])' cannot overload each other because they differ only by parameters declared 'ByRef' or 'ByVal'.

REM LineNo: 37
REM ExpectedError: BC30401
REM ErrorMessage: 'S' cannot implement 'S' because there is no matching sub on interface 'I'.

REM LineNo: 39
REM ExpectedError: BC30401
REM ErrorMessage: 'S' cannot implement 'S' because there is no matching sub on interface 'I'.

REM LineNo: 44
REM ExpectedError: BC30583
REM ErrorMessage: 'I.S' cannot be implemented more than once.

Imports System
Interface I
	Sub S(byVal a As Integer,Optional b As Integer=20)	
End Interface
Class C1
	Implements I
	Sub S() Implements I.S
	End Sub
	Sub S(byRef a As Integer) Implements I.S
	End Sub
	Sub S(byVal a as Integer, Optional b As Integer=30) Implements I.S
	End Sub
End Class
Class C2 	'Class implements the same method more than once
	Implements I
	Public Sub S(byVal a As Integer,Optional b As Integer=20) Implements I.S
	End Sub
	Public Sub H(byVal a As Integer,Optional b As Integer=20) Implements I.S
	End Sub
End Class
Module InterfaceC6
	Sub Main()
		
	End Sub
End Module

