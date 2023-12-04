using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PiRhoSoft.Utilities.Editor
{
	public class AssetList
	{
		public AssetList(Type type)
		{
			Type = type;
		}

		public Type Type { get; private set; }

		public List<Object> Assets;
		public List<string> Paths;
	}

	public class AssetHelper : AssetPostprocessor
	{
		private const string INVALID_PATH_ERROR = "(UAHIP) failed to create asset at path {0}: the path must be inside the 'Assets' folder for this project";

		private static readonly Dictionary<string, AssetList> _assetLists = new Dictionary<string, AssetList>();

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			_assetLists.Clear(); // no reason to figure out what actually changed - just clear all the lists so they are rebuilt on next access
		}

		#region Creation

		public static TObject Create<TObject>() where TObject : ScriptableObject
		{
			return (TObject)Create(typeof(TObject));
		}
		
		public static Object Create(Type createType)
		{
			var title = $"Create a new {createType.Name}";
			var path = EditorUtility.SaveFilePanel(title, "Assets", $"{createType.Name}.asset", "asset");
		
			if (!string.IsNullOrEmpty(path))
			{
				var asset = CreateAssetAtPath(path, createType);
		
				if (asset == null)
                {
                    Debug.LogErrorFormat(INVALID_PATH_ERROR, path);
                }

                return asset;
			}
		
			return null;
		}
		
		public static TAsset CreateAsset<TAsset>(string name) where TAsset : ScriptableObject
		{
			return CreateAsset(name, typeof(TAsset)) as TAsset;
		}
		
		public static TAsset GetOrCreateAsset<TAsset>(string name) where TAsset : ScriptableObject
		{
			var asset = GetAsset<TAsset>();
		
			if (asset == null)
            {
                asset = CreateAsset<TAsset>(name);
            }

            return asset;
		}
		
		public static ScriptableObject CreateAsset(string name, Type type)
		{
			var asset = ScriptableObject.CreateInstance(type);
			var path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + name + ".asset");
		
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
		
			return asset;
		}
		
		public static ScriptableObject GetOrCreateAsset(string name, Type type)
		{
			var asset = GetAsset(type) as ScriptableObject;
		
			if (asset == null)
            {
                asset = CreateAsset(name, type);
            }

            return asset;
		}
		
		public static ScriptableObject CreateAssetAtPath(string path, Type type)
		{
			if (!path.StartsWith(Application.dataPath))
            {
                return null;
            }

            path = path.Substring(Application.dataPath.Length - 6); // keep 'Assets' as the root folder
		
			var asset = ScriptableObject.CreateInstance(type);
		
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
		
			return asset;
		}

		#endregion

		#region Lookup

		public static TAsset GetAsset<TAsset>() where TAsset : Object
		{
			return GetAssets<TAsset>().FirstOrDefault();
		}

		public static TAsset GetAssetWithId<TAsset>(string id) where TAsset : Object
		{
			var path = AssetDatabase.GUIDToAssetPath(id);
			return GetAssetAtPath<TAsset>(path);
		}

		public static TAsset GetAssetAtPath<TAsset>(string path) where TAsset : Object
		{
			return AssetDatabase.LoadAssetAtPath<TAsset>(path);
		}

		public static Object GetAsset(Type assetType)
		{
			return GetAssets(assetType).FirstOrDefault();
		}

		public static Object GetAssetWithId(string id, Type type)
		{
			var path = AssetDatabase.GUIDToAssetPath(id);
			return GetAssetAtPath(path, type);
		}

		public static Object GetAssetAtPath(string path, Type type)
		{
			return AssetDatabase.LoadAssetAtPath(path, type) as Object;
		}

		#endregion

		#region Listing

		public static IEnumerable<TAsset> GetAssets<TAsset>() where TAsset : Object
		{
			return GetAssets(typeof(TAsset)).Select(asset => asset as TAsset);
		}

		public static IEnumerable<Object> GetAssets(Type assetType)
		{
			return AssetDatabase.FindAssets($"t:{assetType.Name}").Select(id => GetAssetWithId(id, assetType)).Where(asset => asset);
		}

		public static AssetList GetAssetList<TAsset>() where TAsset : Object
		{
			return GetAssetList(typeof(TAsset));
		}

		public static AssetList GetAssetList(Type assetType)
		{
			var listName = assetType.AssemblyQualifiedName;

			if (!_assetLists.TryGetValue(listName, out var list))
			{
				list = new AssetList(assetType);

				var assets = GetAssets(assetType);
				var paths = assets.Select(GetPath);
				var prefix = FindCommonPath(paths);

				list.Assets = assets.ToList();
				list.Paths = assets.Select(asset =>
				{
					var path = GetPath(asset).Substring(prefix.Length);
					return path.Length > 0 ? path + asset.name : asset.name;
				}).ToList();

				_assetLists.Add(listName, list);
			}

			return list;
		}

		#endregion

		#region Helpers

		public static string GetPath(Object asset)
		{
			var path = AssetDatabase.GetAssetPath(asset);
			var slash = path.LastIndexOf('/');

			return path.Substring(0, slash + 1);
		}

		public static string FindCommonPath(IEnumerable<string> paths)
		{
			var prefix = paths.FirstOrDefault() ?? string.Empty;

			foreach (var path in paths)
			{
				var index = 0;
				var count = Math.Min(prefix.Length, path.Length);

				for (; index < count; index++)
				{
					if (prefix[index] != path[index])
                    {
                        break;
                    }
                }

				prefix = prefix.Substring(0, index);

				var slash = prefix.LastIndexOf('/');
				if (slash != prefix.Length - 1)
                {
                    prefix = slash >= 0 ? prefix.Substring(0, slash + 1) : string.Empty;
                }
            }

			return prefix;
		}

		private static readonly Regex _assetPathExpression = new Regex(@".*/(Assets/.*/)[^/]+", RegexOptions.Compiled);
		private static readonly Regex _packagePathExpression = new Regex(@".*/PackageCache(/[^/]+)@[^/]+(/.*/)[^/]+", RegexOptions.Compiled);
        
        private const string PACKAGE_FORMAT = "Packages{0}{1}";
        private const string UNKNOWN_PATH = "UNKNOWN";

        public static string GetScriptPath([CallerFilePath] string filename = "")
		{
			return GetAssetPath(filename);
		}

		public static string GetAssetPath(string fullPath)
		{
			var normalized = fullPath.Replace('\\', '/');

			var assetMatch = _assetPathExpression.Match(normalized);
			if (assetMatch.Success)
            {
                return assetMatch.Groups[1].Value;
            }

            var packageMatch = _packagePathExpression.Match(normalized);
			if (packageMatch.Success)
            {
                return string.Format(PACKAGE_FORMAT, packageMatch.Groups[1].Value, packageMatch.Groups[2].Value);
            }

            return UNKNOWN_PATH;
		}

		#endregion
	}
}
