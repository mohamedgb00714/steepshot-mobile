﻿using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Utils.Animations;
using Steepshot.Utils.Animations.Interfaces;
using Object = Java.Lang.Object;

namespace Steepshot.Adapter
{
    public sealed class PostPagerAdapter<T> : Android.Support.V4.View.PagerAdapter, ViewPager.IPageTransformer
        where T : BasePostPresenter
    {
        private const int CachedPagesCount = 5;
        private readonly float _pageOffset;
        private readonly T _presenter;
        private readonly Context _context;
        private readonly List<PostViewHolder> _viewHolders;
        private int _itemsCount;
        private View _loadingView;
        public Action<ActionType, Post> PostAction;
        public Action<string> TagAction;
        public Action CloseAction;
        public int CurrentItem { get; set; }

        public PostPagerAdapter(Context context, T presenter)
        {
            _context = context;
            _presenter = presenter;
            _viewHolders = new List<PostViewHolder>(_presenter.Count);
            _viewHolders.AddRange(Enumerable.Repeat<PostViewHolder>(null, CachedPagesCount));
            _itemsCount = 0;
            _pageOffset = BitmapUtils.DpToPixel(20, _context.Resources);
        }

        public void BeginInflating(ViewGroup container)
        {
            for (int i = 0; i < _viewHolders.Count; i++)
            {
                InfalteViewholder(container, i);
            }
        }

        private void InfalteViewholder(ViewGroup container, int number)
        {
            if (_viewHolders[number] == null)
            {
                var itemView = LayoutInflater.From(_context)
                    .Inflate(Resource.Layout.lyt_post_view_item, container, false);
                var vh = new PostViewHolder(itemView, PostAction, TagAction, CloseAction, _context.Resources.DisplayMetrics.WidthPixels);
                _viewHolders[number] = vh;
                container.AddView(vh.ItemView);
            }
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            if (position == _presenter.Count)
            {
                if (_loadingView == null)
                {
                    _loadingView = LayoutInflater.From(_context)
                        .Inflate(Resource.Layout.lyt_postcard_loading, container, false);
                }

                container.AddView(_loadingView);
                return _loadingView;
            }

            var reusePosition = position % CachedPagesCount;
            InfalteViewholder(container, reusePosition);
            var vh = _viewHolders[reusePosition];
            vh.UpdateData(_presenter[position], _context);
            return vh.ItemView;
        }

        public override void NotifyDataSetChanged()
        {
            if (_presenter.Count > 0)
            {
                var reusePosition = CurrentItem % CachedPagesCount;
                if (_presenter[CurrentItem] != null)
                    _viewHolders[reusePosition]?.UpdateData(_presenter[CurrentItem], _context);
                _itemsCount = _presenter.IsLastReaded ? _presenter.Count : _presenter.Count + 1;
                base.NotifyDataSetChanged();
                ResetVisibleItems();
            }
        }

        private void ResetVisibleItems()
        {
            var pos = -_pageOffset;
            for (int i = CurrentItem - 1; i <= CurrentItem + 1; i++)
            {
                if (i < 0 || i == _presenter.Count)
                {
                    pos += _pageOffset;
                    continue;
                }
                TransformPage(_viewHolders[i % CachedPagesCount]?.ItemView, pos);
                pos += _pageOffset;
            }
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return @object == view;
        }

        public override int GetItemPosition(Object @object)
        {
            if (@object != _loadingView)
                return PositionUnchanged;
            return PositionNone;
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            if (@object == _loadingView)
            {
                container.RemoveView(_loadingView);
            }
        }

        public override int Count => _itemsCount;

        public void TransformPage(View page, float position)
        {
            if (page == _loadingView || page == null) return;
            var pageWidth = page.Width;
            var positionOffset = _pageOffset / pageWidth;

            var postHeader = page.FindViewById<RelativeLayout>(Resource.Id.title);
            var postFooter = page.FindViewById<RelativeLayout>(Resource.Id.subtitle);
            if (position == 1 + positionOffset * 1.5)
            {
                postHeader.TranslationX = _pageOffset;
                postFooter.TranslationX = _pageOffset;
            }
            else
            {
                var translation = (int)(position * _pageOffset);
                postHeader.TranslationX = translation;
                postFooter.TranslationX = translation;
            }
        }

        public Storyboard Storyboard
        {
            get
            {
                var reusePosition = CurrentItem % CachedPagesCount;
                var itemView = _viewHolders[reusePosition]?.ItemView;
                if (itemView != null)
                {
                    var photoPager = itemView.FindViewById<ViewPager>(Resource.Id.post_photos_pager);
                    var headerLeft = itemView.FindViewById<LinearLayout>(Resource.Id.header_left);
                    var headerRight = itemView.FindViewById<LinearLayout>(Resource.Id.header_right);
                    var subtitle = itemView.FindViewById<RelativeLayout>(Resource.Id.subtitle);
                    var footer = itemView.FindViewById<LinearLayout>(Resource.Id.comment_footer);

                    var entities = new List<IAnimator>();
                    entities.AddRange(new[]
                    {
                        photoPager.Translation(-photoPager.Width,itemView.Height,0,0,300,Easing.CubicOut),
                        photoPager.Scaling(0,0,1,1,300,Easing.CubicOut),
                        headerLeft.Translation(-headerLeft.Width,0,0,0,300,Easing.CubicOut),
                        headerRight.Translation(headerRight.Width,0,0,0,300,Easing.CubicOut),
                        subtitle.Translation(0,itemView.Height,0,0,300,Easing.CubicOut).WithDelay(50),
                        footer.Translation(0,itemView.Height,0,0,300,Easing.CubicOut).WithDelay(100)
                    });

                    if (CurrentItem - 1 >= 0)
                    {
                        reusePosition = (CurrentItem - 1) % CachedPagesCount;
                        itemView = _viewHolders[reusePosition]?.ItemView;
                        if (itemView != null)
                        {
                            photoPager = itemView.FindViewById<ViewPager>(Resource.Id.post_photos_pager);
                            entities.Add(photoPager.Translation(-_pageOffset, 0, 0, 0, 300, Easing.CubicOut));
                        }
                    }

                    if (CurrentItem + 1 <= _presenter.Count - 1)
                    {
                        reusePosition = (CurrentItem + 1) % CachedPagesCount;
                        itemView = _viewHolders[reusePosition]?.ItemView;
                        if (itemView != null)
                        {
                            photoPager = itemView.FindViewById<ViewPager>(Resource.Id.post_photos_pager);
                            entities.Add(photoPager.Translation(_pageOffset, 0, 0, 0, 300, Easing.CubicOut));
                        }
                    }
                    return Storyboard.From(entities);
                }
                return null;
            }
        }
    }

    public sealed class PostViewHolder : FeedViewHolder
    {
        private readonly Action _closeAction;
        public PostViewHolder(View itemView, Action<ActionType, Post> postAction, Action<string> tagAction, Action closeAction, int height) : base(itemView, postAction, tagAction, height)
        {
            PhotoPagerType = PostPagerType.PostScreen;
            _closeAction = closeAction;
            var closeButton = itemView.FindViewById<ImageButton>(Resource.Id.close);
            closeButton.Click += CloseButtonOnClick;

            var postHeader = itemView.FindViewById<RelativeLayout>(Resource.Id.title);
            var postFooter = itemView.FindViewById<RelativeLayout>(Resource.Id.subtitle);
            postHeader.SetLayerType(LayerType.Hardware, null);
            postFooter.SetLayerType(LayerType.Hardware, null);

            NsfwMask.ViewTreeObserver.GlobalLayout += ViewTreeObserverOnGlobalLayout;
        }

        protected override void SetNsfwMaskLayout()
        {
            base.SetNsfwMaskLayout();
            ((RelativeLayout.LayoutParams)NsfwMask.LayoutParameters).AddRule(LayoutRules.AlignParentTop);
        }

        private void ViewTreeObserverOnGlobalLayout(object sender, EventArgs eventArgs)
        {
            if (NsfwMask.Height < BitmapUtils.DpToPixel(200, Context.Resources))
                NsfwMaskSubMessage.Visibility = ViewStates.Gone;
        }

        protected override void OnTitleOnClick(object sender, EventArgs e)
        {
            base.OnTitleOnClick(sender, e);
            UpdateData(Post, Context);
        }

        private void CloseButtonOnClick(object sender, EventArgs eventArgs)
        {
            _closeAction?.Invoke();
        }
    }
}