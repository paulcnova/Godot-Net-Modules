
namespace FLCore.UI;

using Godot;

public class FadeTransition : UITransition
{
	#region Properties
	
	public FadeTransition(float fade)
	{
		this.FadeTransition = fade;
		this.PreviousFadeTransition = fade;
		this.UseAsyncFades = false;
	}
	
	public FadeTransition(float fade, float prevFade)
	{
		this.FadeTransition = fade;
		this.PreviousFadeTransition = prevFade;
		this.UseAsyncFades = true;
	}
	
	public FadeTransition(bool shouldReset) : this()
	{
		this.ShouldReset = shouldReset;
	}
	
	public FadeTransition() : this(0.0f) {}
	
	#endregion // Properties
	
	#region Public Methods
	
	public override object GetStartingData(UIControl control) => control.Modulate.A;
	public override object GetEndingData(UIControl control) => control.IsOn ? 1.0f : 0.0f;
	public override void Update(UIControl control, object from, object to, float t)
	{
		control.Call(UIControl.MethodName.SetAlpha, Mathf.Lerp((float)from, (float)to, t));
	}
	
	#endregion // Public Methods
}
