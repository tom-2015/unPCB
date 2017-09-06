Imports System.Windows.Forms
Imports System.Reflection

Public Class FrmAbout

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub FrmAbout_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        LblVersion.Text = "V" & Assembly.GetExecutingAssembly().GetName().Version.ToString()
        LinkLabel1.Links.Remove(LinkLabel1.Links(0))
        LinkLabel1.Links.Add(0, LinkLabel1.Text.Length, LinkLabel1.Text)
    End Sub

    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Dim sInfo As New ProcessStartInfo(e.Link.LinkData.ToString())
        Process.Start(sInfo)
    End Sub
End Class
