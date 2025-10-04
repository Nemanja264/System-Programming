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
        public (string Text, double P)? TopPositive {  get; private set; }
        public (string Text, double P)? TopNegative { get; private set; }


        public void Add(YouTubeComment prediction)
        {
            TotalComments++;
            _probSum += prediction.Probability;

            if (prediction.IsPositive)
            {
                PositiveCount++;
                if ((TopPositive is null || prediction.Probability > TopPositive.Value.P))
                    TopPositive = (prediction.Text, prediction.Probability);
            }
            else
            {
                var negScore = 1.0 - prediction.Probability;
                if (TopNegative is null || negScore > TopNegative.Value.P)
                    TopNegative = (prediction.Text, negScore);
            }
        }
    }
}
