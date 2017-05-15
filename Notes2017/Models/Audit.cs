/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: Audit.cs
**
**  Description:
**      Audit Data Model
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes2017.Models
{
    public class Audit
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditID { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Event Type")]
        public string EventType { get; set; }

        // Name of the user did made it
        [Required]
        [StringLength(256)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(450)]
        public string UserID { get; set; }

        [Required]
        [Display(Name = "Event Time")]
        public DateTime EventTime { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Event")]
        public string Event { get; set; }
    }
}
