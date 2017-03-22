using System;
 
namespace WEB.API
{
	public class EnumExtensions
	{
		public static T ParseEnum<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value, true);
		}
	}
}