
namespace FLCore.SaveLoad;

using FLCore;

using Godot;

using Newtonsoft.Json;

using System.Collections;

using File = System.IO.File;
using Path = System.IO.Path;

/// <summary>A class that holds the details of the save file</summary>
public sealed class SaveFile
{
	#region Properties
	
	/// <summary>Gets the database for the save file</summary>
	public SaveTable Database { get; private set; } = new SaveTable();
	
	/// <summary>Gets and sets if the save files saves automatically once it goes out of scope</summary>
	public bool AutoSave { get; set; } = false;
	
	/// <summary>Gets if the save file is readonly</summary>
	public bool IsReadonly { get; private set; } = false;
	
	/// <summary>Gets the path to the save file relative to the <see cref="SaveManager.RootPath"/></summary>
	public string FilePath { get; private set; }
	
	/// <summary>Gets the name of the file</summary>
	public string FileName { get; private set; }
	
	/// <summary>Gets the absolute path of the file</summary>
	public string AbsolutePath => Path.Combine(SaveManager.RootPath, this.FilePath, $"{this.FileName}{SaveManager.SaveFileExtension}");
	
	/// <summary>Gets if the save file is cached into memory</summary>
	public bool IsCached => SaveManager.Instance != null && SaveManager.Instance.IsCached(this.AbsolutePath);
	
	/// <summary>A base constructor that gets a save file, unloaded</summary>
	/// <param name="path">The path to the file relative to the <see cref="SaveManager.RootPath"/></param>
	/// <param name="fileName">The name of the file</param>
	/// <param name="isReadonly">Set to true to make the save file readonly</param>
	/// <param name="autoSave">Set to true to have the file automatically save after it goes out of scope</param>
	public SaveFile(string path, string fileName, bool isReadonly = false, bool autoSave = false)
	{
		this.FilePath = path;
		this.FileName = fileName;
		this.IsReadonly = false;
		this.AutoSave = false;
	}
	
	/// <summary>A constructor to copy over a save file</summary>
	/// <param name="file">The save file to clone</param>
	public SaveFile(SaveFile file) : this(file.FilePath, file.FileName, file.IsReadonly, file.AutoSave)
	{
		this.Database = new SaveTable(file.Database);
	}
	
	/// <summary>A destructor that automatically saves if <see cref="AutoSave"/> is set to true</summary>
	~SaveFile()
	{
		if(this.AutoSave)
		{
			GD.Print("Save instance going out of scope, auto saving");
			this.Save();
		}
	}
	
	#endregion // Properties
	
	#region Public Methods
	
	/// <summary>Clones the file as a readonly version of itself</summary>
	/// <returns>Returns a cloned version of the file as a readonly version</returns>
	public SaveFile AsReadonly()
	{
		SaveFile readonlyFile = new SaveFile(this);
		
		readonlyFile.IsReadonly = true;
		
		return readonlyFile;
	}
	
	/// <summary>Clones the file as an auto save version of itself</summary>
	/// <returns>Returns a cloned version of the file as an auto save version</returns>
	public SaveFile WithAutoSave()
	{
		SaveFile autoSaveFile = new SaveFile(this);
		
		autoSaveFile.AutoSave = true;
		
		return autoSaveFile;
	}
	
	/// <summary>Caches the save file into memory</summary>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Cache()
	{
		SaveManager.TryToCache(this);
		return this;
	}
	
	/// <summary>Uncaches the save file from memory</summary>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Uncache()
	{
		SaveManager.TryToUncache(this);
		return this;
	}
	
	/// <summary>Saves the save file</summary>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Save()
	{
		if(this.IsReadonly)
		{
			GD.PrintErr("Cannot save using a readonly save file!");
			return this;
		}
		try
		{
			SaveManager.EnsureSavePath(
				string.IsNullOrEmpty(this.FilePath)
					? SaveManager.RootPath
					: Path.Combine(SaveManager.RootPath, this.FilePath)
			);
			this.Database.Save(this.AbsolutePath);
			SaveManager.isCacheDirty = true;
		}
		catch(System.Exception e)
		{
			GD.PrintErr($"Could not save file:\n{e}");
		}
		return this;
	}
	
	/// <summary>Loads the save file into this class</summary>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Load()
	{
		try
		{
			SaveManager.EnsureSavePath(
				string.IsNullOrEmpty(this.FilePath)
					? SaveManager.RootPath
					: Path.Combine(SaveManager.RootPath, this.FilePath)
			);
			this.Database.Load(this.AbsolutePath);
		}
		catch(System.Exception e)
		{
			GD.PrintErr($"Could not load save file:\n{e}");
		}
		return this;
	}
	
	/// <summary>Deletes the save file</summary>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Delete()
	{
		if(this.IsReadonly)
		{
			GD.PrintErr("Cannot delete a readonly save file!");
			return this;
		}
		if(File.Exists(this.AbsolutePath))
		{
			GD.Print($"Deleting save file @{this.AbsolutePath}");
			File.Delete(this.AbsolutePath);
		}
		this.Database.Clear();
		return this;
	}
	
	/// <summary>Gets a save file from a subfolder of the this file name</summary>
	/// <param name="path">The path to the new save file</param>
	/// <param name="asReadonly">Set to true to make the save file readonly</param>
	/// <param name="autoSave">Set to true to make the save file automatically save after it goes out of scope</param>
	/// <returns>Returns the child save file</returns>
	public SaveFile GetChild(string path, bool? asReadonly = null, bool? autoSave = null)
	{
		if(path.StartsWith("./"))
		{
			return this.GetSibling(path.Substring(2), asReadonly ?? this.IsReadonly, autoSave ?? this.AutoSave);
		}
		
		return SaveManager.GetSave(
			Path.Combine(this.FilePath, this.FileName, path),
			asReadonly ?? this.IsReadonly,
			autoSave ?? this.AutoSave
		);
	}
	
	/// <summary>Gets a save file from the same folder as this save file</summary>
	/// <param name="path">The path to the new save file</param>
	/// <param name="asReadonly">Set to true to make the save file readonly</param>
	/// <param name="autoSave">Set to true to make the save file automatically save after it goes out of scope</param>
	/// <returns>Returns the sibling save file</returns>
	public SaveFile GetSibling(string path, bool? asReadonly = null, bool? autoSave = null)
	{
		return SaveManager.GetSave(
			Path.Combine(this.FilePath, path),
			asReadonly ?? this.IsReadonly,
			autoSave ?? this.AutoSave
		);
	}
	
	/// <summary>Save the save file asynchronously</summary>
	/// <param name="progress">A callback that gives the current amount of items saved and the total amount of items to be saved</param>
	/// <param name="saved">A callback that gives the completed save file, once it's done with the save</param>
	/// <returns>Returns the handle to the coroutine</returns>
	public CoroutineHandle SaveAsync(System.Action<int, int> progress, System.Action<SaveFile> saved)
	{
		if(this.IsReadonly)
		{
			GD.PrintErr("Cannot save using a readonly save file!");
			return default(CoroutineHandle);
		}
		SaveManager.EnsureSavePath(
			string.IsNullOrEmpty(this.FilePath)
				? SaveManager.RootPath
				: Path.Combine(SaveManager.RootPath, this.FilePath)
		);
		return Timing.RunCoroutine(this.Database.SaveAsync(
			this.AbsolutePath,
			progress,
			() => {
				SaveManager.isCacheDirty = true;
				saved?.Invoke(this);
			}
		));
	}
	
	/// <summary>Loads the save file asynchronously</summary>
	/// <param name="progress">A callback that gives the current amount of items loaded and the total amount of items to be loaded</param>
	/// <param name="loaded">A callback that gives the completed save file, once it's done with the load</param>
	/// <returns>Returns the handle to the coroutine</returns>
	public CoroutineHandle LoadAsync(System.Action<int, int> progress, System.Action<SaveFile> loaded)
	{
		SaveManager.EnsureSavePath(
			string.IsNullOrEmpty(this.FilePath)
				? SaveManager.RootPath
				: Path.Combine(SaveManager.RootPath, this.FilePath)
		);
		return Timing.RunCoroutine(this.Database.LoadAsync(
			this.AbsolutePath,
			progress,
			() => loaded?.Invoke(this)
		));
	}
	
	/// <summary>Defines the data point without overriding any data in that data point that already exists</summary>
	/// <param name="id">The ID of the data point</param>
	/// <param name="data">The data to define with</param>
	/// <typeparam name="T">The type of the data point</typeparam>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Define<T>(string id, T data) => this.Define(typeof(T), id, data);
	
	/// <summary>Defines the data point without overriding any data in that data point that already exists</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="id">The ID of the data point</param>
	/// <param name="data">The data to define with</param>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Define(System.Type type, string id, object data)
	{
		if(this.IsReadonly)
		{
			GD.PrintErr("Cannot define data in a readonly save file!");
			return this;
		}
		if(!this.IsAppropriateDictionaryType(type, data))
		{
			GD.PrintErr($"Type is using an inappropriate dictionary type: {type}");
			return this;
		}
		
		string key = this.GetKey(type, id);
		
		if(!this.Database.ContainsKey(key))
		{
			this.Database.Add(key, data);
		}
		else if(this.Database[key] is null)
		{
			this.Database[key] = data;
		}
		
		return this;
	}
	
	/// <summary>Set the data point with data, overriding any data that already exists in this data point</summary>
	/// <param name="id">The ID of the data point</param>
	/// <param name="data">The data to set with</param>
	/// <typeparam name="T">The type of the data point</typeparam>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Set<T>(string id, T data) => this.Set(typeof(T), id, data);
	
	/// <summary>Set the data point with data, overriding any data that already exists in this data point</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="id">The ID of the data point</param>
	/// <param name="data">The data to set with</param>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Set(System.Type type, string id, object data)
	{
		if(this.IsReadonly)
		{
			GD.PrintErr("Cannot set data in a readonly save file!");
			return this;
		}
		if(!this.IsAppropriateDictionaryType(type, data))
		{
			GD.PrintErr($"Type is using an inappropriate dictionary type: {type}");
			return this;
		}
		
		string key = this.GetKey(type, id);
		
		if(!this.Database.ContainsKey(key))
		{
			this.Database.Add(key, data);
		}
		else
		{
			this.Database[key] = data;
		}
		
		return this;
	}
	
	/// <summary>Gets the data point while returning the save file to chain together methods</summary>
	/// <param name="id">The ID of the data point</param>
	/// <param name="data">The data to output</param>
	/// <param name="defaultData">The default data to return if the data point doesn't exist</param>
	/// <typeparam name="T">The type of the data point</typeparam>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Get<T>(string id, out T data, T defaultData = default(T))
	{
		data = this.Get<T>(id, defaultData);
		return this;
	}
	
	/// <summary>Gets the data point while returning the save file to chain together methods</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="id">The ID of the data point</param>
	/// <param name="data">The data to output</param>
	/// <param name="defaultData">The default data to return if the data point doesn't exist</param>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Get(System.Type type, string id, out object data, object defaultData = null)
	{
		data = this.Get(type, id, defaultData);
		return this;
	}
	
	/// <summary>Gets and returns the data point</summary>
	/// <param name="id">The ID of the data point</param>
	/// <param name="defaultData">The default data to return if the data point doesn't exist</param>
	/// <typeparam name="T">The type of the data point</typeparam>
	/// <returns>Returns the data point</returns>
	public T Get<T>(string id, T defaultData = default(T)) => (T)this.Get(typeof(T), id, defaultData);
	
	/// <summary>Gets and returns the data point</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="id">The ID of the data point</param>
	/// <param name="defaultData">The default data to return if the data point doesn't exist</param>
	/// <returns>Returns the data point</returns>
	public object Get(System.Type type, string id, object defaultData = null)
	{
		string key = this.GetKey(type, id);
		
		if(!this.Database.ContainsKey(key) || this.Database[key] is null)
		{
			return defaultData;
		}
		
		if(type.IsArray)
		{
			System.Array arr = this.Database[key] as System.Array;
			System.Array converted = System.Array.CreateInstance(type.GetElementType(), arr.Length);
			
			System.Array.Copy(arr, converted, arr.Length);
			
			return (object)converted;
		}
		if(type.IsPrimitive)
		{
			return System.Convert.ChangeType(this.Database[key], type);
		}
		return this.Database[key];
	}
	
	/// <summary>Updates the data point, if the data point exists and is not null</summary>
	/// <param name="id">The ID of the data point</param>
	/// <param name="updater">A callback that gives the data point and expects to return a modifies version of that data point</param>
	/// <typeparam name="T">The type of the data point</typeparam>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Update<T>(string id, System.Func<T, T> updater)
	{
		if(this.IsReadonly)
		{
			GD.PrintErr("Cannot update data in a readonly save file!");
			return this;
		}
		string key = this.GetKey(typeof(T), id);
		
		if(this.Database.ContainsKey(key) && !(this.Database[key] is null))
		{
			if(updater != null)
			{
				this.Set<T>(id, updater((T)this.Database[key]));
			}
		}
		else
		{
			GD.PrintErr($"No data to be updated for [{typeof(T)} : {id}]");
		}
		
		return this;
	}
	
	/// <summary>Updates the data point, if the data point exists and is not null</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="id">The ID of the data point</param>
	/// <param name="updater">A callback that gives the data point and expects to return a modifies version of that data point</param>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Update(System.Type type, string id, System.Func<object, object> updater)
	{
		if(this.IsReadonly)
		{
			GD.PrintErr("Cannot update data in a readonly save file!");
			return this;
		}
		string key = this.GetKey(type, id);
		
		if(this.Database.ContainsKey(key) && !(this.Database[key] is null))
		{
			if(updater != null)
			{
				this.Set(type, id, updater(this.Database[key]));
			}
		}
		else
		{
			GD.PrintErr($"No data to be updated for [{type} : {id}]");
		}
		
		return this;
	}
	
	/// <summary>Updates/inserts the data point, even if the data point doesn't exist and is not null</summary>
	/// <param name="id">The ID of the data point</param>
	/// <param name="updater">A callback that gives the data point (could be null) and expects to return a modified version of that data point</param>
	/// <typeparam name="T">The type of the data point</typeparam>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Upsert<T>(string id, System.Func<T, T> updater)
	{
		if(this.IsReadonly)
		{
			GD.PrintErr("Cannot upsert data in a readonly save file!");
			return this;
		}
		string key = this.GetKey(typeof(T), id);
		
		if(this.Database.ContainsKey(key) && !(this.Database[key] is null))
		{
			if(updater != null)
			{
				this.Set<T>(id, updater((T)this.Database[key]));
			}
		}
		else
		{
			if(updater != null)
			{
				this.Set<T>(id, updater(default(T)));
			}
		}
		
		return this;
	}
	
	/// <summary>Updates/inserts the data point, even if the data point doesn't exist and is not null</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="id">The ID of the data point</param>
	/// <param name="updater">A callback that gives the data point (could be null) and expects to return a modified version of that data point</param>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Upsert(System.Type type, string id, System.Func<object, object> updater)
	{
		if(this.IsReadonly)
		{
			GD.PrintErr("Cannot upsert data in a readonly save file!");
			return this;
		}
		string key = this.GetKey(type, id);
		
		if(this.Database.ContainsKey(key) && !(this.Database[key] is null))
		{
			if(updater != null)
			{
				this.Set(type, id, updater(this.Database[key]));
			}
		}
		else
		{
			if(updater != null)
			{
				this.Set(type, id, updater(null));
			}
		}
		
		return this;
	}
	
	/// <summary>Removes the data point</summary>
	/// <param name="id">The ID of the data point</param>
	/// <typeparam name="T">The type of the data point</typeparam>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Remove<T>(string id) => this.Remove(typeof(T), id);
	
	/// <summary>Removes the data point</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="id">The ID of the data point</param>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Remove(System.Type type, string id)
	{
		if(this.IsReadonly)
		{
			GD.PrintErr("Cannot remove data in a readonly save file!");
			return this;
		}
		string key = this.GetKey(type, id);
		
		if(this.Database.ContainsKey(key))
		{
			this.Database.Remove(key);
		}
		else
		{
			GD.PrintErr($"No data to be deleted for [{type} : {id}]");
		}
		return this;
	}
	
	/// <summary>Checks to see if the data point exists and is not null</summary>
	/// <param name="id">The ID of the data point</param>
	/// <param name="hasData">The boolean that gets outputted to see if the data exists</param>
	/// <typeparam name="T">The type of the data point</typeparam>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Check<T>(string id, out bool hasData)
	{
		hasData = this.Check<T>(id);
		return this;
	}
	
	/// <summary>Checks to see if the data point exists and is not null</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="id">The ID of the data point</param>
	/// <param name="hasData">The boolean that gets outputted to see if the data exists</param>
	/// <returns>Returns this save file to chain together methods</returns>
	public SaveFile Check(System.Type type, string id, out bool hasData)
	{
		hasData = this.Check(type, id);
		return this;
	}
	
	/// <summary>Checks to see if the data point exists and is not null</summary>
	/// <param name="id">The ID of the data point</param>
	/// <typeparam name="T">The type of the data point</typeparam>
	/// <returns>Returns true if the data point exists and is not null</returns>
	public bool Check<T>(string id) => this.Check(typeof(T), id);
	
	/// <summary>Checks to see if the data point exists and is not null</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="id">The ID of the data point</param>
	/// <returns>Returns true if the data point exists and is not null</returns>
	public bool Check(System.Type type, string id)
	{
		string key = this.GetKey(type, id);
		
		return this.Database.ContainsKey(key) && !(this.Database[key] is null);
	}
	
	#endregion // Public Methods
	
	#region Private Methods
	
	/// <summary>Gets the key using the type and id</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="id">The ID of the data point</param>
	/// <returns>Returns the key from the type and id</returns>
	private string GetKey(System.Type type, string id) =>$"{SaveTable.TrimFullName(type.AssemblyQualifiedName)}:{id}";
	
	/// <summary>Stringifies the object</summary>
	/// <param name="value">The object to stringify</param>
	/// <returns>Returns the stringify version of the object</returns>
	private string Stringify(object value)
	{
		if(value is null) { return "null"; }
		if(value is bool) { return value.ToString().ToLower(); }
		if(
			value is byte
			|| value is sbyte
			|| value is ushort
			|| value is short
			|| value is uint
			|| value is int
			|| value is ulong
			|| value is long
			|| value is float
			|| value is double
			|| value is decimal
			|| value is char
		)
		{
			return value.ToString();
		}
		if(value is string)
		{
			return $"\"{value.ToString().Replace("\"", "\\\"")}\"";
		}
		return JsonConvert.SerializeObject(value);
	}
	
	/// <summary>Gets if the dictionary is appropriate to use</summary>
	/// <param name="type">The type of the data point</param>
	/// <param name="obj">The data itself</param>
	/// <returns>Returns true if the given object is using the correct dictionary type</returns>
	private bool IsAppropriateDictionaryType(System.Type type, object obj)
	{
		if(obj as IDictionary != null)
		{
			System.Type[] types = type.GetGenericArguments();
			
			// Assume hashtable
			if(types.Length == 0) { return true; }
			if(types[0] == typeof(bool)) { return true; }
			if(types[0] == typeof(byte)) { return true; }
			if(types[0] == typeof(sbyte)) { return true; }
			if(types[0] == typeof(ushort)) { return true; }
			if(types[0] == typeof(short)) { return true; }
			if(types[0] == typeof(uint)) { return true; }
			if(types[0] == typeof(int)) { return true; }
			if(types[0] == typeof(ulong)) { return true; }
			if(types[0] == typeof(long)) { return true; }
			if(types[0] == typeof(float)) { return true; }
			if(types[0] == typeof(double)) { return true; }
			if(types[0] == typeof(string)) { return true; }
			if(types[0] == typeof(char)) { return true; }
			
			return false;
		}
		
		return true;
	}
	
	#endregion // Private Methods
}
