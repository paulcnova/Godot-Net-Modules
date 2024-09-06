
namespace FLCore.UI;

using Godot;

using System.Collections.Generic;

/// <summary>A manager that keeps track of UI pages and widgets</summary>
[GlobalClass] public sealed partial class UIManagerNode : Control
{
	#region Properties
	
	[Export] private Page startingPage;
	[Export] private Page currentlyOpenPage;
	
	private Dictionary<System.Type, Page> pages = new Dictionary<System.Type, Page>();
	private Dictionary<System.Type, Widget> widgets = new Dictionary<System.Type, Widget>();
	private HashSet<System.Type> widgetsShown = new HashSet<System.Type>();
	private Stack<System.Type> history = new Stack<System.Type>();
	private Stack<System.Type> future = new Stack<System.Type>();
	private bool ignoreAddingToHistory = false;
	
	public System.Type PreviousPage => this.history.Count > 0 ? this.history.Peek() : null;
	public System.Type NextPage => this.future.Count > 0 ? this.future.Peek() : null;
	
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
	
	#endregion // Godot Methods
	
	#region Public Methods
	
	public void GoBackOnePage(object parameter = null, UITransition transition = null)
	{
		if(this.history.Count == 0)
		{
			GD.PrintErr("No page to go back to");
			return;
		}
		
		System.Type prevType = this.history.Pop();
		
		if(this.currentlyOpenPage != null)
		{
			System.Type currType = this.currentlyOpenPage.GetType();
			
			this.future.Push(currType);
		}
		
		this.ignoreAddingToHistory = true;
		this.OpenPage(prevType, parameter, transition);
		this.ignoreAddingToHistory = false;
	}
	
	public void GoForwardOnePage(object parameter = null, UITransition transition = null)
	{
		if(this.future.Count == 0)
		{
			GD.PrintErr("No page to go forward to");
			return;
		}
		
		System.Type nextType = this.future.Pop();
		
		if(this.currentlyOpenPage != null)
		{
			System.Type currType = this.currentlyOpenPage.GetType();
			
			this.history.Push(currType);
		}
		
		this.ignoreAddingToHistory = true;
		this.OpenPage(nextType, parameter, transition);
		this.ignoreAddingToHistory = false;
	}
	
	/// <summary>Gets the page from the given type</summary>
	/// <typeparam name="T">The type of page to query</typeparam>
	/// <returns>Returns the page, null otherwise</returns>
	public T GetPage<T>() where T : Page => this.GetPage(typeof(T)) as T;
	
	/// <summary>Gets the page from the given type</summary>
	/// <param name="type">The type of page to query</param>
	/// <returns>Returns the page, null otherwise</returns>
	public Page GetPage(System.Type type) => type != null ? (this.pages.TryGetValue(type, out Page page) ? page : null) : null;
	
	/// <summary>Gets the widget from the given type</summary>
	/// <typeparam name="T">The type of widget to query</typeparam>
	/// <returns>Returns the page, null otherwise</returns>
	public T GetWidget<T>() where T : Widget => this.GetWidget(typeof(T)) as T;
	
	/// <summary>Gets the widget from the given type</summary>
	/// <param name="type">The type of widget to query</param>
	/// <returns>Returns the page, null otherwise</returns>
	public Widget GetWidget(System.Type type) => type != null ? (this.widgets.TryGetValue(type, out Widget widget) ? widget : null) : null;
	
	/// <summary>Closes the currently opened page</summary>
	/// <param name="transition">The transition the page would go through when it gets toggled, setting to null will use default settings for transitions</param>
	public void ClosePage(UITransition transition = null)
	{
		if(this.currentlyOpenPage != null)
		{
			if(transition == null)
			{
				transition = new UITransition();
			}
			
			this.currentlyOpenPage.Toggle(false, null, transition);
			this.history.Push(this.currentlyOpenPage.GetType());
			if(this.future.Count > 0)
			{
				this.future.Clear();
			}
			this.currentlyOpenPage = null;
		}
	}
	
	/// <summary>Opens the page up, closing the previous page</summary>
	/// <param name="parameter">Data to pass into the page, could be null</param>
	/// <param name="transition">The transition the page would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <typeparam name="T">The type of page to query</typeparam>
	/// <returns>Returns the page that just got opened up, null otherwise</returns>
	public T OpenPage<T>(object parameter = null, UITransition transition = null) where T : Page => this.OpenPage(typeof(T), parameter, transition) as T;
	
	/// <summary>Opens the page up, closing the previous page</summary>
	/// <param name="type">The type of page to query</param>
	/// <param name="parameter">Data to pass into the page, could be null</param>
	/// <param name="transition">The transition the page would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <returns>Returns the page that just got opened up, null otherwise</returns>
	public Page OpenPage(System.Type type, object parameter = null, UITransition transition = null)
	{
		Page page = this.GetPage(type);
		
		if(page == null) { return null; }
		if(transition == null && parameter is UITransition)
		{
			transition = parameter as UITransition;
			parameter = null;
		}
		else if(transition == null)
		{
			transition = new UITransition();
		}
		
		if(this.currentlyOpenPage != null)
		{
			this.currentlyOpenPage.Toggle(false, null, transition);
			if(!this.ignoreAddingToHistory)
			{
				this.history.Push(this.currentlyOpenPage.GetType());
			}
		}
		this.currentlyOpenPage = page;
		page.Toggle(true, parameter, transition);
		if(!this.ignoreAddingToHistory && this.future.Count > 0)
		{
			this.future.Clear();
		}
		
		return page;
	}
	
	/// <summary>Shows the widget</summary>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <typeparam name="T">The type of widget to query</typeparam>
	/// <returns>Returns the widget that just got shown, null otherwise</returns>
	public T ShowWidget<T>(object parameter = null, UITransition transition = null) where T : Widget => this.ShowWidget(typeof(T), parameter, transition) as T;
	
	/// <summary>Shows the widget</summary>
	/// <param name="type">The type of widget to query</param>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <returns>Returns the widget that just got shown, null otherwise</returns>
	public Widget ShowWidget(System.Type type, object parameter = null, UITransition transition = null)
	{
		Widget widget = this.GetWidget(type);
		
		if(widget == null) { return null; }
		if(transition == null && parameter is UITransition)
		{
			transition = parameter as UITransition;
			parameter = null;
		}
		else if(transition == null)
		{
			transition = new UITransition();
		}
		
		widget.Toggle(true, parameter, transition);
		this.widgetsShown.Add(type);
		
		return widget;
	}
	
	/// <summary>Hides the widget</summary>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <typeparam name="T">The type of widget to query</typeparam>
	/// <returns>Returns the widget that just got hidden, null otherwise</returns>
	public T HideWidget<T>(object parameter = null, UITransition transition = null) where T : Widget => this.HideWidget(typeof(T), parameter, transition) as T;
	
	/// <summary>Hides the widget</summary>
	/// <param name="type">The type of widget to query</param>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <returns>Returns the widget that just got hidden, null otherwise</returns>
	public Widget HideWidget(System.Type type, object parameter = null, UITransition transition = null)
	{
		Widget widget = this.GetWidget(type);
		
		if(widget == null) { return null; }
		if(transition == null && parameter is UITransition)
		{
			transition = parameter as UITransition;
			parameter = null;
		}
		else if(transition == null)
		{
			transition = new UITransition();
		}
		
		widget.Toggle(false, parameter, transition);
		this.widgetsShown.Remove(type);
		
		return widget;
	}
	
	/// <summary>Toggles the widget</summary>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <typeparam name="T">The type of widget to query</typeparam>
	/// <returns>Returns the widget that just got toggled on/off, null otherwise</returns>
	public T ToggleWidget<T>(object parameter = null, UITransition transition = null) where T : Widget => this.ToggleWidget(typeof(T), parameter, transition) as T;
	
	/// <summary>Toggles the widget</summary>
	/// <param name="type">The type of widget to query</param>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <returns>Returns the widget that just got toggled on/off, null otherwise</returns>
	public Widget ToggleWidget(System.Type type, object parameter = null, UITransition transition = null)
	{
		Widget widget = this.GetWidget(type);
		
		if(widget == null) { return null; }
		if(transition == null && parameter is UITransition)
		{
			transition = parameter as UITransition;
			parameter = null;
		}
		else if(transition == null)
		{
			transition = new UITransition();
		}
		
		widget.Toggle(!widget.IsOn, parameter, transition);
		
		if(widget.IsOn) { this.widgetsShown.Add(type); }
		else { this.widgetsShown.Remove(type); }
		
		return widget;
	}
	
	/// <summary>Hides all the currently opened widgets</summary>
	/// <param name="transition">The transition to hide all the widgets with</param>
	public void HideAllWidgets(UITransition transition = null)
	{
		foreach(KeyValuePair<System.Type, Widget> pair in this.widgets)
		{
			this.HideWidget(pair.Key, null, transition);
		}
	}
	
	/// <summary>Gets the list of all the currently opened widgets</summary>
	/// <returns>Returns the list of all the currently opened widgets</returns>
	public List<Widget> GetAllShownWidgets()
	{
		List<Widget> widgets = new List<Widget>();
		
		foreach(System.Type type in this.widgetsShown)
		{
			widgets.Add(this.widgets[type]);
		}
		
		return widgets;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Awakens the manager and makes it into a singleton</summary>
	private void OnEnterTree()
	{
		this.FindUIElements();
		this.AwakenUIElements();
		this.currentlyOpenPage = this.startingPage;
		if(this.startingPage != null)
		{
			this.startingPage.Toggle(true, null, new UITransition());
		}
	}
	
	/// <summary>Looks for all the UI elements within the children of the UI Manager</summary>
	private void FindUIElements()
	{
		List<Widget> sortedWidgets = new List<Widget>();
		
		foreach(Page page in this.GetAllChildrenOf<Page>())
		{
			page.SetupFocus();
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
			widget.SetupFocus();
		}
	}
	
	/// <summary>Awakens all the pages and widgets, hiding them away</summary>
	private void AwakenUIElements()
	{
		foreach(Page page in this.pages.Values)
		{
			page.HideAway();
		}
		
		foreach(Page page in this.pages.Values)
		{
			page.CallEnterTree();
		}
		
		foreach(Widget widget in this.widgets.Values)
		{
			widget.HideAway();
			if(widget.ShowOnStartup)
			{
				widget.Toggle(widget.IsOn, null, new UITransition() { ShouldReset = true });
				this.widgetsShown.Add(widget.GetType());
			}
		}
		
		foreach(Widget widget in this.widgets.Values)
		{
			widget.CallEnterTree();
		}
	}
	
	#endregion // Private Methods
}
