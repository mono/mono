Imports System

Module ArrayE

    Dim a As Integer() = {1, 2, 3, 4}

    Public Property myprop() As Integer()
        Get
            Return a
        End Get
        Set(ByVal Value As Integer())
            a = Value
        End Set
    End Property

    Sub main()

        ReDim Preserve myprop(6)
        Dim arr As Integer() = {1, 2, 3, 4, 10, 12, 14}
        myprop(4) = 10
        myprop(5) = 12
        myprop(6) = 14

        For i As Integer = 0 to myprop.Length - 1
            Console.WriteLine(myprop(i))
            If myprop(i) <> arr(i) Then
                Throw New Exception("#AE1 - Array Statement failed")
            End If
        Next

    End Sub

End Module