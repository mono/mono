'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionSingletoStringA
	Sub Main()
			Dim a as Single= 123.052
			Dim b as String= a
			if b <> "123.052"
				Throw new System.Exception("Conversion of Single to String not working. Expected 123.052 but got " &b) 
			End if		
	End Sub
End Module
