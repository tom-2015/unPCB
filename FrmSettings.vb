Imports System.Windows.Forms
Imports System.Xml

Public Class FrmSettings

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Dim LibraryNode As XmlNode = FrmMain.ProgramSettings.SelectSingleNode("unPCB/libraries")

        If TxtAutoSaveInterval.Value <= 0 Then
            MsgBox("Auto save interval must be > 0.")
            Exit Sub
        End If

        LibraryNode.RemoveAll()
        For Each Item As String In LstLibraries.Items
            LibraryNode.AppendChild(FrmMain.ProgramSettings.CreateElement("library")).InnerText = Item
        Next
        FrmMain.ProgramSettings.SelectSingleNode("unPCB/auto_save").InnerText = ChAutoSave.Checked
        FrmMain.ProgramSettings.SelectSingleNode("unPCB/auto_save_interval").InnerText = TxtAutoSaveInterval.Value * 60
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub FrmSettings_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim LibraryNodes As XmlNodeList = FrmMain.ProgramSettings.SelectNodes("unPCB/libraries/library")
        For Each LibraryNode As XmlNode In LibraryNodes
            LstLibraries.Items.Add(LibraryNode.InnerText)
        Next
        ChAutoSave.Checked = FrmMain.ProgramSettings.SelectSingleNode("unPCB/auto_save").InnerText
        TxtAutoSaveInterval.Value = CInt(FrmMain.ProgramSettings.SelectSingleNode("unPCB/auto_save_interval").InnerText) \ 60
    End Sub

    Private Sub CmdAddLibrary_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdAddLibrary.Click
        OpenFileDialog.Multiselect = True
        If OpenFileDialog.ShowDialog() Then
            For Each FileName As String In OpenFileDialog.FileNames
                AddLibrary(FileName)
            Next
        End If
    End Sub

    Private Function LibraryExists(ByVal FileName As String) As Boolean
        Dim ShortFileName As String = System.IO.Path.GetFileName(FileName)
        For Each Item As String In LstLibraries.Items
            If System.IO.Path.GetFileName(Item) = shortfilename Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub AddLibrary(ByVal FileName As String)
        Dim Project As New Eagle.Project
        If Not Project.Load(FileName) Then
            MsgBox("Invalid Eagle project! Must be saved by a newer eagle version first. " & FileName, MsgBoxStyle.Critical)
            Exit Sub
        End If
        If Project.Drawing.DrawingType <> Eagle.DrawingType.library Then
            MsgBox("This is not an eagle library (schematic or something else)! " & FileName)
            Exit Sub
        End If
        If LibraryExists(FileName) Then
            MsgBox("Another library with the same name " & System.IO.Path.GetFileName(FileName) & " is already added, remove that first!")
            Exit Sub
        End If
        LstLibraries.Items.Add(FileName)
    End Sub

    Private Sub CmdDel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdDel.Click
        If LstLibraries.SelectedIndex > -1 Then
            LstLibraries.Items.RemoveAt(LstLibraries.SelectedIndex)
        End If
    End Sub
End Class
