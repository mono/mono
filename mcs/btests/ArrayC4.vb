REM LineNo: 18
REM ExpectedError: BC30526
REM ErrorMessage: Property 'myprop' is 'ReadOnly'

Imports System

Module ArrayC4

    Dim a As Integer() = {1, 2, 3, 4}

    Public ReadOnly Property myprop() As Integer()
        Get
            Return a
        End Get
    End Property

    Sub main()
        ReDim Preserve myprop(6)
        myprop(4) = 10
        myprop(5) = 12     
    End Sub

End Module