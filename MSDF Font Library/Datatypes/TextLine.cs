namespace MSDF_Font_Library.Datatypes
{
    public struct TextLine
    {
        public TextLine(string? text = null, float width = 0)
        {
            Text = text ?? string.Empty;
            Width = width;
        }

        public string Text { get; set; }
        public float Width { get; set; }


        public static TextLine operator +(TextLine value1, TextLine value2)
        {
            value1.Text += value2.Text;
            value1.Width += value2.Width;
            return value1;
        }
    }
}
