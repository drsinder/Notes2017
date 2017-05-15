/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NoteContent.cs
**
**  Description:
**      Note Content Data Model
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
    /// This is the guts of the App.  A NoteContent object contains all
    /// the data for a Note of any kind, Base or Response.
    /// </summary>
    public class NoteContent
    {
        // Uniquely identifies the note
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long NoteID { get; set; }

        // The fileid the note belongs to
        [Required]
        public int NoteFileID { get; set; }

        [ForeignKey("NoteFileID")]
        public NoteFile NoteFile { get; set; }

        // the ordinal on a Base note and all its responses
        [Required]
        [Display(Name = "Note #")]
        public int NoteOrdinal { get; set; }

        // The ordinal of the response where 0 is a Base Note
        [Required]
        [Display(Name = "Response #")]
        public int ResponseOrdinal { get; set; }

        // Subject/Title of a note
        [Required]
        [StringLength(200)]
        [Display(Name = "Subject")]
        public string NoteSubject { get; set; }

        // When the note was created or last edited
        [Required]
        [Display(Name = "Last Edited")]
        public DateTime LastEdited { get; set; }

        // The Body or content of the note
        [Required]
        [StringLength(100000)]
        [Display(Name = "Note")]
        public string NoteBody { get; set; }

        // The Tags for a note
        [StringLength(200)]
        [Display(Name = "Tag Line")]
        public string DirectorMesssage { get; set; }

        // UserID of the writer of the note
        [StringLength(450)]
        public string AuthorID { get; set; }

        // Name of the author of the note
        [Required]
        [StringLength(256)]
        public string AuthorName { get; set; }
    }
}
