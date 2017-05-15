/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: Jobs.cs
**
**  Description:
**      Pusher Jobs Methods
**
**  This program is free software: you can redistribute it and/or modify
**  it under the terms of the GNU General Public License version 3 as
**  published by the Free Software Foundation.
**  
**  This program is distributed in the hope that it will be useful,
**  but WITHOUT ANY WARRANTY; without even the implied warranty of
**  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
**  GNU General Public License version 3 for more details.
**  
**  You should have received a copy of the GNU General Public License
**  version 3 along with this program in file "license-gpl-3.0.txt".
**  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
**
**--------------------------------------------------------------------------
*/

using System;
using PusherServer;
using Hangfire;
using Notes2017.Models;
using Notes2017.App_Code;

namespace Notes2017
{
    public class Jobs
    {
        public void UpdateHomePageTime(string username, TZone tzone)
        {
            string stuff = tzone.Local(DateTime.Now.ToUniversalTime()).ToShortTimeString() + " " + tzone.Abbreviation + " - " +
                tzone.Local(DateTime.Now.ToUniversalTime()).ToLongDateString();

            var options = new PusherOptions()
            {
                Encrypted = true
            };
            Pusher pusher = new Pusher(Global.PusherAppId, Global.PusherKey, Global.PusherSecret, options);
            var data = new { message = stuff };
            pusher.Trigger("private-data-" + username, "update-time", data);
        }

        public void CleanUpHomePageTime(string username)
        {
            RecurringJob.RemoveIfExists(username);
        }
    }
}
