'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 17
REM ExpectedError: BC30266
REM ErrorMessage: 'Public Overrides Sub fun([i As Integer = 7])' cannot override 'Public Overridable Sub fun([i As Integer = 9])' because they differ by the default values of optional parameters.

Class C1
	  Public Overridable Sub fun(Optional i As Integer=9)
	  End Sub
End Class

Class C2
        Inherits C1
        Public Overrides Sub fun(Optional i As Integer=7)
        End Sub
End Class

Module InheritanceM
        Sub Main()            
        End Sub
End Module
