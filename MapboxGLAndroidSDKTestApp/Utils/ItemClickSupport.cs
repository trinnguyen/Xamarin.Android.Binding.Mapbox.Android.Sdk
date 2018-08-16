using Android.Support.V7.Widget;
using Android.Views;

namespace MapboxGLAndroidSDKTestApp.Utils
{
    public class ItemClickSupport : Java.Lang.Object
    {
        public RecyclerView RecyclerView;
        public IOnItemClickListener OnItemClickListener;
        public IOnItemLongClickListener OnItemLongClickListener;
        public View.IOnClickListener OnClickListener;
        public View.IOnLongClickListener OnLongClickListener;
        RecyclerView.IOnChildAttachStateChangeListener attachListener;

        public ItemClickSupport()
        {
            OnClickListener = new MyViewOnClickListener(this);
            OnLongClickListener = new MyViewOnLongClickListener(this);
            attachListener = new MyRecyclerViewOnChildAttachStateChangeListener(this);
        }

        public ItemClickSupport(RecyclerView recyclerView) : this()
        {
            this.RecyclerView = recyclerView;
            this.RecyclerView.SetTag(Resource.Id.item_click_support, this);
            this.RecyclerView.AddOnChildAttachStateChangeListener(attachListener);
        }

        void Detach(RecyclerView view)
        {
            view.RemoveOnChildAttachStateChangeListener(attachListener);
            view.SetTag(Resource.Id.item_click_support, null);
        }

        public ItemClickSupport SetOnItemClickListener(IOnItemClickListener listener)
        {
            OnItemClickListener = listener;
            return this;
        }

        public ItemClickSupport SetOnItemLongClickListener(IOnItemLongClickListener listener)
        {
            OnItemLongClickListener = listener;
            return this;
        }

        public static ItemClickSupport AddTo(RecyclerView view)
        {
            var support = view.GetTag(Resource.Id.item_click_support) as ItemClickSupport;
            if (support == null)
                support = new ItemClickSupport(view);
            return support;
        }

        public static ItemClickSupport RemoveFrom(RecyclerView view)
        {
            var support = view.GetTag(Resource.Id.item_click_support) as ItemClickSupport;
            if (support != null)
                support.Detach(view);
            return support;
        }
    }

    public class MyRecyclerViewOnChildAttachStateChangeListener
        : Java.Lang.Object, RecyclerView.IOnChildAttachStateChangeListener
    {
        ItemClickSupport parent;

        public MyRecyclerViewOnChildAttachStateChangeListener(ItemClickSupport parent)
        {
            this.parent = parent;
        }

        public void OnChildViewAttachedToWindow(View view)
        {
            if (parent.OnItemClickListener != null)
                view.SetOnClickListener(parent.OnClickListener);
            if (parent.OnItemLongClickListener != null)
                view.SetOnLongClickListener(parent.OnLongClickListener);
        }

        public void OnChildViewDetachedFromWindow(View view)
        {
        }
    }

    public class MyViewOnLongClickListener : Java.Lang.Object, View.IOnLongClickListener
    {
        ItemClickSupport parent;

        public MyViewOnLongClickListener(ItemClickSupport parent)
        {
            this.parent = parent;
        }

        public bool OnLongClick(View v)
        {
            if (parent.OnItemLongClickListener != null)
            {
                RecyclerView.ViewHolder holder = parent.RecyclerView.GetChildViewHolder(v);
                return parent.OnItemLongClickListener.OnItemLongClicked(parent.RecyclerView, holder.AdapterPosition, v);
            }

            return false;
        }
    }

    public class MyViewOnClickListener : Java.Lang.Object, View.IOnClickListener
    {
        ItemClickSupport parent;

        public MyViewOnClickListener(ItemClickSupport parent)
        {
            this.parent = parent;
        }

        public void OnClick(View v)
        {
            if (parent.OnItemClickListener != null)
            {
                RecyclerView.ViewHolder holder = parent.RecyclerView.GetChildViewHolder(v);
                parent.OnItemClickListener.OnItemClicked(parent.RecyclerView, holder.AdapterPosition, v);
            }
        }
    }

    public interface IOnItemClickListener
    {
        void OnItemClicked(RecyclerView recyclerView, int position, View view);
    }

    public interface IOnItemLongClickListener
    {
        bool OnItemLongClicked(RecyclerView recyclerView, int position, View view);
    }
}
