Public Class FrmNew

    Dim TopImage As Image
    Dim BottomImage As Image

    Private Sub CmdBrowseTop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdBrowseTop.Click
        If OpenFile.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            TxtTopLayer.Text = OpenFile.FileName
        End If
    End Sub


    Private Sub CmdBrowseBottom_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdBrowseBottom.Click
        If OpenFile.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            TxtBottomLayer.Text = OpenFile.FileName
        End If
    End Sub

    Private Sub TxtTopLayer_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TxtTopLayer.TextChanged
        If System.IO.File.Exists(TxtTopLayer.Text) Then
            Try
                TopImage = Image.FromFile(TxtTopLayer.Text)
                RefreshTopImage()
            Catch ex As Exception
                MsgBox("Invalid image format for top layer.", MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub TxtBottomLayer_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TxtBottomLayer.TextChanged
        If System.IO.File.Exists(TxtBottomLayer.Text) Then
            Try
                BottomImage = Image.FromFile(TxtBottomLayer.Text)
                RefreshBottomImage()
            Catch ex As Exception
                MsgBox("Invalid image format for bottom layer.", MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub RefreshTopImage()
        If Not TopImage Is Nothing Then
            PctTop.Image = TopImage.Clone()
            If ChFlipTopHorizontal.Checked Then
                PctTop.Image.RotateFlip(RotateFlipType.RotateNoneFlipX)
            End If
            If ChFlipTopVertical.Checked Then
                PctTop.Image.RotateFlip(RotateFlipType.RotateNoneFlipY)
            End If
        End If
        PctTop.Refresh()
    End Sub

    Private Sub RefreshBottomImage()
        If Not BottomImage Is Nothing Then
            PctBottom.Image = BottomImage.Clone()
            If ChFlipBottomHorizontal.Checked Then
                PctBottom.Image.RotateFlip(RotateFlipType.RotateNoneFlipX)
            End If
            If ChFlipBottomVertical.Checked Then
                PctBottom.Image.RotateFlip(RotateFlipType.RotateNoneFlipY)
            End If
        End If
        PctBottom.Refresh()
    End Sub

    Private Sub ChFlipTopHorizontal_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChFlipTopHorizontal.CheckedChanged
        RefreshTopImage()
    End Sub

    Private Sub ChFlipTopVertical_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChFlipTopVertical.CheckedChanged
        RefreshTopImage()
    End Sub

    Private Sub ChFlipBottomHorizontal_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChFlipBottomHorizontal.CheckedChanged
        RefreshBottomImage()
    End Sub

    Private Sub ChFlipBottomVertical_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChFlipBottomVertical.CheckedChanged
        RefreshBottomImage()
    End Sub

    Private Sub CmdCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdCreate.Click
        TxtProjectName.Text = Trim(TxtProjectName.Text)
        If TxtProjectName.Text = "" Then
            MsgBox("Please enter a project name", MsgBoxStyle.Critical)
            TxtProjectName.Focus()
            Exit Sub
        End If
        FrmMain.CreateNewProject(TxtProjectName.Text, TxtTopLayer.Text, TxtBottomLayer.Text, ChFlipTopHorizontal.Checked, ChFlipTopVertical.Checked, ChFlipBottomHorizontal.Checked, ChFlipBottomVertical.Checked)
        Me.Close()
    End Sub

    Private Sub CmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdCancel.Click
        Me.Close()
    End Sub

End Class