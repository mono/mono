Imports System
Imports System.Console
Module AssignmentStatements1
   Sub main()

	 Dim a As Decimal = 100.123
       Try
       Dim o As Boolean=CBool(a)   
       Console.WriteLine(o.GetType().ToString() & " = " & o)
       Catch e As System.Exception

WriteLine("Runtime Exception occured-->See Stack traces below.....")
WriteLine ("Runtime exception-->" &e.GetType.Name)
WriteLine(e)
End Try
    End Sub
End Module