Imports System
Imports System.Console
Module AssignmentStatements1
   Sub main()

       Dim o As Boolean= True      
       Try
	 Dim a As Single = o
       Console.WriteLine(a.GetType().ToString() & " = " & a)
       Catch e As System.Exception

WriteLine("Runtime Exception occured-->See Stack traces below.....")
WriteLine ("Runtime exception-->" &e.GetType.Name)
WriteLine(e)
End Try
    End Sub
End Module