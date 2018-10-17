//#define DEBUG

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using RimTwitch.IRC;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTwitch.Interactions.Me
{
    public static class NameQueue
    {
        private static JobGiver_ConfigurableHostilityResponse response = new JobGiver_ConfigurableHostilityResponse();
        private static JobGiver_GetFood feedMe = new JobGiver_GetFood();

        private static MethodInfo Flee = AccessTools.Method(typeof(JobGiver_ConfigurableHostilityResponse),
            "TryGetFleeJob", new[] {typeof(Pawn)});

        private static MethodInfo Eat = AccessTools.Method(typeof(JobGiver_GetFood),
            "TryGiveJob", new[] {typeof(Pawn)});

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
                }
                else if (command.StartsWith(MeCommands.die.ToString()))
                {
                    Die(me, message);
                }
                else if (command.StartsWith(MeCommands.vomit.ToString()))
                {
                    Barf(me);
                }
                else if (command.StartsWith(MeCommands.cower.ToString()))
                {
                    message.Append(RunAndFlee(me) ? "RUN AWAY!!!" : "Nothing to flee from.");
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
                    message.Append(EatSomething(me) ? "Eating!" : "Not hungry");
                }


                else if (command.StartsWith(MeCommands.fun.ToString()))
                {
                    foreach (var i in getTimeSpan(me, command))
                        FunTime(me, message, i);
                }

                else if (command.StartsWith(MeCommands.sleep.ToString()))
                {
                    foreach (var i in getTimeSpan(me, command))

                        SleepTime(me, message, i);
                }

                else if (command.StartsWith(MeCommands.work.ToString()))
                {
                    foreach (var i in getTimeSpan(me, command))
                        Work(me, message, i);
                }

                else if (command.StartsWith(MeCommands.mental.ToString()))
                {
                    MentalBreak(me, message);
                }
                else
                {
                    message.Append("Sorry, What?");
                }
            }
            else
            {
                message.Append("Status: ").Append(Summarize(me));
            }

            ircClient.SendPublicChatMessage(message.ToString());
        }

        private static IEnumerable<int> getTimeSpan(Pawn me, string command)
        {
            if (command.Contains(Times.now.ToString()))
            {
                yield return GenLocalDate.HourOfDay(me);
            }

            if (command.Contains(Times.dawn.ToString()))
            {
                yield return 5;
                yield return 6;
                yield return 7;
                yield return 8;
            }

            if (command.Contains(Times.morning.ToString()))
            {
                yield return 18;
                yield return 19;
                yield return 20;
                yield return 21;
            }

            if (command.Contains(Times.noon.ToString()))
            {
                yield return 11;
                yield return 12;
                yield return 13;
            }

            if (command.Contains(Times.evening.ToString()))
            {
                yield return 18;
                yield return 19;
                yield return 20;
                yield return 21;
            }


            if (command.Contains(Times.midnight.ToString()))
            {
                yield return 23;
                yield return 0;
                yield return 1;
                yield return 2;
            }

            if (command.Contains(Times.day.ToString()))
            {
                for (int i = 1; i < 16; i++)
                    yield return (i + 8) % 24;
            }

            if (command.Contains(Times.night.ToString()))
            {
                for (int i = 1; i < 16; i++)
                    yield return (i + 16) % 24;
            }


            if (command.Contains(Times.always.ToString()))
            {
                for (int i = 0; i < 24; i++)
                    yield return i;
            }
        }

        private static void MentalBreak(Pawn me, StringBuilder message)
        {
            message.Append(me.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak()
                ? " broke down into a mental state!"
                : " is ok.");
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

        private static bool EatSomething(Pawn me)
        {
            var result = (Job) Eat.Invoke(feedMe, new object[] {me});
            if (result == null) return false;
            me.jobs.StartJob(result, JobCondition.InterruptForced, null, true, true, null,
                null, false);
            return true;
        }

        private static bool RunAndFlee(Pawn me)
        {
            var result = (Job) Flee.Invoke(response, new object[] {me});
            if (result == null) return false;
            me.jobs.StartJob(result, JobCondition.InterruptForced, null, true, true, null,
                null, false);
            return true;
        }

        private static void Die(Pawn me, StringBuilder message)
        {
            me.Name = PawnBioAndNameGenerator.GeneratePawnName(me);
            message.Append("Your pawn has been released. Someone else's problem now.");
        }

        private static void Barf(Pawn me)
        {
            me.jobs.StartJob(new Job(JobDefOf.Vomit), JobCondition.InterruptForced, null, true, true, null, null,
                false);
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

    public enum Times
    {
        day,
        dawn,
        morning,
        noon,
        evening,
        night,
        midnight,
        always,
        now
    }
}