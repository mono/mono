Imports System
Imports Microsoft.VisualBasic

Module AssignmentStatementsC

    Private str As String = "Hello VB.NET World"
    Public Property mystr() As String
        Get
            Return str
        End Get
        Set(ByVal Value As String)
            str = Value
        End Set
    End Property

    Sub main()

        Mid(str, 7) = "MS.NET"
        If str <> "Hello MS.NET World" Then
            Throw New Exception("#ASC1 - Assignment Statement failed")
        End If

        Mid(str, 7, 5) = "ASP.NET"
        If str <> "Hello ASP.NT World" Then
            Throw New Exception("#ASC2 - Assignment Statement failed")
        End If

        Mid(str, 7) = "VisualBasic .NET World"
        If str <> "Hello VisualBasic " Then
            Throw New Exception("#ASC3 - Assignment Statement failed")
        End If

        Mid(str, 7) = 2.5 * 34.59
        If str <> "Hello 86.475Basic " Then
            Throw New Exception("#ASC4 - Assignment Statement failed")
        End If

        Mid(mystr, 7) = "MS.NET"
        If mystr <> "Hello MS.NETBasic " Then
            Throw New Exception("#ASC5 - Assignment Statement failed")
        End If

    End Sub

End Module
