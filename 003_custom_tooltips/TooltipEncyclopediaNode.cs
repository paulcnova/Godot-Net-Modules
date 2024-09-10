
using FLCore;

using Godot;
using Godot.Collections;

namespace FLCore.Tooltips
{
	public partial class TooltipEncyclopediaNode : Node
	{
		#region Properties
		
		private const string GameDataPath = "res://content/game_data/";
		
		public static TooltipEncyclopediaNode Instance { get; private set; }
		
		#endregion // Properties
		
		#region Godot Methods
		
		public override void _EnterTree()
		{
			if(Instance == null)
			{
				Instance = this;
			}
			else
			{
				this.QueueFree();
				return;
			}
			this.FindTooltipEntries();
			base._EnterTree();
		}
		
		public override void _ExitTree()
		{
			if(Instance == this)
			{
				Instance = null;
			}
			base._ExitTree();
		}
		
		#endregion // Godot Methods
		
		#region Public Methods
		
		public BaseTooltipUI CreateTooltip(string tooltipID)
		{
			DisplayableResource resource = TooltipEncyclopedia.FindEntry(tooltipID);
			
			if(resource == null) { return null; }
			
			BaseTooltipUI tooltip = BaseTooltipUI.Create(
				tooltipID,
				!string.IsNullOrEmpty(resource.OverrideTooltipCategory)
					? resource.OverrideTooltipCategory
					: resource.TooltipCategory
			);
			
			if(tooltip == null) { return null; }
			
			return tooltip;
		}
		
		#endregion // Public Methods
		
		#region Private Methods
		
		private void FindTooltipEntries()
		{
			Array<DisplayableResource> entries = ResourceLocator.LoadAll<DisplayableResource>(GameDataPath, ".+");
			
			foreach(DisplayableResource entry in entries)
			{
				if(string.IsNullOrEmpty(entry.TooltipID)) { continue; }
				
				string[] paths = entry.TooltipID.Split('/');
				int i = 0;
				Node current = this;
				
				foreach(string path in paths)
				{
					if(i == paths.Length - 1)
					{
						break;
					}
					Node next = current.GetNodeOrNull(path);
					
					if(next == null)
					{
						Node folder = new Node();
						
						folder.Name = path;
						current.AddChild(folder);
						next = folder;
					}
					
					current = next;
					++i;
				}
				
				TooltipEntryNode entryNode = new TooltipEntryNode();
				
				entryNode.TooltipPath = entry.ResourcePath;
				entryNode.Name = paths[paths.Length - 1];
				current.AddChild(entryNode);
			}
		}
		
		#endregion // Private Methods
	}
}

namespace FLCore
{
	using FLCore.Tooltips;
	
	public static class TooltipEncyclopedia
	{
		#region Public Methods
		
		public static BaseTooltipUI CreateTooltip(string tooltipID)
		{
			if(TooltipEncyclopediaNode.Instance == null)
			{
				GDX.PrintError($"Tooltip Encyclopedia isn't instantiated! Could not find tooltip {tooltipID}");
				return null;
			}
			
			return TooltipEncyclopediaNode.Instance.CreateTooltip(tooltipID);
		}
		
		public static DisplayableResource FindEntry(string entryID)
		{
			if(TooltipEncyclopediaNode.Instance == null)
			{
				GDX.PrintError($"Tooltip Encyclopedia isn't instantiated! Could not find entry {entryID}");
				return null;
			}
			
			string correctedID = entryID.Replace("-", "/").ToLower();
			TooltipEntryNode entryNode = TooltipEncyclopediaNode.Instance.GetNodeOrNull<TooltipEntryNode>(correctedID);
			
			if(entryNode == null)
			{
				GDX.PrintError($"Entry {entryID} does not exist. Path {correctedID} does not exist");
				return null;
			}
			
			return ResourceLoader.Load<DisplayableResource>(entryNode.TooltipPath);
		}
		
		#endregion // Public Methods
}
}
