using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treci_Projekat.Core.Entities;

namespace Treci_Projekat.Core.Interfaces
{
    internal interface IYouTubeCommentService : IDisposable
    {
        IObservable<YouTubeComment> GetTopLevelComments(string videoId);
    }
}
