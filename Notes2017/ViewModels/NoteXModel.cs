/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NoteXModel.cs
**
**  Description:
**      Note X View Model
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

using Notes2017.Models;

namespace Notes2017.ViewModels
{
    public class NoteXModel
    {
        public NoteContent note { get; set; }
        public BaseNoteHeader bnh { get; set; }
        public NoteAccess myAccess { get; set; }
        public bool CanDelete { get; set; }
        public bool IsSeq { get; set; }
        public string DeleteMessage { get; set; }
        public TZone tZone { get; set; }
    }
}
