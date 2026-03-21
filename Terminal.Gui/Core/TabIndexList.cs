//
// TabIndexList.cs: Support for TabIndex
//
// Authors:
//   Serg V. Zhdanovskikh
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal.Gui.Core
{
	public class TabIndexList<T> where T : View
	{
		private class Node
		{
			public T Item { get; }
			public int CachedIndex { get; }

			public Node (T item)
			{
				Item = item;
				CachedIndex = item.TabIndex;
			}
		}

		private readonly List<Node> _nodes = new List<Node> ();

		public int Count => _nodes.Count;

		public T this [int index]
		{
			get {
				return _nodes [index].Item;
			}
		}

		public void Clear ()
		{
			_nodes.Clear ();
		}

		public void AddOrUpdate (T item)
		{
			if (item == null)
				throw new ArgumentNullException (nameof (item));

			int existingIndex = IndexOf (item);
			if (existingIndex != -1) {
				if (_nodes [existingIndex].CachedIndex == item.TabIndex) {
					return;
				}
				_nodes.RemoveAt (existingIndex);
			}

			int insertPosition = FindInsertionPosition (item.TabIndex);
			_nodes.Insert (insertPosition, new Node (item));
		}

		public bool Contains (T item)
		{
			int existingIndex = IndexOf (item);
			return (existingIndex != -1);
		}

		public void Remove (T item)
		{
			if (item == null)
				throw new ArgumentNullException (nameof (item));

			int existingIndex = IndexOf (item);
			if (existingIndex != -1) {
				_nodes.RemoveAt (existingIndex);
			}
		}

		/// <summary>
		/// If TabIndex repeats existing values ​​one or more times,
		/// we move to the end, after all existing items.
		/// </summary>
		private int FindInsertionPosition (int index)
		{
			int left = 0;
			int right = _nodes.Count;
			while (left < right) {
				int mid = left + (right - left) / 2;
				if (_nodes [mid].CachedIndex <= index) {
					left = mid + 1;
				} else {
					right = mid;
				}
			}
			return left;
		}

		public int IndexOf (T item)
		{
			for (int i = 0; i < _nodes.Count; i++) {
				if (ReferenceEquals (_nodes [i].Item, item)) {
					return i;
				}
			}
			return -1;
		}

		public T GetNext (T item)
		{
			/*int currentIndex = FindObjectIndex (item);
			if (currentIndex == -1 || currentIndex >= _nodes.Count - 1) {
				return default (T);
			}

			for (int i = currentIndex + 1; i < _nodes.Count; i++) {
				var view = _nodes [i].Item;
				if (View.CanFocused (view))
					return view;
			}*/

			return null;
		}

		public T GetPrevious (T item)
		{
			/*int currentIndex = FindObjectIndex (item);
			if (currentIndex <= 0) {
				return default (T);
			}

			for (var i = currentIndex - 1; i >= 0; i--) {
				var view = _nodes [i].Item;
				if (View.CanFocused (view))
					return view;
			}*/

			return null;
		}

		public T GetFirst ()
		{
			for (int i = 0; i < _nodes.Count; i++) {
				var view = _nodes [i].Item;
				if (View.CanFocused (view))
					return view;
			}
			return default (T);
		}

		public T GetLast ()
		{
			for (int i = _nodes.Count - 1; i >= 0; i--) {
				var view = _nodes [i].Item;
				if (View.CanFocused (view))
					return view;
			}
			return default (T);
		}

		public List<T> GetList ()
		{
			return _nodes.Select (x => x.Item).ToList ();
		}

		public List<T> GetReversed ()
		{
			var items = _nodes.Select (x => x.Item);
			return items.Reverse ().ToList ();
		}
	}
}
