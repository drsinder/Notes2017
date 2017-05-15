/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: TextViewModel.cs
**
**  Description:
**      Text View Model
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
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Notes2017.Models;

namespace Notes2017.ViewModels
{
    /// <summary>
    /// Model used to input data for a note.
    /// </summary>
    public class TextViewModel
    {
        [Required(ErrorMessage = "A Note body is required.")]
        [StringLength(100000)]
        [Display(Name = "MyNote")]
        public string MyNote { get; set; }

        [Required(ErrorMessage = "A Subject is required.")]
        [StringLength(200)]
        [Display(Name = "MySubject")]
        public string MySubject { get; set; }

        [Required]
        public int NoteFileID { get; set; }

        [Required]
        public long BaseNoteHeaderID { get; set; }

        public int NoteID { get; set; }

        [StringLength(200)]
        [Display(Name = "Tag Line")]
        public string TagLine { get; set; }

        public NoteFile noteFile { get; set; }
    }

    /// <summary>
    /// Model used for Exporting files
    /// </summary>
    public class NFViewModel
    {
        [Required]
        [StringLength(20)]
        public string FileName { get; set; }

        public int FileNum { get; set; }

        public List<SelectListItem> AFiles { get; set; }

        public List<SelectListItem> ATitles { get; set; }

        [Display(Name = "As Html - otherwise plain text.  Plain text can be reimported.")]
        public bool isHtml { get; set; }

        [Display(Name = "Collapsible/Expandable")]
        public bool isCollapsible { get; set; }

        [Display(Name = "Direct Output to Browser")]
        public bool directOutput { get; set; }

        public int NoteOrdinal { get; set; }

        public TZone tzone { get; set; }

        public string Message { get; set; }
    }

    /// <summary>
    /// For getting timezone of a given time in short form (PDT/PST, CDT/CST...)
    /// </summary>
    public static class TZ
    {
        public static string GetTimeZone(DateTime tim)
        {
            TimeZone tz = TimeZone.CurrentTimeZone;
            string fullname = tim.IsDaylightSavingTime() ? tz.DaylightName : tz.StandardName;
            return fullname;
            //string[] words = fullname.Split(new char[]{ ' ' });
            //string shortname = string.Empty;
            //for ( int i = 0; i < words.Length; i++)
            //{
            //    shortname += words[i].Substring(0, 1);
            //}

            //return shortname; 
        }
    }


}
