'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 16
REM ExpectedError: BC40004
REM ErrorMessage: event 'E' conflicts with event 'E' in the base class 'C' and so should be declared 'Shadows'.

Class C
        Public Event E
End Class

Class C1
	Inherits C
        Private Event E
End Class

Module A
	Sub Main()
	End Sub
End Module
