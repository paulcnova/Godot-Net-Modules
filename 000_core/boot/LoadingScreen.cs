
namespace FLCore.UI;

using Godot;

public partial class LoadingScreen : Control
{
	#region Public Methods
	
	public virtual void UpdateLoadingBar(DisplayableResource resource, int current, int max) {}
	public virtual void LoadingIsCompleted(Callable completedCallback)
	{
		completedCallback.Call();
	}
	
	#endregion // Public Methods
}
