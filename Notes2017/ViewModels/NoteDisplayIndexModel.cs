/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NoteDislayIndexModel.cs
**
**  Description:
**      Note Dislay Index View Model
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
using Notes2017.Models;

namespace Notes2017.ViewModels
{
    public class NoteDisplayIndexModel
    {
        public NoteFile noteFile { get; set; }
        public NoteAccess myAccess { get; set; }
        public bool isMarked { get; set; }
        public string rPath { get; set; }
        public int ExpandOrdinal { get; set; }
        public List<NoteContent> Notes { get; set; }
        public List<string> Cheaders { get; set; }
        public List<string> Lheaders { get; set; }
        public string panelEnd { get; set; }
        public TZone tZone { get; set; }
    }
}
