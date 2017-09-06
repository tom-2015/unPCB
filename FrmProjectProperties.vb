Imports System.Windows.Forms

Public Class FrmProjectProperties

    Dim m_PCB As PCB

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        m_PCB.Name = TxtProjectName.Text
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub FrmProjectProperties_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        TxtProjectName.Text = m_PCB.Name
        For Each Library As Eagle.Project In m_PCB.Libraries
            LstLibraries.Items.Add(New ListBoxItem(Of Eagle.Project)(Library.ShortFileName, Library))
        Next
    End Sub

    Public Sub New(ByVal PCB As PCB)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_PCB = PCB
    End Sub

    Private Sub CmdUnloadLibrary_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdUnloadLibrary.Click
        If LstLibraries.SelectedIndex > -1 Then
            Dim Library As Eagle.Project = CType(LstLibraries.Items(LstLibraries.SelectedIndex), ListBoxItem(Of Eagle.Project)).Value
            If m_PCB.RemoveEagleLibrary(Library.ShortFileName) Then
                m_PCB.AddUndoItem(New UndoRedoRemoveLibrary(m_PCB, Library))
                LstLibraries.Items.Remove(LstLibraries.Items(LstLibraries.SelectedIndex))
            Else
                MsgBox("Library in use, can't delete now.")
            End If
        End If
    End Sub
End Class
