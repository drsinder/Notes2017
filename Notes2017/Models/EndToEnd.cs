/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: EndtoEnd.cs
**
**  Description:
**      EndtoEnd Data Model
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
    public class EndToEnd
    {
        // A word set user has made
        [Required]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        [Column(Order = 1)]
        public string Word0 {get; set;}

        [Required]
        [StringLength(100)]
        [Column(Order = 2)]
        public string Word1 { get; set; }

        [Required]
        [StringLength(100)]
        [Column(Order = 3)]
        public string Word2 { get; set; }

        [Required]
        [StringLength(100)]
        [Column(Order = 4)]
        public string Word3 { get; set; }

        [Required]
        [StringLength(100)]
        [Column(Order = 5)]
        public string Word4 { get; set; }

        [Required]
        [StringLength(100)]
        [Column(Order = 6)]
        public string Word5 { get; set; }

        [Required]
        [StringLength(100)]
        [Column(Order = 7)]
        public string Word6 { get; set; }

        [Required]
        [StringLength(100)]
        [Column(Order = 8)]
        public string Word7 { get; set; }

        [Required]
        [StringLength(100)]
        [Column(Order = 9)]
        public string Word8 { get; set; }

        [Required]
        [StringLength(100)]
        [Column(Order = 10)]
        public string Word9 { get; set; }

        [Required]
        [StringLength(100)]
        [Column(Order = 11)]
        public string Word10 { get; set; }

        // Name of the user who made it
        [Required]
        [StringLength(256)]
        [Display(Name = "User Name")]
        [Column(Order = 12)]
        public string UserName { get; set; }

        // Which list number does it refer to
        [Required]
        [Column(Order = 13)]
        [Display(Name = "List Number")]
        public int ListNum { get; set; }

        // When did the use enter the words
        [Required]
        [Column(Order = 14)]
        [Display(Name = "Entered")]
        public DateTime Entered { get; set; }

        [Column(Order = 15)]
        public bool IsMaster { get; set; }

    }
}
