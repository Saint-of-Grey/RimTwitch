using System.Threading;

namespace RimTwitch.IRC
{
    public class KeepAlive : Loop
    {
        public KeepAlive(IrcClient irc) : base(irc)
        {}

        // Send PING to irc server every 5 minutes
        protected override void Run()
        {
            while (!Stopped)
            {
                _irc.SendIrcMessage("PING irc.twitch.tv");
                Thread.Sleep(300000); // 5 minutes
            }
        }

    }
}