﻿using System;
using System.Threading.Tasks;
using CoreGraphics;
using FFImageLoading;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class LoginViewController : BaseViewController
    {
        protected LoginViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public LoginViewController() { }

        public string AvatarLink { get; set; }
        public string Username { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            if ((float)UIScreen.MainScreen.Bounds.Height < 500)
            {
                topMargin.Constant = 5;
                bottomMargin.Constant = 5;
                photoMargin.Constant = 5;
                photoBottomMargin.Constant = 5;
            }
            if ((float)UIScreen.MainScreen.Bounds.Height == 568)
            {
                topMargin.Constant = 15;
                bottomMargin.Constant = 15;
            }
            loginButton.TouchDown += (object sender, EventArgs e) => Login();
            avatar.Layer.CornerRadius = avatar.Frame.Height / 2;
            eyeButton.TouchDown += (sender, e) =>
            {
                password.SecureTextEntry = !password.SecureTextEntry;
            };
            ImageService.Instance.LoadUrl(AvatarLink, TimeSpan.FromDays(30))
                                             .Retry(2, 200)
                                             .FadeAnimation(false, false, 0)
                                             .DownSample(width: (int)avatar.Frame.Width)
                                             .Into(avatar);

            loginTitle.Text = $"Hello, {Username}";
            loginTitle.Font = Constants.Bold225;
            postingLabel.Font = Constants.Bold175;
            password.Font = Constants.Bold135;
            loginButton.Font = Constants.Heavy115;
            postingKeyButton.Font = Constants.Bold15;
#if DEBUG
            password.Text = "***REMOVED***";
#endif
            password.ShouldReturn += (textField) =>
            {
                password.ResignFirstResponder();
                return true;
            };
            password.RightView = new UIView(new CGRect(0, 0, eyeButton.Frame.Width + 10, 0));
            password.RightViewMode = UITextFieldViewMode.Always;
            var tw = new UILabel(new CGRect(0, 0, 120, NavigationController.NavigationBar.Frame.Height));
            tw.TextColor = UIColor.White;
            tw.Text = "PROFILE"; // to constants
            tw.BackgroundColor = UIColor.Clear;
            tw.TextAlignment = UITextAlignment.Center;
            tw.Font = Constants.Heavy165;
            NavigationItem.TitleView = tw;

            qrButton.Font = Constants.Bold135;
            //qrButton.ImageEdgeInsets = new UIEdgeInsets(5, 5, -5, -5);
            qrButton.TouchDown += async (sender, e) =>
            {
                var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                var result = await scanner.Scan();

                if (result != null)
                    password.Text = result.Text;
            };
            tosButton.TouchDown += (sender, e) =>
            {
                UIApplication.SharedApplication.OpenUrl(new Uri(Tos));
            };
            ppButton.TouchDown += (sender, e) =>
            {
                UIApplication.SharedApplication.OpenUrl(new Uri(Pp));
            };
        }

        private async Task Login()
        {
            if (!tosSwitch.On)
            {
                ShowAlert("Make sure you accept the terms of service and privacy policy");
                return;
            }

            activityIndicator.StartAnimating();
            loginButton.Enabled = false;
            tosSwitch.Enabled = false;
            var titleColor = loginButton.TitleColor(UIControlState.Disabled);
            loginButton.SetTitleColor(UIColor.Clear, UIControlState.Disabled);
            try
            {
                var request = new LoginWithPostingKeyRequest(Username, password.Text);
                var response = await Api.LoginWithPostingKey(request);

                if (response.Success)
                {

                    User.AddAndSwitchUser(response.Result.SessionId, Username, password.Text, Chain);
                    IsHomeFeedLoaded = false;
                    var myViewController = new MainTabBarController();

                    NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
                    NavigationController.PopViewController(true);
                }
                else
                {
                    ShowAlert(response.Errors[0]);
                }
            }
            catch (ArgumentNullException)
            {
                ShowAlert("PostingKey cannot be empty");
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
            }
            finally
            {
                loginButton.Enabled = true;
                tosSwitch.Enabled = true;
                loginButton.SetTitleColor(titleColor, UIControlState.Disabled);
                activityIndicator.StopAnimating();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, true);
            ViewDidDisappear(animated);
        }
    }
}
