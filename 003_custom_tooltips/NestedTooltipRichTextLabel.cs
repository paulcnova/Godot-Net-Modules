
namespace FLCore.Tooltips;

using Godot;

[GlobalClass] public partial class NestedTooltipRichTextLabel : RichTextLabel
{
	#region Godot Methods
	
	public override void _Ready()
	{
		this.MetaHoverStarted += this.OnLinkHoverStarted;
		this.MetaHoverEnded += this.OnLinkHoverEnded;
	}
	
	#endregion // Godot Methods
	
	#region Private Methods
	
	private void OnLinkHoverStarted(Variant meta)
	{
		string id = meta.AsString();
		BaseTooltipUI tooltip = TooltipEncyclopedia.CreateTooltip(id);
		
		if(tooltip == null) { return; }
		
		string correctedID = id.Replace('/', '-');
		Node oldVersion = this.GetNodeOrNull(correctedID);
		
		if(oldVersion != null)
		{
			this.RemoveChild(oldVersion);
		}
		
		tooltip.Name = correctedID;
		tooltip.Hide();
		this.AddChild(tooltip, true);
		tooltip.ShowTooltip();
	}
	
	private void OnLinkHoverEnded(Variant meta)
	{
		string id = meta.AsString().Replace('/', '-');
		BaseTooltipUI tooltip = this.GetNodeOrNull<BaseTooltipUI>(id);
		
		if(tooltip == null) { return; }
		
		tooltip.TryToQueueFree();
	}
	
	#endregion // Private Methods
}
