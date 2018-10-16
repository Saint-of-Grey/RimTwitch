using Verse;

namespace RimTwitch.IRC
{
    public static class Broadcast
    {

        
        private static IrcClient IrcClient;
        private static Loop ping, mainLoop;

        private static string _channelName, _botName;


        public static IrcClient Start(string twitchOAuth, string channelName, string botName)
        {
            if (IrcClient != null) return IrcClient;
            _channelName = channelName;
            _botName = botName;

            IrcClient = new IrcClient("irc.twitch.tv", 6667,
                botName, twitchOAuth, channelName);

            ping = new KeepAlive(IrcClient);
            ping.Start();
            
            mainLoop = new Loop(IrcClient);
            mainLoop.Start();

            return IrcClient;
        }

        public static void Stop()
        {
            if (IrcClient == null) return;
            ping?.Stop();
            mainLoop?.Stop();
            IrcClient = null;
        }


        public static void Tick()
        {
            if (IrcClient == null) return;

            string message = IrcClient.ReadMessage();
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
                    StateMachine.AdminStateMachine(IrcClient, message);
                }

                StateMachine.StateMachineBehaviour(IrcClient, userName, message);
            }
        }
    }
}