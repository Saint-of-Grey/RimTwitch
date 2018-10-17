using System.Collections.Generic;
using System.Text;
using RimTwitch.Interactions.Me;
using RimTwitch.IRC;
using RimWorld;
using Verse;

namespace RimTwitch.Interactions.Raid
{
    public static class RaidCommand
    {
        private static Def helpText;
        private static int _twitchRaiders = 0;
        public static readonly List<string> Names = new List<string>();
        
        //TODO ButAScratch_: I don't know anything about the restrictions, but a roll to start a raid would be cool.

        
        

        public static void Start(float points)
        {
            IncidentParms incidentParams = new IncidentParms {target = Find.AnyPlayerHomeMap, points = points, forced=true};
            IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParams);
        }
        
        
        
        
        public static void Me(IrcClient ircClient, string userName, string message)
        {
            userName += " Raider";
            
            if (Names.Any(x => x.EqualsIgnoreCase(userName)))
            {
                ircClient.SendPublicChatMessage("@" + userName + " : Please be patient you are queued to be spawned");
                return; //pending
            }

            Pawn me = PawnCommand.FindMe(userName);

            if (me == null)
            {
                _twitchRaiders++;
                ircClient.SendPublicChatMessage("@" + userName + " : Added name to the raider queue");
                Names.Add(userName); //queued
            }
            else
                DoRaider(me, ircClient, userName, message);


        }

        private static void DoRaider(Pawn me, IrcClient ircClient, string userName, string command)
        {
            
            var message = new StringBuilder("@" + userName + " : ");

            command = command?.Substring(command.IndexOf(" "))?.Trim()?.ToLower();
            
            if (!command.NullOrEmpty())
            {
                if(PawnCommand.CommonPawnCommands(me, command, message)) {} else
                if (command.StartsWith(MeCommands.help.ToString()))
                {
                    if (helpText == null)
                        helpText = DefDatabase<HediffDef>.GetNamedSilentFail("rimtwitch_explain_raid");
                    //Help Text
                    message.Append(helpText.description);
                }
                else if (command.StartsWith(RaidCommands.now.ToString()))
                {
                    PawnCommand.Enter(me, message);
                }
                else if (command.StartsWith(RaidCommands.escape.ToString()))
                {
                    Escape(me, message);
                }
                else
                {
                    message.Append("Sorry, What?");
                }
            }
            else
            {
                message.Append("Status: ").Append(me.Summarize());
            }

            ircClient.SendPublicChatMessage(message.ToString());
        }

        private static void Escape(Pawn me, StringBuilder message)
        {
            if (!me.IsPrisoner)
            {
                message.Append("You're not in jail?");
                return;
            }
            
            PrisonBreakUtility.StartPrisonBreak(me);
        }
    }

    public enum RaidCommands
    {
        now, 
        escape
    }
}