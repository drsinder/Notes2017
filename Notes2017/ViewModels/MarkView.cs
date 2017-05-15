/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: MarkView.cs
**
**  Description:
**      Mark View Model
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

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Notes2017.ViewModels
{
    public class MarkView
    {
        //[Key]
        public IEnumerable<SelectListItem> option { get; set; }

        public string SelectedValue { get; set; }

        public long NoteID { get; set; }
    }
}
