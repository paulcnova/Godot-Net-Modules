
namespace FLCore;

public sealed class SceneLocationAttribute : System.Attribute
{
	#region Properties
	
	public string ScenePath { get; private set; }
	
	public SceneLocationAttribute(string scenePath)
	{
		this.ScenePath = scenePath;
	}
	
	#endregion // Properties
}
