Imports System
Imports Microsoft.VisualBasic

Module AssignmentStatementsB

    Sub main()

        Dim b As Byte = 0
        Dim ch As Char = "a"
        Dim i As Integer = 0
        Dim str As String = "Hello "

        b += 1
        If b <> CByte(1) Then
            Throw New Exception("#ASB1-Assignment Statement Failed")
        End If

        b += i
        If b <> CByte(1) Then
            Throw New Exception("#ASB2-Assignment Statement Failed")
        End If

        b += CByte(i)
        If b <> CByte(1) Then
            Throw New Exception("#ASB3-Assignment Statement Failed")
        End If

        ch += ChrW(65)
        If ch <> CChar("aA") Then
            Throw New Exception("#ASB4-Assignment Statement Failed")
        End If

        str &= "World"
        If str <> "Hello World" Then
            Throw New Exception("#ASB5-Assignment Statement Failed")
        End If

        i += "12"
        If i <> 12 Then
            Throw New Exception("#ASB6-Assignment Statement Failed")
        End If

    End Sub

End Module
