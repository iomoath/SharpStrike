using System.Drawing;

namespace Models.Common
{
    public class MessageData
    {
        public enum MessageType
        {
            Red,
            Warning,
            Blue,
            Good,
            General
        }

        public string Text { get; }
        public Color ForegroundColor { get; }
        public Color BackgroundColor { get; } = Color.Black;
        public MessageType Type { get; set; }


        public MessageData(string text, Color backgroundColor, Color foregroundColor, MessageType t)
        {
            Text = text;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Type = t;
        }

        public MessageData(string text, Color foregroundColor, MessageType t)
        {
            Text = text;
            ForegroundColor = foregroundColor;
            Type = t;
        }

        public MessageData(string text)
        {
            Text = text;
            Type = MessageType.General;
            ForegroundColor = Color.White;
        }
    }
}
