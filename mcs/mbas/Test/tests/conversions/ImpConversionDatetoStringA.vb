'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.
Imports System
Module ImpConversionDatetoStringA
	Sub Main()
			Dim a as Date= "1/1/0001 12:00:00 AM"
			Dim b as String= a
			if b <> "12:00:00 AM"
				Throw new System.Exception("Conversion of Date to String not working. Expected 12:00:00 AM but got " &b) 
			End if		
			Console.WriteLine("Value of b is {0}", b)	
	End Sub
End Module
