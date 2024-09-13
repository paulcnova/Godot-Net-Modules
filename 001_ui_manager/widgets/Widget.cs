
namespace FLCore.UI;

using Godot;

using System.Collections.Generic;

public abstract partial class Widget : UIControl
{
	#region Properties
	
	private CoroutineHandle fadeTransition;
	
	[Export] public bool ShowOnStartup { get; private set; } = false;
	[Export] public int Priority { get; private set; } = 0;
	
	[Signal] public delegate void ToggledEventHandler(Widget widget);
	[Signal] public delegate void ToggledOnEventHandler(Widget widget);
	[Signal] public delegate void ToggledOffEventHandler(Widget widget);
	
	#endregion // Properties
	
	#region Public Methods
	
	public override void BringToFront()
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
	
	public void MoveForwardOne()
	{
		int index = this.GetIndex();
		
		if(index + 1 >= this.GetParent().GetChildCount()) { return; }
		
		Widget widget = this.GetParent().GetChild(index + 1) as Widget;
		
		if(widget == null) { return; }
		if(widget.Priority != this.Priority) { return; }
		this.GetParent().MoveChild(this, index + 1);
	}
	
	public void MoveBackOne()
	{
		int index = this.GetIndex();
		
		if(index - 1 < 0) { return; }
		
		Widget widget = this.GetParent().GetChild(index - 1) as Widget;
		
		if(widget == null) { return; }
		if(widget.Priority != this.Priority) { return; }
		
		this.GetParent().MoveChild(this, index - 1);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	internal void Toggle(ViewType viewType, bool on, UITransition transition = null)
	{
		if(this.IsOn != on || (transition != null && transition.ShouldReset))
		{
			this.IsOn = on;
			this.ViewType = viewType;
			if(this.IsOn && transition != null && transition.ShouldBeBroughtToFront)
			{
				this.BringToFront();
			}
			if(this.fadeTransition.IsValid)
			{
				Timing.KillCoroutines(this.fadeTransition);
				this.fadeTransition = default;
			}
			if(transition == null || (
				transition.UseAsyncFades
					? ((this.IsOn && transition.FadeTransition <= 0.0f) || (!this.IsOn && transition.PreviousFadeTransition <= 0.0f))
					: transition.FadeTransition <= 0.0f
			))
			{
				this.SetAlpha(this.IsOn ? 1.0f : 0.0f);
				this.SetActive(this.IsOn);
				this.CallToggledEvents();
			}
			else
			{
				this.fadeTransition = Timing.RunCoroutine(this.Transition(transition));
			}
			this.OnToggle(this.ViewType);
		}
		else if(this.ViewType != viewType)
		{
			this.ViewType = viewType;
			this.ChangeView(this.ViewType);
		}
	}
	
	private IEnumerator<double> Transition(UITransition transition)
	{
		float time = 0.0f;
		float duration = transition.UseAsyncFades
			? this.IsOn
				? transition.FadeTransition
				: transition.PreviousFadeTransition
			: transition.FadeTransition;
		object from = transition.GetStartingData(this);
		object to = transition.GetEndingData(this);
		
		if(!this.IsOn)
		{
			this.SetActive(false);
		}
		
		yield return Timing.WaitForOneFrame;
		while(time <= duration)
		{
			time += (float)Timing.DeltaTime;
			transition.Update(this, from, to, time);
			yield return Timing.WaitForOneFrame;
		}
		this.SetActive(this.IsOn);
		this.CallToggledEvents();
		Timing.KillCoroutines(this.fadeTransition);
		this.fadeTransition = default;
	}
	
	protected override void HideAway()
	{
		this.SetAlpha(this.ShowOnStartup ? 1.0f : 0.0f);
		this.SetActive(this.ShowOnStartup);
		this.IsOn = this.ShowOnStartup;
	}
	
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
	
	#endregion // Private Methods
}
