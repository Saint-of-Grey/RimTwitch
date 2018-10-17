using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using RimTwitch.Interactions.Me;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTwitch.Interactions
{
    public static class PawnCommand
    {
        public static string twitch = "Twitch:";
        
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

        private static JobGiver_AIFightEnemies fight = new JobGiver_AIFightEnemies();
        private static JobGiver_ConfigurableHostilityResponse response = new JobGiver_ConfigurableHostilityResponse();
        private static JobGiver_GetFood feedMe = new JobGiver_GetFood();

        private static MethodInfo Fight = AccessTools.Method(typeof(JobGiver_AIFightEnemies),
            "TryGiveJob", new[] {typeof(Pawn)});
        
        private static MethodInfo Flee = AccessTools.Method(typeof(JobGiver_ConfigurableHostilityResponse),
            "TryGetFleeJob", new[] {typeof(Pawn)});

        private static MethodInfo Eat = AccessTools.Method(typeof(JobGiver_GetFood),
            "TryGiveJob", new[] {typeof(Pawn)});

        public static void MentalBreak(this Pawn me, StringBuilder message)
        {
            message.Append(me.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak()
                ? " broke down into a mental state!"
                : " is ok.");
        }

        public static bool EatSomething(this Pawn me)
        {
            var result = (Job) Eat.Invoke(feedMe, new object[] {me});
            return StartJobIfCan(me, result);
        }

        public static bool RunAndFlee(this Pawn me)
        {
            var result = (Job) Flee.Invoke(response, new object[] {me});
            return StartJobIfCan(me, result);
        }
        
        public static bool AttackSomething(this Pawn me)
        {
            var result = (Job) Fight.Invoke(fight, new object[] {me});
            return StartJobIfCan(me, result);
        }

        public static bool StartJobIfCan(this Pawn me, Job result)
        {
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
        
        

        public static bool CommonPawnCommands(Pawn me, string command, StringBuilder message)
        {
            if (command.StartsWith(MeCommands.die.ToString()))
            {
                me.Die(message);
            }
            else if (command.StartsWith(MeCommands.eat.ToString()))
            {
                message.Append(me.EatSomething() ? "Eating!" : "Not hungry");
            }
            else if (command.StartsWith(MeCommands.vomit.ToString()))
            {
                me.Barf();
                message.Append(" nice one!");
            }
            else if (command.StartsWith(MeCommands.cower.ToString()))
            {
                message.Append(me.RunAndFlee() ? "RUN AWAY!!!" : "Nothing to flee from.");
            }
            else if (command.StartsWith(MeCommands.aggressive.ToString()))
            {
                if (me.playerSettings == null) me.playerSettings = new Pawn_PlayerSettings(me);
                me.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
                message.Append("will now attack the bad dudes!");
            }
            else if (command.StartsWith(MeCommands.pacifist.ToString()))
            {
                if (me.playerSettings == null) me.playerSettings = new Pawn_PlayerSettings(me);
                me.playerSettings.hostilityResponse = HostilityResponseMode.Flee;
                message.Append("will now bravely run away!");
            }

            else if (command.StartsWith(MeCommands.mental.ToString()))
            {
                me.MentalBreak(message);
            }
            else if (command.StartsWith(MeCommands.attack.ToString()))
            {
                message.Append(me.AttackSomething() ? "Blood for the blood gods!!!" : "Nothing to attack");
            }
            else if (command.StartsWith(MeCommands.now.ToString()))
            {
                EnterTheBattlefield.Enter(me, message);
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}