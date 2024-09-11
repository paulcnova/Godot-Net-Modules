
namespace FLCore.UI;

using Godot;

public abstract partial class UIView : Control
{
	#region Properties
	
	[Export] public ViewType ViewType { get; set; }
	
	#endregion // Properties
	
	#region Public Methods
	
	public virtual void OnEnterTree() {}
	public virtual void OnQuit() {}
	public virtual void OnProcess(float delta) {}
	public virtual void OnEnable() {}
	public virtual void OnDisable() {}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	protected void SetActive(bool isActive)
	{
		this.Visible = isActive;
		this.ProcessMode = isActive
			? Node.ProcessModeEnum.Inherit
			: Node.ProcessModeEnum.Disabled;
	}
	
	protected void SetAlpha(float alpha)
	{
		Color color = this.Modulate;
		
		color.A = alpha;
		this.Modulate = color;
	}
	
	#endregion // Private Methods
}
