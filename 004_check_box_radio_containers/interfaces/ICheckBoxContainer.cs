
namespace FLCore.UI;

public interface ICheckBoxContainer
{
	#region Properties
	
	int MinSelections { get; set; }
	int MaxSelections { get; set; }
	
	int SelectionsCount { get; set; }
	bool IsAtMinimumSelections => this.SelectionsCount <= this.MinSelections;
	bool IsAtMaximumSelections => this.SelectionsCount >= this.MaxSelections;
	
	#endregion // Properties
	
	#region Methods
	
	void CheckToDisableRest();
	void SelectUsingText(string text);
	void SelectUsingText(string[] texts);
	
	#endregion // Methods
}
