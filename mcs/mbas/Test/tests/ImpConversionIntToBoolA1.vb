Imports System
Imports System.Console
Module AssignmentStatements1
   Sub main()

	 Dim a As Integer = 1
       Try
       Dim o As Boolean=a 
       Console.WriteLine(o.GetType().ToString() & " = " & o)
       Catch e As System.Exception

WriteLine("Runtime Exception occured-->See Stack traces below.....")
WriteLine ("Runtime exception-->" &e.GetType.Name)
WriteLine(e)
End Try
    End Sub
End Module