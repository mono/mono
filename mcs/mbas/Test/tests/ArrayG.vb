Imports System

Module ArrayG

    Sub Main()
        Dim arr As Integer(,) = {{1, 2, 3}, {3, 4, 7}}
        ReDim arr(-1, -1)
        If arr.Length <> 0 Then
            Throw New Exception("#AG1 - ReDim Statement failed")
        End If

        If arr Is Nothing Then
            Throw New Exception("#AG2 - ReDim Statement failed")
        End If

        Erase arr
        If Not arr Is Nothing Then
            Throw New Exception("#AG3 - Erase Statement failed")
        End If
    End Sub

End Module
