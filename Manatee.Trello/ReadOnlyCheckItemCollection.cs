﻿using System.Linq;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;

namespace Manatee.Trello
{
	/// <summary>
	/// A read-only collection of checklist items.
	/// </summary>
	public class ReadOnlyCheckItemCollection : ReadOnlyCollection<ICheckItem>, IReadOnlyCheckItemCollection
	{
		private readonly CheckListContext _context;

		/// <summary>
		/// Retrieves a check list item which matches the supplied key.
		/// </summary>
		/// <param name="key">The key to match.</param>
		/// <returns>The matching check list item, or null if none found.</returns>
		/// <remarks>
		/// Matches on <see cref="ICheckItem.Id"/> and <see cref="ICheckItem.Name"/>.  Comparison is case-sensitive.
		/// </remarks>
		public ICheckItem this[string key] => GetByKey(key);

		internal ReadOnlyCheckItemCollection(CheckListContext context, TrelloAuthorization auth)
			: base(() => context.Data.Id, auth)
		{
			_context = context;
		}

		/// <summary>
		/// Implement to provide data to the collection.
		/// </summary>
		protected sealed override void Update()
		{
			_context.Synchronize();
			if (_context.Data.CheckItems == null) return;
			foreach (var jsonCheckItem in _context.Data.CheckItems)
			{
				var checkItem = Items.SingleOrDefault(ci => ci.Id == jsonCheckItem.Id);
				if (checkItem == null)
					Items.Add(new CheckItem(jsonCheckItem, _context.Data.Id));
				else
					((CheckItem)checkItem).Json = jsonCheckItem;
			}
			foreach (var checkItem in Items.ToList())
			{
				if (_context.Data.CheckItems.All(jci => jci.Id != checkItem.Id))
					Items.Remove(checkItem);
			}
		}

		private ICheckItem GetByKey(string key)
		{
			return this.FirstOrDefault(ci => key.In(ci.Id, ci.Name));
		}
	}
}