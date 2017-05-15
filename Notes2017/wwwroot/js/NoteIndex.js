/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: NoteIndex.js
**
**  Description:
**      Java Script for Notes 2017 Notes Index Page
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

function code(e) {
    e = e || window.event;
    return (e.keyCode || e.which);
}
window.onload = function () {
    document.onkeypress = function (e) {
        var key = code(e);
        // do something with key
        if (key == 76) {
            var x = document.getElementById("toIndex");
            if (x != null) {
                x.click();
            }
        }
        else if (key == 72 && e.shiftKey == true) {
            var x = document.getElementById("asHtml");
            if (x != null) {
                x.click();
            }
        }
        else if (key == 104 && e.shiftKey == false) {
            var x = document.getElementById("asHtmlAlt");
            if (x != null) {
                x.click();
            }
        }
        else if (key == 109 && e.shiftKey == false) {
            var x = document.getElementById("toMail");
            if (x != null) {
                x.click();
            }
        } else if (key == 78) {
            var x = document.getElementById("toWrite");
            if (x != null) {
                x.click();
            }
        }
        else if (key == 79) {
            var x = document.getElementById("toXMarked");
            if (x != null) {
                x.click();
            }
        }
        else if (key == 65) {
            var x = document.getElementById("toAccess");
            if (x != null) {
                x.click();
            }
        }
        else if (key == 83) {
            var x = document.getElementById("toSearch");
            if (x != null) {
                x.click();
            }
        }
        else if (key == 88) {
            var x = document.getElementById("toExport");
            if (x != null) {
                x.click();
            }
        }
        else if (key == 112) {
            $('#myModal').modal('show');
        }
    }
};