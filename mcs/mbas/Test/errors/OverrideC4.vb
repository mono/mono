REM LineNo: 31
REM ExpectedError: BC30284
REM ErrorMessage: sub 'S' cannot be declared 'Overrides' because it does not override a sub in a base class.

REM LineNo: 31
REM ExpectedError: BC40003
REM ErrorMessage: sub 'S' shadows an overloadable member declared in the base class 'C1'.  If you want to overload the base method, this method must be declared 'Overloads'.

REM LineNo: 33
REM ExpectedError: BC30266
REM ErrorMessage: 'Private Overrides Sub S(a As Integer)' cannot override 'Public Overridable Sub S(a As Integer)' because they have different access levels.

REM LineNo: 35
REM ExpectedError: BC30398
REM ErrorMessage: 'Public Overrides Sub S(ByRef a As Integer)' cannot override 'Public Overridable Sub S(a As Integer)' because they differ by a parameter that is marked as 'ByRef' versus 'ByVal'.

REM LineNo: 37
REM ExpectedError: BC30284
REM ErrorMessage: sub 'S' cannot be declared 'Overrides' because it does not override a sub in a base class.

REM LineNo: 37
REM ExpectedError: BC40003
REM ErrorMessage: sub 'S' shadows an overloadable member declared in the base class 'C1'.  If you want to overload the base method, this method must be declared 'Overloads'.

Class C1
	Public Overridable Sub S(byVal a As Integer)
	End Sub
End Class
Class C2
   Inherits C1
	Public Overrides Sub S()
	End Sub
	Private Overrides Sub S(byVal a As Integer)
	End Sub
	Public Overrides Sub S(byRef a As Integer)
	End Sub
	Public Overrides Sub S(byVal a As String)
	End Sub
End Class
Module OverrideC2
	Sub Main()
	End Sub
End Module
