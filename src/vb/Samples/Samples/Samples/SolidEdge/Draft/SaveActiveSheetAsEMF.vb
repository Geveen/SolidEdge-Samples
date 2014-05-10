﻿Imports ApiSamples.Samples.SolidEdge
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms

Namespace ApiSamples.Samples.SolidEdge.Draft
	''' <summary>
	''' Saves the active sheet of the active draft to a EMF file.
	''' </summary>
	Friend Class SaveActiveSheetAsEMF
		Private Shared Sub RunSample(ByVal breakOnStart As Boolean)
			If breakOnStart Then
				System.Diagnostics.Debugger.Break()
			End If

			Dim application As SolidEdgeFramework.Application = Nothing
			Dim draftDocument As SolidEdgeDraft.DraftDocument = Nothing
			Dim sheet As SolidEdgeDraft.Sheet = Nothing

			Try
				' Register with OLE to handle concurrency issues on the current thread.
				OleMessageFilter.Register()

				' Connect to or start Solid Edge.
				application = ApplicationHelper.Connect(False)

				' Get a reference to the active draft document.
				draftDocument = application.TryActiveDocumentAs(Of SolidEdgeDraft.DraftDocument)()

				If draftDocument IsNot Nothing Then
					' Get a reference to the active sheet.
					sheet = draftDocument.ActiveSheet

					Dim dialog As New SaveFileDialog()

					' Set a default file name
					dialog.FileName = Path.ChangeExtension(sheet.Name, ".emf")
					dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
					dialog.Filter = "Enhanced Metafile (*.emf)|*.emf"

					If dialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
						' Save the sheet as an EMF file.
						sheet.SaveAsEMF(dialog.FileName)

						Console.WriteLine("Created '{0}'", dialog.FileName)
					End If
				Else
					Throw New System.Exception(Resources.NoActiveDraftDocument)
				End If
			Catch ex As System.Exception
				Console.WriteLine(ex.Message)
			Finally
				OleMessageFilter.Unregister()
			End Try
		End Sub
	End Class
End Namespace
