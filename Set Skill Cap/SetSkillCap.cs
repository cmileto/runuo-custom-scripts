// Set Skill Cap v1.0.4
// Author: Felladrin
// Contributor: zerodowned
// Started: 2015-12-20
// Updated: 2017-10-02

using System;
using Server;

namespace Felladrin.Automations
{
    public static class SetSkillCap
    {
        public static class Config
        {
            public static bool Enabled = true;                // Is this system enabled?
            public static int IndividualSkillCap = 100;       // Cap for each skill. Default: 100.
            public static int TotalSkillCap = 700;            // Cap for the sum of all skills. Default: 700.
            public static bool OverrideExistingValues = true; // Set to False to prevent a player's cap from being reset
                                                              // in the event that skill cap is set to 100 and player uses
                                                              // Powerscroll to raise cap to 120, so the skill cap won't be
                                                              // reset to 100 on login.
        }

        public static void Initialize()
        {
            if (Config.Enabled)
            {
                Config.IndividualSkillCap *= 10;
                Config.TotalSkillCap *= 10;
                EventSink.Login += OnLogin;
            }
        }

        static void OnLogin(LoginEventArgs args)
        {
            var m = args.Mobile;
            
            if (m.AccessLevel > AccessLevel.Player)
                return;
            
            var skills = m.Skills;

            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i].CapFixedPoint != Config.IndividualSkillCap && Config.OverrideExistingValues)
                    skills[i].CapFixedPoint = Config.IndividualSkillCap;

                skills[i].BaseFixedPoint = Math.Min(skills[i].BaseFixedPoint, skills[i].CapFixedPoint);
            }
            
            m.SkillsCap = Config.TotalSkillCap;
            
            for (int j = 0; (m.SkillsTotal > m.SkillsCap) && (j < skills.Length); j++)
            {
                double diff = ((m.SkillsTotal - m.SkillsCap) / 10) + 1;
                
                if (skills[SkillName.Focus].Base > 0)
                {
                    skills[SkillName.Focus].Base = Math.Max(skills[SkillName.Focus].Base - diff, 0);
                    continue;
                }
                
                if (skills[SkillName.Meditation].Base > 0)
                {
                    skills[SkillName.Meditation].Base = Math.Max(skills[SkillName.Meditation].Base - diff, 0);
                    continue;
                }
                
                int lowestSkillId = -1;
                double lowestSkillBase = Config.IndividualSkillCap;
                
                for (int i = 0; i < skills.Length; i++)
                {
                    if (skills[i].Base > 0 && skills[i].Base < lowestSkillBase)
                    {
                        lowestSkillId = Math.Max(lowestSkillId, i);
                        lowestSkillBase = skills[i].Base;
                    }
                }
                
                if (lowestSkillId == -1)
                    break;

                skills[lowestSkillId].Base = Math.Max(skills[lowestSkillId].Base - diff, 0);
            }
        }
    }
}
