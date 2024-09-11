
namespace FLCore.UI;

using Godot;

public abstract partial class UIControl : Control
{
	#region Properties
	
	[Export] public UIData Data { get; private set; }
	[Export] public ViewType ViewType { get; set; }
	[Export] public UIView KeyboardView { get; private set; }
	[Export] public UIView GamepadView { get; private set; }
	[Export] public UIView MobileView { get; private set; }
	[Export] public bool AlwaysUpdate { get; protected set; } = false;
	
	public bool IsOn { get; protected set; } = false;
	
	[Signal] public delegate void ViewChangedEventHandler(Page page, UIView oldView, PageView newView);
	
	#endregion // Properties
	
	#region Public Methods
	
	public T DataAs<T>() where T : PageData => this.Data as T;
	public T KeyboardViewAs<T>() where T : PageView => this.KeyboardView as T;
	public T GamepadViewAs<T>() where T : PageView => this.KeyboardView as T;
	public T MobileViewAs<T>() where T : PageView => this.KeyboardView as T;
	
	public virtual void BringToFront()
	{
		Node parent = this.GetParent();
		
		parent.MoveChild(this, parent.GetChildCount());
	}
	
	public virtual void ChangeView(ViewType nextViewType)
	{
		UIView oldView = this.GetViewByType(this.ViewType);
		UIView newView = this.GetViewByType(this.ViewType);
		
		this.OnToggle(nextViewType);
		this.EmitSignal(SignalName.ViewChanged, this, oldView, newView);
	}
	
	#endregion // Public Methods
	
	#region Godot Methods
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		if(this.IsOn || this.AlwaysUpdate)
		{
			this.OnProcess((float)delta);
		}
	}
	
	#endregion // Godot Methods
	
	#region Private Methods
	
	protected virtual void OnEnterTree()
	{
		this.KeyboardView?.OnEnterTree();
		this.GamepadView?.OnEnterTree();
		this.MobileView?.OnEnterTree();
	}
	
	protected virtual void OnQuit()
	{
		this.KeyboardView?.OnQuit();
		this.GamepadView?.OnQuit();
		this.MobileView?.OnQuit();
	}
	
	protected virtual void OnProcess(float delta) => this.GetViewByType(this.ViewType)?.OnProcess(delta);
	
	protected virtual void TransitionView(ViewType current, ViewType nextViewType, UITransition transition)
	{
		// TODO: Add transition
	}
	
	protected virtual void OnToggle(ViewType nextViewType)
	{
		if(nextViewType != this.ViewType)
		{
			UIView view = this.GetViewByType(this.ViewType);
			
			view?.OnDisable();
			view?.SetActive(false);
		}
		
		UIView newView = this.GetViewByType(nextViewType);
		
		this.ViewType = nextViewType;
		newView?.OnEnable();
		newView?.SetActive(true);
	}
	
	protected Node FindTopmostParent()
	{
		Node parent = this.GetParent();
		
		while(parent.GetParent() != null && parent.GetParent() as UIManagerNode == null)
		{
			parent = parent.GetParent();
		}
		
		if(parent.GetParent() == null)
		{
			return this.GetParent();
		}
		
		return parent;
	}
	
	protected void SetupFocus()
	{
		Node parent = this.FindTopmostParent();
		
		if(this.GetParent() != parent)
		{
			this.GetParent().RemoveChild(this);
			parent.AddChild(this);
		}
		else
		{
			this.BringToFront();
		}
	}
	
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
	
	protected virtual void HideAway()
	{
		this.SetAlpha(0.0f);
		this.SetActive(false);
		this.IsOn = false;
	}
	
	protected UIView GetViewByType(ViewType type) => type switch
	{
		ViewType.Keyboard => this.KeyboardView,
		ViewType.Gamepad => this.GamepadView,
		ViewType.Mobile => this.MobileView,
		_ => this.KeyboardView,
	};
	
	#endregion // Private Methods
}
