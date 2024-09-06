
namespace FLCore.UI;

public sealed class UITransition
{
	#region Properties
	
	public float FadeTransition { get; set; }
	public float PreviousFadeTransition { get; set; }
	public bool UseAsyncFades { get; set; }
	public bool ShouldReset { get; set; }
	public bool ShouldBeBroughtToFront { get; set; }
	
	public UITransition(float fade)
	{
		this.FadeTransition = fade;
		this.PreviousFadeTransition = fade;
		this.UseAsyncFades = false;
	}
	
	public UITransition(float fade, float prevFade)
	{
		this.FadeTransition = fade;
		this.PreviousFadeTransition = prevFade;
		this.UseAsyncFades = true;
	}
	
	public UITransition() : this(0.0f) {}
	
	#endregion // Properties
	
	#region Public Methods
	
	public static implicit operator UITransition(float fade) => new UITransition(fade);
	public static implicit operator UITransition((float, float) fades) => new UITransition(fades.Item1, fades.Item2);
	
	#endregion // Public Methods
}
