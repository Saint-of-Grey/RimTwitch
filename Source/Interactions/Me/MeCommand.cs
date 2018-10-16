//#define DEBUG

using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimTwitch.IRC;
using RimWorld;
using Verse;

namespace RimTwitch.Interactions.Me
{
    public static class NameQueue
    {
        public static readonly List<string> Names = new List<string>();

        private static Def helpText;


        public static void Me(IrcClient ircClient, string userName, string message)
        {
#if DEBUG
            Log.Message("!me for "+userName);
#endif
            if (Names.Any(x => x.EqualsIgnoreCase(userName)))
            {
#if DEBUG
                Log.Message("Already got that name");
#endif
                ircClient.SendPublicChatMessage("@" + userName + " : Please be patient you are queued to be spawned");
                return; //pending
            }


#if DEBUG
            Log.Message("Name not queued");
#endif
            Pawn me = FindMe(userName);

            if (me == null)
            {
#if DEBUG
                Log.Message("Queued");
#endif
                ircClient.SendPublicChatMessage("@" + userName + " : Added name to the queue");
                Names.Add(userName); //queued
            }
            else
                SummarizeMe(me, ircClient, userName, message);


#if DEBUG
            Log.Message("!me done");
#endif
        }

        private static void SummarizeMe(Pawn me, IrcClient ircClient, string userName, string command)
        {
            //TODO Command
            var message = new StringBuilder("@" + userName + " : ");
            
            command = command?.Substring(3)?.Trim()?.ToLower();
            if (!command.NullOrEmpty())
            {
                
                if (command.StartsWith(MeCommands.help.ToString()))
                {
                    if (helpText == null)
                        helpText = DefDatabase<HediffDef>.GetNamedSilentFail("rimtwitch_explain_me");
                    //Help Text
                    message.Append(helpText.description);
                   


                }else if (command.StartsWith(MeCommands.die.ToString()))
                {
                    
                    me.Name = PawnBioAndNameGenerator.GeneratePawnName(me);
                    message.Append("Your pawn has been released. Someone else's problem now.");
                }
            }
            else
            {
                message.Append("Status: ").Append(Summarize(me));
            }

            
            ircClient.SendPublicChatMessage(message.ToString());
        }

        private static string Summarize(Pawn me)
        {
#if DEBUG
            Log.Message("Found you");
#endif
            StringBuilder sb = new StringBuilder();
            sb.Append(me.CurJob?.GetReport(me) ?? "[Idle]");
            sb.Append(" HP: ").Append(me.health.summaryHealth.SummaryHealthPercent * 100).Append("% ");
            foreach (var need in me.needs.AllNeeds)
            {
                if (!need.ShowOnNeedList) continue;
                sb.Append(need.LabelCap).Append(": ").Append(need.CurLevelPercentage.ToStringPercent()).Append(" ");
            }

            var s = sb.ToString();
#if DEBUG
            Log.Message("Sending Summary : "+s);
#endif
            return s;
        }

        private static Pawn FindMe(string userName)
        {
            foreach (var map in Find.Maps.ToArray())
            {
                foreach (var pawn in map.mapPawns.AllPawns.Where(x =>
                    x.Name is NameTriple triple && triple.Nick.EqualsIgnoreCase(userName)))
                {
                    return pawn;
                }
            }
#if DEBUG
            Log.Message("Couldnt find you");
#endif
            return null;
        }
    }

    public enum MeCommands
    {
        help,
        die,
        cower,
        mental,
        eat,
        sleep,
        work, 
        fun,
        vomit,
        aggressive,
        pacifist
        
    }
}