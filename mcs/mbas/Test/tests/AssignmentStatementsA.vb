Imports System

Module AssignmentStatementsA

    Sub main()
        Dim i As Integer

        i = 2
        If i <> 2 Then
            Throw New Exception("#ASA1 - Assignment Statement failed")
        End If

        i = 2.3 * 3.45 / 2.3   ' Implicit type conversion
        If i <> 3 Then
            Throw New Exception("#ASA2 - Assignment Statement failed")
        End If

        Dim s As String = 2.3 * 3.45 / 2.3
        If s <> 3.45 Then
            Throw New Exception("#ASA3 - Assignment Statement failed")
        End If

        s = New Date(2004, 8, 17)
        If s <> New Date(2004, 8, 17) Then
            Throw New Exception("#ASA4 - Assignment Statement failed")
        End If

        If s <> "8/17/2004" Then
            Throw New Exception("#ASA5 - Assignment Statement failed")
        End If

        Dim obj As New Object()
        Dim obj1, obj2 As Object
        obj1 = obj
        obj2 = obj
        If Not obj1 Is obj2 Then
            Throw New Exception("#ASA6 - Assignment Statement failed")
        End If

        Dim obj3 As Object
        obj3 = i
        If obj3 <> 3 Then
            Throw New Exception("#ASA7 - Assignment Statement failed")
        End If

        i = 12
        i = obj3
        If i <> 3 Then
            Throw New Exception("#ASA8 - Assignment Statement failed")
        End If

    End Sub

End Module
