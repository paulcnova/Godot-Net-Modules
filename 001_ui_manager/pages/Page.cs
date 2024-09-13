
namespace FLCore.UI;

using Godot;

using System.Collections.Generic;

public abstract partial class Page : UIControl
{
	#region Properties
	
	private CoroutineHandle fadeTransition;
	
	[Signal] public delegate void ToggledEventHandler(Page page);
	[Signal] public delegate void ToggledOnEventHandler(Page page);
	[Signal] public delegate void ToggledOffEventHandler(Page page);
	
	#endregion // Properties
	
	#region Private Methods
	
	internal void Toggle(ViewType viewType, bool on, UITransition transition = null)
	{
		if(this.IsOn != on)
		{
			this.IsOn = on;
			this.ViewType = viewType;
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
				if(this.fadeTransition.IsValid)
				{
					Timing.KillCoroutines(this.fadeTransition);
					this.fadeTransition = default;
				}
				if(transition.UseAsyncFades)
				{
					if(this.IsOn)
					{
						this.BringToFront();
					}
				}
				this.fadeTransition = Timing.RunCoroutine(this.Transition(transition));
			}
			
			if(this.IsOn)
			{
				this.OnEnable();
				this.OnToggle(this.ViewType);
			}
			else
			{
				this.OnDisable();
			}
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
			transition.Update(this, from, to, Mathf.Clamp(time / duration, 0.0f, 1.0f));
			yield return Timing.WaitForOneFrame;
		}
		this.SetActive(this.IsOn);
		this.CallToggledEvents();
		Timing.KillCoroutines(this.fadeTransition);
		this.fadeTransition = default;
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
