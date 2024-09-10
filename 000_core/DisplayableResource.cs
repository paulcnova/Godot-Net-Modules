
namespace FLCore;

using Godot;

[GlobalClass] public partial class DisplayableResource : Resource
{
	#region Properties
	
	[Export] public bool IsFinished { get; set; }
	[Export] public Texture2D Icon { get; set; }
	[Export] public string Name { get; set; }
	[Export(PropertyHint.MultilineText)] public string Description { get; set; }
	
	[ExportGroup("IDs")]
	[Export] public string ExpansionID { get; set; }
	[Export] public string ID { get; set; }
	
	[ExportGroup("Tooltips")]
	[Export] public string TooltipID { get; set; }
	[Export] public string TooltipCategory { get; set; }
	[Export] public int RecommendedTooltipWidth { get; set; } = -1;
	[Export] public int RecommendedTooltipHeight { get; set; } = -1;
	[Export] public string OverrideTooltipCategory { get; set; }
	
	#endregion // Properties
}
