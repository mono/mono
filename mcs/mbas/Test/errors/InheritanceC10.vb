'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 20
REM ExpectedError: BC30590
REM ErrorMessage: Event 'E' cannot be found.

Class C
        Public Event E
        Public Sub S()
                RaiseEvent E
        End Sub
End Class

Class C1
        Sub call_S()              
        End Sub
        Sub EH() Handles MyBase.E
        End Sub
End Class
Module A
	Sub Main()
	End Sub
End Module
