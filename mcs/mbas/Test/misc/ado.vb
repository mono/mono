Imports System
Imports System.Data
Imports System.Data.SqlClient

Module Module1

    Sub Main()
        Dim con As SqlConnection
        Dim cmd As SqlCommand
        Dim reader As SqlDataReader
        Dim firstName As String

        con = New SqlConnection("Server=localhost;database=pubs;user id=someuser;password=somepass")
        con.Open()
        cmd = con.CreateCommand()
        cmd.CommandText = "select fname from employee"
        reader = cmd.ExecuteReader()
        If (reader.Read()) Then
            firstName = reader.GetString(0)
            Console.WriteLine(firstName)
        Else
            Console.WriteLine("No data returned.")
        End If
    End Sub

End Module
