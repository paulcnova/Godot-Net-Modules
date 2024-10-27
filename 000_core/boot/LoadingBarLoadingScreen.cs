
namespace FLCore.UI;

using Godot;

public partial class LoadingBarLoadingScreen : LoadingScreen
{
	#region Properties
	
	[Export] public ProgressBar ProgressBar { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	public override void UpdateLoadingBar(DisplayableResource resource, int current, int max)
	{
		this.ProgressBar.MaxValue = max;
		this.ProgressBar.Value = current;
	}
	
	public override void LoadingIsCompleted(Callable completedCallback)
	{
		this.ProgressBar.Value = this.ProgressBar.MaxValue;
		base.LoadingIsCompleted(completedCallback);
	}
	
	#endregion // Public Methods
}
