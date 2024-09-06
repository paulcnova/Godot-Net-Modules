
namespace FLCore;

using Godot;

[GlobalClass] public partial class DisplayableResource : Resource
{
	#region Properties
	
	[Export] public Texture2D Icon { get; set; }
	[Export] public string Name { get; set; }
	[Export(PropertyHint.MultilineText)] public string Description { get; set; }
	
	[ExportGroup("IDs")]
	[Export] public string ID { get; set; }
	[Export] public string ExpansionID { get; set; }
	[Export] public string TooltipID { get; set; }
	[Export] public string TooltipCategory { get; set; }
	
	#endregion // Properties
}
