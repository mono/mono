Namespace nms1
    Module NSB
        Public Function f() As Integer
            Dim c As NSA = New NSA()
            Return c.z
        End Function

        Sub Main()
            Dim i As Integer = f()
            If i <> 5 Then
                Throw New System.Exception("value of nms1.NSA.z got changed")
            End If
        End Sub
    End Module
End Namespace