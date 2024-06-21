using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient.Extensions
{
	public class SortableBindingList<T> : BindingList<T>
	{
		private bool isSorted;
		private ListSortDirection sortDirection;
		private PropertyDescriptor sortProperty;
		private Func<T, bool> statusFunc;

		public SortableBindingList() : base() { }

		public SortableBindingList(IList<T> list) : base(list) { }

		public void SetStatusFunc(Func<T, bool> statusFunc)
		{
			this.statusFunc = statusFunc;
		}

		protected override bool SupportsSortingCore => true;

		protected override bool IsSortedCore => isSorted;

		protected override ListSortDirection SortDirectionCore => sortDirection;

		protected override PropertyDescriptor SortPropertyCore => sortProperty;

		protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
		{
			List<T> items = this.Items as List<T>;

			if (items != null)
			{
				items.Sort((x, y) => CompareWithStatus(x, y, prop, direction));
				isSorted = true;
				sortDirection = direction;
				sortProperty = prop;
				OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			}
		}

		private int CompareWithStatus(T x, T y, PropertyDescriptor prop, ListSortDirection direction)
		{
			bool xStatus = statusFunc(x);
			bool yStatus = statusFunc(y);

			if (xStatus == yStatus)
			{
				return Compare(x, y, prop, direction);
			}
			return xStatus ? -1 : 1;
		}

		private int Compare(T x, T y, PropertyDescriptor prop, ListSortDirection direction)
		{
			int result = Comparer<object>.Default.Compare(prop.GetValue(x), prop.GetValue(y));
			if (direction == ListSortDirection.Descending)
			{
				result = -result;
			}
			return result;
		}

		protected override void RemoveSortCore()
		{
			isSorted = false;
			sortProperty = null;
		}
	}
}
