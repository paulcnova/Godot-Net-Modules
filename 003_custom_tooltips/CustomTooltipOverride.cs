
namespace FLCore.Tooltips;

using FLCore;

using Godot;

[GlobalClass] public sealed partial class CustomTooltipOverride : Node
{
	#region Properties
	
	private BaseTooltipUI tooltip;
	private Timer delayTimer;
	
	[Export] public string TooltipEntryID { get; set; }
	[Export] public string TooltipPrefabCategory { get; set; }
	[Export] public float DelayTime { get; set; } = 0.0f;
	[Export] public DisplayableResource TooltipData { get; set; }
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _Ready()
	{
		Control parent = this.GetParentOrNull<Control>();
		
		if(parent != null)
		{
			parent.MouseEntered += this.OnEntered;
			parent.MouseExited += this.OnExited;
		}
		
		this.delayTimer = new Timer();
		this.delayTimer.Timeout += this.ShowTooltip;
		this.AddChild(this.delayTimer);
		
		this.tooltip = this.CreateTooltip();
		if(this.tooltip == null)
		{
			this.QueueFree();
			return;
		}
		this.tooltip.isTryingToFree = false;
		this.tooltip.TopLevel = true;
		this.tooltip.Container.ResetSize();
		this.tooltip.Hide();
		this.AddChild(this.tooltip);
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	public void Setup(DisplayableResource resource)
	{
		this.TooltipEntryID = resource.TooltipID;
		this.TooltipPrefabCategory = resource.TooltipCategory;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private BaseTooltipUI CreateTooltip() => this.TooltipData != null
		? BaseTooltipUI.Create(this.TooltipData, this.TooltipPrefabCategory)
		: BaseTooltipUI.Create(this.TooltipEntryID, this.TooltipPrefabCategory);
	
	private void ShowTooltip()
	{
		if(this.tooltip == null)
		{
			GDX.PrintError("Cannot show tooltip, tooltip is null!");
			return;
		}
		this.delayTimer.Stop();
		this.tooltip.ShowTooltip();
	}
	
	private void OnEntered()
	{
		if(this.tooltip.IsInspecting)
		{
			this.tooltip.TryToEnter();
			return;
		}
		if(this.DelayTime > 0.0f)
		{
			this.delayTimer.Start(this.DelayTime);
		}
		else
		{
			this.ShowTooltip();
		}
	}
	
	private void OnExited()
	{
		if(this.tooltip.IsInspecting)
		{
			this.tooltip.TryToExit();
			return;
		}
		this.delayTimer.Stop();
		this.tooltip.TryToHide();
	}
	
	#endregion // Private Methods
}