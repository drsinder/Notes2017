/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: Forward.cs
**
**  Description:
**      Forward View Model
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

namespace Notes2017.ViewModels
{
    public class ForwardView
    {
        public long NoteID { get; set; }
        public int FileID { get; set; }
        public int NoteOrdinal { get; set; }

        [Display(Name = "Subject")]
        public string NoteSubject { get; set; }

        [Display(Name = "Forward whole note string")]
        public bool wholestring { get; set; }

        public bool hasstring { get; set; }

        public bool IsAdmin { get; set; }

        public bool toAllUsers { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Forward to Email Address")]
        public string ToEmail { get; set; }

    }
}
