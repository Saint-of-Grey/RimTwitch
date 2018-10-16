namespace RimTwitch.IRC
{
    public class StateMachine
    {
        const string supportGirlDevelopItHttpsWwwGirldevelopitComDonate =
            "Support Girl Develop It - https://www.girldevelopit.com/donate";


         public static void AdminStateMachine(IrcClient IrcClient, string message)
        {
            if (message.Equals("!exitbot"))
            {
                Broadcast.Stop();
            }

            // General commands anyone can use
            if (message.Equals("!hello"))
            {
                IrcClient.SendPublicChatMessage(supportGirlDevelopItHttpsWwwGirldevelopitComDonate);
            }
        }

        public static void StateMachineBehaviour(IrcClient IrcClient, string userName, string message)
        {
            // General commands anyone can use
            if (message.Equals("!help"))
            {
                IrcClient.SendPublicChatMessage("Help Not Available.");
            }
        }
    }
}