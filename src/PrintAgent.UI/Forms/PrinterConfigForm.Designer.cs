namespace PrintAgent.UI.Forms;

partial class PrinterConfigForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblName = new Label();
        txtName = new TextBox();
        lblSystemPrinter = new Label();
        cboSystemPrinter = new ComboBox();
        lblPaperWidth = new Label();
        numPaperWidth = new NumericUpDown();
        lblType = new Label();
        cboType = new ComboBox();
        chkIsDefault = new CheckBox();
        btnOK = new Button();
        btnCancel = new Button();
        lblWidthHelp = new Label();

        ((System.ComponentModel.ISupportInitialize)numPaperWidth).BeginInit();
        SuspendLayout();

        // lblName
        lblName.AutoSize = true;
        lblName.Location = new Point(12, 20);
        lblName.Name = "lblName";
        lblName.Size = new Size(54, 15);
        lblName.Text = "Nombre:";

        // txtName
        txtName.Location = new Point(140, 17);
        txtName.Name = "txtName";
        txtName.Size = new Size(200, 23);
        txtName.TabIndex = 0;
        txtName.PlaceholderText = "ej: factura, cocina";

        // lblSystemPrinter
        lblSystemPrinter.AutoSize = true;
        lblSystemPrinter.Location = new Point(12, 55);
        lblSystemPrinter.Name = "lblSystemPrinter";
        lblSystemPrinter.Size = new Size(120, 15);
        lblSystemPrinter.Text = "Impresora del sistema:";

        // cboSystemPrinter
        cboSystemPrinter.DropDownStyle = ComboBoxStyle.DropDown;
        cboSystemPrinter.FormattingEnabled = true;
        cboSystemPrinter.Location = new Point(140, 52);
        cboSystemPrinter.Name = "cboSystemPrinter";
        cboSystemPrinter.Size = new Size(200, 23);
        cboSystemPrinter.TabIndex = 1;

        // lblPaperWidth
        lblPaperWidth.AutoSize = true;
        lblPaperWidth.Location = new Point(12, 90);
        lblPaperWidth.Name = "lblPaperWidth";
        lblPaperWidth.Size = new Size(110, 15);
        lblPaperWidth.Text = "Ancho de papel:";

        // numPaperWidth
        numPaperWidth.Location = new Point(140, 87);
        numPaperWidth.Maximum = new decimal(new int[] { 80, 0, 0, 0 });
        numPaperWidth.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
        numPaperWidth.Name = "numPaperWidth";
        numPaperWidth.Size = new Size(80, 23);
        numPaperWidth.TabIndex = 2;
        numPaperWidth.Value = new decimal(new int[] { 42, 0, 0, 0 });

        // lblWidthHelp
        lblWidthHelp.AutoSize = true;
        lblWidthHelp.ForeColor = Color.Gray;
        lblWidthHelp.Location = new Point(226, 90);
        lblWidthHelp.Name = "lblWidthHelp";
        lblWidthHelp.Size = new Size(110, 15);
        lblWidthHelp.Text = "caracteres por línea";

        // lblType
        lblType.AutoSize = true;
        lblType.Location = new Point(12, 125);
        lblType.Name = "lblType";
        lblType.Size = new Size(34, 15);
        lblType.Text = "Tipo:";

        // cboType
        cboType.DropDownStyle = ComboBoxStyle.DropDownList;
        cboType.FormattingEnabled = true;
        cboType.Location = new Point(140, 122);
        cboType.Name = "cboType";
        cboType.Size = new Size(200, 23);
        cboType.TabIndex = 3;

        // chkIsDefault
        chkIsDefault.AutoSize = true;
        chkIsDefault.Location = new Point(140, 160);
        chkIsDefault.Name = "chkIsDefault";
        chkIsDefault.Size = new Size(150, 19);
        chkIsDefault.TabIndex = 4;
        chkIsDefault.Text = "Impresora predeterminada";
        chkIsDefault.UseVisualStyleBackColor = true;

        // btnOK
        btnOK.Location = new Point(140, 200);
        btnOK.Name = "btnOK";
        btnOK.Size = new Size(95, 30);
        btnOK.TabIndex = 5;
        btnOK.Text = "Guardar";
        btnOK.UseVisualStyleBackColor = true;
        btnOK.Click += btnOK_Click;

        // btnCancel
        btnCancel.Location = new Point(245, 200);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(95, 30);
        btnCancel.TabIndex = 6;
        btnCancel.Text = "Cancelar";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += btnCancel_Click;

        // PrinterConfigForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(360, 250);
        Controls.Add(lblName);
        Controls.Add(txtName);
        Controls.Add(lblSystemPrinter);
        Controls.Add(cboSystemPrinter);
        Controls.Add(lblPaperWidth);
        Controls.Add(numPaperWidth);
        Controls.Add(lblWidthHelp);
        Controls.Add(lblType);
        Controls.Add(cboType);
        Controls.Add(chkIsDefault);
        Controls.Add(btnOK);
        Controls.Add(btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "PrinterConfigForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Configurar Impresora";
        Load += PrinterConfigForm_Load;

        ((System.ComponentModel.ISupportInitialize)numPaperWidth).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblName;
    private TextBox txtName;
    private Label lblSystemPrinter;
    private ComboBox cboSystemPrinter;
    private Label lblPaperWidth;
    private NumericUpDown numPaperWidth;
    private Label lblWidthHelp;
    private Label lblType;
    private ComboBox cboType;
    private CheckBox chkIsDefault;
    private Button btnOK;
    private Button btnCancel;
}
