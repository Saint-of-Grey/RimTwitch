using System;
using RimTwitch.IRC;
using UnityEngine;
using Verse;

namespace RimTwitch
{
    public class RimTwitch : Mod
    {
        public static TwitchModSettings latest;
        private TwitchModSettings _settings;
        public RimTwitch(ModContentPack content) : base(content)
        {
            latest = this._settings = GetSettings<TwitchModSettings>();
            
        }
        
        public override string SettingsCategory() => "-RimTwitch";


        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Def oauth = DefDatabase<HediffDef>.GetNamedSilentFail("rimtwitch_explain_oauth");
            Def bot = DefDatabase<HediffDef>.GetNamedSilentFail("rimtwitch_explain_bot");
            Def you = DefDatabase<HediffDef>.GetNamedSilentFail("rimtwitch_explain_you");

            if (you == null || bot == null || oauth == null)
            {
                Widgets.TextArea(inRect, "Missing explain defs", true);
                _settings.Clear();
                return;
            }
            
            var topHalf = inRect.TopHalf();
            var fourth = topHalf.TopHalf();
            var mySlice = fourth.TopHalf();
            
            Widgets.Label(mySlice.LeftHalf(), oauth.description);
            //_settings.oauth = Widgets.TextArea(mySlice.RightHalf().ContractedBy(4f), _settings.oauth);

            mySlice = fourth.BottomHalf();
            
            Widgets.Label(mySlice.LeftHalf(), you.description);
            _settings.yourName = Widgets.TextArea(mySlice.RightHalf().ContractedBy(4f), _settings.yourName);

            fourth = topHalf.BottomHalf();
            mySlice = fourth.TopHalf();
            Widgets.Label(mySlice.LeftHalf(), bot.description);
            _settings.botName = Widgets.TextArea(mySlice.RightHalf().ContractedBy(4f), _settings.botName);


            mySlice = inRect.BottomHalf().BottomHalf().BottomHalf().BottomHalf();
            if (Widgets.ButtonText(mySlice.LeftHalf().ContractedBy(2f), "Test"))
            {
                try
                {
                    Broadcast.Start(_settings.oauth, _settings.yourName, _settings.botName).SendIrcMessage("RimWorld Twitch IRC Test");

                    Broadcast.Stop();
                    
                    Log.Message("Test : Success");

                }
                catch (Exception e)
                {
                    Log.Error("Twitch IRC Failed! "+e.StackTrace);
                }
            }
            
            if (Widgets.ButtonText(mySlice.RightHalf().ContractedBy(2f), "Start"))
            {
                try
                {
                    Broadcast.Start(_settings.oauth, _settings.yourName, _settings.botName);

                    Log.Message("Startup Success");

                }
                catch (Exception e)
                {
                    Log.Error("Twitch IRC Failed! "+e.StackTrace);
                }
            }
            
            this._settings.Write();
        }
    }
    
    
    public class TwitchModSettings : ModSettings
    {
        public String oauth = "oauth:XXXXX", botName = "RimWorld", yourName = "MyAccountName";

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.oauth, "oauth", "oauth:XXXXX");
            Scribe_Values.Look(ref this.botName, "botName", "");
            Scribe_Values.Look(ref this.yourName, "yourName", "");
        }

        public void Clear()
        {
            oauth = "oauth:XXXXX";
            botName = "RimWorldBot"; 
            yourName = "MyAccountName";
        }
    }
}