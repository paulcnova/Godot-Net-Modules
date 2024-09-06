
namespace FLCore;

using Godot;
using Godot.Collections;

using System.Collections.Generic;

internal static class ArrayExtensions
{
	#region Public Methods
	
	public static T[] ToArray<[MustBeVariant] T>(this Array<T> arr)
	{
		List<T> list = new List<T>(arr);
		
		return list.ToArray();
	}
	
	#endregion // Public Methods
}
