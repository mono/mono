'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionSingletoLongC
	Sub Main()
			Dim a as Single= 111
			Dim b as Long = 111 + a
			if b <> 222
				Throw new System.Exception("Addition of Single & Long not working. Expected 222 but got " &b) 
			End if		
	End Sub
End Module
