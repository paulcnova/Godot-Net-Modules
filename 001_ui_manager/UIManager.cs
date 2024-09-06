
namespace FLCore;

using FLCore.UI;

using Godot;

using System.Collections.Generic;

/// <summary>A manager that keeps track of UI pages and widgets</summary>
public static class UIManager
{
	#region Properties
	
	public static System.Type PreviousPage
	{
		get
		{
			if(UIManagerNode.Instance == null)
			{
				GD.PrintErr("UI Manager is not instantiated! Could not retrieve previous page");
				GD.PushWarning("UI Manager is not instantiated! Could not retrieve previous page");
				return null;
			}
			
			return UIManagerNode.Instance.PreviousPage;
		}
	}
	
	public static System.Type NextPage
	{
		get
		{
			if(UIManagerNode.Instance == null)
			{
				GD.PrintErr("UI Manager is not instantiated! Could not retrieve next page");
				GD.PushWarning("UI Manager is not instantiated! Could not retrieve next page");
				return null;
			}
			
			return UIManagerNode.Instance.NextPage;
		}
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	public static void GoBackOnePage(object parameter = null, UITransition transition = null)
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr("UI Manager is not instantiated! Could not go back one page");
			GD.PushWarning("UI Manager is not instantiated! Could not go back one page");
			return;
		}
		
		UIManagerNode.Instance.GoBackOnePage(parameter, transition);
	}
	
	public static void GoForwardOnePage(object parameter = null, UITransition transition = null)
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr("UI Manager is not instantiated! Could not go forward one page");
			GD.PushWarning("UI Manager is not instantiated! Could not go forward one page");
			return;
		}
		
		UIManagerNode.Instance.GoForwardOnePage(parameter, transition);
	}
	
	/// <summary>Gets the page from the given type</summary>
	/// <typeparam name="T">The type of page to query</typeparam>
	/// <returns>Returns the page, null otherwise</returns>
	public static T GetPage<T>() where T : Page => GetPage(typeof(T)) as T;
	
	/// <summary>Gets the page from the given type</summary>
	/// <param name="type">The type of page to query</param>
	/// <returns>Returns the page, null otherwise</returns>
	public static Page GetPage(System.Type type)
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr($"UI Manager is not instantiated! Could not retrieve page: {type}");
			return null;
		}
		
		return UIManagerNode.Instance.GetPage(type);
	}
	
	/// <summary>Gets the widget from the given type</summary>
	/// <typeparam name="T">The type of widget to query</typeparam>
	/// <returns>Returns the page, null otherwise</returns>
	public static T GetWidget<T>() where T : Widget => GetWidget(typeof(T)) as T;
	
	/// <summary>Gets the widget from the given type</summary>
	/// <param name="type">The type of widget to query</param>
	/// <returns>Returns the page, null otherwise</returns>
	public static Widget GetWidget(System.Type type)
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr($"UI Manager is not instantiated! Could not retrieve widget: {type}");
			return null;
		}
		
		return UIManagerNode.Instance.GetWidget(type);
	}
	
	/// <summary>Closes the currently opened page</summary>
	/// <param name="transition">The transition the page would go through when it gets toggled, setting to null will use default settings for transitions</param>
	public static void ClosePage(UITransition transition = null)
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr($"UI Manager is not instantiated! Could not close page");
			return;
		}
		
		UIManagerNode.Instance.ClosePage(transition);
	}
	
	/// <summary>Opens the page up, closing the previous page</summary>
	/// <param name="parameter">Data to pass into the page, could be null</param>
	/// <param name="transition">The transition the page would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <typeparam name="T">The type of page to query</typeparam>
	/// <returns>Returns the page that just got opened up, null otherwise</returns>
	public static T OpenPage<T>(object parameter = null, UITransition transition = null) where T : Page => OpenPage(typeof(T), parameter, transition) as T;
	
	/// <summary>Opens the page up, closing the previous page</summary>
	/// <param name="type">The type of page to query</param>
	/// <param name="parameter">Data to pass into the page, could be null</param>
	/// <param name="transition">The transition the page would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <returns>Returns the page that just got opened up, null otherwise</returns>
	public static Page OpenPage(System.Type type, object parameter = null, UITransition transition = null)
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr($"UI Manager is not instantiated! Could not open page: {type}");
			return null;
		}
		
		return UIManagerNode.Instance.OpenPage(type, parameter, transition);
	}
	
	/// <summary>Shows the widget</summary>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <typeparam name="T">The type of widget to query</typeparam>
	/// <returns>Returns the widget that just got shown, null otherwise</returns>
	public static T ShowWidget<T>(object parameter = null, UITransition transition = null) where T : Widget => ShowWidget(typeof(T), parameter, transition) as T;
	
	/// <summary>Shows the widget</summary>
	/// <param name="type">The type of widget to query</param>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <returns>Returns the widget that just got shown, null otherwise</returns>
	public static Widget ShowWidget(System.Type type, object parameter = null, UITransition transition = null)
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr($"UI Manager is not instantiated! Could not show widget: {type}");
			return null;
		}
		
		return UIManagerNode.Instance.ShowWidget(type, parameter, transition);
	}
	
	/// <summary>Hides the widget</summary>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <typeparam name="T">The type of widget to query</typeparam>
	/// <returns>Returns the widget that just got hidden, null otherwise</returns>
	public static T HideWidget<T>(object parameter = null, UITransition transition = null) where T : Widget => HideWidget(typeof(T), parameter, transition) as T;
	
	/// <summary>Hides the widget</summary>
	/// <param name="type">The type of widget to query</param>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <returns>Returns the widget that just got hidden, null otherwise</returns>
	public static Widget HideWidget(System.Type type, object parameter = null, UITransition transition = null)
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr($"UI Manager is not instantiated! Could not hide widget: {type}");
			return null;
		}
		
		return UIManagerNode.Instance.HideWidget(type, parameter, transition);
	}
	
	/// <summary>Toggles the widget</summary>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <typeparam name="T">The type of widget to query</typeparam>
	/// <returns>Returns the widget that just got toggled on/off, null otherwise</returns>
	public static T ToggleWidget<T>(object parameter = null, UITransition transition = null) where T : Widget => ToggleWidget(typeof(T), parameter, transition) as T;
	
	/// <summary>Toggles the widget</summary>
	/// <param name="type">The type of widget to query</param>
	/// <param name="parameter">Data to pass into the widget, could be null</param>
	/// <param name="transition">The transition the widget would go through when it gets toggled, setting to null will use default settings for transitions</param>
	/// <returns>Returns the widget that just got toggled on/off, null otherwise</returns>
	public static Widget ToggleWidget(System.Type type, object parameter = null, UITransition transition = null)
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr($"UI Manager is not instantiated! Could not toggle widget: {type}");
			return null;
		}
		
		return UIManagerNode.Instance.ToggleWidget(type, parameter, transition);
	}
	
	/// <summary>Hides all the currently opened widgets</summary>
	/// <param name="transition">The transition to hide all the widgets with</param>
	public static void HideAllWidgets(UITransition transition = null)
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr($"UI Manager is not instantiated! Could not hide all widgets");
			return;
		}
		
		UIManagerNode.Instance.HideAllWidgets(transition);
	}
	
	/// <summary>Gets the list of all the currently opened widgets</summary>
	/// <returns>Returns the list of all the currently opened widgets</returns>
	public static List<Widget> GetAllShownWidgets()
	{
		if(UIManagerNode.Instance == null)
		{
			GD.PrintErr($"UI Manager is not instantiated! Could not retrieve all shown widgets");
			return new List<Widget>();
		}
		
		return UIManagerNode.Instance.GetAllShownWidgets();
	}
	
	#endregion // Public Methods
}
