/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: Word.cs
**
**  Description:
**      Word Data Model for CrazyWords
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
    /// Model for CrazyWords Game records
    /// </summary>
    public class Words
    {
        // A word user has made
        [Required]
        [StringLength(4)]
        [Display(Name = "Word")]
        [Column(Order = 0)]
        public string Word { get; set; }

        // Name of the user who made it
        [Required]
        [StringLength(256)]
        [Display(Name = "User Name")]
        [Column(Order = 1)]
        public string UserName { get; set; }

        // Which list number does it refer to
        [Required]
        [Column(Order = 2)]
        public int ListNum { get; set; }

        // When did the use enter the word
        [Required]
        [Display(Name = "Entered")]
        public DateTime Entered { get; set; }

    }
}
