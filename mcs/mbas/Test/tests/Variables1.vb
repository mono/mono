'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Unhandled Exception: System.NullReferenceException: Object variable or With block variable not set.
'This is done mainly to check if a variable is automatically assigned as Object if its type is not specified.

Imports System

Module Default1	
	Sub Main()
		Try
			Dim a
			Console.WriteLine(a.GetTypeCode())
		Catch e as Exception 
			System.Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
