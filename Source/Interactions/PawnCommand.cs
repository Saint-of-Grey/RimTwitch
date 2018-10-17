using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using RimTwitch.Interactions.Me;
using RimTwitch.Interactions.Raid;
using RimTwitch.IRC;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTwitch.Interactions
{
    public static class PawnCommand
    {
        const string twitch = "Twitch:";
        
        public static Pawn FindMe(string userName)
        {
            foreach (var map in Find.Maps.ToArray())
            {
                foreach (var pawn in map.mapPawns.AllPawns.Where(x =>
                    x.Name is NameTriple triple && triple.Nick.EqualsIgnoreCase(userName)))
                {
                    return pawn;
                }
                
                
            }
            
            foreach (var pawn in Find.WorldPawns.AllPawnsAlive.Where(x =>
                x.Name is NameTriple triple && triple.Nick.EqualsIgnoreCase(userName)))
            {
                return pawn;
            }
#if DEBUG
            Log.Message("Couldnt find you");
#endif
            return null;
        }

        public static string Summarize(this Pawn me)
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

        private static JobGiver_ConfigurableHostilityResponse response = new JobGiver_ConfigurableHostilityResponse();
        private static JobGiver_GetFood feedMe = new JobGiver_GetFood();

        private static MethodInfo Flee = AccessTools.Method(typeof(JobGiver_ConfigurableHostilityResponse),
            "TryGetFleeJob", new[] {typeof(Pawn)});

        private static MethodInfo Eat = AccessTools.Method(typeof(JobGiver_GetFood),
            "TryGiveJob", new[] {typeof(Pawn)});

        public static IEnumerable<int> getTimeSpan(this Pawn me, string command)
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

        public static void MentalBreak(this Pawn me, StringBuilder message)
        {
            message.Append(me.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak()
                ? " broke down into a mental state!"
                : " is ok.");
        }

        public static bool EatSomething(this Pawn me)
        {
            var result = (Job) Eat.Invoke(feedMe, new object[] {me});
            if (result == null) return false;
            me.jobs.StartJob(result, JobCondition.InterruptForced, null, true, true, null,
                null, false);
            return true;
        }

        public static bool RunAndFlee(this Pawn me)
        {
            var result = (Job) Flee.Invoke(response, new object[] {me});
            if (result == null) return false;
            me.jobs.StartJob(result, JobCondition.InterruptForced, null, true, true, null,
                null, false);
            return true;
        }

        public static void Die(this Pawn me, StringBuilder message)
        {
            me.Name = PawnBioAndNameGenerator.GeneratePawnName(me);
            message.Append("Your pawn has been released. Someone else's problem now.");
        }

        public static void Barf(this Pawn me)
        {
            me.jobs.StartJob(new Job(JobDefOf.Vomit), JobCondition.InterruptForced, null, true, true, null, null,
                false);
        }

        public static void RenamePawnIfCan(this Pawn pawn)
        {
            var pawnName = pawn.Name;
            if (pawnName is NameTriple triple)
            {
                if (!triple.First.EqualsIgnoreCase(twitch))
                {
                    ReNamePawn(pawn, ref pawnName);
                    pawn.Name = pawnName;
                    
                    
                } 
            }
        }

        public static void ReNamePawn(this Pawn pawn, ref Name __result , bool allowRaiders = true)
        {
            if (!pawn.RaceProps.Humanlike) return;

            List<string> names;
            bool raider = false;

            if (pawn.IsColonist)
            {
                names = MeCommand.Names;
            }
            else if (allowRaiders && pawn.Faction.GoodwillWith(Faction.OfPlayer) < -20)
            {
                raider = true;
                names = RaidCommand.Names;
            }
            else
            {
                return;
            }

            var newName = names.FirstOrDefault();


            if (!(__result is NameTriple triple)) return;

            if (newName != null && names.Remove(newName))
            {
                
                __result = new NameTriple(twitch, newName, raider ? "Raider" : newName);

                //preserve pawn

                Broadcast.OnAir()
                    ?.SendPublicChatMessage("@" + newName +
                                            " - You're now in the game. type `!" + (raider ? "raid" : "me") +
                                            " help` for what you can do.");
            }
            else
            {
                //maybe next time
            }
        }
    }
}