'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 21
REM ExpectedError: BC30398
REM ErrorMessage: 'Public Overrides Sub fun1(ByRef i As Integer)' cannot override 'Public Overridable Sub fun1(i As Integer)' because they differ by a parameter that is marked as 'ByRef' versus 'ByVal'.

Class A
	Public Overridable Sub fun(i As Integer)
	End Sub
	Public Overridable Sub fun1(i As Integer)
	End Sub
End Class

Class B
	Inherits A
	Public Overrides Sub fun(ByVal i As Integer)
	End Sub
	Public Overrides Sub fun1(ByRef i As Integer)
	End Sub
End Class

Module Default1	
	Sub Main()
	End Sub
End Module
