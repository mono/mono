REM LineNo: 19
REM ExpectedError: BC30524
REM ErrorMessage: Property 'mystr' is 'WriteOnly'.

Imports System
Imports Microsoft.VisualBasic

Module AssignmentStatementsC4

    Private str As String = "Hello VB.NET World"
    Public WriteOnly Property mystr() As String
        Set(ByVal Value As String)
            str = Value
        End Set
    End Property

    Sub main()

        Mid(mystr, 7) = "MS.NET"
        If mystr <> "Hello MS.NETBasic " Then
            Throw New Exception("#ASC41 - Assignment Statement failed")
        End If

    End Sub

End Module
