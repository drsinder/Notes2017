/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: ApplicationUser.cs
**
**  Description:
**      Application User Data Model
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

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Notes2017.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public int TimeZoneID { get; set; }

        public int Ipref2 { get; set; }

        public int Ipref3 { get; set; }

        public int Ipref4 { get; set; }

        public int Ipref5 { get; set; }

        public int Ipref6 { get; set; }

        public int Ipref7 { get; set; }

        public int Ipref8 { get; set; }


        [Display(Name = "Hide Note Menu")]
        public bool Pref1 { get; set; }

        [Display(Name = "Update time on Home page")]
        public bool Pref2 { get; set; }

        public bool Pref3 { get; set; }

        public bool Pref4 { get; set; }

        public bool Pref5 { get; set; }

        public bool Pref6 { get; set; }

        public bool Pref7 { get; set; }

        public bool Pref8 { get; set; }

        [Display(Name = "Style Preferences")]
        [StringLength(5000)]
        public string MyStyle { get; set; }

    }
}
