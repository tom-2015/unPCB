<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmNew
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.ChFlipBottomVertical = New System.Windows.Forms.CheckBox
        Me.ChFlipBottomHorizontal = New System.Windows.Forms.CheckBox
        Me.ChFlipTopVertical = New System.Windows.Forms.CheckBox
        Me.ChFlipTopHorizontal = New System.Windows.Forms.CheckBox
        Me.CmdBrowseBottom = New System.Windows.Forms.Button
        Me.TxtBottomLayer = New System.Windows.Forms.TextBox
        Me.CmdBrowseTop = New System.Windows.Forms.Button
        Me.TxtTopLayer = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.TxtProjectName = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.CmdCreate = New System.Windows.Forms.Button
        Me.CmdCancel = New System.Windows.Forms.Button
        Me.OpenFile = New System.Windows.Forms.OpenFileDialog
        Me.PctBottom = New System.Windows.Forms.PictureBox
        Me.PctTop = New System.Windows.Forms.PictureBox
        Me.GroupBox1.SuspendLayout()
        CType(Me.PctBottom, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PctTop, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.ChFlipBottomVertical)
        Me.GroupBox1.Controls.Add(Me.ChFlipBottomHorizontal)
        Me.GroupBox1.Controls.Add(Me.PctBottom)
        Me.GroupBox1.Controls.Add(Me.ChFlipTopVertical)
        Me.GroupBox1.Controls.Add(Me.ChFlipTopHorizontal)
        Me.GroupBox1.Controls.Add(Me.PctTop)
        Me.GroupBox1.Controls.Add(Me.CmdBrowseBottom)
        Me.GroupBox1.Controls.Add(Me.TxtBottomLayer)
        Me.GroupBox1.Controls.Add(Me.CmdBrowseTop)
        Me.GroupBox1.Controls.Add(Me.TxtTopLayer)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.TxtProjectName)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(585, 405)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Project Properties"
        '
        'ChFlipBottomVertical
        '
        Me.ChFlipBottomVertical.AutoSize = True
        Me.ChFlipBottomVertical.Location = New System.Drawing.Point(93, 272)
        Me.ChFlipBottomVertical.Name = "ChFlipBottomVertical"
        Me.ChFlipBottomVertical.Size = New System.Drawing.Size(119, 17)
        Me.ChFlipBottomVertical.TabIndex = 13
        Me.ChFlipBottomVertical.Text = "Flip Image Vertically"
        Me.ChFlipBottomVertical.UseVisualStyleBackColor = True
        '
        'ChFlipBottomHorizontal
        '
        Me.ChFlipBottomHorizontal.AutoSize = True
        Me.ChFlipBottomHorizontal.Location = New System.Drawing.Point(93, 248)
        Me.ChFlipBottomHorizontal.Name = "ChFlipBottomHorizontal"
        Me.ChFlipBottomHorizontal.Size = New System.Drawing.Size(131, 17)
        Me.ChFlipBottomHorizontal.TabIndex = 12
        Me.ChFlipBottomHorizontal.Text = "Flip Image Horizontally"
        Me.ChFlipBottomHorizontal.UseVisualStyleBackColor = True
        '
        'ChFlipTopVertical
        '
        Me.ChFlipTopVertical.AutoSize = True
        Me.ChFlipTopVertical.Location = New System.Drawing.Point(93, 121)
        Me.ChFlipTopVertical.Name = "ChFlipTopVertical"
        Me.ChFlipTopVertical.Size = New System.Drawing.Size(119, 17)
        Me.ChFlipTopVertical.TabIndex = 10
        Me.ChFlipTopVertical.Text = "Flip Image Vertically"
        Me.ChFlipTopVertical.UseVisualStyleBackColor = True
        '
        'ChFlipTopHorizontal
        '
        Me.ChFlipTopHorizontal.AutoSize = True
        Me.ChFlipTopHorizontal.Location = New System.Drawing.Point(93, 97)
        Me.ChFlipTopHorizontal.Name = "ChFlipTopHorizontal"
        Me.ChFlipTopHorizontal.Size = New System.Drawing.Size(131, 17)
        Me.ChFlipTopHorizontal.TabIndex = 9
        Me.ChFlipTopHorizontal.Text = "Flip Image Horizontally"
        Me.ChFlipTopHorizontal.UseVisualStyleBackColor = True
        '
        'CmdBrowseBottom
        '
        Me.CmdBrowseBottom.Location = New System.Drawing.Point(486, 222)
        Me.CmdBrowseBottom.Name = "CmdBrowseBottom"
        Me.CmdBrowseBottom.Size = New System.Drawing.Size(75, 23)
        Me.CmdBrowseBottom.TabIndex = 7
        Me.CmdBrowseBottom.Text = "Browse"
        Me.CmdBrowseBottom.UseVisualStyleBackColor = True
        '
        'TxtBottomLayer
        '
        Me.TxtBottomLayer.Location = New System.Drawing.Point(93, 222)
        Me.TxtBottomLayer.Name = "TxtBottomLayer"
        Me.TxtBottomLayer.Size = New System.Drawing.Size(386, 20)
        Me.TxtBottomLayer.TabIndex = 6
        '
        'CmdBrowseTop
        '
        Me.CmdBrowseTop.Location = New System.Drawing.Point(486, 67)
        Me.CmdBrowseTop.Name = "CmdBrowseTop"
        Me.CmdBrowseTop.Size = New System.Drawing.Size(75, 23)
        Me.CmdBrowseTop.TabIndex = 5
        Me.CmdBrowseTop.Text = "Browse"
        Me.CmdBrowseTop.UseVisualStyleBackColor = True
        '
        'TxtTopLayer
        '
        Me.TxtTopLayer.Location = New System.Drawing.Point(93, 71)
        Me.TxtTopLayer.Name = "TxtTopLayer"
        Me.TxtTopLayer.Size = New System.Drawing.Size(386, 20)
        Me.TxtTopLayer.TabIndex = 4
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(13, 206)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(125, 13)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "PCB Bottom Layer Image"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(13, 55)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(114, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "PCB Top Layer Image:"
        '
        'TxtProjectName
        '
        Me.TxtProjectName.Location = New System.Drawing.Point(93, 19)
        Me.TxtProjectName.Name = "TxtProjectName"
        Me.TxtProjectName.Size = New System.Drawing.Size(386, 20)
        Me.TxtProjectName.TabIndex = 1
        Me.TxtProjectName.Tag = "Description of your project"
        Me.TxtProjectName.Text = "New Project"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(13, 23)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(74, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Project Name:"
        '
        'CmdCreate
        '
        Me.CmdCreate.Location = New System.Drawing.Point(522, 447)
        Me.CmdCreate.Name = "CmdCreate"
        Me.CmdCreate.Size = New System.Drawing.Size(75, 23)
        Me.CmdCreate.TabIndex = 1
        Me.CmdCreate.Text = "Create"
        Me.CmdCreate.UseVisualStyleBackColor = True
        '
        'CmdCancel
        '
        Me.CmdCancel.Location = New System.Drawing.Point(416, 447)
        Me.CmdCancel.Name = "CmdCancel"
        Me.CmdCancel.Size = New System.Drawing.Size(75, 23)
        Me.CmdCancel.TabIndex = 2
        Me.CmdCancel.Text = "Cancel"
        Me.CmdCancel.UseVisualStyleBackColor = True
        '
        'OpenFile
        '
        Me.OpenFile.Filter = "Image Files|*.png;*.bmp;*.jpg|Alle files|*"
        Me.OpenFile.SupportMultiDottedExtensions = True
        '
        'PctBottom
        '
        Me.PctBottom.Location = New System.Drawing.Point(379, 248)
        Me.PctBottom.Name = "PctBottom"
        Me.PctBottom.Size = New System.Drawing.Size(100, 100)
        Me.PctBottom.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PctBottom.TabIndex = 11
        Me.PctBottom.TabStop = False
        '
        'PctTop
        '
        Me.PctTop.Location = New System.Drawing.Point(379, 97)
        Me.PctTop.Name = "PctTop"
        Me.PctTop.Size = New System.Drawing.Size(100, 100)
        Me.PctTop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PctTop.TabIndex = 8
        Me.PctTop.TabStop = False
        '
        'FrmNew
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(613, 483)
        Me.Controls.Add(Me.CmdCancel)
        Me.Controls.Add(Me.CmdCreate)
        Me.Controls.Add(Me.GroupBox1)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmNew"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "New Project"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.PctBottom, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PctTop, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TxtProjectName As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ChFlipBottomVertical As System.Windows.Forms.CheckBox
    Friend WithEvents ChFlipBottomHorizontal As System.Windows.Forms.CheckBox
    Friend WithEvents PctBottom As System.Windows.Forms.PictureBox
    Friend WithEvents ChFlipTopVertical As System.Windows.Forms.CheckBox
    Friend WithEvents ChFlipTopHorizontal As System.Windows.Forms.CheckBox
    Friend WithEvents PctTop As System.Windows.Forms.PictureBox
    Friend WithEvents CmdBrowseBottom As System.Windows.Forms.Button
    Friend WithEvents TxtBottomLayer As System.Windows.Forms.TextBox
    Friend WithEvents CmdBrowseTop As System.Windows.Forms.Button
    Friend WithEvents TxtTopLayer As System.Windows.Forms.TextBox
    Friend WithEvents CmdCreate As System.Windows.Forms.Button
    Friend WithEvents CmdCancel As System.Windows.Forms.Button
    Friend WithEvents OpenFile As System.Windows.Forms.OpenFileDialog
End Class
