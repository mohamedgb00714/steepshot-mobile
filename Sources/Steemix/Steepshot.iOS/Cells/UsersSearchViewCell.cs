﻿using System;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public partial class UsersSearchViewCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString("UsersSearchViewCell");
		public static readonly UINib Nib;
		private IScheduledWork _scheduledWorkAvatar;
		protected UsersSearchViewCell(IntPtr handle) : base(handle) { }
		private UserSearchResult _currentUser;

		static UsersSearchViewCell()
		{
			Nib = UINib.FromName("UsersSearchViewCell", NSBundle.MainBundle);
		}

		public override void LayoutSubviews()
		{
			avatar.Layer.CornerRadius = avatar.Frame.Size.Width / 2;
			this.SelectionStyle = UITableViewCellSelectionStyle.None;
		}

		public void UpdateCell(UserSearchResult user)
		{
			_currentUser = user;
			avatar.Image = UIImage.FromFile("ic_user_placeholder.png");

			_scheduledWorkAvatar?.Cancel();
			_scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentUser.ProfileImage, TimeSpan.FromDays(30))
																						 .Retry(2, 200)
																						 .FadeAnimation(false, false, 0)
																						 .DownSample(width: (int)avatar.Frame.Width)
																						 .Into(avatar);
			if (!string.IsNullOrEmpty(_currentUser.Name))
			{
				username.Text = _currentUser.Name;
				usernameHeight.Constant = 18;
			}
			else
				usernameHeight.Constant = 0;
			
			login.Text = _currentUser.Username;
		}
	}
}