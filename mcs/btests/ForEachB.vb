Imports System

Module ForEachB

    Class C1
'        public index As Integer = 0

        Sub w()
            Dim arr() As Integer = {1, 2, 3}
            For Each index as integer In arr
                Console.WriteLine(index)
            Next
        End Sub
    End Class

    Sub main()
        Dim c As New C1()
        c.w()
'	If c.index <> 3 Then
 '           Throw New Exception("#FEB1")
  '      End If
    End Sub

End Module
