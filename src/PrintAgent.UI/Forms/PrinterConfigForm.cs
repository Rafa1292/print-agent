using PrintAgent.UI.Models;

namespace PrintAgent.UI.Forms;

public partial class PrinterConfigForm : Form
{
    private readonly List<string> _systemPrinters;
    private readonly PrinterInfo? _existingPrinter;

    public PrinterConfig? PrinterConfig { get; private set; }

    public PrinterConfigForm(List<string> systemPrinters, PrinterInfo? existingPrinter = null)
    {
        InitializeComponent();
        _systemPrinters = systemPrinters;
        _existingPrinter = existingPrinter;
    }

    private void PrinterConfigForm_Load(object sender, EventArgs e)
    {
        // Populate system printers dropdown
        cboSystemPrinter.Items.Clear();
        foreach (var printer in _systemPrinters)
        {
            cboSystemPrinter.Items.Add(printer);
        }

        // Populate type dropdown
        cboType.Items.Clear();
        cboType.Items.Add("Receipt");
        cboType.Items.Add("Kitchen");
        cboType.SelectedIndex = 0;

        // Set default values
        numPaperWidth.Value = 42;

        // If editing, populate fields
        if (_existingPrinter != null)
        {
            Text = "Editar Impresora";
            txtName.Text = _existingPrinter.Name;
            txtName.Enabled = false; // Don't allow changing the name when editing

            // Select system printer
            int index = cboSystemPrinter.Items.IndexOf(_existingPrinter.SystemName);
            if (index >= 0) cboSystemPrinter.SelectedIndex = index;
            else cboSystemPrinter.Text = _existingPrinter.SystemName;

            numPaperWidth.Value = _existingPrinter.PaperWidth;

            // Select type
            index = cboType.Items.IndexOf(_existingPrinter.Type);
            if (index >= 0) cboType.SelectedIndex = index;

            chkIsDefault.Checked = _existingPrinter.IsDefault;
        }
        else
        {
            Text = "Agregar Impresora";
        }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("El nombre es requerido", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(cboSystemPrinter.Text))
        {
            MessageBox.Show("Debe seleccionar una impresora del sistema", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cboSystemPrinter.Focus();
            return;
        }

        PrinterConfig = new PrinterConfig
        {
            Name = txtName.Text.Trim(),
            SystemName = cboSystemPrinter.Text,
            PaperWidth = (int)numPaperWidth.Value,
            Type = cboType.SelectedItem?.ToString() ?? "Receipt",
            IsDefault = chkIsDefault.Checked
        };

        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
