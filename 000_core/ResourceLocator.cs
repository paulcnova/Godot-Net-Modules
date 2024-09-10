
namespace FLCore;

using Godot;
using Godot.Collections;

using System.Text.RegularExpressions;

public static class ResourceLocator
{
	#region Public Methods
	
	public static bool HasFiles(string path)
	{
		if(!DirAccess.DirExistsAbsolute(path)) { return false; }
		
		Array<string> files = GetFiles(path, false);
		
		return files.Count > 0;
	}
	
	public static Array<string> GetFiles(string path, bool recursive = true)
	{
		if(path.EndsWith('/') || path.EndsWith('\\'))
		{
			path = path.Substring(0, path.Length - 1);
		}
		
		Array<string> files = new Array<string>();
		string[] foundFiles = DirAccess.GetFilesAt(path);
		
		foreach(string file in foundFiles)
		{
			files.Add($"{path}/{file}");
		}
		
		if(recursive)
		{
			string[] foundDirectories = DirAccess.GetDirectoriesAt(path);
			
			foreach(string subDir in foundDirectories)
			{
				Array<string> nestedFiles = GetFiles($"{path}/{subDir}");
				
				foreach(string file in nestedFiles)
				{
					files.Add($"{file}");
				}
			}
		}
		
		return files;
	}
	
	public static Array<string> GetFiles(string path, string suffix, bool recursive = true)
	{
		Array<string> files = GetFiles(path, recursive);
		Array<string> results = new Array<string>();
		
		foreach(string file in files)
		{
			if(Regex.IsMatch(file, $@"\.{suffix}\.t?res(\.remap)?$"))
			{
				results.Add(file);
			}
		}
		
		return results;
	}
	
	public static Array<T> LoadAll<[MustBeVariant] T>(string path, bool recursive = true) where T : Resource
	{
		Array<T> resources = new Array<T>();
		Array<string> files = GetFiles(path, recursive);
		
		foreach(string file in files)
		{
			string correctedFilename = CorrectFileName(file);
		
			if(ResourceLoader.Exists(correctedFilename))
			{
				resources.Add(ResourceLoader.Load<T>(correctedFilename));
			}
		}
		
		return resources;
	}
	
	public static Array<T> LoadAll<[MustBeVariant] T>(string path, string suffix, bool recursive = true) where T : Resource
	{
		Array<T> resources = new Array<T>();
		Array<string> files = GetFiles(path, suffix, recursive);
		
		foreach(string file in files)
		{
			string correctedFilename = CorrectFileName(file);
			
			if(ResourceLoader.Exists(correctedFilename))
			{
				resources.Add(ResourceLoader.Load<T>(correctedFilename));
			}
		}
		
		return resources;
	}
	
	public static Array<T> LoadAllWithSpecificSuffix<[MustBeVariant] T>(string path, string suffix, bool recursive = true) where T : Resource
	{
		Array<T> resources = new Array<T>();
		Array<string> files = GetFiles(path, recursive);
		
		foreach(string file in files)
		{
			string correctedFilename = CorrectFileName(file);
		
			if(Regex.IsMatch(file, $@"\.{suffix}$"))
			{
				if(ResourceLoader.Exists(correctedFilename))
				{
					resources.Add(ResourceLoader.Load<T>(correctedFilename));
				}
			}
		}
		
		return resources;
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	private static string CorrectFileName(string filename) => filename.Replace(".remap", "");
	
	#endregion // Private Methods
}
