
namespace FLCore;

using Godot;

using System.Diagnostics;

public static class GDX
{
	#region Properties
	
	public static Control FocusedControl { get; set; }
	public static bool ControlHasFocus => FocusedControl != null;
	
	#endregion // Properties
	
	#region Public Methods
	
	public static T Instantiate<T>(string path) where T : Node
		=> ResourceLoader.Exists(path)
			? (GD.Load<PackedScene>(path)?.InstantiateOrNull<T>() ?? null)
			: null;
	
	public static T Instantiate<T>(PackedScene scene) where T : Node
		=> scene != null
			? scene.InstantiateOrNull<T>()
			: null;
	
	public static void Print(params object[] objs)
	{
		string content = GetTimestamp('I') + string.Join("", objs);
		
		GD.PrintRich(content);
	}
	
	public static void PrintS(params object[] objs)
	{
		string content = GetTimestamp('I') + string.Join(' ', objs);
		
		GD.PrintRich(content);
	}
	
	public static void PrintT(params object[] objs)
	{
		string content = GetTimestamp('I') + string.Join('\t', objs);
		
		GD.PrintRich(content);
	}
	
	public static void PrintWarning(params object[] objs)
	{
		string content = string.Join("", objs);
		
		GD.PrintErr(GetFileHint() + ": " + content);
		GD.PushWarning(content);
	}
	
	public static void PrintWarningS(params object[] objs)
	{
		string content = string.Join(' ', objs);
		
		GD.PrintErr(GetFileHint() + ": " + content);
		GD.PushWarning(content);
	}
	
	public static void PrintWarningT(params object[] objs)
	{
		string content = string.Join('\t', objs);
		
		GD.PrintErr(GetFileHint() + ": " +content);
		GD.PushWarning(content);
	}
	
	public static void PrintError(params object[] objs)
	{
		string content = string.Join("", objs);
		
		GD.PrintErr(GetFileHint() + ": " +content);
		GD.PushError(content);
	}
	
	public static void PrintErrorS(params object[] objs)
	{
		string content = string.Join(' ', objs);
		
		GD.PrintErr(GetFileHint() + ": " +content);
		GD.PushError(content);
	}
	
	public static void PrintErrorT(params object[] objs)
	{
		string content = string.Join('\t', objs);
		
		GD.PrintErr(GetFileHint() + ": " +content);
		GD.PushError(content);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private static string GetTimestamp(char level, string color = "#484")
	{
		System.TimeSpan time = System.DateTime.Now.TimeOfDay;
		bool isPM = time.Hours >= 12;
		string hours = time.Hours == 12 || time.Hours == 0
			? "12"
			: (time.Hours % 12).ToString().PadLeft(2, '0');
		string minutes = time.Minutes.ToString().PadLeft(2, '0');
		string timeStr = $"{hours}:{minutes} {(isPM ? "PM" : "AM")}";
		string hint = GetFileLink();
		
		return $"[hint=\"{hint}\"]{level} [{timeStr}][/hint]: ";
	}
	
	private static string GetFileHint(int frameIndex = 3)
	{
		StackTrace trace = new StackTrace(true);
		StackFrame frame = trace.GetFrame(frameIndex);
		
		if(frame == null) { return ""; }
		
		System.Reflection.MethodBase method = frame.GetMethod();
		
		if(method == null) { return ""; }
		
		return $"{method.DeclaringType?.Name}.{method.Name}";
	}
	
	private static string GetFileLink(int frameIndex = 3)
	{
		StackTrace trace = new StackTrace(true);
		StackFrame frame = trace.GetFrame(frameIndex);
		
		if(frame == null) { return ""; }
		
		System.Reflection.MethodBase method = frame.GetMethod();
		
		if(method == null) { return ""; }
		
		return $"{ProjectSettings.LocalizePath(frame.GetFileName())}:{frame.GetFileLineNumber()}";
	}
	
	#endregion // Private Methods
}
