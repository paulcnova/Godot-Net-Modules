
namespace FLCore.Boot;

using FLCore.UI;

using Godot;

public partial class BootLoader : Node
{
	#region Properties
	
	private LoadingScreen loadingScreen;
	
	[Export] public PackedScene StartScene { get; set; }
	[Export] public PackedScene DefaultLoadScreen { get; set; }
	[Export] public Node SceneContainer { get; set; }
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _Ready()
	{
		LoadingScreen loading = this.DefaultLoadScreen.Instantiate<LoadingScreen>();
		
		this.loadingScreen = loading;
		this.SceneContainer.AddChild(loading);
		
		DRML.ContentLoaded += this.OnContentLoaded;
		DRML.LoadingCompleted += this.OnLoadingCompleted;
		DRML.LoadAllContent();
	}
	
	#endregion // Godot Methods
	
	#region Private Methods
	
	private void OnContentLoaded(DisplayableResource resource, int current, int max)
	{
		if(this.loadingScreen == null) { return; }
		
		this.loadingScreen.UpdateLoadingBar(resource, current, max);
	}
	
	private void OnLoadingCompleted()
	{
		if(this.loadingScreen == null) { return; }
		
		this.loadingScreen.LoadingIsCompleted(Callable.From(this.ChangeSceneToStart));
	}
	
	private void ChangeSceneToStart()
	{
		this.SceneContainer.RemoveChild(this.loadingScreen);
		this.loadingScreen = null;
		
		Node start = this.StartScene.Instantiate<Node>();
		
		this.SceneContainer.AddChild(start);
	}
	
	#endregion // Private Methods
}
