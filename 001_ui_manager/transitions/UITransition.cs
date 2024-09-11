
namespace FLCore.UI;

public abstract class UITransition
{
	#region Properties
	
	public float FadeTransition { get; set; } = 0.0f;
	public float PreviousFadeTransition { get; set; } = 0.0f;
	public bool UseAsyncFades { get; set; } = false;
	public bool ShouldReset { get; set; } = false;
	public bool ShouldBeBroughtToFront { get; set; } = false;
	
	#endregion // Properties
	
	#region Public Methods
	
	public abstract object GetStartingData(UIControl control);
	public abstract object GetEndingData(UIControl control);
	public abstract void Update(UIControl control, object from, object to, float t);
	
	public static implicit operator UITransition(float fade) => new FadeTransition(fade);
	public static implicit operator UITransition((float, float) fades) => new FadeTransition(fades.Item1, fades.Item2);
	
	#endregion // Public Methods
}
