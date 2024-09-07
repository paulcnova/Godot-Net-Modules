
namespace FLCore.SaveLoad;

using Godot;

using System.Collections.Generic;

/// <summary>A node for the <see cref="SaveManager"/>'s instance</summary>
[GlobalClass] public sealed partial class SaveManagerNode : Node
{
	#region Properties
	
	internal Dictionary<string, SaveFile> cachedFiles = new Dictionary<string, SaveFile>();
	
	#endregion // Properties
	
	#region Private Methods
	
	/// <summary>Finds if the given save file is cached</summary>
	/// <param name="path">The absolute path to the save file</param>
	/// <returns>Returns true if the save file is cached</returns>
	internal bool IsCached(string path) => this.cachedFiles.ContainsKey(path);
	
	/// <summary>Gets the cached save file</summary>
	/// <param name="path">The absolute path of the save file</param>
	/// <param name="asReadonly">Set to true to make the save file readonly</param>
	/// <param name="autoSave">Set to true to make the save file automatically save once it goes out of scope</param>
	/// <returns>Returns the cached save file</returns>
	internal SaveFile GetCached(string path, bool asReadonly = false, bool autoSave = false)
	{
		if(!this.cachedFiles.ContainsKey(path)) { return null; }
		
		SaveFile save = this.cachedFiles[path];
		
		if(asReadonly)
		{
			save = save.AsReadonly();
		}
		if(autoSave)
		{
			save = save.WithAutoSave();
		}
		SaveManager.isCacheDirty = true;
		
		return save;
	}
	
	#endregion // Private Methods
}
