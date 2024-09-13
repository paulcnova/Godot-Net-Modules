
namespace FLCore.UI;

public abstract partial class WidgetView : UIView
{
	#region Properties
	
	public Widget Widget => this.GetParentOrNull<Widget>();
	
	#endregion // Properties
}
