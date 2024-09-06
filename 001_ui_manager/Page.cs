
namespace FLCore.UI;

using FLCore;

using Godot;

using System.Collections.Generic;

/// <summary>A base class for UI Pages that are out one at a time</summary>
[GlobalClass] public abstract partial class Page : Control
{
	#region Properties
	
	[Export] private bool alwaysUpdate = false;
	
	private CoroutineHandle fadeTransition;
	
	public bool IsOn { get; set; } = false;
	
	[Signal] public delegate void ToggledEventHandler(Page page);
	[Signal] public delegate void ToggledOnEventHandler(Page page);
	[Signal] public delegate void ToggledOffEventHandler(Page page);
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _Ready()
	{
		if(this.GetParent() == null || this.GetParent() is Window)
		{
			this.IsOn = true;
			this.OnToggle(true, null);
		}
	}
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		if(this.IsOn || this.alwaysUpdate)
		{
			this.OnProcess((float)delta);
		}
	}
	
	public override void _Notification(int what)
	{
		if(what == NotificationWMCloseRequest)
		{
			this.OnQuit();
		}
		base._Notification(what);
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	/// <summary>Brings the page up to the front of the rendering order</summary>
	/// <remarks>This is mostly to accommodate for async fades</remarks>
	public void BringToFront()
	{
		Node parent = this.GetParent();
		
		parent.MoveChild(this, parent.GetChildCount());
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Called when the page enters the tree</summary>
	protected virtual void OnEnterTree() {}
	
	protected virtual void OnQuit() {}
	
	/// <summary>Called when the page gets updated</summary>
	protected virtual void OnProcess(float delta) {}
	
	/// <summary>Called when the page gets toggled on/off</summary>
	/// <param name="on">Set to true to toggle the page on, off otherwise</param>
	/// <param name="parameter">The parameter to pass over to the page to fill with data (could be null)</param>
	protected virtual void OnToggle(bool on, object parameter) {}
	
	/// <summary>Finds the topmost parent that holds all the widgets, typically `UIManager > Widgets`</summary>
	/// <returns>Returns the topmost parent that holds all the widgets</returns>
	private Node FindTopmostParent()
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
	
	/// <summary>Sets up the ability to focus the widget and have it be brought to the front</summary>
	/// <remarks>This assumes that the UI Manager sorts out the widgets before running this method</remarks>
	internal void SetupFocus()
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
	
	/// <summary>Toggles the page on/off</summary>
	/// <param name="on">Set to true to toggle the page on, off otherwise</param>
	/// <param name="parameter">The parameter to pass over to the page to fill with data (could be null)</param>
	/// <param name="transition">The transition the page would go through when it gets toggled</param>
	internal void Toggle(bool on, object parameter, UITransition transition)
	{
		if(this.IsOn != on)
		{
			this.IsOn = on;
			if(this.fadeTransition.IsValid)
			{
				FLCore.Timing.KillCoroutines(this.fadeTransition);
				this.fadeTransition = default;
			}
			if(!transition.UseAsyncFades)
			{
				if(transition.FadeTransition <= 0.0f)
				{
					this.SetAlpha(this.IsOn ? 1.0f : 0.0f);
					this.SetActive(this.IsOn);
					this.CallToggledEvents();
				}
				else
				{
					this.fadeTransition = Timing.RunCoroutine(this.FadeTransition(transition));
				}
			}
			else
			{
				if(this.IsOn)
				{
					this.BringToFront();
				}
				if((this.IsOn && transition.FadeTransition <= 0.0f) || (!this.IsOn && transition.PreviousFadeTransition <= 0.0f))
				{
					this.SetAlpha(this.IsOn ? 1.0f : 0.0f);
					this.SetActive(this.IsOn);
					this.CallToggledEvents();
				}
				else
				{
					this.fadeTransition = Timing.RunCoroutine(this.FadeTransition(transition));
				}
			}
			this.OnToggle(this.IsOn, parameter);
		}
	}
	
	/// <summary>A coroutine to fade the page in/out</summary>
	/// <param name="transition">The transition data to update with</param>
	/// <returns>Returns the list of coroutine instructions for fading the page</returns>
	private IEnumerator<double> FadeTransition(UITransition transition)
	{
		float time = 0.0f;
		float duration = transition.UseAsyncFades
			? this.IsOn
				? transition.FadeTransition
				: transition.PreviousFadeTransition
			: transition.FadeTransition;
		float from = this.Modulate.A;
		float to = this.IsOn ? 1.0f : 0.0f;
		
		if(!this.IsOn)
		{
			this.SetActive(false);
		}
		
		yield return Timing.WaitForOneFrame;
		while(time <= duration)
		{
			time += (float)Timing.DeltaTime;
			this.SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp(time / duration, 0.0f, 1.0f)));
			yield return Timing.WaitForOneFrame;
		}
		this.SetActive(this.IsOn);
		this.CallToggledEvents();
		Timing.KillCoroutines(this.fadeTransition);
		this.fadeTransition = default;
	}
	
	/// <summary>Calls the 3 toggled events for the page</summary>
	private void CallToggledEvents()
	{
		this.EmitSignal(SignalName.Toggled, this);
		if(this.IsOn)
		{
			this.EmitSignal(SignalName.ToggledOn, this);
		}
		else
		{
			this.EmitSignal(SignalName.ToggledOff, this);
		}
	}
	
	/// <summary>Hides away the page from view</summary>
	internal void HideAway()
	{
		this.SetAlpha(0.0f);
		this.SetActive(false);
		this.IsOn = false;
	}
	
	/// <summary>Calls the <see cref="OnAwake()"/> method from another class within the module</summary>
	internal void CallEnterTree() => this.OnEnterTree();
	
	/// <summary>Sets the alpha of the page</summary>
	/// <param name="alpha">The alpha to set</param>
	protected void SetAlpha(float alpha)
	{
		Color color = this.Modulate;
		
		color.A = alpha;
		
		this.Modulate = color;
	}
	
	#endregion // Private Methods
}
