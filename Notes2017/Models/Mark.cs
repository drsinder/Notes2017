/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: Mark.cs
**
**  Description:
**      Marked notes Data Model
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
    /// Notes Marked for Output
    /// </summary>
    public class Mark
    {
        [Required]
        [StringLength(450)]
        [Column(Order = 0)]
        public string UserID { get; set; }

        [Required]
        [Column(Order = 1)]
        public int NoteFileID { get; set; }

        [Required]
        [Column(Order = 2)]
        public int MarkOrdinal { get; set; }

        [Required]
        public int NoteOrdinal { get; set; }

        [Required]
        public long BaseNoteHeaderID { get; set; }

        [Required]
        public int ResponseOrdinal { get; set; }  // -1 == whole string, 0 base note only, > 0 Response

        [ForeignKey("NoteFileID")]
        public NoteFile NoteFile { get; set; }

    }
}
