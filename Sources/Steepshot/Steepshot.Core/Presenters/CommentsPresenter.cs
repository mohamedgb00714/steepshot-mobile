﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class CommentsPresenter : BasePostPresenter
    {
        private const int ItemsLimit = 60;

        public async Task<List<string>> TryLoadNextComments(string postUrl)
        {
            return await RunAsSingleTask(LoadNextComments, postUrl);
        }

        private async Task<List<string>> LoadNextComments(CancellationToken ct, string postUrl)
        {
            var request = new NamedInfoRequest(postUrl)
            {
                Login = User.Login
            };

            var response = await Api.GetComments(request, ct);
            return OnLoadNextPostsResponce(response, ItemsLimit);
        }

        public async Task<OperationResult<CreateCommentResponse>> TryCreateComment(string comment, string url)
        {
            return await TryRunTask<string, string, CreateCommentResponse>(CreateComment, OnDisposeCts.Token, comment, url);
        }

        private async Task<OperationResult<CreateCommentResponse>> CreateComment(CancellationToken ct, string comment, string url)
        {
            var reqv = new CreateCommentRequest(User.UserInfo, url, comment, AppSettings.AppInfo);
            return await Api.CreateComment(reqv, ct);
        }
    }
}
