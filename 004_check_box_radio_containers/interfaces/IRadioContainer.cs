
namespace FLCore.UI;

using Godot;

public interface IRadioContainer
{
	#region Properties
	
	bool DefaultSelectFirstSlot { get; set; }
	Button Selected { get; }
	
	#endregion // Properties
	
	#region Public Methods
	
	void ForceSelectFirstSlot();
	void SetSelected(Button button);
	void SelectUsingText(string text);
	
	#endregion // Public Methods
}
