Imports System

Module ForB

    Class C1
        Public index As Integer = 0

        Sub x()
            For index = 0 To 2
                Console.WriteLine(index)
            Next
        End Sub

    End Class

    Sub main()
        Dim c As New C1()
	c.x ()
        If c.index <> 3 Then
            Throw New Exception("#ForB1")
        End If
    End Sub

End Module
