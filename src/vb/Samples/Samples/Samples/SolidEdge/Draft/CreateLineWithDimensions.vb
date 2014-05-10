﻿Imports ApiSamples.Samples.SolidEdge
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace ApiSamples.Samples.SolidEdge.Draft
	''' <summary>
	''' Creates a new draft with a line and dimensions.
	''' </summary>
	Friend Class CreateLineWithDimensions
		Private Shared Sub RunSample(ByVal breakOnStart As Boolean)
			If breakOnStart Then
				System.Diagnostics.Debugger.Break()
			End If

			Dim application As SolidEdgeFramework.Application = Nothing
			Dim documents As SolidEdgeFramework.Documents = Nothing
			Dim draftDocument As SolidEdgeDraft.DraftDocument = Nothing
			Dim sheet As SolidEdgeDraft.Sheet = Nothing
			Dim lines2d As SolidEdgeFrameworkSupport.Lines2d = Nothing
			Dim line2d As SolidEdgeFrameworkSupport.Line2d = Nothing
			Dim dimensions As SolidEdgeFrameworkSupport.Dimensions = Nothing
			Dim dimension As SolidEdgeFrameworkSupport.Dimension = Nothing
			Dim dimStyle As SolidEdgeFrameworkSupport.DimStyle = Nothing

			Try
				' Register with OLE to handle concurrency issues on the current thread.
				OleMessageFilter.Register()

				' Connect to or start Solid Edge.
				application = ApplicationHelper.Connect(True, True)

				' Get a reference to the documents collection.
				documents = application.Documents

				' Create a new draft document.
				draftDocument = documents.AddDraftDocument()

				' Get a reference to the active sheet.
				sheet = draftDocument.ActiveSheet

				' Get a reference to the Lines2d collection.
				lines2d = sheet.Lines2d

				' Draw a new line.
				line2d = lines2d.AddBy2Points(x1:= 0.2, y1:= 0.2, x2:= 0.3, y2:= 0.2)

				' Get a reference to the Dimensions collection.
				dimensions = DirectCast(sheet.Dimensions, SolidEdgeFrameworkSupport.Dimensions)

				' Add a dimension to the line.
				dimension = dimensions.AddLength(line2d)

				' Get a reference to the dimension style.
				' DimStyle has a ton of options...
				dimStyle = dimension.Style
			Catch ex As System.Exception
				Console.WriteLine(ex.Message)
			Finally
				OleMessageFilter.Unregister()
			End Try
		End Sub
	End Class
End Namespace
