'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDatetoStringB
	Sub Main()
			Dim a as Date = "1/1/0001 12:00:00 AM"
			Dim b as String = "hello" + a
			if b <> "hello12:00:00 AM"
				Throw new System.Exception("Concat of Date & String not working. Expected helloa but got " &b) 
			End if		
	End Sub
End Module



