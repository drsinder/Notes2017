/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NoteFile.cs
**
**  Description:
**      Note File Data Model
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
using System.Collections.Generic;

namespace Notes2017.Models
{
    /// <summary>
    /// Represents a file in the DB
    /// </summary>
    public class NoteFile
    {
        // Identity of the file
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NoteFileID { get; set; }

        // file name of the file
        [Required]
        [StringLength(20)]
        [Display(Name = "NoteFile Name")]
        public string NoteFileName { get; set; }

        // title of the file
        [Required]
        [StringLength(200)]
        [Display(Name = "NoteFile Title")]
        public string NoteFileTitle { get; set; }

        // when anything in the file was last created or edited
        [Required]
        [Display(Name = "Last Edited")]
        public DateTime LastEdited { get; set; }

        public List<BaseNoteHeader> BaseNoteHeaders { get; set; }
    }
}
