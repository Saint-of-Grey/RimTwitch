#define DEBUG
using System;
using System.Linq;
using RimTwitch.Interactions.Me;
using RimTwitch.Interactions.Raid;
using Verse;

namespace RimTwitch.IRC
{
    public class StateMachine
    {
        const string supportGirlDevelopItHttpsWwwGirldevelopitComDonate =
            "Support Girl Develop It - https://www.girldevelopit.com/donate";

        public State currentState = State.NoGame;

        public static bool AdminStateMachine(IrcClient ircClient, string message)
        {
            if (message.StartsWith("!admin raid"))
            {
                var points = message.Split(' ').ToArray()[2].Trim();
                if (!points.NullOrEmpty())
                {
                    Log.Message("Admin Raid! (" + points + ")");
                    int val;
                    if (Int32.TryParse(points, out val))
                    {
                        
                        RaidCommand.Start(val);
                    }
                    else
                    {
                        Log.Error("Couldnt parse that");
                    }

                    return true;
                }
            }
            if (message.Equals("!exitbot"))
            {
                Broadcast.Stop();
                
                return true;
            }

            return false;
            ;
        }

        public static void StateMachineBehaviour(IrcClient ircClient, string userName, string message)
        {
            try
            {
                Log.Message("Got [" + message + "] from [" + userName + "]");
                // General commands anyone can use
                if (message.Equals("!help"))
                {
                    ircClient.SendPublicChatMessage("!help - This message, !me help for TwitchPlays Colonist commands, !raid help for TwitchPlays Raider commands, you may be both");
                }
                else if (message.StartsWith("!me"))
                {
                    MeCommand.Me(ircClient, userName, message);
                }
                else if (message.StartsWith("!raid"))
                {
                    RaidCommand.Me(ircClient, userName, message);
                }
                else if (message.Equals("!hello"))
                {
                    ircClient.SendPublicChatMessage(supportGirlDevelopItHttpsWwwGirldevelopitComDonate);
                }
                else
                {
                    Log.Message("Unable to use that message");
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }
    }

    public enum State
    {
        NoGame,
        GameStarted,
        VoteEvent
    }
}