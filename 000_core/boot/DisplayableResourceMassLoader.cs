
using FLCore.Boot;

using Godot;
using Godot.Collections;

namespace FLCore.Internal
{
	public partial class DisplayableResourceMassLoader : Node
	{
		#region Properties
		
		private const string GameDataPath = "res://content/game_data/";
		
		[Signal] public delegate void ContentLoadedEventHandler(DisplayableResource resource, int current, int max);
		[Signal] public delegate void LoadingCompletedEventHandler();
		
		public static DisplayableResourceMassLoader Instance { get; private set; }
		
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
			base._EnterTree();
		}
		
		public override void _Ready()
		{
			BootLoader boot = this.GetTree().Root.GetNodeOrNull<BootLoader>("Boot");
			
			if(boot == null)
			{
				this.LoadAllContent(GameDataPath);
			}
			base._Ready();
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
		
		public void LoadAllContent(params string[] paths)
		{
			Array<DisplayableResource> resources = new Array<DisplayableResource>();
			
			if(paths.Length == 0)
			{
				paths = new string[] { GameDataPath };
			}
			
			foreach(string path in paths)
			{
				resources.AddRange(ResourceLocator.LoadAll<DisplayableResource>(path));
			}
			
			int current = 0;
			int max = resources.Count;
			
			foreach(DisplayableResource resource in resources)
			{
				this.EmitSignal(SignalName.ContentLoaded, resource, current++, max);
			}
			this.EmitSignal(SignalName.LoadingCompleted);
		}
		
		#endregion // Public Methods
	}
}

namespace FLCore
{
	using FLCore.Internal;
	
	public static class DRML
	{
		#region Properties
		
		public static event DisplayableResourceMassLoader.ContentLoadedEventHandler ContentLoaded
		{
			add
			{
				if(DisplayableResourceMassLoader.Instance == null)
				{
					GDX.PrintWarning("Displayable Resource Mass Loader is not instantiated! Could not listen to load content");
					return;
				}
				DisplayableResourceMassLoader.Instance.ContentLoaded += value;
			}
			remove
			{
				if(DisplayableResourceMassLoader.Instance == null)
				{
					GDX.PrintWarning("Displayable Resource Mass Loader is not instantiated! Could not stop listening to load content");
					return;
				}
				DisplayableResourceMassLoader.Instance.ContentLoaded -= value;
			}
		}
		
		public static event DisplayableResourceMassLoader.LoadingCompletedEventHandler LoadingCompleted
		{
			add
			{
				if(DisplayableResourceMassLoader.Instance == null)
				{
					GDX.PrintWarning("Displayable Resource Mass Loader is not instantiated! Could not listen to completed loading");
					return;
				}
				DisplayableResourceMassLoader.Instance.LoadingCompleted += value;
			}
			remove
			{
				if(DisplayableResourceMassLoader.Instance == null)
				{
					GDX.PrintWarning("Displayable Resource Mass Loader is not instantiated! Could not stop listening to completed loading");
					return;
				}
				DisplayableResourceMassLoader.Instance.LoadingCompleted -= value;
			}
		}
		
		#endregion // Properties
		
		#region Public Methods
		
		public static void LoadAllContent(params string[] paths)
		{
			if(DisplayableResourceMassLoader.Instance == null)
			{
				GDX.PrintWarning("Displayable Resource Mass Loader is not instantiated! Could not load all content");
				return;
			}
			DisplayableResourceMassLoader.Instance.LoadAllContent(paths);
		}
		
		#endregion // Public Methods
	}
}
