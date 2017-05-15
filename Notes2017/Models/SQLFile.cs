/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: SQLFile.cs
**
**  Description:
**      SQL File Data Model
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
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes2017.Models
{
    public class SQLFile
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FileId { get; set; }

        [Required]
        [StringLength(300)]
        public string FileName { get; set; }

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; }

        [Required]
        [StringLength(300)]
        public string Contributor { get; set; }

        [Required]
        public long ContentID { get; set; }

        [StringLength(1000)]
        public string Comments { get; set; }

    }

    public class SQLFileContent
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ContentId { get; set; }

        [Required]
        public byte[] Content { get; set; }
    }

    /// <summary>
    /// a view class.  not in the db.  composed by app
    /// </summary>
    public class SQLFileComplete
    {
        public SQLFile File { get; set; }

        public SQLFileContent FileContent { get; set; }
    }

}
