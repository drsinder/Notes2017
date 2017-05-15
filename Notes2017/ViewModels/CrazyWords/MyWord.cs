/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: MyWord.cs
**
**  Description:
**      CrazyWords View Model
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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Notes2017.Models;

namespace Notes2017.ViewModels.CrazyWords
{
    /// <summary>
    /// Just the word we are working on with  constraints
    /// </summary>
    public class MyWord
    {
        [Required]
        [StringLength(4, MinimumLength = 4)]
        [Display(Name = "Word")]
        public string Word { get; set; }
    }

    /// <summary>
    /// Model for input View of words
    /// </summary>
    public class NewWordViewModel
    {
        public MyWord myWord { get; set; }

        public List<Words> myList { get; set; }

        public int wordList { get; set; }

        public string letters { get; set; }

        public char[] letterarray { get; set; }

        [Display(Name = "Most Recent First")]
        public bool MostRecentFirst { get; set; }
    }

}
