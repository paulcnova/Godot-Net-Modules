
namespace FLCore.Tooltips;

using FLCore;

using Godot;

[GlobalClass] public abstract partial class BaseTooltipUI : Control
{
	#region Properties
	
	private const string TooltipPrefabBase = "res://interface/tooltips";
	private const string GeneralTooltipPrefabBase = $"{TooltipPrefabBase}/general_tooltip.tscn";
	
	private Timer delayTimer;
	private Vector2 tooltipSize;
	private bool isFadingIn = false;
	private bool isInspecting = false;
	internal bool isTryingToExit = false;
	internal bool isTryingToFree = true;
	
	[Export] public ColorRect Backdrop { get; set; }
	[Export] public PanelContainer Container { get; set; }
	[Export] public Label TooltipName { get; set; }
	[Export] public RichTextLabel TooltipDescription { get; set; }
	
	[ExportGroup("Tooltip Settings")]
	[Export] public float DelayTime { get; set; } = 0.0f;
	[Export] public bool FollowMouse { get; set; } = true;
	[Export] public Vector2 Offset { get; set; } = new Vector2(24.0f, -16.0f);
	[Export] public Vector2 Padding { get; set; } = 32.0f * Vector2.One;
	[Export] public string InspectAction { get; set; } = "tooltip_inspect";
	[Export] public string ExitAction { get; set; } = "tooltip_exit";
	[Export] public float FadeInTime { get; set; } = 0.0f;
	[Export] public float FadeInTimer { get; set; } = 0.15f;
	
	public bool IsInspecting => this.isInspecting;
	public bool IsNestedTooltip
	{
		get
		{
			Node parent = this.GetParent();
			
			while(parent != null)
			{
				if(parent is BaseTooltipUI) { return true; }
				
				parent = parent.GetParent();
			}
			
			return false;
		}
	}
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _Ready()
	{
		this.MouseFilter = MouseFilterEnum.Ignore;
		this.Backdrop.Visible = false;
		this.delayTimer = new Timer();
		this.delayTimer.Timeout += this.ShowTooltip;
		this.AddChild(this.delayTimer);
		
		this.Container.ResetSize();
		this.tooltipSize = this.Container.GetRect().Size;
		this.PositionTooltip();
	}
	
	public override void _Process(double delta)
	{
		if(!this.isInspecting)
		{
			this.Container.ResetSize();
			this.tooltipSize = this.Container.GetRect().Size;
			this.PositionTooltip();
		}
		if(this.Visible)
		{
			if(this.isFadingIn)
			{
				this.FadeInTime += (float)delta;
				this.SetAlpha(Mathf.Lerp(0.0f, 1.0f, (this.FadeInTime / this.FadeInTimer)));
				if(this.FadeInTime >= this.FadeInTimer)
				{
					this.isFadingIn = false;
				}
			}
			this.tooltipSize = this.Container.GetRect().Size;
			if(!this.isInspecting)
			{
				if(Input.IsActionJustPressed(this.InspectAction))
				{
					this.isInspecting = true;
					this.MouseFilter = MouseFilterEnum.Stop;
					this.Backdrop.Visible = !this.IsNestedTooltip;
				}
			}
			else
			{
				if(Input.IsActionJustPressed(this.ExitAction))
				{
					this.isInspecting = false;
					this.MouseFilter = MouseFilterEnum.Ignore;
					this.Backdrop.Visible = false;
					if(this.isTryingToExit)
					{
						this.Hide();
						this.delayTimer.Stop();
						this.isTryingToExit = false;
						if(this.isTryingToFree)
						{
							this.QueueFree();
						}
					}
				}
				return;
			}
		}
	}
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	public static BaseTooltipUI Create(string entryID, string prefabPath)
	{
		DisplayableResource entry = TooltipEncyclopedia.FindEntry(entryID);
		
		if(entry == null)
		{
			GDX.PrintError($"Entry [{entryID}] does not exist, no tooltip is being rendered");
			return null;
		}
		return Create(entry, prefabPath);
	}
	
	public static BaseTooltipUI Create(DisplayableResource entry, string prefabPath)
	{
		BaseTooltipUI tooltip = GDX.Instantiate<BaseTooltipUI>($"{TooltipPrefabBase}/{prefabPath}_tooltip.tscn");
		
		if(tooltip == null)
		{
			tooltip = GDX.Instantiate<BaseTooltipUI>(GeneralTooltipPrefabBase);
		}
		
		if(tooltip == null)
		{
			GDX.PrintError($"General Tooltip prefab doesn't exist: could not create tooltip");
			return null;
		}
		
		tooltip.TopLevel = true;
		tooltip.Container.CustomMinimumSize = new Vector2(
			entry.RecommendedTooltipWidth > 0
				? entry.RecommendedTooltipWidth
				: tooltip.Container.CustomMinimumSize.X,
			entry.RecommendedTooltipHeight > 0
				? entry.RecommendedTooltipHeight
				: tooltip.Container.CustomMinimumSize.Y
		);
		tooltip.Setup(entry);
		tooltip.Container.ResetSize();
		
		return tooltip;
	}
	
	public abstract void Setup(DisplayableResource entry);
	
	public void TryToEnter() => this.isTryingToExit = false;
	public void TryToExit() => this.isTryingToExit = true;
	
	public void TryToHide()
	{
		if(this.isInspecting) { return; }
		
		this.Hide();
	}
	
	public void TryToQueueFree()
	{
		this.isTryingToExit = true;
		this.isTryingToFree = true;
		if(!this.isInspecting)
		{
			this.QueueFree();
		}
	}
	
	public void ShowTooltip()
	{
		this.SetAlpha(0.0f);
		this.FadeInTime = 0.0f;
		this.isFadingIn = true;
		this.Container.ResetSize();
		this.tooltipSize = this.Container.GetRect().Size;
		this.PositionTooltip();
		this.Show();
		this.delayTimer.Stop();
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private void PositionTooltip()
	{
		Vector2 border = this.GetViewport().GetVisibleRect().Size - this.Padding;
		Vector2 position = this.GetTooltipPosition();
		float finalX = position.X + this.Offset.X;
		float finalY = position.Y + this.Offset.Y;
		
		if(finalX + this.tooltipSize.X > border.X)
		{
			finalX = position.X - this.Offset.X - this.tooltipSize.X;
		}
		if(finalY + this.tooltipSize.Y > border.Y)
		{
			finalY = position.Y - this.Offset.Y - this.tooltipSize.Y;
		}
		
		this.Container.Position = new Vector2(finalX, finalY);
	}
	
	private void OnEntered()
	{
		if(this.isInspecting)
		{
			this.isTryingToExit = false;
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
		if(this.isInspecting)
		{
			this.isTryingToExit = true;
			return;
		}
		this.delayTimer.Stop();
		this.Hide();
	}
	
	private Vector2 GetTooltipPosition()
	{
		if(this.FollowMouse)
		{
			return this.GetViewport().GetMousePosition();
		}
		
		Vector2 position = Vector2.Zero;
		Node parent = this.GetParent();
		
		if(parent is Node2D parent2D)
		{
			position = parent2D.GetGlobalTransformWithCanvas().Origin;
		}
		else if(parent is Control parentControl)
		{
			position = parentControl.GetGlobalTransformWithCanvas().Origin;
		}
		
		
		return position;
	}
	
	private void SetAlpha(float alpha)
	{
		Color modulate = this.Modulate;
		
		modulate.A = alpha;
		
		this.Modulate = modulate;
	}
	
	#endregion // Private Methods
}
