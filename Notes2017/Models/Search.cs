/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: Search.cs
**
**  Description:
**      Search Data Model
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
    public enum SearchOption {Author, Title, Content, Tag, TimeAfter, TimeBefore  }

    /// <summary>
    /// Model for searching a notefile
    /// </summary>
    public class Search
    {
        // User doing the search
        [StringLength(450)]
        public string UserID { get; set; }

        // search specs Option
        [Display(Name = "Search By")]
        public SearchOption Option { get; set; }

        // Text to search for
        [Display(Name ="Search Text")]
        public string Text { get; set; }

        // DateTime to compare to
        [Display(Name = "Search Date/Time")]
        public DateTime Time { get; set; }

        // current/next info -- where we are in the search
        [Column(Order = 0)]
        public int NoteFileID { get; set; }
        [Column(Order = 1)]
        public int BaseOrdinal { get; set; }
        [Column(Order = 2)]
        public int ResponseOrdinal { get; set; }
        [Column(Order = 3)]
        public long NoteID { get; set; }

        [ForeignKey("NoteFileID")]
        public NoteFile NoteFile { get; set; }

        // Makes a clone of the object.  Had to do this to avoid side effects.
        public Search Clone(Search s)
        {
            Search cloned = new Search();
            cloned.BaseOrdinal = s.BaseOrdinal;
            cloned.NoteFileID = s.NoteFileID;
            cloned.NoteID = s.NoteID;
            cloned.Option = s.Option;
            cloned.ResponseOrdinal = s.ResponseOrdinal;
            cloned.Text = s.Text;
            cloned.Time = s.Time;
            cloned.UserID = s.UserID;
            return cloned;
        }
    }
}
