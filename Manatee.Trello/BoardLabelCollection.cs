﻿using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello
{
	/// <summary>
	/// A collection of labels for boards.
	/// </summary>
	public class BoardLabelCollection : ReadOnlyCollection<ILabel>, IBoardLabelCollection
	{
		private Dictionary<string, object> _additionalParameters;

		internal BoardLabelCollection(Func<string> getOwnerId, TrelloAuthorization auth)
			: base(getOwnerId, auth) {}
		internal BoardLabelCollection(BoardLabelCollection source, TrelloAuthorization auth)
			: this(() => source.OwnerId, auth)
		{
			if (source._additionalParameters != null)
				_additionalParameters = new Dictionary<string, object>(source._additionalParameters);
		}

		/// <summary>
		/// Creates a new label on the board.
		/// </summary>
		/// <param name="name">The name of the label.</param>
		/// <param name="color">The color of the label to add.</param>
		/// <returns>The <see cref="Label"/> generated by Trello.</returns>
		public ILabel Add(string name, LabelColor? color)
		{
			var json = TrelloConfiguration.JsonFactory.Create<IJsonLabel>();
			json.Name = name ?? string.Empty;
			json.Color = color;
			json.ForceNullColor = !color.HasValue;
			json.Board = TrelloConfiguration.JsonFactory.Create<IJsonBoard>();
			json.Board.Id = OwnerId;

			var endpoint = EndpointFactory.Build(EntityRequestType.Board_Write_AddLabel);
			var newData = JsonRepository.Execute(Auth, endpoint, json);

			return new Label(newData, Auth);
		}

		/// <summary>
		/// Adds a filter to the collection.
		/// </summary>
		/// <param name="labelColor">The filter value.</param>
		public void Filter(LabelColor labelColor)
		{
			if (_additionalParameters == null)
				_additionalParameters = new Dictionary<string, object>();
			_additionalParameters["filter"] = labelColor.GetDescription();
		}

		/// <summary>
		/// Implement to provide data to the collection.
		/// </summary>
		protected sealed override void Update()
		{
			IncorporateLimit(_additionalParameters);

			var endpoint = EndpointFactory.Build(EntityRequestType.Board_Read_Labels, new Dictionary<string, object> {{"_id", OwnerId}});
			var newData = JsonRepository.Execute<List<IJsonLabel>>(Auth, endpoint);

			Items.Clear();
			Items.AddRange(newData.Select(jb =>
				{
					var board = jb.GetFromCache<Label>(Auth);
					board.Json = jb;
					return board;
				}));
		}
	}
}