﻿Imports ApiSamples.Samples.SolidEdge
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace ApiSamples.Samples.SolidEdge.SheetMetal
	''' <summary>
	''' Creates a new sheetmetal with a base tab.
	''' </summary>
	Friend Class CreateBaseTab
		Private Shared Sub RunSample(ByVal breakOnStart As Boolean)
			If breakOnStart Then
				System.Diagnostics.Debugger.Break()
			End If

			Dim application As SolidEdgeFramework.Application = Nothing
			Dim documents As SolidEdgeFramework.Documents = Nothing
			Dim sheetMetalDocument As SolidEdgePart.SheetMetalDocument = Nothing
			Dim model As SolidEdgePart.Model = Nothing
			Dim tabs As SolidEdgePart.Tabs = Nothing
			Dim tab As SolidEdgePart.Tab = Nothing
			Dim selectSet As SolidEdgeFramework.SelectSet = Nothing

			Try
				' Register with OLE to handle concurrency issues on the current thread.
				OleMessageFilter.Register()

				' Connect to or start Solid Edge.
				application = ApplicationHelper.Connect(True, True)

				' Make sure user can see the GUI.
				application.Visible = True

				' Bring Solid Edge to the foreground.
				application.Activate()

				' Get a reference to the Documents collection.
				documents = application.Documents

				' Create a new SheetMetal document.
				sheetMetalDocument = documents.AddSheetMetalDocument()

				' Always a good idea to give SE a chance to breathe.
				application.DoIdle()

				' Call helper method to create the actual geometry.
				model = SheetMetalHelper.CreateBaseTab(sheetMetalDocument)

				' Get a reference to the Tabs collection.
				tabs = model.Tabs

				' Get a reference to the new Tab.
				tab = tabs.Item(1)

				' Get a reference to the ActiveSelectSet.
				selectSet = application.ActiveSelectSet

				' Empty ActiveSelectSet.
				selectSet.RemoveAll()

				' Add new Tab to ActiveSelectSet.
				selectSet.Add(tab)

				' Switch to ISO view.
				application.StartCommand(SolidEdgeConstants.SheetMetalCommandConstants.SheetMetalViewISOView)
			Catch ex As System.Exception
				Console.WriteLine(ex.Message)
			Finally
				OleMessageFilter.Unregister()
			End Try
		End Sub
	End Class
End Namespace
