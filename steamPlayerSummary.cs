using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;

namespace steamPlayerInvestigator
{
    public partial class steamPlayerSummary : Form
    {
        public steamPlayerSummary(Player pSteamUser, PlayerBans pSteamUserBans, FriendsList pSteamUserFriends, List<SummaryRoot> pSteamFriendsSummary)
        {
            InitializeComponent();

            steamIdLabel.Text = "Steam ID:  " + pSteamUser.steamid;
            communityVisibilityStateLabel.Text = "Community Visibility State: " + pSteamUser.communityvisibilitystate;
            profileStateLabel.Text = "Profile State: " + pSteamUser.profilestate;
            personaNameLabel.Text = "Persona Name: " + pSteamUser.personaname;
            profileUrlLabel.Text = "Profile URL: " + pSteamUser.profileurl;
            avatarLabel.Text = "Avatar: " + pSteamUser.avatar;
            avatarMediumLabel.Text = "Avatar Medium: " + pSteamUser.avatarmedium;
            avatarFullLabel.Text = "Avatar Full: " + pSteamUser.avatarfull;
            avatarHashLabel.Text = "Avatar Hash: " + pSteamUser.avatarhash;
            lastLogoffLabel.Text = "Last Logoff: " + UnixTimeToDateTime(pSteamUser.lastlogoff);
            personaStateLabel.Text = "Persona State: " + pSteamUser.personastate;
            primaryClanIdLabel.Text = "Primary Clan ID: " + pSteamUser.primaryclanid;
            timeCreatedLabel.Text = "Time Created: " + UnixTimeToDateTime(pSteamUser.timecreated);
            personaStateFlagsLabel.Text = "Persona State Flags: " + pSteamUser.personastateflags;
            locCountryCodeLabel.Text = "Loc Country Code: " + pSteamUser.loccountrycode;

            communityBannedLabel.Text = "Community Banned: " + pSteamUserBans.CommunityBanned;

            if(pSteamUserBans.CommunityBanned == true)
            {
                communityBannedLabel.ForeColor = Color.Red;
            }
            else
            {
                communityBannedLabel.ForeColor = Color.Green;
            }

            vacBannedLabel.Text = "Vac Banned: " + pSteamUserBans.VACBanned;

            if (pSteamUserBans.VACBanned == true)
            {
                vacBannedLabel.ForeColor = Color.Red;
            }
            else
            {
                vacBannedLabel.ForeColor = Color.Green;
            }

            numberOfVacBansLabel.Text = "Number of VAC Bans: " + pSteamUserBans.NumberOfVACBans;

            if (pSteamUserBans.NumberOfVACBans == 0)
            {
                numberOfVacBansLabel.ForeColor = Color.Green;
            }
            else
            {
                numberOfVacBansLabel.ForeColor = Color.Red;
            }

            daysSinceLastBanLabel.Text = "Days Since Last Ban: " + pSteamUserBans.DaysSinceLastBan;

            if (pSteamUserBans.DaysSinceLastBan == 0 && pSteamUserBans.NumberOfVACBans == 0 && pSteamUserBans.NumberOfGameBans == 0 && pSteamUserBans.CommunityBanned == false && pSteamUserBans.EconomyBan == "none")
            {
                daysSinceLastBanLabel.ForeColor = Color.Green;
            }
            else
            {
                daysSinceLastBanLabel.ForeColor = Color.Red;
            }

            numberOfGameBansLabel.Text = "Number of Game Bans: " + pSteamUserBans.NumberOfGameBans;

            if (pSteamUserBans.NumberOfGameBans == 0)
            {
                numberOfGameBansLabel.ForeColor = Color.Green;
            }
            else
            {
                numberOfGameBansLabel.ForeColor = Color.Red;
            }

            economyBanLabel.Text = "Economy Ban: " + pSteamUserBans.EconomyBan;

            if (pSteamUserBans.EconomyBan == "none")
            {
                economyBanLabel.ForeColor = Color.Green;
            }
            else
            {
                economyBanLabel.ForeColor = Color.Red;
            }

            for(int i = 0; i < pSteamFriendsSummary.Count; i++)
            {
                for(int o = 0; o < pSteamFriendsSummary[i].response.players.Count; o++)
                {
                    friendsListBox.Items.Add(pSteamFriendsSummary[i].response.players[o].personaname);
                }
            }
        }

        static public DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime newDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            newDateTime = newDateTime.AddSeconds(unixtime).ToLocalTime();
            return newDateTime;
        }
    }
}
