REM LineNo: 18
REM ExpectedError: BC30524
REM ErrorMessage: Property 'myprop' is 'WriteOnly'

Imports System

Module ArrayC5

    Dim a As Integer() = {1, 2, 3, 4}
    Public WriteOnly Property myprop() As Integer()
        Set(ByVal Value As Integer())
            a = Value
        End Set
    End Property

    Sub Main()

        ReDim Preserve myprop(5)
        myprop(4) = 10
        For i As Integer = 0 to myprop.Length - 1
            Console.WriteLine(myprop(i))
        Next

    End Sub
End Module
