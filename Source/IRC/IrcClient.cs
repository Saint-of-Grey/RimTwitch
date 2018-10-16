#define DEBUG
using System;
using System.IO;
using System.Net.Sockets;
using Verse;

namespace RimTwitch.IRC
{
     public class IrcClient
    {
        private string userName;
        private string channel;

        private TcpClient _tcpClient;
        private StreamReader _inputStream;
        private StreamWriter _outputStream;

        public IrcClient(string ip, int port, string userName, string password, string channel)
        {
            try
            {
                this.userName = userName;
                this.channel = channel;

                _tcpClient = new TcpClient(ip, port);
                _inputStream = new StreamReader(_tcpClient.GetStream());
                _outputStream = new StreamWriter(_tcpClient.GetStream());

                // Try to join the room
                _outputStream.WriteLine("PASS " + password);
                _outputStream.WriteLine("NICK " + userName);
                _outputStream.WriteLine("USER " + userName + " 8 * :" + userName);
                _outputStream.WriteLine("JOIN #" + channel);
                _outputStream.Flush();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        public void SendIrcMessage(string message)
        {
            try
            {
                _outputStream.WriteLine(message);
                _outputStream.Flush();
#if DEBUG
                Log.Message("Sent IRC Message");
#endif
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public void SendPublicChatMessage(string messages)
        {
            if (messages.NullOrEmpty()) return;
            foreach (var message in messages.Split('\n'))
                try
                {
                    SendIrcMessage(":" + userName + "!" + userName + "@" + userName +
                                   ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
                
#if DEBUG                
                    Log.Message("Sent Pub Chat Message");
#endif
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
        }

        public string ReadMessage()
        {
            try
            {
                string message = _inputStream.ReadLine();
                return message;
            }
            catch (Exception ex)
            {
                return "Error receiving message: " + ex.Message;
            }
        }
    }
}