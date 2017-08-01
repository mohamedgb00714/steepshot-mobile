﻿using System.Collections.ObjectModel;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Adapter
{
	public class PostsGridAdapter : RecyclerView.Adapter
	{
		ObservableCollection<Post> _posts;
		private Context _context;

		public System.Action<int> Click;

        public PostsGridAdapter(Context context, ObservableCollection<Post> posts)
        {
            _context = context;
            _posts = posts;
        }

        public Post GetItem(int position)
        {
             return _posts[position];
        }

        public override int ItemCount
        {
            get
            {
                return _posts.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
			Picasso.With(_context).Load(_posts[position].Body).NoFade().Resize(_context.Resources.DisplayMetrics.WidthPixels / 3 - 2, _context.Resources.DisplayMetrics.WidthPixels / 3 - 2).CenterCrop().Into(((ImageViewHolder)holder).Photo);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
			ImageView view = new ImageView(_context);
			view.SetScaleType(ImageView.ScaleType.CenterInside);
			view.LayoutParameters = new ViewGroup.LayoutParams(_context.Resources.DisplayMetrics.WidthPixels / 3 - 1, _context.Resources.DisplayMetrics.WidthPixels / 3 - 1);

			ImageViewHolder vh = new ImageViewHolder(view,Click);
            return vh;
        }

		public class ImageViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Photo { get; private set; }
			System.Action<int> _click;

			public ImageViewHolder(Android.Views.View itemView,System.Action<int> click) : base(itemView)
            {
				_click = click;
				Photo = (ImageView)itemView;
                Photo.Clickable = true;
                Photo.Click += OnClick;
            }

            private void OnClick(object sender, System.EventArgs e)
            {
                _click.Invoke(AdapterPosition);
            }

		}
    }
}