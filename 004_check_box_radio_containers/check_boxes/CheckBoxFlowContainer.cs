
namespace FLCore.UI;

using Godot;

using System.Collections.Generic;

[GlobalClass] public partial class CheckBoxFlowContainer : FlowContainer, ICheckBoxContainer
{
	#region Properties
	
	[Export] public int MinSelections { get; set; } = 1;
	[Export] public int MaxSelections { get; set; } = 1;
	
	public int SelectionsCount { get; set; } = 0;
	public bool IsAtMinimumSelections => this.SelectionsCount <= this.MinSelections;
	public bool IsAtMaximumSelections => this.SelectionsCount >= this.MaxSelections;
	
	[Signal] public delegate void OnSelectedEventHandler(Button[] buttons);
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _Ready()
	{
		foreach(Node child in this.GetChildren())
		{
			this.OnChildEnteredTree(child);
		}
		this.ChildEnteredTree += this.OnChildEnteredTree;
		this.CheckToDisableRest();
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	public void CheckToDisableRest()
	{
		if(this.SelectionsCount >= this.MaxSelections)
		{
			this.DisableAllUnselected();
		}
	}
	
	public void SelectUsingText(string text) => this.SelectUsingText(text, true);
	public void SelectUsingText(string text, bool emit)
	{
		if(this.IsAtMaximumSelections) { return; }
		
		foreach(Node child in this.GetChildren())
		{
			if(child is Button button)
			{
				if(button.Text == text)
				{
					if(emit)
					{
						button.EmitSignal(Button.SignalName.Pressed);
					}
					else
					{
						button.ButtonPressed = true;
						this.OnSelect(button);
					}
					break;
				}
			}
		}
	}
	
	public void SelectUsingText(string[] text) => this.SelectUsingText(text, true);
	public void SelectUsingText(string[] texts, bool emit)
	{
		foreach(string text in texts)
		{
			this.SelectUsingText(text, emit);
		}
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private void OnChildEnteredTree(Node child)
	{
		foreach(StringName group in this.GetGroups())
		{
			child.AddToGroup(group);
		}
		if(child is Button button)
		{
			button.Pressed += () => this.OnSelect(button);
		}
	}
	
	protected virtual void OnSelect(Button button)
	{
		// Ignore the button press since it shouldn't get lowered
		if(button.ButtonPressed == false && this.SelectionsCount <= this.MinSelections)
		{
			button.ButtonPressed = true;
			return;
		}
		if(button.ButtonPressed == false)
		{
			--this.SelectionsCount;
			this.EnableAll();
		}
		else
		{
			++this.SelectionsCount;
		}
		
		if(this.SelectionsCount >= this.MaxSelections)
		{
			this.DisableAllUnselected();
		}
		
		List<Button> buttons = new List<Button>();
		
		foreach(Node child in this.GetChildren())
		{
			if(child is Button btn)
			{
				if(btn.ButtonPressed)
				{
					buttons.Add(btn);
				}
			}
		}
		
		this.EmitSignal(SignalName.OnSelected, buttons.ToArray());
	}
	
	protected void EnableAll()
	{
		foreach(Node child in this.GetChildren())
		{
			if(child is Button button)
			{
				button.Disabled = false;
			}
		}
	}
	
	protected void DisableAllUnselected()
	{
		foreach(Node child in this.GetChildren())
		{
			if(child is Button button)
			{
				if(!button.ButtonPressed)
				{
					button.Disabled = true;
				}
			}
		}
	}
	
	#endregion // Private Methods
}
