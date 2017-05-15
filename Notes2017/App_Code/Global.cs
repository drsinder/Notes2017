/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: Global.cs
**
**  Description:
**      Global variables for Notes 2017
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

namespace Notes2017.App_Code
{
    public static class Global
    {
        public static string AdminEmail { get; set; }

        public static string SendGridEmail { get; set; }

        public static string SendGridApiKey { get; set; }

        public static string EmailName { get; set; }

        public static string TwilioName { get; set; }
        public static string TwilioPassword { get; set; }
        public static string TwilioNumber { get; set; }

        public static string AccessOther() { return "Other"; }

        public static string ImportedAuthorId() { return "imported"; }

        public static string InstKey { get; set; }

        public static string ProductionUrl { get; set; }

        public static int ZoneMinId { get; set; }
        public static int ZoneUtcid { get; set; }

        public static string PusherAppId { get; set; }
        public static string PusherKey { get; set; }
        public static string PusherSecret { get; set; }

        public static Services.IEmailSender EmailSender { get; set; }
    }
}
