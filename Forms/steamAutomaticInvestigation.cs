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
        public steamAutomaticInvestigation(Player pSteamUser, PlayerBans pSteamUserBans, FriendsList pSteamUserFriends, List<SummaryRoot> pSteamFriendsSummary, int currentSummaryCount)
        {
            InitializeComponent();
        }
    }
}
