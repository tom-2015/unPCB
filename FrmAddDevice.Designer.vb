<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmAddDevice
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
        Me.components = New System.ComponentModel.Container
        Me.OFDialog = New System.Windows.Forms.OpenFileDialog
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer
        Me.TrvDevices = New System.Windows.Forms.TreeView
        Me.TxtValue = New System.Windows.Forms.TextBox
        Me.Label5 = New System.Windows.Forms.Label
        Me.CmdCancel = New System.Windows.Forms.Button
        Me.CmdCreate = New System.Windows.Forms.Button
        Me.TxtName = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.TxtDeviceDescription = New System.Windows.Forms.TextBox
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip
        Me.LibraryToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.UseLibraryToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.PctSymbol = New System.Windows.Forms.PictureBox
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        CType(Me.PctSymbol, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'OFDialog
        '
        Me.OFDialog.Filter = "Eagle .lbr files|*.lbr|All files|*.*"
        Me.OFDialog.Multiselect = True
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 24)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.TrvDevices)
        Me.SplitContainer1.Panel1.Controls.Add(Me.TxtValue)
        Me.SplitContainer1.Panel1.Controls.Add(Me.Label5)
        Me.SplitContainer1.Panel1.Controls.Add(Me.CmdCancel)
        Me.SplitContainer1.Panel1.Controls.Add(Me.CmdCreate)
        Me.SplitContainer1.Panel1.Controls.Add(Me.TxtName)
        Me.SplitContainer1.Panel1.Controls.Add(Me.Label3)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.TxtDeviceDescription)
        Me.SplitContainer1.Panel2.Controls.Add(Me.PctSymbol)
        Me.SplitContainer1.Size = New System.Drawing.Size(855, 626)
        Me.SplitContainer1.SplitterDistance = 285
        Me.SplitContainer1.TabIndex = 15
        '
        'TrvDevices
        '
        Me.TrvDevices.Dock = System.Windows.Forms.DockStyle.Top
        Me.TrvDevices.Location = New System.Drawing.Point(0, 0)
        Me.TrvDevices.Name = "TrvDevices"
        Me.TrvDevices.Size = New System.Drawing.Size(285, 476)
        Me.TrvDevices.TabIndex = 19
        '
        'TxtValue
        '
        Me.TxtValue.Location = New System.Drawing.Point(60, 561)
        Me.TxtValue.Name = "TxtValue"
        Me.TxtValue.Size = New System.Drawing.Size(222, 20)
        Me.TxtValue.TabIndex = 18
        Me.ToolTip1.SetToolTip(Me.TxtValue, "Enter the value on the PCB (for ex. 1K, 1µF,...)")
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(12, 564)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(37, 13)
        Me.Label5.TabIndex = 17
        Me.Label5.Text = "Value:"
        '
        'CmdCancel
        '
        Me.CmdCancel.Location = New System.Drawing.Point(126, 590)
        Me.CmdCancel.Name = "CmdCancel"
        Me.CmdCancel.Size = New System.Drawing.Size(75, 23)
        Me.CmdCancel.TabIndex = 16
        Me.CmdCancel.Text = "Cancel"
        Me.CmdCancel.UseVisualStyleBackColor = True
        '
        'CmdCreate
        '
        Me.CmdCreate.Location = New System.Drawing.Point(216, 590)
        Me.CmdCreate.Name = "CmdCreate"
        Me.CmdCreate.Size = New System.Drawing.Size(66, 24)
        Me.CmdCreate.TabIndex = 15
        Me.CmdCreate.Text = "Create"
        Me.CmdCreate.UseVisualStyleBackColor = True
        '
        'TxtName
        '
        Me.TxtName.Location = New System.Drawing.Point(60, 535)
        Me.TxtName.Name = "TxtName"
        Me.TxtName.Size = New System.Drawing.Size(222, 20)
        Me.TxtName.TabIndex = 14
        Me.ToolTip1.SetToolTip(Me.TxtName, "Enter the name of the device on the PCB")
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 538)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(38, 13)
        Me.Label3.TabIndex = 13
        Me.Label3.Text = "Name:"
        '
        'TxtDeviceDescription
        '
        Me.TxtDeviceDescription.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TxtDeviceDescription.Location = New System.Drawing.Point(0, 476)
        Me.TxtDeviceDescription.Multiline = True
        Me.TxtDeviceDescription.Name = "TxtDeviceDescription"
        Me.TxtDeviceDescription.ReadOnly = True
        Me.TxtDeviceDescription.Size = New System.Drawing.Size(566, 150)
        Me.TxtDeviceDescription.TabIndex = 16
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.LibraryToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(855, 24)
        Me.MenuStrip1.TabIndex = 17
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'LibraryToolStripMenuItem
        '
        Me.LibraryToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.UseLibraryToolStripMenuItem})
        Me.LibraryToolStripMenuItem.Name = "LibraryToolStripMenuItem"
        Me.LibraryToolStripMenuItem.Size = New System.Drawing.Size(50, 20)
        Me.LibraryToolStripMenuItem.Text = "Library"
        '
        'UseLibraryToolStripMenuItem
        '
        Me.UseLibraryToolStripMenuItem.Name = "UseLibraryToolStripMenuItem"
        Me.UseLibraryToolStripMenuItem.Size = New System.Drawing.Size(132, 22)
        Me.UseLibraryToolStripMenuItem.Text = "Load Library"
        '
        'PctSymbol
        '
        Me.PctSymbol.BackColor = System.Drawing.Color.White
        Me.PctSymbol.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.PctSymbol.Dock = System.Windows.Forms.DockStyle.Top
        Me.PctSymbol.Location = New System.Drawing.Point(0, 0)
        Me.PctSymbol.Name = "PctSymbol"
        Me.PctSymbol.Size = New System.Drawing.Size(566, 476)
        Me.PctSymbol.TabIndex = 15
        Me.PctSymbol.TabStop = False
        '
        'FrmAddDevice
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(855, 650)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmAddDevice"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Add Device"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel1.PerformLayout()
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        Me.SplitContainer1.Panel2.PerformLayout()
        Me.SplitContainer1.ResumeLayout(False)
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        CType(Me.PctSymbol, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents OFDialog As System.Windows.Forms.OpenFileDialog
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents TrvDevices As System.Windows.Forms.TreeView
    Friend WithEvents TxtValue As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents CmdCancel As System.Windows.Forms.Button
    Friend WithEvents CmdCreate As System.Windows.Forms.Button
    Friend WithEvents TxtName As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents PctSymbol As System.Windows.Forms.PictureBox
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents LibraryToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UseLibraryToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents TxtDeviceDescription As System.Windows.Forms.TextBox
End Class
