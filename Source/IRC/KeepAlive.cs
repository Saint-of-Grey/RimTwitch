using System.Threading;
using Verse;

namespace RimTwitch.IRC
{
    public class KeepAlive : Loop
    {
        public KeepAlive(IrcClient irc) : base(irc)
        {
        }

        // Send PING to irc server every 5 minutes
        protected override void Run()
        {
            while (!Stopped)
            {
                _irc.SendIrcMessage("PING irc.twitch.tv");
                Thread.Sleep(100000);
                if (Rand.Chance(.2f))
                    _irc.SendPublicChatMessage(
                        "RimWorld Twitch Bot (non-commercial) is Listening. Type !me to add yourself to the game. Support Girl Develop It - https://www.girldevelopit.com/donate");
            }
        }
    }
}