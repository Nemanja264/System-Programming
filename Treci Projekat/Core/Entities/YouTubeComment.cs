using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treci_Projekat.Core.Entities
{
    public sealed class YouTubeComment
    {
        public string VideoId { get; set; } = "";
        public string Text { get; set; } = "";
        public string Author { get; set; } = "";
        public string? CommentId { get; set; }

        public bool IsPositive { get; set; }
        public double Probability { get; set; }
    }
}
