
namespace FLCore;

using System.Linq;
using System.Reflection;

internal static class Extension_String
{
	#region Public Methods
	
	public static System.Type AsType(this string str) => Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == str);
	
	#endregion // Public Methods
}
