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
                if (command.StartsWith(MeCommands.help.ToString()))
                {
                    if (helpText == null)
                        helpText = DefDatabase<HediffDef>.GetNamedSilentFail("rimtwitch_explain_raid");
                    //Help Text
                    message.Append(helpText.description);
                }
                else if (command.StartsWith(MeCommands.die.ToString()))
                {
                    PawnCommand.Die(me, message);
                }
                else if (command.StartsWith(MeCommands.eat.ToString()))
                {
                    message.Append(PawnCommand.EatSomething(me) ? "Eating!" : "Not hungry");
                }
                else if (command.StartsWith(MeCommands.vomit.ToString()))
                {
                    PawnCommand.Barf(me);
                    message.Append(" nice one!");
                }
                else if (command.StartsWith(MeCommands.cower.ToString()))
                {
                    message.Append(PawnCommand.RunAndFlee(me) ? "RUN AWAY!!!" : "Nothing to flee from.");
                }
                else if (command.StartsWith(MeCommands.aggressive.ToString()))
                {
                    if(me.playerSettings == null) me.playerSettings = new Pawn_PlayerSettings(me);
                    me.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
                    message.Append("will now attack the bad dudes!");
                }
                else if (command.StartsWith(MeCommands.pacifist.ToString()))
                {
                    if(me.playerSettings == null) me.playerSettings = new Pawn_PlayerSettings(me);
                    me.playerSettings.hostilityResponse = HostilityResponseMode.Flee;
                    message.Append("will now bravely run away!");
                }

                else if (command.StartsWith(MeCommands.mental.ToString()))
                {
                    PawnCommand.MentalBreak(me, message);
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