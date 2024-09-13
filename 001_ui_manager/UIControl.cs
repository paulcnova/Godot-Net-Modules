
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
	
	public T DataAs<T>() where T : UIData => this.Data as T;
	public T KeyboardViewAs<T>() where T : UIView => this.KeyboardView as T;
	public T GamepadViewAs<T>() where T : UIView => this.KeyboardView as T;
	public T MobileViewAs<T>() where T : UIView => this.KeyboardView as T;
	
	public virtual void BringToFront()
	{
		Node parent = this.GetParent();
		
		parent.MoveChild(this, parent.GetChildCount());
	}
	
	public virtual void ChangeView(ViewType nextViewType)
	{
		UIView oldView = this.GetView(this.ViewType);
		UIView newView = this.GetView(nextViewType);
		
		this.OnViewChanged(oldView, newView);
		this.EmitSignal(SignalName.ViewChanged, this, oldView, newView);
		this.OnChangeView(nextViewType);
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
	
	protected virtual void OnViewChanged(UIView oldView, UIView newView) {}
	
	protected virtual void OnEnterTree()
	{
		this.KeyboardView?.OnEnterTree();
		this.GamepadView?.OnEnterTree();
		this.MobileView?.OnEnterTree();
	}
	
	protected virtual void OnExitTree()
	{
		this.KeyboardView?.OnExitTree();
		this.GamepadView?.OnExitTree();
		this.MobileView?.OnExitTree();
	}
	
	protected virtual void OnInput(InputEvent ev)
	{
		this.GetView(this.ViewType)?.OnInput(ev);
	}
	
	protected virtual void OnProcess(float delta) => this.GetView(this.ViewType)?.OnProcess(delta);
	
	protected virtual void TransitionView(ViewType current, ViewType nextViewType, UITransition transition)
	{
		// TODO: Add transition
	}
	
	protected virtual void OnChangeView(ViewType nextViewType)
	{
		if(nextViewType != this.ViewType)
		{
			UIView view = this.GetView(this.ViewType);
			
			view?.OnDisable();
			view?.SetActive(false);
		}
		
		UIView newView = this.GetView(nextViewType);
		
		this.ViewType = nextViewType;
		newView?.OnEnable();
		newView?.SetActive(true);
	}
	
	protected virtual void OnToggle(ViewType nextViewType)
	{
		UIView newView = this.GetView(nextViewType);
		
		this.ViewType = nextViewType;
		newView?.OnEnable();
		newView?.SetActive(true);
	}
	
	protected virtual void OnEnable() {}
	protected virtual void OnDisable() {}
	
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
	
	protected T GetCurrentView<T>() where T : UIView => this.GetView<T>(this.ViewType);
	protected T GetView<T>(ViewType type) where T : UIView => this.GetView(this.ViewType) as T;
	
	protected UIView GetCurrentView() => this.GetView(this.ViewType);
	protected UIView GetView(ViewType type) => type switch
	{
		ViewType.Keyboard => this.KeyboardView,
		ViewType.Gamepad => this.GamepadView,
		ViewType.Mobile => this.MobileView,
		_ => this.KeyboardView,
	};
	
	#endregion // Private Methods
}
