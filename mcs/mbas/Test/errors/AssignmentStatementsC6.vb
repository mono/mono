'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 20
REM ExpectedError:  BC30526
REM ErrorMessage: Property 'Pro' is 'ReadOnly'.

Module Test
    Private PValue As Integer

    Public ReadOnly Property Pro As Integer
        Get
            Return PValue
        End Get
    End Property

    Sub Main()
        Pro = 10        
    End Sub
End Module
