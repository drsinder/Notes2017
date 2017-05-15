/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: Sequencer.cs
**
**  Description:
**      Sequncer Data Model
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
    /// <summary>
    /// Model for sequencing a file.
    /// </summary>
    public class Sequencer
    {
        // ID of the user who owns the item
        [Required]
        [StringLength(450)]
        [Column(Order = 0)]
        public string UserID { get; set; }

        // ID of target notfile
        [Required]
        [Column(Order = 1)]
        public int NoteFileID { get; set; }

        [Required]
        [Display(Name = "Position in List")]
        public int Ordinal { get; set; }

        // Time we last completed a run with this
        [Display(Name = "Last Time")]
        public DateTime LastTime { get; set; }

        // Time a run in this file started - will get copied to LastTime when complete
        public DateTime StartTime { get; set; }

        // Is this item active now?  Are we sequencing this file
        public bool Active { get; set; }

        [ForeignKey("NoteFileID")]
        public NoteFile NoteFile { get; set; }

    }
}
