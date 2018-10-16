#define DEBUG
using System;
using RimTwitch.Interactions.Me;
using Verse;

namespace RimTwitch.IRC
{
    public class StateMachine
    {
        const string supportGirlDevelopItHttpsWwwGirldevelopitComDonate =
            "Support Girl Develop It - https://www.girldevelopit.com/donate";

        public State currentState = State.NoGame;

        public static void AdminStateMachine(IrcClient ircClient, string message)
        {
            if (message.Equals("!exitbot"))
            {
                Broadcast.Stop();
            }
        }

        public static void StateMachineBehaviour(IrcClient ircClient, string userName, string message)
        {
            try
            {
                Log.Message("Got [" + message + "] from [" + userName + "]");
                // General commands anyone can use
                if (message.Equals("!help"))
                {
                    ircClient.SendPublicChatMessage("Help Not Available.");
                }
                else if (message.Contains("!me"))
                {
                    NameQueue.Me(ircClient, userName, message);
                    Log.Message("Me Done");
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