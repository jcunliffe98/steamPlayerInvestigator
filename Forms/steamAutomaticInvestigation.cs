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
using Newtonsoft.Json;

namespace steamPlayerInvestigator.Forms
{
    public partial class steamAutomaticInvestigation : Form
    {
        public steamAutomaticInvestigation(Player pSteamUser, PlayerBans pSteamUserBans, SummaryRoot pSteamFriendsSummary)
        {
            InitializeComponent();
            List<Player> bannedPlayers = new List<Player>();

            for(int i = 0; i < pSteamFriendsSummary.response.players.Count; i++)
            {
                if(pSteamFriendsSummary.response.players[i].VACBanned == true || pSteamFriendsSummary.response.players[i].NumberOfGameBans != 0)
                {
                    bannedPlayers.Add(pSteamFriendsSummary.response.players[i]);
                }
                if(pSteamFriendsSummary.response.players[i].friendsOfFriends == null || pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist == null)
                {
                    continue;
                }
                for(int y = 0; y < pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends.Count; y++)
                {
                    if (pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].VACBanned == true ||
                        pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].NumberOfGameBans != 0)
                    {
                        Player tempPlayer = getPlayer(pSteamFriendsSummary, i, y);

                        bannedPlayers.Add(tempPlayer);
                    }
                }
            }
            MessageBox.Show("hi lol");
        }

        public Player getPlayer(SummaryRoot pSteamFriendsSummary, int i, int y)
        {
            Player tempPlayer = new Player();
            tempPlayer.avatar = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].avatar;
            tempPlayer.avatarfull = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].avatarfull;
            tempPlayer.avatarhash = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].avatarhash;
            tempPlayer.avatarmedium = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].avatarmedium;
            tempPlayer.CommunityBanned = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].CommunityBanned;
            tempPlayer.communityvisibilitystate = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].communityvisibilitystate;
            tempPlayer.DaysSinceLastBan = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].DaysSinceLastBan;
            tempPlayer.EconomyBan = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].EconomyBan;
            tempPlayer.friendsOfFriends = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].friendsOfFriends;
            tempPlayer.friend_since = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].friend_since;
            tempPlayer.lastlogoff = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].lastlogoff;
            tempPlayer.loccountrycode = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].loccountrycode;
            tempPlayer.NumberOfGameBans = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].NumberOfGameBans;
            tempPlayer.NumberOfVACBans = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].NumberOfVACBans;
            tempPlayer.personaname = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].personaname;
            tempPlayer.personastate = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].personastate;
            tempPlayer.personastateflags = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].personastateflags;
            tempPlayer.primaryclanid = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].primaryclanid;
            tempPlayer.profilestate = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].profilestate;
            tempPlayer.profileurl = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].profileurl;
            tempPlayer.relationship = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].relationship;
            tempPlayer.steamid = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].steamid;
            tempPlayer.timecreated = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].timecreated;
            tempPlayer.VACBanned = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].VACBanned;
            return tempPlayer;
        }
    }
}
