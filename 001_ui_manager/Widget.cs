
namespace FLCore.UI;

using FLCore;

using Godot;

using System.Collections.Generic;

/// <summary>A base class for widgets that get overlaid on top of pages</summary>
[GlobalClass] public abstract partial class Widget : Control
{
	#region Properties
	
	private CoroutineHandle fadeTransition;
	
	[Export] private bool alwaysUpdate = false;
	
	[Export] public bool ShowOnStartup { get; private set; } = false;
	[Export] public int Priority { get; private set; } = 0;
	
	public bool IsOn { get; private set; } = false;
	
	[Signal] public delegate void ToggledEventHandler(Widget widget);
	[Signal] public delegate void ToggledOnEventHandler(Widget widget);
	[Signal] public delegate void ToggledOffEventHandler(Widget widget);
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		if(this.IsOn || this.alwaysUpdate)
		{
			this.OnProcess((float)delta);
		}
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	/// <summary>Moves the widget back one within it's priority ranking</summary>
	public void MoveBackOne()
	{
		int index = this.GetIndex();
		
		if(index - 1 < 0) { return; }
		
		Widget widget = this.GetParent().GetChild(index - 1) as Widget;
		
		if(widget == null) { return; }
		if(widget.Priority != this.Priority) { return; }
		
		this.GetParent().MoveChild(this, index - 1);
	}
	
	/// <summary>Moves the widget forward one within it's priority ranking</summary>
	public void MoveForwardOne()
	{
		int index = this.GetIndex();
		
		if(index + 1 >= this.GetParent().GetChildCount()) { return; }
		
		Widget widget = this.GetParent().GetChild(index + 1) as Widget;
		
		if(widget == null) { return; }
		if(widget.Priority != this.Priority) { return; }
		this.GetParent().MoveChild(this, index + 1);
	}
	
	/// <summary>Brings the widget to the very back of it's priority</summary>
	public void BringToBack()
	{
		int index = this.GetIndex();
		int nextIndex = index - 1;
		
		while(nextIndex >= 0)
		{
			Widget widget = this.GetParent().GetChild(nextIndex) as Widget;
			
			if(widget == null) { break; }
			if(widget.Priority != this.Priority) { break; }
			
			index = nextIndex;
			--nextIndex;
		}
		
		this.GetParent().MoveChild(this, index);
	}
	
	public void BringToFront()
	{
		int index = this.GetIndex();
		int nextIndex = index + 1;
		
		while(nextIndex < this.GetParent().GetChildCount())
		{
			Widget widget = this.GetParent().GetChild(nextIndex) as Widget;
			
			if(widget == null) { break; }
			if(widget.Priority != this.Priority) { break; }
			
			index = nextIndex;
			++nextIndex;
		}
		
		this.GetParent().MoveChild(this, index);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Called when the widget awakens the first time around</summary>
	protected virtual void OnEnterTree() {}
	
	/// <summary>Called when the widget gets updated</summary>
	protected virtual void OnProcess(float delta) {}
	
	/// <summary>Called when the widget gets toggled on/off</summary>
	/// <param name="on">Set to true to toggle the widget on, off otherwise</param>
	/// <param name="parameter">The parameter to pass over to the widget to fill in with data (could be null)</param>
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
	
	/// <summary>Toggles the widget on/off</summary>
	/// <param name="on">Set to true to toggle the widget on, off otherwise</param>
	/// <param name="parameter">The parameter to pass over to the widget to fill in with data (could be null)</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled</param>
	internal void Toggle(bool on, object parameter, UITransition transition)
	{
		if(this.IsOn != on || (transition != null && transition.ShouldReset))
		{
			this.IsOn = on;
			if(this.IsOn && (transition != null && transition.ShouldBeBroughtToFront))
			{
				this.BringToFront();
			}
			if(this.fadeTransition.IsValid)
			{
				Timing.KillCoroutines(this.fadeTransition);
				this.fadeTransition = default;
			}
			if(transition == null || transition.FadeTransition <= 0.0f)
			{
				this.SetAlpha(this.IsOn ? 1.0f : 0.0f);
				this.Visible = this.IsOn;
				this.SetActive(this.IsOn);
				this.CallToggledEvents();
			}
			else
			{
				this.fadeTransition = Timing.RunCoroutine(this.FadeTransition(transition));
			}
			this.OnToggle(this.IsOn, parameter);
		}
	}
	
	/// <summary>A coroutine to fade the widget in/out</summary>
	/// <param name="transition">The transition data to update with</param>
	/// <returns>Returns the list of coroutine instructions for fading the widget</returns>
	private IEnumerator<double> FadeTransition(UITransition transition)
	{
		float time = 0.0f;
		float duration = transition.UseAsyncFades
			? this.IsOn
				? transition.FadeTransition
				: transition.PreviousFadeTransition
			: transition.FadeTransition;
		float from = transition.ShouldReset
			? this.IsOn
				? 0.0f
				: 1.0f
			: this.Modulate.A;
		float to = this.IsOn ? 1.0f : 0.0f;
		
		if(!this.IsOn)
		{
			this.SetActive(false);
		}
		
		yield return Timing.WaitForOneFrame;
		while(time <= transition.FadeTransition)
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
	
	/// <summary>Calls the 3 toggled events for the widget</summary>
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
	
	/// <summary>Hides the widget away on scene start</summary>
	internal void HideAway()
	{
		this.SetAlpha(this.ShowOnStartup ? 1.0f : 0.0f);
		this.Visible = this.ShowOnStartup;
		this.SetActive(this.ShowOnStartup);
		this.IsOn = this.ShowOnStartup;
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
