
namespace FLCore.UI;

public abstract partial class WidgetView<TControl, TData> : WidgetView
	where TControl : Widget
	where TData : WidgetData
{
	#region Public Methods
	
	public TControl GetWidget() => this.Widget as TControl;
	public TData GetData() => this.DataAs<TData>();
	
	#endregion // Public Methods
}
