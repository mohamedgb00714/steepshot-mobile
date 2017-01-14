﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Steemix.Droid.Activity;

namespace Steemix.Droid.Views
{
	[Activity(Label = "SteepShot", MainLauncher = true, Icon = "@mipmap/ic_launcher", ScreenOrientation = ScreenOrientation.Portrait)]
	public class SplashActivity : BaseActivity<SplashViewModel>
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			if (ViewModel.IsGuest)
			{
				StartActivity(typeof(GuestActivity));
			}
			else
			{
				StartActivity(typeof(RootActivity));
			}
		}
	}
}