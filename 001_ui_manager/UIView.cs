
namespace FLCore.UI;

using Godot;

public abstract partial class UIView : Control
{
	#region Properties
	
	[Export] public ViewType ViewType { get; set; }
	
	public UIControl Parent => this.GetParentOrNull<UIControl>();
	
	#endregion // Properties
	
	#region Public Methods
	
	public virtual void OnEnterTree() {}
	public virtual void OnExitTree() {}
	public virtual void OnProcess(float delta) {}
	public virtual void OnEnable() {}
	public virtual void OnDisable() {}
	public virtual void OnInput(InputEvent ev) {}
	
	public T DataAs<T>() where T : UIData => this.Parent.DataAs<T>();
	
	public void SetActive(bool isActive)
	{
		this.Visible = isActive;
		this.ProcessMode = isActive
			? Node.ProcessModeEnum.Inherit
			: Node.ProcessModeEnum.Disabled;
	}
	
	public void SetAlpha(float alpha)
	{
		Color color = this.Modulate;
		
		color.A = alpha;
		this.Modulate = color;
	}
	
	#endregion // Public Methods
}
