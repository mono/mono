'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 23
REM ExpectedError: BC30266
REM ErrorMessage: 'Private Overrides Sub fun1(i As Integer)' cannot override 'Public Overridable Sub fun1(i As Integer)' because they have different access levels.

'Inheritance of the members with no scope are done in default public..

Class A
	Overridable Sub fun(ByVal i As Integer)
	End Sub
	Overridable Sub fun1(ByVal i As Integer)
	End Sub
End Class

Class B
	Inherits A
	Public Overrides Sub fun(i As Integer)
	End Sub
	Private Overrides Sub fun1(i As Integer)
	End Sub
End Class

Module Default1	
	Sub Main()
	End Sub
End Module
