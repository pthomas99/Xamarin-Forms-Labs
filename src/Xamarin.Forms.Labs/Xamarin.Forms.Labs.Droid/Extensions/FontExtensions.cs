﻿using System;
using Android.Graphics;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using Xamarin.Forms.Labs.Services;
using System.Collections.Generic;

namespace Xamarin.Forms.Labs.Droid
{

	public interface ITypefaceCache{
		void StoreTypeface(string key,Typeface typeface);
		void RemoveTypeface(string key);
		Typeface RetrieveTypeface(string key);
	}

	public static class TypefaceCache{
		private static ITypefaceCache _sharedCache;
		public static ITypefaceCache SharedCache {
			get {
				if(_sharedCache == null) {
					_sharedCache = new DefaultTypefaceCache();
				}
				return _sharedCache;
			}
			set{
				if(_sharedCache != null && _sharedCache.GetType() == typeof(DefaultTypefaceCache)){
					((DefaultTypefaceCache)_sharedCache).PurgeCache();
				}
				_sharedCache = value;
			}
		}




	}
	internal class DefaultTypefaceCache : ITypefaceCache{
		private Dictionary<string,Typeface> _cacheDict;

		public DefaultTypefaceCache(){
			_cacheDict = new Dictionary<string, Typeface>();
		}


		public Typeface RetrieveTypeface(string key){
			if(_cacheDict.ContainsKey(key)){
				return _cacheDict[key];
			}else{
				return null;
			}
		}
		public void StoreTypeface(string key,Typeface typeface){
			_cacheDict[key] = typeface;
		}
		public void RemoveTypeface(string key){
			_cacheDict.Remove(key);
		}

		public void PurgeCache(){
			_cacheDict = new Dictionary<string, Typeface>();
		}



	}




	public static class FontExtensions
	{
		/**
		 * This method returns typeface for given typeface using following rules:
		 * 1. Lookup in the cache
		 * 2. If not found, look in the assets in the fonts folder. Save your font under its FontFamily name. 
		 * If no extension is written in the family name .ttf is asumed
		 * 3. If not found look in the files under fonts/ folder
		 * If no extension is written in the family name .ttf is asumed
		 * 4. If not found, try to return typeface from Xamarin.Forms ToTypeface() method
		 * 5. If not successfull, return Typeface.Default
		 * 
		 * Result is stored in TypefaceCache for performance and memory reasons.
		 */
		public static Typeface ToExtendedTypeface(this Font font,Context context){
			Typeface typeface = null;

			//1. Lookup in the cache
			var hashKey = font.ToHasmapKey();
			typeface = TypefaceCache.SharedCache.RetrieveTypeface(hashKey);
			#if DEBUG
			if(typeface != null)
				Console.WriteLine("Typeface for font {0} found in cache",font);
			#endif

			//2. If not found, try custom asset folder
			if (typeface == null && !string.IsNullOrEmpty(font.FontFamily))
			{
				string filename = font.FontFamily;
				//if no extension given then assume and add .ttf
				if (filename.LastIndexOf(".", System.StringComparison.Ordinal) != filename.Length - 4)
				{
					filename = string.Format("{0}.ttf", filename);
				}
				try
				{
					var path =  "fonts/" + filename;
					#if DEBUG
					Console.WriteLine("Lookking for font file: {0}",path);
					#endif
					typeface = Typeface.CreateFromAsset(context.Assets,path);
					#if DEBUG
					Console.WriteLine ("Found in assets and cached.");
					#endif
				}
				catch (Exception ex)
				{
					#if DEBUG
					Console.WriteLine("not found in assets. Exception: {0}", ex);
					Console.WriteLine("Trying creation from file");
					#endif
					try
					{
						typeface = Typeface.CreateFromFile("fonts/" + filename);
						#if DEBUG
						Console.WriteLine ("Found in file and cached.");
						#endif
					}
					catch (Exception ex1)
					{
						#if DEBUG
						Console.WriteLine("not found by file. Exception: {0}", ex1);
						Console.WriteLine("Trying creation using Xamarin.Forms implementation");
						#endif

					}
				}

			}
			//3. If not found, fall back to default Xamarin.Forms implementation to load system font
			if(typeface == null){
				typeface = font.ToTypeface();
			}

			if(typeface == null){
				#if DEBUG
				Console.WriteLine ("Falling back to default typeface");
				#endif
				typeface =  Typeface.Default;
			}
			//Store in cache
			TypefaceCache.SharedCache.StoreTypeface(hashKey, typeface);

			return typeface;

		} 


		private static string ToHasmapKey(this Font font){
			return string.Format("{0}.{1}.{2}.{3}", font.FontFamily, font.FontSize, font.NamedSize, (int)font.FontAttributes);
				//return string.Equals(this.FontFamily, other.FontFamily) 
				//&& this.FontSize.Equals(other.FontSize) 
				//&& this.NamedSize == other.NamedSize 
				//&& this.FontAttributes == other.FontAttributes;
		}
	}
}

