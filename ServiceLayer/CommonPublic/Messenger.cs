using System;
using System.Drawing;
using Models.Common;

namespace ServiceLayer
{
    public class Messenger
    {
        public static event EventHandler<MessageData> MessageAvailable;

        public static void RedMessage(string output)
        {
            OnMessageAvailable(new MessageData(output, Color.Black, Color.Red, MessageData.MessageType.Red));
        }

        public static void RedMessage(string format, params object[] msg)
        {
            var m = string.Format(format, msg);
            OnMessageAvailable(new MessageData(m, Color.Black, Color.Red, MessageData.MessageType.Red));
        }


        public static void YellowMessage(string output)
        {
            OnMessageAvailable(new MessageData(output, Color.Black, Color.Yellow, MessageData.MessageType.Warning));
        }

        public static void YellowMessage(string format, params object[] msg)
        {
            var m = string.Format(format, msg);
            OnMessageAvailable(new MessageData(m, Color.Black, Color.Yellow, MessageData.MessageType.Warning));
        }

        public static void BlueMessage(string output)
        {
            OnMessageAvailable(new MessageData(output, Color.Black, Color.Cyan, MessageData.MessageType.Blue));
        }

        public static void BlueMessage(string format, params object[] msg)
        {
            var m = string.Format(format, msg);
            OnMessageAvailable(new MessageData(m, Color.Black, Color.Cyan, MessageData.MessageType.Blue));
        }

        public static void GoodMessage(string output)
        {
            OnMessageAvailable(new MessageData(output, Color.Black, Color.Lime, MessageData.MessageType.Good));
        }

        public static void GoodMessage(string format, params object[] msg)
        {
            var m = string.Format(format, msg);
            OnMessageAvailable(new MessageData(m, Color.Black, Color.Lime, MessageData.MessageType.Good));
        }

        public static void Info(string output)
        {
            OnMessageAvailable(new MessageData(output));
        }

        public static void Info(object output)
        {
            OnMessageAvailable(new MessageData(output?.ToString()));
        }

        public static void Info(string format, params object[] msg)
        {
            OnMessageAvailable(new MessageData(string.Format(format, msg)));
        }


        public static void OnMessageAvailable(MessageData e)
        {
            MessageAvailable?.Invoke(null, e);
        }

    }
}
