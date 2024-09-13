
namespace FLCore.UI;

public abstract partial class PageView : UIView
{
	#region Properties
	
	public Page Page => this.GetParentOrNull<Page>();
	
	#endregion // Properties
}
