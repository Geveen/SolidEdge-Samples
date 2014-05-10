﻿Imports ApiSamples.Samples.SolidEdge
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace ApiSamples.Samples.SolidEdge.Part
	''' <summary>
	''' Saves the active part as a JT file.
	''' </summary>
	Friend Class SaveAsJT
		Private Shared Sub RunSample(ByVal breakOnStart As Boolean)
			If breakOnStart Then
				System.Diagnostics.Debugger.Break()
			End If

			Dim application As SolidEdgeFramework.Application = Nothing
			Dim document As SolidEdgePart.PartDocument = Nothing

			Try
				' Register with OLE to handle concurrency issues on the current thread.
				OleMessageFilter.Register()

				' Connect to Solid Edge.
				application = ApplicationHelper.Connect(True, True)

				' Get a reference to the active document.
				document = application.TryActiveDocumentAs(Of SolidEdgePart.PartDocument)()

				If document IsNot Nothing Then
					SolidEdgeDocumentHelper.SaveAsJT(document)
				Else
					Throw New System.Exception(Resources.NoActivePartDocument)
				End If
			Catch ex As System.Exception
				Console.WriteLine(ex.Message)
			Finally
				OleMessageFilter.Unregister()
			End Try
		End Sub
	End Class
End Namespace
