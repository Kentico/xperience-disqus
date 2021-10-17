using CMS.Core;
using Disqus.OnlineMarketing;
using Disqus.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Disqus.Models
{
    public class DisqusPost
    {
        private DisqusThread mThread;
        private DisqusForum mForum;

        [HiddenInput]
        public string Id { get; set; }

        [HiddenInput]
        public string Thread { get; set; }

        [HiddenInput]
        public string Message { get; set; }

        [HiddenInput]
        public string Forum { get; set; }

        public string Raw_Message { get; set; }

        public int NumReports { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public int Points { get; set; }

        public DateTime EditableUntil { get; set; }

        public DateTime CreatedAt { get; set; }

        public DisqusUser Author { get; set; }

        public bool IsSpam { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsApproved { get; set; }

        public bool IsHighlighted { get; set; }

        public bool IsFlagged { get; set; }

        public bool IsAtFlagLimit { get; set; }

        public bool IsEdited { get; set; }

        public bool CanVote { get; set; }

        public string Parent { get; set; } = string.Empty;

        public bool IsNewUserNeedsApproval { get; set; }

        public int NestingLevel { get; set; } = 0;

        public DisqusThread ThreadObject
        {
            get
            {
                if(mThread == null)
                {
                    var repository = Service.Resolve<DisqusRepository>();
                    mThread = repository.GetThread(Thread).Result;
                }

                return mThread;
            }

            set => mThread = value;
        }

        public DisqusForum ForumObject
        {
            get
            {
                if (mForum == null)
                {
                    var repository = Service.Resolve<DisqusRepository>();
                    mForum = repository.GetForum(Forum).Result;
                }

                return mForum;
            }

            set => mForum = value;
        }

        public string Permalink
        {
            get => $"{ThreadObject.Link}#comment-{Id}";
        }
    }
}
