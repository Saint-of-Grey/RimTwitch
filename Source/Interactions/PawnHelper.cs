using System.Collections.Generic;
using System.Linq;
using RimTwitch.Interactions.Me;
using RimTwitch.Interactions.Raid;
using RimTwitch.IRC;
using RimWorld;
using Verse;

namespace RimTwitch.Interactions
{
    public static class PawnHelper
    {
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

        public static Name ReNamePawn(this Pawn pawn, Name initial , bool allowRaiders = true)
        {
            if (!pawn.RaceProps.Humanlike) return initial;

            List<string> names;
            bool raider = false;

            if (pawn.IsColonist)
            {
                names = MeCommand.Names;
            }
            else if (allowRaiders && pawn?.Faction?.GoodwillWith(Faction.OfPlayer) < -20)
            {
                raider = true;
                names = RaidCommand.Names;
            }
            else
            {
                return initial;
            }

            var newName = names.FirstOrDefault();


            if (!(initial is NameTriple triple)) return initial;

            if (newName != null && names.Remove(newName))
            {
                
                var __result = new NameTriple(PawnCommand.twitch, newName, raider ? "Raider" : newName);

                //preserve pawn

                Broadcast.OnAir()
                    ?.SendPublicChatMessage("@" + newName +
                                            " - You're now in the game. type `!" + (raider ? "raid" : "me") +
                                            " help` for what you can do.");

                return __result;
            }
            else
            {
                //maybe next time
            }

            return initial;
        }

        public static void RenamePawnIfCan(this Pawn pawn)
        {
            var pawnName = pawn.Name;
            if (!(pawnName is NameTriple triple)) return;
            if (triple.First.EqualsIgnoreCase(PawnCommand.twitch)) return;
            pawn.Name = pawn.ReNamePawn(pawnName);
        }
    }
}