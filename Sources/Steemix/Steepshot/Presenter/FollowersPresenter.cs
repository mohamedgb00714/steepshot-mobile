﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class FollowersPresenter : BasePresenter
	{
		public FollowersPresenter(FollowersView view):base(view)
		{
		}

		public readonly ObservableCollection<UserFriendViewMode> Collection = new ObservableCollection<UserFriendViewMode>();
		private bool _hasItems = true;
		private string _offsetUrl = string.Empty;
		public void ViewLoad(FollowType friendsType, string username)
		{
			if (Collection.Count == 0)
				Task.Run(() => GetItems(20, friendsType, username));
		}

		public async Task GetItems(int limit, FollowType followType, string username)
		{
			if (!_hasItems)
				return;
			var request = new UserFriendsRequest(username,
                followType == FollowType.Follow ? FriendsType.Followers : FriendsType.Following)
			{
                SessionId = UserPrincipal.Instance.Cookie,
                Offset = _offsetUrl,
				Limit = limit
			};

			var responce = await Api.GetUserFriends(request);
			//TODO:KOA -- Errors not processed
			if (responce.Success)
			{
				var lastItem = responce.Result.Results.Last();
				if (responce.Result.Results.Count == 20)
					responce.Result.Results.Remove(lastItem);
				
				else
					_hasItems = false;
				
				_offsetUrl = lastItem.Author;
				foreach (var item in responce.Result.Results)
					Collection.Add(new UserFriendViewMode(item, followType == FollowType.Follow));
				
			}
		}

		public async Task<OperationResult<FollowResponse>> Follow(UserFriendViewMode item)
		{
			var request = new FollowRequest(UserPrincipal.Instance.CurrentUser.SessionId, item.IsFollow ? FollowType.Follow : FollowType.UnFollow, item.Author);
			return await Api.Follow(request);
		}
	}
}