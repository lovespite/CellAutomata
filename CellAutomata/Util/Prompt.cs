namespace CellAutomata.Util;
internal delegate Exception? PromptValidator(string input);

internal partial class Prompt : IDisposable
{
    private readonly Bitmap? _icon;

    private readonly Form _form;
    private readonly TextBox _input;
    private readonly Button _cancelButton;
    private readonly Button _okButton;

    public bool CancelError { get; set; } = false;
    public string? Result { get; private set; } = string.Empty;
    public DialogResult DialogResult { get; private set; } = DialogResult.Cancel;

    public Prompt(string title, string prompt, string? defaultText = null, Bitmap? icon = null)
    {
        var @default = defaultText ?? string.Empty;
        _icon = icon;

        var label = new Label
        {
            Text = prompt,
            AutoSize = true,
            Location = new Point(50, 20)
        };

        _input = new TextBox
        {
            Text = @default,
            Location = new Point(50, 50),
            Size = new Size(400, 20),
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };

        _cancelButton = new Button
        {
            DialogResult = DialogResult.Cancel,
            Text = @"Cancel",
            Location = new Point(375, 80),
            Size = new Size(75, 28)
        };

        _okButton = new Button
        {
            DialogResult = DialogResult.OK,
            Text = @"OK",
            Location = new Point(300, 80),
            Size = new Size(75, 28)
        };

        _form = new Form
        {
            Width = 500,
            Height = 160,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = title,
            StartPosition = FormStartPosition.CenterScreen,
            MaximizeBox = false,
            MinimizeBox = false,
        };

        _form.Controls.Add(label);
        _form.Controls.Add(_input);
        _form.Controls.Add(_cancelButton);
        _form.Controls.Add(_okButton);
    }

    public void SetValidator(PromptValidator validator)
    {
        _input.TextChanged += (_, _) =>
        {
            var error = validator(_input.Text);
            _okButton.Enabled = error is null;
            _form.AcceptButton = _okButton;
        };
    }

    public void Show()
    {
        if (_form.IsDisposed) throw new ObjectDisposedException(nameof(Prompt));

        if (_icon is not null)
        {
            _form.Icon = Icon.FromHandle(_icon.GetHicon());
        }

        _okButton.Enabled = true;
        _form.AcceptButton = _okButton;

        _okButton.Click += (_, _) =>
        { 
            Result = _input.Text;
            DialogResult = DialogResult.OK; 
            // ReSharper disable once AccessToDisposedClosure
            _form.Close();
        };

        _cancelButton.Click += (_, _) =>
        {
            Result = null;
            DialogResult = DialogResult.Cancel;
            // ReSharper disable once AccessToDisposedClosure
            _form.Close();
        };

        _form.ShowDialog();
        _form.Dispose(); // Dispose of the form after it's closed

        if (DialogResult == DialogResult.Cancel && CancelError)
        {
            // CancelError is set to true, throw an exception
            throw new OperationCanceledException();
        }
    }

    public void Dispose()
    {
        _form.Dispose();
    }
}

internal partial class Prompt
{
    public static string? Show(string title, string prompt, string? defaultText = null, Bitmap? icon = null)
    {
        using var pb = new Prompt(title, prompt, defaultText, icon);
        pb.Show();
        return pb.DialogResult == DialogResult.OK ? pb.Result : null;
    }

    public static DialogResult Show(string title, string prompt, out string? result, string? defaultText = null, Bitmap? icon = null)
    {
        using var pb = new Prompt(title, prompt, defaultText, icon);
        pb.Show();
        result = pb.Result;
        return pb.DialogResult;
    }
}
