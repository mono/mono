Imports System

Module ForB
    Class C1
        Sub x()
            For index as integer = 0 To 2
               Console.WriteLine(index)
            Next

        End Sub

    End Class

    Sub main()
        Dim c As New C1()
        c.x()
    End Sub

End Module
