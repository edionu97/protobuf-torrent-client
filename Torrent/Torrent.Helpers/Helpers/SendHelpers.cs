using System;
using Google.Protobuf;
using System.Net.Sockets;

namespace Torrent.Helpers.Helpers
{
    public class SendHelpers
    {
        /// <summary>
        /// This method it is used for converting a message into a byte buffer and send it over the network
        ///     The first 4 bytes represents the message length and the others represents the message
        ///     The first 4 bytes are in big endian format so little endian conversion it is needed
        /// </summary>
        /// <param name="message">the message that will be send</param>
        /// <param name="toSocket">the destination</param>
        public static void SendMessage(Message message, Socket toSocket)
        {
            //get the array of bytes
            var byteArray = message.ToByteArray();

            //get the message length
            var len = byteArray.Length;

            //reverse the bytes if little endian
            var lenAsBigEndian = BitConverter.GetBytes(len);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lenAsBigEndian);
            }

            //send the length of the message
            toSocket.Send(lenAsBigEndian);

            //send the message
            toSocket.Send(byteArray);
        }

        /// <summary>
        /// Converts a byte buffer into message.
        ///     The first 4 bytes represents the message length and the others represents the message
        ///     The first 4 bytes are in big endian format so little endian conversion it is needed
        /// </summary>
        /// <param name="fromSocket">the socket from which we are waiting to read a message</param>
        /// <returns>an instance of message</returns>
        public static Message ReceiveMessage(Socket fromSocket)
        {
            //get the number of bytes
            var lenByteArray = new byte[4];
            fromSocket.Receive(lenByteArray);

            //convert to machine endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lenByteArray);
            }

            //get the message length
            var length = BitConverter
                .ToInt32(lenByteArray, 0);

            //receive the message bytes
            var messageBytes = new byte[length];
            fromSocket.Receive(messageBytes);

            //get the message
            return Message.Parser.ParseFrom(messageBytes);
        }

    }
}
