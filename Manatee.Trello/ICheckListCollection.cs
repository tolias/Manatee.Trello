﻿namespace Manatee.Trello
{
	/// <summary>
	/// A collection of checklists.
	/// </summary>
	public interface ICheckListCollection : IReadOnlyCheckListCollection
	{
		/// <summary>
		/// Creates a new checklist, optionally by copying a checklist.
		/// </summary>
		/// <param name="name">The name of the checklist to add.</param>
		/// <param name="source">A checklist to use as a template.</param>
		/// <returns>The <see cref="ICheckList"/> generated by Trello.</returns>
		ICheckList Add(string name, ICheckList source = null);
	}
}