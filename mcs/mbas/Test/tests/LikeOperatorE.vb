'Author:
'   K. Satya Sudha (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
                                                                                                               
Imports System
Module Test
        Sub Main()
                Dim a As Boolean = "7" Like "[!0-6]"
                if a <> true then
                        throw new Exception ("#A1 - Like operator failed")
                end if
		
		a = "7" Like "[][!0-6][]"
                if a <> true then
                        throw new Exception ("#A2 - Like operator failed")
                end if

		a = "7" Like "[][!06][]"
                if a <> true then
                        throw new Exception ("#A3 - Like operator failed")
                end if

		a = "6" Like "[][0-6!8-9][]"
                if a <> true then
                        throw new Exception ("#A4 - Like operator failed")
                end if

		a = "67" Like "[!0-58-9][567]"
                if a <> true then
                        throw new Exception ("#A5 - Like operator failed")
                end if

		a = "7" Like "[0-5!]"
                if a <> true then
                        throw new Exception ("#A6 - Like operator failed")
                end if

		a = "6" Like "[][0-6!8-9][]"
                if a <> true then
                        throw new Exception ("#A7 - Like operator failed")
                end if

        End Sub
End Module

