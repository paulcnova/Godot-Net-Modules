
namespace FLCore.UI;

using Godot;
using Godot.Collections;

public abstract partial class Page<TData, [MustBeVariant] TView> : Page
	where TData : PageData
	where TView : PageView
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
