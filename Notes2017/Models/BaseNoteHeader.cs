/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: BaseNoteHeader.cs
**
**  Description:
**      Base Note Header Data Model
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
    /// This Model is an abbreviated form of a Note.  It does not contain the 
    /// potentially long content/body.  It is used to show the list of notes
    /// in a file and to find base notes.
    /// </summary>
    public class BaseNoteHeader
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BaseNoteHeaderID { get; set; }

        [Required]
        public int NoteFileID { get; set; }

        [ForeignKey("NoteFileID")]
        public NoteFile NoteFile { get; set; }

        [Required]
        public long NoteID { get; set; }

        [Required]
        [Display(Name = "Note #")]
        public int NoteOrdinal { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Subject")]
        public string NoteSubject { get; set; }

        [Required]
        [Display(Name = "Last Edited")]
        public DateTime LastEdited { get; set; }

        [Required]
        [Display(Name = "Created")]
        public DateTime CreateDate { get; set; }

        [Required]
        public int Responses { get; set; }

        [StringLength(450)]
        public string AuthorID { get; set; }

        [Required]
        [StringLength(256)]
        public string AuthorName { get; set; }
    }
}
