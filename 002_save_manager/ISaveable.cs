
using FLCore.SaveLoad;

public interface ISaveable
{
	#region Public Methods
	
	void DefineToFile(SaveFile file);
	void SaveToFile(SaveFile file);
	void LoadFromFile(SaveFile file);
	
	#endregion // Public Methods
}
