'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionStringtoDateA
	Sub Main()
			Dim a as Date
			Dim b as String= "1/1/0001 12:00:00 AM"
			a = b
			if a <> "1/1/0001 12:00:00 AM"
				Throw new System.Exception("Conversion of String to Date not working. Expected 1/1/0001 12:00:00 AM but got " &a) 
			End if		
	End Sub
End Module
