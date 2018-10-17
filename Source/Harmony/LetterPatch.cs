using System.Text;
using Harmony;
using RimTwitch.Interactions;
using RimTwitch.IRC;
using RimWorld;
using Verse;

namespace RimTwitch.Harmony
{
    [HarmonyPatch(typeof(Letter), "Received")]
    public static class Letter_Received
    {
        [HarmonyPostfix]
        public static void Postfix(Letter __instance)
        {
            var msg = new StringBuilder();
            var arch = ((IArchivable) __instance);
            
            if (arch.LookTargets.PrimaryTarget.Thing is Pawn p)
            {
                if (p.Name is NameTriple triple && triple.First.EqualsIgnoreCase(PawnCommand.twitch))
                {
                    msg.Append('@').Append(triple.Nick).Append(" : ");
                }
            }
            
            msg.Append("[").Append( arch.ArchivedLabel ).Append("] - ").Append( arch.ArchivedTooltip.Replace("\n", " "));
            
            
            Broadcast.OnAir()?.SendPublicChatMessage(msg.ToString());
        }
    }
}