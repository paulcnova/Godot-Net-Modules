
namespace FLCore.UI;

public abstract partial class PageView<TControl, TView> : PageView
	where TControl : Page
	where TView : PageData
{
	#region Public Methods
	
	public TControl GetPage() => this.Page as TControl;
	public TView GetData() => this.DataAs<TView>();
	
	#endregion // Public Methods
}
