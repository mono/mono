'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionShorttoStringA
	Sub Main()
			Dim a as Short = 123
			Dim b as String= a.toString()
			if b <> "123"
				Throw new System.Exception("Conversion of Short to String not working. Expected 123 but got " &b) 
			End if		
	End Sub
End Module
