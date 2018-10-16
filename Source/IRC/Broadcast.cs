using Verse;

namespace RimTwitch.IRC
{
    public static class Broadcast
    {

        
        private static IrcClient _ircClient;
        private static Loop _ping, _mainLoop;

        private static string _channelName, _botName;

        public static IrcClient OnAir()
        {
            return _ircClient;
        }
        public static IrcClient Start(string twitchOAuth, string channelName, string botName)
        {
            if (_ircClient != null) return _ircClient;
            _channelName = channelName;
            _botName = botName;

            _ircClient = new IrcClient("irc.twitch.tv", 6667,
                botName, twitchOAuth, channelName);

            _ping = new KeepAlive(_ircClient);
            _ping.Start();
            
            _mainLoop = new Loop(_ircClient);
            _mainLoop.Start();

            return _ircClient;
        }

        public static void Stop()
        {
            if (_ircClient == null) return;
            _ping?.Stop();
            _mainLoop?.Stop();
            _ircClient = null;
        }


        public static void Tick()
        {
            if (_ircClient == null) return;

            string message = _ircClient.ReadMessage();
            Log.Message(message); // Print raw irc messages

            if (message.Contains("PRIVMSG"))
            {
                // Messages from the users will look something like this (without quotes):
                // Format: ":[user]![user]@[user].tmi.twitch.tv PRIVMSG #[channel] :[message]"

                // Modify message to only retrieve user and message
                int intIndexParseSign = message.IndexOf('!');
                string userName =
                    message.Substring(1,
                        intIndexParseSign - 1); // parse username from specific section (without quotes)
                // Format: ":[user]!"
                // Get user's message
                intIndexParseSign = message.IndexOf(" :");
                message = message.Substring(intIndexParseSign + 2);

                // Broadcaster commands
                if (userName.Equals(_channelName))
                {
                    StateMachine.AdminStateMachine(_ircClient, message);
                }

                StateMachine.StateMachineBehaviour(_ircClient, userName, message);
            }
        }
    }
}