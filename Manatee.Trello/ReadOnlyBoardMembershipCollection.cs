﻿using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello
{
	/// <summary>
	/// A read-only collection of board memberships.
	/// </summary>
	public class ReadOnlyBoardMembershipCollection : ReadOnlyCollection<IBoardMembership>, IReadOnlyBoardMembershipCollection
	{
		private Dictionary<string, object> _additionalParameters;

		internal ReadOnlyBoardMembershipCollection(Func<string> getOwnerId, TrelloAuthorization auth)
			: base(getOwnerId, auth)
		{
			_additionalParameters = new Dictionary<string, object> {{"fields", "all"}};
		}
		internal ReadOnlyBoardMembershipCollection(ReadOnlyBoardMembershipCollection source, TrelloAuthorization auth)
			: base(() => source.OwnerId, auth)
		{
			if (source._additionalParameters != null)
				_additionalParameters = new Dictionary<string, object>(source._additionalParameters);
		}

		/// <summary>
		/// Retrieves a membership which matches the supplied key.
		/// </summary>
		/// <param name="key">The key to match.</param>
		/// <returns>The matching membership, or null if none found.</returns>
		/// <remarks>
		/// Matches on <see cref="IBoardMembership.Id"/>, BoardMembership.<see cref="IMember.Id"/>, BoardMembership.<see cref="IMember.FullName"/>, and BoardMembership.<see cref="IMember.UserName"/>. Comparison is case-sensitive.
		/// </remarks>
		public IBoardMembership this[string key] => GetByKey(key);

		/// <summary>
		/// Implement to provide data to the collection.
		/// </summary>
		protected sealed override void Update()
		{
			var endpoint = EndpointFactory.Build(EntityRequestType.Board_Read_Memberships, new Dictionary<string, object> {{"_id", OwnerId}});
			var newData = JsonRepository.Execute<List<IJsonBoardMembership>>(Auth, endpoint, _additionalParameters);

			Items.Clear();
			Items.AddRange(newData.Select(jbm =>
				{
					var membership = TrelloConfiguration.Cache.Find<BoardMembership>(c => c.Id == jbm.Id) ?? new BoardMembership(jbm, OwnerId, Auth);
					membership.Json = jbm;
					return membership;
				}));
		}

		private IBoardMembership GetByKey(string key)
		{
			return this.FirstOrDefault(bm => key.In(bm.Id, bm.Member.Id, bm.Member.FullName, bm.Member.UserName));
		}

		/// <summary>
		/// Adds a filter to the collection.
		/// </summary>
		/// <param name="membership">The filter value.</param>
		public void Filter(MembershipFilter membership)
		{
			var memberships = membership.GetFlags().Cast<MembershipFilter>();
			Filter(memberships);
		}

		/// <summary>
		/// Adds a filter to the collection.
		/// </summary>
		/// <param name="memberships">The filter values.</param>
		public void Filter(IEnumerable<MembershipFilter> memberships)
		{
			if (_additionalParameters == null)
				_additionalParameters = new Dictionary<string, object> {{"filter", string.Empty}};
			var filter = ((string) _additionalParameters["filter"]);
			if (!filter.IsNullOrWhiteSpace())
				filter += ",";
			filter += memberships.Select(a => a.GetDescription()).Join(",");
			_additionalParameters["filter"] = filter;
		}
	}
}