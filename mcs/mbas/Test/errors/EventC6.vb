'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 22
REM ExpectedError: BC30585
REM ErrorMessage: Event 'E' cannot be handled because it is not accessible from 'Class C1'.

Class C
        Private Event E
        Public Sub S()
                RaiseEvent E
        End Sub
End Class

Class C1
        Inherits C
        Sub call_S()
               S()
        End Sub
        Sub EH() Handles MyBase.E
        End Sub
End Class
Module A
	Sub Main()
	End Sub
End Module
