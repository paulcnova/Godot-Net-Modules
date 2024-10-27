
namespace FLCore.UI;

using Godot;

public partial class LoadingScreen : Control
{
	#region Public Methods
	
	public void UpdateLoadingBar(DisplayableResource resource, int current, int max) {}
	public void LoadingIsCompleted(Callable completedCallback) {}
	
	#endregion // Public Methods
}
