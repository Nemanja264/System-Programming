using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treci_Projekat.Core.Entities
{
    public sealed class VideoSentimentSummary
    {
        public string VideoId { get; }
        public int TotalComments { get; private set; }
        public int PositiveCount { get; private set; }
        public double PositiveRatio => TotalComments == 0 ? 0 : (double)PositiveCount / TotalComments;
        public VideoSentimentSummary(string videoId) { VideoId = videoId; }

        private double _probSum;

        public void Add(YouTubeComment prediction)
        {
            TotalComments++;
            _probSum += prediction.Probability;

            if (prediction.isPositive) PositiveCount++;

        }
    }
}
