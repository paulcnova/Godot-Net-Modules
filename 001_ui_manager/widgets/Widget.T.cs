
namespace FLCore.UI;

using Godot;
using Godot.Collections;

public abstract partial class Widget<TData, [MustBeVariant] TView> : Widget
	where TData : WidgetData
	where TView : WidgetView
{
	#region Public Methods
	
	public TData GetData() => this.DataAs<TData>();
	public TView CurrentView() => this.GetCurrentView<TView>();
	public Array<TView> GetAllViews() => new Array<TView>()
	{
		this.KeyboardViewAs<TView>(),
		this.GamepadViewAs<TView>(),
		this.MobileViewAs<TView>()
	};
	
	#endregion // Public Methods
}
