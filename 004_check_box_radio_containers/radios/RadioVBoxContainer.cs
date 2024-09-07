
namespace FLCore.UI;

using FLCore;

using Godot;

[GlobalClass] public partial class RadioVBoxContainer : VBoxContainer, IRadioContainer
{
	#region Properties
	
	[Export] public bool DefaultSelectFirstSlot { get; set; } = true;
	
	public Button Selected { get; private set; }
	
	public bool IsChildSelected
	{
		get
		{
			foreach(Button button in this.GetAllChildrenOf<Button>())
			{
				if(button.ButtonPressed) { return true;}
			}
			
			return false;
		}
	}
	
	[Signal] public delegate void OnSelectedEventHandler(Button selected);
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _Ready()
	{
		foreach(Button child in this.GetAllChildrenOf<Button>())
		{
			this.OnChildEnteredTree(child);
		}
		this.ChildEnteredTree += this.OnChildEnteredTree;
		
		if(this.DefaultSelectFirstSlot && !this.IsChildSelected)
		{
			if(this.GetChildCount() > 0)
			{
				Node node = this.GetChild(0);
				
				if(node is Button button)
				{
					button.EmitSignal(Button.SignalName.Pressed);
				}
			}
		}
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	public void ForceSelectFirstSlot()
	{
		if(this.DefaultSelectFirstSlot && this.GetChildCount() >= 1 && !this.IsChildSelected)
		{
			Node child = this.GetChild(0);
			
			if(child is Button button)
			{
				this.SetSelected(button);
			}
		}
	}
	
	public void SetSelected(Button button)
	{
		this.Selected = button;
	}
	
	public void SelectUsingText(string text) => this.SelectUsingText(text, true);
	public void SelectUsingText(string text, bool emit)
	{
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
			if(this.GetChildCount() == 1 && this.DefaultSelectFirstSlot)
			{
				this.SetSelected(button);
			}
			button.Pressed += () => this.OnSelect(button);
		}
	}
	
	private void OnChildExitingTree(Node child)
	{
		if(this.Selected == child)
		{
			this.Selected = null;
		}
	}
	
	protected virtual void OnSelect(Button button)
	{
		foreach(Button btn in this.GetAllChildrenOf<Button>())
		{
			btn.ButtonPressed = false;
		}
		button.ButtonPressed = true;
		this.SetSelected(button);
		this.EmitSignal(SignalName.OnSelected, button);
	}
	
	#endregion // Private Methods
}
