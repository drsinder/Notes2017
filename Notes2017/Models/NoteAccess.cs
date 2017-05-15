/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NoteAccess.cs
**
**  Description:
**      Note Access Data Model
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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes2017.Models
{
    /// <summary>
    /// This Model represents the access a user has in a file.
    /// Keyed by UserID and NoteFileID
    /// </summary>
    public class NoteAccess
    {
        [Required]
        [StringLength(450)]
        [Column(Order = 0)]
        public string UserID { get; set; }

        [Required]
        [Column(Order = 1)]
        public int NoteFileID { get; set; }

        //[ForeignKey("NoteFileID")]
        //public NoteFile NoteFile { get; set; }

        // Control options

        [Required]
        [Display(Name = "Read")]
        public bool Read { get; set; }

        [Required]
        [Display(Name = "Respond")]
        public bool Respond { get; set; }

        [Required]
        [Display(Name = "Write")]
        public bool Write { get; set; }

        [Required]
        [Display(Name = "SetTag")]
        public bool SetTag { get; set; }

        [Required]
        [Display(Name = "Delete/Edit")]
        public bool DeleteEdit { get; set; }

        [Required]
        [Display(Name = "View Access")]
        public bool Director { get; set; }

        [Required]
        [Display(Name = "Edit Access")]
        public bool EditAccess { get; set; }

    }
}
