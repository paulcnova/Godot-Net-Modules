
namespace FLCore;

using FLCore.SaveLoad;

using Godot;

using System.Collections.Generic;
using System.Text.RegularExpressions;

using Directory = System.IO.Directory;
using Path = System.IO.Path;
using File = System.IO.File;

/// <summary>A manager that saves and loads save files</summary>
public static class SaveManager
{
	#region Properties
	
	/// <summary>The file extension of every save file managed</summary>
	public const string SaveFileExtension = ".save";
	/// <summary>The default save file name used within <see cref="Default"/> and <see cref="DefaultNoLoad"/></summary>
	public const string DefaultSaveFileName = "save_data";
	/// <summary>The local path to the save location</summary>
	public const string LocalPath = "user://save_data";
	
	private static SaveManagerNode instance = null;
	
	/// <summary>Set to true to declare that the cache is dirtied</summary>
	internal static bool isCacheDirty = false;
	
	/// <summary>Gets the root path of the save data</summary>
	public static string RootPath => ProjectSettings.GlobalizePath(LocalPath);
	
	/// <summary>Gets the path to the <see cref="Default"/> save file</summary>
	public static string DefaultPath => Path.Combine(RootPath, DefaultSaveFileName);
	
	/// <summary>Gets the default save file</summary>
	public static SaveFile Default => GetSave(DefaultPath);
	
	/// <summary>Gets the default save file without loading any of the data</summary>
	public static SaveFile DefaultNoLoad => GetSaveNoLoad(DefaultPath);
	
	/// <summary>Gets the instance to the save manager node</summary>
	public static SaveManagerNode Instance
	{
		get
		{
			if(instance == null)
			{
				instance = ((SceneTree)Engine.GetMainLoop()).Root.GetNodeOrNull<SaveManagerNode>(typeof(SaveManagerNode).Name);
				if(instance == null)
				{
					instance = new SaveManagerNode();
					instance.Name = typeof(SaveManagerNode).Name;
				}
			}
			
			return instance;
		}
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	#region Resource Saving
	
	/// <summary>Saves the resource</summary>
	/// <param name="path">The path of the resource to save</param>
	/// <param name="resource">The resource to save</param>
	/// <param name="flags">The saving flags used to save the resource with</param>
	/// <typeparam name="T">The type of resource to save</typeparam>
	/// <returns>Returns the possible error that could happen, returns <see cref="Error.Ok"/> if the save was successful</returns>
	public static Error Save<T>(string path, T resource, ResourceSaver.SaverFlags flags = ResourceSaver.SaverFlags.None) where T : Resource
	{
		string filePath = GetResourcePath<T>(path);
		
		return ResourceSaver.Save(resource, filePath, flags);
	}
	
	/// <summary>Loads the resource</summary>
	/// <param name="path">The path of the resource to load</param>
	/// <param name="cacheMode">The cache mode used when loading</param>
	/// <typeparam name="T">The type of resource to load</typeparam>
	/// <returns>Returns the loaded resource</returns>
	public static T Load<T>(string path, ResourceLoader.CacheMode cacheMode = ResourceLoader.CacheMode.Reuse) where T : Resource => Load<T>(path, out _, cacheMode);
	
	/// <summary>Loads the resource</summary>
	/// <param name="path">The path of the resource to load</param>
	/// <param name="error">The error that could happen, returns <see cref="Error.Ok"/> if the load was successful</param>
	/// <param name="cacheMode">The cache mode used when loading</param>
	/// <typeparam name="T">The type of resource to load</typeparam>
	/// <returns>Returns the loaded resource</returns>
	public static T Load<T>(string path, out Error error, ResourceLoader.CacheMode cacheMode = ResourceLoader.CacheMode.Reuse) where T : Resource
	{
		string filePath = GetResourcePath<T>(path);
		
		if(!ResourceLoader.Exists(filePath))
		{
			error = Error.FileNotFound;
			return null;
		}
		
		try
		{
			T resource = ResourceLoader.Load<T>(filePath, cacheMode: cacheMode);
			
			error = Error.Ok;
			
			return resource;
		}
		catch
		{
			error = Error.Failed;
			return null;
		}
	}
	
	/// <summary>Deletes the resource</summary>
	/// <param name="path">The path of the resource to delete</param>
	/// <typeparam name="T">The type of resource to delete</typeparam>
	/// <returns>Returns the error that could happen, returns <see cref="Error.Ok"/> if the delete was successful</returns>
	public static Error Delete<T>(string path) where T : Resource
	{
		string filePath = ProjectSettings.GlobalizePath(GetResourcePath<T>(path));
		
		if(!File.Exists(filePath))
		{
			return Error.FileNotFound;
		}
		
		File.Delete(filePath);
		return Error.Ok;
	}
	
	/// <summary>Defines the resource, saving it only if it hasn't been saved yet</summary>
	/// <param name="path">The path of the resource to define</param>
	/// <param name="resource">The type of resource to define</param>
	/// <param name="flags">The saving flags used to save the resource with</param>
	/// <typeparam name="T">The type of resource to define</typeparam>
	/// <returns>Returns the error that could happen, returns <see cref="Error.Ok"/> if the define was successful</returns>
	public static Error Define<T>(string path, T resource, ResourceSaver.SaverFlags flags = ResourceSaver.SaverFlags.None) where T : Resource => Exists<T>(path) ? Error.AlreadyExists : Save<T>(path, resource, flags);
	
	/// <summary>Finds if the resource exists (saved)</summary>
	/// <param name="path">The path of the resource to define</param>
	/// <typeparam name="T">The type of resource to define</typeparam>
	/// <returns>Returns true if the resource exists</returns>
	public static bool Exists<T>(string path) where T : Resource
	{
		string filePath = GetResourcePath<T>(path);
		
		return ResourceLoader.Exists(filePath);
	}
	
	/// <summary>Updates the resource, only if the resource exists and is not null</summary>
	/// <param name="path">The path of the resource to update</param>
	/// <param name="updater">The updater function that updates the resource</param>
	/// <param name="cacheMode">The cache mode used when loading</param>
	/// <param name="flags">The saving flags used to save the resource with</param>
	/// <typeparam name="T">The type of resource to update</typeparam>
	/// <returns>Returns the updated resource</returns>
	public static T Update<T>(string path, System.Func<T, T> updater, ResourceLoader.CacheMode cacheMode = ResourceLoader.CacheMode.Reuse, ResourceSaver.SaverFlags flags = ResourceSaver.SaverFlags.None) where T : Resource => Update<T>(path, updater, out _, cacheMode, flags);
	
	/// <summary>Updates the resource, only if the resource exists and is not null</summary>
	/// <param name="path">The path of the resource to update</param>
	/// <param name="updater">The updater function that updates the resource</param>
	/// <param name="error">The error that could happen, returns <see cref="Error.Ok"/> if the update was successful</param>
	/// <param name="cacheMode">The cache mode used when loading</param>
	/// <param name="flags">The saving flags used to save the resource with</param>
	/// <typeparam name="T">The type of resource to update</typeparam>
	/// <returns>Returns the updated resource</returns>
	public static T Update<T>(string path, System.Func<T, T> updater, out Error error, ResourceLoader.CacheMode cacheMode = ResourceLoader.CacheMode.Reuse, ResourceSaver.SaverFlags flags = ResourceSaver.SaverFlags.None) where T : Resource
	{
		if(updater == null)
		{
			error = Error.CantResolve;
			return null;
		}
		
		T content = Load<T>(path, out error, cacheMode);
		
		if(error == Error.Ok)
		{
			content = updater(content);
			error = Save<T>(path, content, flags);
		}
		
		return content;
	}
	
	#endregion // Resource Saving
	
	#region ISaveable Saving
	
	public static void SaveToFile<T>(string path, T obj) where T : ISaveable
	{
		SaveFile file = GetSave(path);
		
		obj.SaveToFile(file);
		file.Save();
	}
	
	public static void DefineToFile<T>(string path, T obj) where T : ISaveable
	{
		SaveFile file = GetSave(path);
		
		obj.DefineToFile(file);
		file.Save();
	}
	
	public static void LoadFromFile<T>(string path, T obj) where T : ISaveable
	{
		SaveFile file = GetSave(path);
		
		obj.LoadFromFile(file);
	}
	
	#endregion // ISaveable Saving
	
	/// <summary>Gets the save file</summary>
	/// <param name="path">The path to the save file</param>
	/// <param name="isReadonly">Set to true to make the save file readonly</param>
	/// <param name="autoSave">Set to true to make the save file automatically save once it goes out of scope</param>
	/// <returns>Returns the save file</returns>
	public static SaveFile GetSave(string path, bool isReadonly = false, bool autoSave = false) => GetSaveWithoutCaching(path, isReadonly, autoSave).Cache();
	
	/// <summary>Gets the save file without caching it directly into memory</summary>
	/// <param name="path">The path to the save file</param>
	/// <param name="isReadonly">Set to true to make the save file readonly</param>
	/// <param name="autoSave">Set to true to make the save file automatically save once it goes out of scope</param>
	/// <returns>Returns the save file without caching it directly into memory</returns>
	public static SaveFile GetSaveWithoutCaching(string path, bool isReadonly = false, bool autoSave = false)
	{
		string pattern = @"[\/\\]?([^\/\\]+)$";
		Match match = Regex.Match(path, pattern);
		string fileName = match.Groups[1].Value;
		string subPath = Regex.Replace(path, pattern, "");
		string absolutePath = Path.Combine(RootPath, $"{path}{SaveFileExtension}");
		
		if(Instance != null && Instance.IsCached(absolutePath))
		{
			return Instance.GetCached(absolutePath, isReadonly, autoSave);
		}
		
		SaveFile save = new SaveFile(subPath, fileName, isReadonly, autoSave);
		
		return save.Load();
	}
	
	/// <summary>Gets the save file without loading any of the data in it yet</summary>
	/// <param name="path">The path to the save file</param>
	/// <param name="isReadonly">Set to true to make the save file readonly</param>
	/// <param name="autoSave">Set to true to make the save file automatically save once it goes out of scope</param>
	/// <returns>Returns the save file without loading any of the data in it yet</returns>
	public static SaveFile GetSaveNoLoad(string path, bool isReadonly = false, bool autoSave = false)
	{
		string pattern = @"[\/\\]?([^\/\\]+)$";
		Match match = Regex.Match(path, pattern);
		string fileName = match.Groups[1].Value;
		string subPath = Regex.Replace(path, pattern, "");
		SaveFile save = new SaveFile(subPath, fileName, isReadonly, autoSave);
		
		return save;
	}
	
	/// <summary>Gets the save file being loaded in asynchronously</summary>
	/// <param name="path">The path to the save file</param>
	/// <param name="isReadonly">Set to true to make the save file readonly</param>
	/// <param name="autoSave">Set to true to make the save file automatically save once it goes out of scope</param>
	/// <param name="progress">A callback that gives the current amount of items saved and the total amount of items to be saved</param>
	/// <param name="completed">A callback that gives the loaded save file</param>
	public static void GetSaveAsyncLoaded(string path, bool isReadonly = false, bool autoSave = false, System.Action<int, int> progress = null, System.Action<SaveFile> completed = null)
	{
		string pattern = @"[\/\\]?([^\/\\]+)$";
		Match match = Regex.Match(path, pattern);
		string fileName = match.Groups[1].Value;
		string subPath = Regex.Replace(path, pattern, "");
		string absolutePath = Path.Combine(RootPath, $"{path}{SaveFileExtension}");
		
		if(Instance != null && Instance.IsCached(absolutePath))
		{
			SaveFile cached = Instance.GetCached(absolutePath, isReadonly, autoSave);
			
			progress?.Invoke(1, 1);
			completed?.Invoke(cached);
			return;
		}
		
		SaveFile save = new SaveFile(subPath, fileName, isReadonly, autoSave);
		
		save.LoadAsync(progress, completed);
	}
	
	/// <summary>Deletes the default save</summary>
	public static void DeleteDefaultSave() => DefaultNoLoad.Delete();
	
	/// <summary>Deletes all saves</summary>
	public static void DeleteAllSaves()
	{
		foreach(string file in GetSaveFiles())
		{
			SaveFile save = GetSaveNoLoad(file.Substring(file.Length - SaveFileExtension.Length));
			
			save.Delete();
		}
	}
	
	/// <summary>Gets the list of all the save files</summary>
	/// <returns>Returns the list of all the save files</returns>
	public static List<string> GetSaveFiles() => new List<string>(Directory.GetFiles(RootPath, $"*{SaveFileExtension}", System.IO.SearchOption.AllDirectories));
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Tries to cache the save file into memory</summary>
	/// <param name="save">The save file to cache</param>
	internal static void TryToCache(SaveFile save)
	{
		if(Instance == null)
		{
			GD.PrintErr("Save Manager is not instantiated! Could not cache save file");
			return;
		}
		if(!Instance.cachedFiles.ContainsKey(save.AbsolutePath))
		{
			Instance.cachedFiles.Add(save.AbsolutePath, save);
		}
		else
		{
			Instance.cachedFiles[save.AbsolutePath] = save;
		}
	}
	
	/// <summary>Tries to uncache the save file from memory</summary>
	/// <param name="save">The save file to uncache</param>
	internal static void TryToUncache(SaveFile save)
	{
		if(Instance == null)
		{
			GD.PrintErr("Save Manager is not instantiated! Could not uncache save file");
			return;
		}
		Instance.cachedFiles.Remove(save.AbsolutePath);
	}
	
	/// <summary>Ensures the save path for the given path</summary>
	/// <param name="path">The absolute path to ensure</param>
	internal static void EnsureSavePath(string path)
	{
		if(!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}
	
	/// <summary>Gets the path to the resource save file</summary>
	/// <param name="path">The path to the resource</param>
	/// <returns>Returns the localized path to the resource</returns>
	private static string GetResourcePath<T>(string path) where T : Resource
	{
		string pattern = @"[\/\\]?([^\/\\]+)$";
		Match match = Regex.Match(path, pattern);
		string fileName = match.Groups[1].Value;
		string subPath = Regex.Replace(path, pattern, "");
		string dirPath = Path.Combine(RootPath, subPath);
		
		EnsureSavePath(dirPath);
		
		return Path.Combine(LocalPath, $"{path}.tres");
	}
	
	#endregion // Private Methods
}
