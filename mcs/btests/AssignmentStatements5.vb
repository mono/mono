
Imports System

Module AssignmentStatements5

    Private str As String = "Hello VB.NET World"

    Public ReadOnly Property mystr() As String
        Get
            Return str
        End Get
    End Property

    Sub main()

        Mid(mystr, 7) = "MS.NET"
        Console.WriteLine(mystr)
        If mystr <> "Hello MS.NETBasic " Then
            Throw New Exception("#AS5 - Assignment Statement failed")
        End If

    End Sub

End Module
