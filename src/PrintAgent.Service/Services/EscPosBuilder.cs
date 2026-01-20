using System.Text;

namespace PrintAgent.Service.Services;

public class EscPosBuilder
{
    private readonly List<byte> _buffer = new();
    private readonly int _lineWidth;
    private static readonly Encoding Latin1 = Encoding.GetEncoding("ISO-8859-1");

    public EscPosBuilder(int lineWidth = 48)
    {
        _lineWidth = lineWidth;
        Initialize();
    }

    // ESC/POS Commands
    private static readonly byte[] CmdInitialize = { 0x1B, 0x40 };
    private static readonly byte[] CmdCut = { 0x1D, 0x56, 0x41, 0x00 }; // Partial cut
    private static readonly byte[] CmdFullCut = { 0x1D, 0x56, 0x00 };
    private static readonly byte[] CmdBoldOn = { 0x1B, 0x45, 0x01 };
    private static readonly byte[] CmdBoldOff = { 0x1B, 0x45, 0x00 };
    private static readonly byte[] CmdAlignLeft = { 0x1B, 0x61, 0x00 };
    private static readonly byte[] CmdAlignCenter = { 0x1B, 0x61, 0x01 };
    private static readonly byte[] CmdAlignRight = { 0x1B, 0x61, 0x02 };
    private static readonly byte[] CmdDoubleHeight = { 0x1B, 0x21, 0x10 };
    private static readonly byte[] CmdDoubleWidth = { 0x1B, 0x21, 0x20 };
    private static readonly byte[] CmdDoubleSize = { 0x1B, 0x21, 0x30 };
    private static readonly byte[] CmdNormalSize = { 0x1B, 0x21, 0x00 };
    private static readonly byte[] CmdLineFeed = { 0x0A };
    private static readonly byte[] CmdUnderlineOn = { 0x1B, 0x2D, 0x01 };
    private static readonly byte[] CmdUnderlineOff = { 0x1B, 0x2D, 0x00 };

    public EscPosBuilder Initialize()
    {
        _buffer.AddRange(CmdInitialize);
        return this;
    }

    public EscPosBuilder Text(string text)
    {
        _buffer.AddRange(Latin1.GetBytes(NormalizeText(text)));
        return this;
    }

    public EscPosBuilder Line(string text = "")
    {
        Text(text);
        _buffer.AddRange(CmdLineFeed);
        return this;
    }

    public EscPosBuilder Lines(int count = 1)
    {
        for (int i = 0; i < count; i++)
            _buffer.AddRange(CmdLineFeed);
        return this;
    }

    public EscPosBuilder Bold(bool on = true)
    {
        _buffer.AddRange(on ? CmdBoldOn : CmdBoldOff);
        return this;
    }

    public EscPosBuilder Underline(bool on = true)
    {
        _buffer.AddRange(on ? CmdUnderlineOn : CmdUnderlineOff);
        return this;
    }

    public EscPosBuilder AlignLeft()
    {
        _buffer.AddRange(CmdAlignLeft);
        return this;
    }

    public EscPosBuilder AlignCenter()
    {
        _buffer.AddRange(CmdAlignCenter);
        return this;
    }

    public EscPosBuilder AlignRight()
    {
        _buffer.AddRange(CmdAlignRight);
        return this;
    }

    public EscPosBuilder DoubleHeight()
    {
        _buffer.AddRange(CmdDoubleHeight);
        return this;
    }

    public EscPosBuilder DoubleWidth()
    {
        _buffer.AddRange(CmdDoubleWidth);
        return this;
    }

    public EscPosBuilder DoubleSize()
    {
        _buffer.AddRange(CmdDoubleSize);
        return this;
    }

    public EscPosBuilder NormalSize()
    {
        _buffer.AddRange(CmdNormalSize);
        return this;
    }

    /// <summary>
    /// Imprime texto con word-wrap automático según el ancho del papel
    /// </summary>
    public EscPosBuilder WrappedLine(string text, string prefix = "")
    {
        if (string.IsNullOrEmpty(text))
            return this;

        int maxWidth = _lineWidth - prefix.Length;
        if (maxWidth <= 0) maxWidth = _lineWidth;

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length == 0)
            {
                currentLine.Append(word);
            }
            else if (currentLine.Length + 1 + word.Length <= maxWidth)
            {
                currentLine.Append(' ').Append(word);
            }
            else
            {
                Line(prefix + currentLine);
                currentLine.Clear().Append(word);
            }
        }

        if (currentLine.Length > 0)
        {
            Line(prefix + currentLine);
        }

        return this;
    }

    public EscPosBuilder Separator(char c = '-')
    {
        Line(new string(c, _lineWidth));
        return this;
    }

    public EscPosBuilder DoubleSeparator()
    {
        Line(new string('=', _lineWidth));
        return this;
    }

    public EscPosBuilder Row(string left, string right)
    {
        int spaces = _lineWidth - left.Length - right.Length;
        if (spaces < 1) spaces = 1;
        Line($"{left}{new string(' ', spaces)}{right}");
        return this;
    }

    public EscPosBuilder Row(string left, string center, string right)
    {
        int totalContent = left.Length + center.Length + right.Length;
        int totalSpaces = _lineWidth - totalContent;
        int leftSpaces = totalSpaces / 2;
        int rightSpaces = totalSpaces - leftSpaces;
        if (leftSpaces < 1) leftSpaces = 1;
        if (rightSpaces < 1) rightSpaces = 1;
        Line($"{left}{new string(' ', leftSpaces)}{center}{new string(' ', rightSpaces)}{right}");
        return this;
    }

    public EscPosBuilder ItemLine(string name, decimal qty, decimal price, decimal total)
    {
        string qtyStr = qty.ToString("0.##");
        string priceStr = price.ToString("0.00");
        string totalStr = total.ToString("0.00");

        // Format: Name + Qty x Price = Total
        string rightPart = $" {qtyStr}x{priceStr} {totalStr}";
        int nameMaxLen = _lineWidth - rightPart.Length;

        if (name.Length > nameMaxLen)
            name = name[..(nameMaxLen - 2)] + "..";

        Line($"{name.PadRight(nameMaxLen)}{rightPart}");
        return this;
    }

    public EscPosBuilder Cut(bool partial = true)
    {
        Lines(3);
        _buffer.AddRange(partial ? CmdCut : CmdFullCut);
        return this;
    }

    /// <summary>
    /// Agrega bytes raw al buffer (para imágenes bitmap)
    /// </summary>
    public EscPosBuilder RawBytes(byte[] data)
    {
        _buffer.AddRange(data);
        return this;
    }

    public byte[] Build()
    {
        return _buffer.ToArray();
    }

    private static string NormalizeText(string text)
    {
        // Replace special characters that might not print correctly
        return text
            .Replace("á", "a").Replace("Á", "A")
            .Replace("é", "e").Replace("É", "E")
            .Replace("í", "i").Replace("Í", "I")
            .Replace("ó", "o").Replace("Ó", "O")
            .Replace("ú", "u").Replace("Ú", "U")
            .Replace("ñ", "n").Replace("Ñ", "N")
            .Replace("ü", "u").Replace("Ü", "U");
    }
}
