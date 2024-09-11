
namespace FLCore.UI;

public abstract partial class WidgetView : UIView
{
	#region Properties
	
	public Widget Parent => this.GetParentOrNull<Widget>();
	
	#endregion // Properties
}
