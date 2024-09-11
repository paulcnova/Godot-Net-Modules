
using Godot;

using System.Collections.Generic;
using System.Reflection;

namespace FLCore.UI
{
	[SceneLocation("")]
	public sealed partial class UIManagerNode : Control
	{
		#region Properties
		
		private Dictionary<System.Type, Page> pages = new Dictionary<System.Type, Page>();
		private Dictionary<System.Type, Widget> widgets = new Dictionary<System.Type, Widget>();
		private HashSet<System.Type> widgetsShown = new HashSet<System.Type>();
		private Stack<System.Type> history = new Stack<System.Type>();
		private Stack<System.Type> future = new Stack<System.Type>();
		private bool ignoreAddingToHistory = false;
		
		[Export] public Page StartingPage { get; private set; }
		[Export] public Control PagesContainer { get; private set; }
		[Export] public Control WidgetsContainer { get; private set; }
		
		[ExportGroup("Debug")]
		[Export] public Page CurrentPage { get; private set; }
		
		public Page PreviousPage => this.history.Count > 0
			? this.GetPage(this.history.Peek())
			: null;
		public Page NextPage => this.future.Count > 0
			? this.GetPage(this.future.Peek())
			: null;
		
		public static UIManagerNode Instance { get; private set; }
		
		#endregion // Properties
		
		#region Godot Methods
		
		public override void _EnterTree()
		{
			if(Instance == null)
			{
				Instance = this;
			}
			else
			{
				this.QueueFree();
				return;
			}
			this.OnEnterTree();
			base._EnterTree();
		}
		
		public override void _ExitTree()
		{
			if(Instance == this)
			{
				Instance = null;
			}
			base._ExitTree();
		}
		
		// TODO: Check for input changes and then swap views
		
		#endregion // Godot Methods
		
		#region Public Methods
		
		#region Pages Methods
		
		// public T SwapViewForPage<T>(ViewType viewType) where T : Page
		// public T SwapViewForWidget<T>(ViewType viewType) where T : Widget
		
		public T OpenPage<T>(UITransition transition = null) where T : Page => this.OpenPage(typeof(T), transition) as T;
		public Page OpenPage(System.Type type, UITransition transition = null)
		{
			Page page = this.GetPage(type);
			
			if(page == null)
			{
				// TODO: Instance page here
				return null;
			}
			
			if(this.CurrentPage != null)
			{
				this.CurrentPage.Toggle(false, transition);
				if(!this.ignoreAddingToHistory)
				{
					this.history.Push(this.CurrentPage.GetType());
				}
			}
			
			this.CurrentPage = page;
			page.Toggle(true, transition);
			if(!this.ignoreAddingToHistory && this.future.Count > 0)
			{
				this.future.Clear();
			}
			
			return page;
		}
		
		public T OpenPage<T, TData>(System.Action<TData> updateData, UITransition transition = null)
			where T : Page
			where TData : PageData => this.OpenPage<TData>(typeof(T), updateData, transition) as T;
		public Page OpenPage<TData>(System.Type type, System.Action<TData> updateData, UITransition transition = null)
			where TData : PageData
		{
			Page page = this.GetPage(type);
			
			if(page == null)
			{
				// TODO: Instance page here
				return null;
			}
			
			if(this.CurrentPage != null)
			{
				this.CurrentPage.Toggle(false, transition);
				if(!this.ignoreAddingToHistory)
				{
					this.history.Push(this.CurrentPage.GetType());
				}
			}
			
			this.CurrentPage = page;
			if(page.Data.GetType() == typeof(TData) && updateData != null)
			{
				updateData(page.Data as TData);
			}
			page.Toggle(true, transition);
			if(!this.ignoreAddingToHistory && this.future.Count > 0)
			{
				this.future.Clear();
			}
			
			return page;
		}
		
		public void ClosePage(UITransition transition = null)
		{
			if(this.CurrentPage != null)
			{
				this.CurrentPage.Toggle(false, transition);
				this.history.Push(this.CurrentPage.GetType());
				if(this.future.Count > 0)
				{
					this.future.Clear();
				}
				this.CurrentPage = null;
			}
		}
		
		public Page GoBack(UITransition transition = null)
		{
			if(this.history.Count == 0)
			{
				GDX.PrintWarning("No page to go back to");
				return null;
			}
			
			System.Type prevType = this.history.Pop();
			
			if(this.CurrentPage != null)
			{
				System.Type currType = this.CurrentPage.GetType();
				
				this.future.Push(currType);
			}
			
			this.ignoreAddingToHistory = true;
			
			Page page = this.OpenPage(prevType, transition);
			
			this.ignoreAddingToHistory = false;
			
			return page;
		}
		
		public Page GoForward(UITransition transition = null)
		{
			if(this.future.Count == 0)
			{
				GDX.PrintWarning("No page to go forward to");
				return null;
			}
			
			System.Type nextType = this.future.Pop();
			
			if(this.CurrentPage != null)
			{
				System.Type currType = this.CurrentPage.GetType();
				
				this.history.Push(currType);
			}
			
			this.ignoreAddingToHistory = true;
			
			Page page = this.OpenPage(nextType, transition);
			
			this.ignoreAddingToHistory = false;
			
			return page;
		}
		
		#endregion // Pages Methods
		
		#region Widget Methods
		
		public T ShowWidget<T>(UITransition transition = null) where T : Widget => this.ShowWidget(typeof(T), transition) as T;
		public Widget ShowWidget(System.Type type, UITransition transition = null)
		{
			Widget widget = this.GetWidget(type);
			
			if(widget == null)
			{
				// TODO: Instantiate widget
				return null;
			}
			
			widget.Toggle(true, transition);
			this.widgetsShown.Add(type);
			
			return widget;
		}
		
		public T ShowWidget<T, TData>(System.Action<TData> updateData, UITransition transition = null)
			where T : Widget
			where TData : WidgetData => this.ShowWidget<TData>(typeof(T), updateData, transition) as T;
		public Widget ShowWidget<TData>(System.Type type, System.Action<TData> updateData, UITransition transition = null)
			where TData : WidgetData
		{
			Widget widget = this.GetWidget(type);
			
			if(widget == null)
			{
				// TODO: Instantiate widget
				return null;
			}
			
			if(widget.Data.GetType() == typeof(TData) && updateData != null)
			{
				updateData(widget.Data as TData);
			}
			widget.Toggle(true, transition);
			this.widgetsShown.Add(type);
			
			return widget;
		}
		
		public T HideWidget<T>(UITransition transition = null) where T : Widget => this.HideWidget(typeof(T), transition) as T;
		public Widget HideWidget(System.Type type, UITransition transition = null)
		{
			Widget widget = this.GetWidget(type);
			
			if(widget == null) { return null; }
			
			widget.Toggle(false, transition);
			this.widgetsShown.Remove(type);
			
			return widget;
		}
		
		public T ToggleWidget<T>(UITransition transition = null) where T : Widget => this.ToggleWidget(typeof(T), transition) as T;
		public Widget ToggleWidget(System.Type type, UITransition transition = null)
		{
			Widget widget = this.GetWidget(type);
			
			if(widget == null) { return null; }
			
			widget.Toggle(!widget.IsOn, transition);
			
			if(widget.IsOn) { this.widgetsShown.Add(type); }
			else { this.widgetsShown.Remove(type); }
			
			return widget;
		}
		
		public void HideAllWidgets(UITransition transition = null)
		{
			foreach(KeyValuePair<System.Type, Widget> pair in this.widgets)
			{
				this.HideWidget(pair.Key, transition);
			}
		}
		
		public List<Widget> GetAllShownWidgets()
		{
			List<Widget> widgets = new List<Widget>();
			
			foreach(System.Type type in this.widgetsShown)
			{
				widgets.Add(this.widgets[type]);
			}
			
			return widgets;
		}
		
		#endregion // Widget Methods
		
		#region Getter Methods
		
		public T GetPage<T>() where T : Page => this.GetPage(typeof(T)) as T;
		public Page GetPage(System.Type type)
			=> type != null
				? (this.pages.TryGetValue(type, out Page page) ? page : null)
				: null;
		
		public T GetWidget<T>() where T : Widget => this.GetWidget(typeof(T)) as T;
		public Widget GetWidget(System.Type type)
			=> type != null
				? (this.widgets.TryGetValue(type, out Widget widget) ? widget : null)
				: null;
		
		public T GetData<T>() where T : UIData => this.GetData(typeof(T)) as T;
		public UIData GetData(System.Type type)
		{
			if(type == null) { return null; }
			
			if(type.IsSubclassOf(typeof(PageData)))
			{
				TiedToAttribute attribute = type.GetCustomAttribute<TiedToAttribute>();
				
				if(attribute != null)
				{
					Page page = this.GetPage(attribute.LinkedType);
					
					return page.Data;
				}
				else
				{
					foreach(Page page in this.pages.Values)
					{
						if(page.Data.GetType() == type)
						{
							return page.Data;
						}
					}
				}
			}
			else if(type.IsSubclassOf(typeof(WidgetData)))
			{
				TiedToAttribute attribute = type.GetCustomAttribute<TiedToAttribute>();
				
				if(attribute != null)
				{
					Widget widget = this.GetWidget(attribute.LinkedType);
					
					return widget.Data;
				}
				else
				{
					foreach(Widget widget in this.widgets.Values)
					{
						if(widget.Data.GetType() == type)
						{
							return widget.Data;
						}
					}
				}
			}
			
			return null;
		}
		
		#endregion // Getter Methods
		
		#endregion // Public Methods
		
		#region Private Methods
		
		private void OnEnterTree()
		{
			this.FindUIElements();
			this.AwakenUIElements();
			this.CurrentPage = this.StartingPage;
			if(this.StartingPage != null)
			{
				this.StartingPage.Toggle(true);
			}
		}
		
		private void FindUIElements()
		{
			List<Widget> sortedWidgets = new List<Widget>();
			
			foreach(Page page in this.GetAllChildrenOf<Page>())
			{
				page.Call(UIControl.MethodName.SetupFocus);
				
				if(!this.pages.ContainsKey(page.GetType()))
				{
					this.pages.Add(page.GetType(), page);
				}
			}
			
			foreach(Widget widget in this.GetAllChildrenOf<Widget>())
			{
				if(!this.widgets.ContainsKey(widget.GetType()))
				{
					this.widgets.Add(widget.GetType(), widget);
				}
				sortedWidgets.Add(widget);
			}
			
			sortedWidgets.Sort((left, right) => left.Priority.CompareTo(right.Priority));
			
			foreach(Widget widget in sortedWidgets)
			{
				widget.Call(UIControl.MethodName.SetupFocus);
			}
		}
		
		private void AwakenUIElements()
		{
			foreach(Page page in this.pages.Values)
			{
				page.Call(UIControl.MethodName.HideAway);
			}
			
			foreach(Page page in this.pages.Values)
			{
				page.Call(UIControl.MethodName.OnEnterTree);
			}
			
			foreach(Widget widget in this.widgets.Values)
			{
				widget.Call(UIControl.MethodName.HideAway);
				if(widget.ShowOnStartup)
				{
					// TODO: Change this to ResetTransition
					widget.Toggle(widget.IsOn, new FadeTransition(true));
					this.widgetsShown.Add(widget.GetType());
				}
			}
			
			foreach(Widget widget in this.widgets.Values)
			{
				widget.Call(UIControl.MethodName.OnEnterTree);
			}
		}
		
		#endregion // Private Methods
	}
}

namespace FLCore
{
	using FLCore.UI;
	
	public static class UIManager
	{
		#region Properties
		
		public static Page CurrentPage
		{
			get
			{
				if(UIManagerNode.Instance == null)
				{
					GDX.PrintWarning("UI Manager is not instantiated! Could not retrieve current page");
					return null;
				}
				return UIManagerNode.Instance.CurrentPage;
			}
		}
		
		public static Page PreviousPage
		{
			get
			{
				if(UIManagerNode.Instance == null)
				{
					GDX.PrintWarning("UI Manager is not instantiated! Could not retrieve previous page");
					return null;
				}
				return UIManagerNode.Instance.PreviousPage;
			}
		}
		
		public static Page NextPage
		{
			get
			{
				if(UIManagerNode.Instance == null)
				{
					GDX.PrintWarning("UI Manager is not instantiated! Could not retrieve next page");
					return null;
				}
				return UIManagerNode.Instance.NextPage;
			}
		}
		
		#endregion // Properties
		
		#region Public Methods
		
		#region Pages Methods
		
		public static T OpenPage<T>(UITransition transition = null) where T : Page
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not open page: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.OpenPage<T>(transition);
		}
		
		public static Page OpenPage(System.Type type, UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not open page: {type}");
				return null;
			}
			return UIManagerNode.Instance.OpenPage(type, transition);
		}
		
		public static T OpenPage<T, TData>(System.Action<TData> updateData, UITransition transition = null)
			where T : Page
			where TData : PageData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not open page: {typeof(T)}; with updating data");
				return null;
			}
			return UIManagerNode.Instance.OpenPage<T, TData>(updateData, transition);
		}
		
		public static Page OpenPage<TData>(System.Type type, System.Action<TData> updateData, UITransition transition = null)
			where TData : PageData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not open page: {type}; with updating data");
				return null;
			}
			return UIManagerNode.Instance.OpenPage<TData>(type, updateData, transition);
		}
		
		public static void ClosePage(UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not close page");
				return;
			}
			UIManagerNode.Instance.ClosePage(transition);
		}
		
		public static Page GoBack(UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not go back");
				return null;
			}
			return UIManagerNode.Instance.GoBack(transition);
		}
		
		public static Page GoForward(UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not go forward");
				return null;
			}
			return UIManagerNode.Instance.GoForward(transition);
		}
		
		#endregion // Pages Methods
		
		#region Widget Methods
		
		public static T ShowWidget<T>(UITransition transition = null) where T : Widget
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not show widget: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.ShowWidget<T>(transition);
		}
		
		public static Widget ShowWidget(System.Type type, UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not show widget: {type}");
				return null;
			}
			return UIManagerNode.Instance.ShowWidget(type, transition);
		}
		
		public static T ShowWidget<T, TData>(System.Action<TData> updateData, UITransition transition = null)
			where T : Widget
			where TData : WidgetData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not show widget: {typeof(T)}; with updating data");
				return null;
			}
			return UIManagerNode.Instance.ShowWidget<T, TData>(updateData, transition);
		}
		
		public static Widget ShowWidget<TData>(System.Type type, System.Action<TData> updateData, UITransition transition = null)
			where TData : WidgetData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not show widget: {type}; with updating data");
				return null;
			}
			return UIManagerNode.Instance.ShowWidget<TData>(type, updateData, transition);
		}
		
		public static T HideWidget<T>(UITransition transition = null) where T : Widget
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not hide widget: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.HideWidget<T>(transition);
		}
		
		public static Widget HideWidget(System.Type type, UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not hide widget: {type}");
				return null;
			}
			return UIManagerNode.Instance.HideWidget(type, transition);
		}
		
		public static T ToggleWidget<T>(UITransition transition = null) where T : Widget
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not toggle widget: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.ToggleWidget<T>(transition);
		}
		
		public static Widget ToggleWidget(System.Type type, UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not toggle widget: {type}");
				return null;
			}
			return UIManagerNode.Instance.ToggleWidget(type, transition);
		}
		
		public static void HideAllWidgets(UITransition transition = null)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Hide all widgets");
				return;
			}
			UIManagerNode.Instance.HideAllWidgets(transition);
		}
		
		public static List<Widget> GetAllShownWidgets()
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get all shown widgets");
				return new List<Widget>();
			}
			return UIManagerNode.Instance.GetAllShownWidgets();
		}
		
		#endregion // Widget Methods
		
		#region Getter Methods
		
		public static T GetPage<T>() where T : Page
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get page: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.GetPage<T>();
		}
		
		public static Page GetPage(System.Type type)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get page: {type}");
				return null;
			}
			return UIManagerNode.Instance.GetPage(type);
		}
		
		public static T GetWidget<T>() where T : Widget
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get widget: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.GetWidget<T>();
		}
		
		public static Widget GetWidget(System.Type type)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get widget: {type}");
				return null;
			}
			return UIManagerNode.Instance.GetWidget(type);
		}
		
		public static T GetData<T>() where T : UIData
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get data: {typeof(T)}");
				return null;
			}
			return UIManagerNode.Instance.GetData<T>();
		}
		
		public static UIData GetData(System.Type type)
		{
			if(UIManagerNode.Instance == null)
			{
				GDX.PrintWarning($"UI Manager is not instantiated! Could not get data: {type}");
				return null;
			}
			return UIManagerNode.Instance.GetData(type);
		}
		
		#endregion // Getter Methods
		
		#endregion // Public Methods
	}
}
