
namespace FLCore.UI;

public abstract partial class PageView : UIView
{
	#region Properties
	
	public Page Parent => this.GetParentOrNull<Page>();
	
	#endregion // Properties
}
