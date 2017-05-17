/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: TZome.cs
**
**  Description:
**      Time Zone Data Model
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Notes2017.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Notes2017.Models
{
    // ReSharper disable once InconsistentNaming
    public class TZone
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // ReSharper disable once InconsistentNaming
        public int TZoneID { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        public string Abbreviation { get; set; }

        [Required]
        public string Offset { get; set; }

        [Required]
        public int OffsetHours { get; set; }

        [Required]
        public int OffsetMinutes { get; set; }

        public DateTime Local(DateTime dt)
        {
            return dt.AddHours(OffsetHours).AddMinutes(OffsetMinutes);
        }

        public DateTime Universal(DateTime dt)
        {
            return dt.AddHours(-OffsetHours).AddMinutes(-OffsetMinutes);
        }


        public byte[] ToBytes()
        {
            string outp = TZoneID + ";" + Name + ";" + Abbreviation + ";" + Offset + ";" + OffsetHours + ";" + OffsetMinutes;
            return Encoding.ASCII.GetBytes(outp);
        }

        public static TZone FromBytes(byte[] inp)
        {
            TZone tz = new TZone();

            string stuff = Encoding.ASCII.GetString(inp); 
            string[] splits = stuff.Split(new char[] { ';' });
            tz.TZoneID = int.Parse(splits[0]);
            tz.Name = splits[1];
            tz.Abbreviation = splits[2];
            tz.Offset = splits[3];
            tz.OffsetHours = int.Parse(splits[4]);
            tz.OffsetMinutes = int.Parse(splits[5]);

            return tz;
        }

        public static async Task<TZone> GetUserTimeZone(HttpContext httpContext, 
            ClaimsPrincipal userx, UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, ApplicationDbContext db)
        {
            try
            {
                return FromBytes(httpContext.Session.Get("TZone"));
            }
            catch
            {
                int tzid = Globals.ZoneUtcid;
                if (signInManager.IsSignedIn(userx))
                {
                    try
                    {
                        ApplicationUser user = await userManager.Users.SingleAsync(p => p.Id == userManager.GetUserId(userx));
                        tzid = user.TimeZoneID;  // get users timezoneid
                    }
                    catch
                    {
                        // ignored
                    }
                }
                if (tzid < Globals.ZoneMinId)
                    tzid = Globals.ZoneUtcid; // UTC is default timezone

                var tz = await db.TZone.SingleAsync(p => p.TZoneID == tzid);
                httpContext.Session.Set("TZone", tz.ToBytes());  // save TZone in Session

                return FromBytes(httpContext.Session.Get("TZone"));
            }
        }

    }
}
