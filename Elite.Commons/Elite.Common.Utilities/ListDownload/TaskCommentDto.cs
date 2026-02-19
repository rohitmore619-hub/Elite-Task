using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.ListDownload
{
    public partial class TaskCommentDto
    {
        public string Comment { get; set; }

        // ✅ ADD THESE
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
