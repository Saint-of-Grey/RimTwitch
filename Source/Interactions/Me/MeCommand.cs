//#define DEBUG

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using JetBrains.Annotations;
using RimTwitch.IRC;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTwitch.Interactions.Me
{
    public static class MeCommand
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
            Pawn me = PawnCommand.FindMe(userName);

            if (me == null)
            {
#if DEBUG
                Log.Message("Queued");
#endif
                ircClient.SendPublicChatMessage("@" + userName + " : Added name to the queue");
                Names.Add(userName); //queued
            }
            else
                DoMe(me, ircClient, userName, message);


#if DEBUG
            Log.Message("!me done");
#endif
        }

        private static void DoMe(Pawn me, IrcClient ircClient, string userName, [NotNull] string command)
        {
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
                }
                else if (command.StartsWith(MeCommands.die.ToString()))
                {
                    PawnCommand.Die(me, message);
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
                    me.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
                    message.Append("will now attack the bad dudes!");
                }
                else if (command.StartsWith(MeCommands.pacifist.ToString()))
                {
                    me.playerSettings.hostilityResponse = HostilityResponseMode.Flee;
                    message.Append("will now bravely run away!");
                }

                else if (command.StartsWith(MeCommands.eat.ToString()))
                {
                    message.Append(PawnCommand.EatSomething(me) ? "Eating!" : "Not hungry");
                }


                else if (command.StartsWith(MeCommands.fun.ToString()))
                {
                    foreach (var i in PawnCommand.getTimeSpan(me, command))
                        FunTime(me, message, i);
                }

                else if (command.StartsWith(MeCommands.sleep.ToString()))
                {
                    foreach (var i in PawnCommand.getTimeSpan(me, command))

                        SleepTime(me, message, i);
                }

                else if (command.StartsWith(MeCommands.work.ToString()))
                {
                    foreach (var i in PawnCommand.getTimeSpan(me, command))
                        Work(me, message, i);
                }

                else if (command.StartsWith(MeCommands.mental.ToString()))
                {
                    PawnCommand.MentalBreak(me, message);
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

        private static void Work(Pawn me, StringBuilder message, int now)
        {
            me.timetable.SetAssignment(now, TimeAssignmentDefOf.Work);
            message.Append("Work @ " + now);
        }


        private static void SleepTime(Pawn me, StringBuilder message, int now)
        {
            me.timetable.SetAssignment(now, TimeAssignmentDefOf.Sleep);
            message.Append("Sleep @ " + now);
        }

        private static void FunTime(Pawn me, StringBuilder message, int now)
        {
            me.timetable.SetAssignment(now, TimeAssignmentDefOf.Joy);
            message.Append("Fun @ " + now);
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