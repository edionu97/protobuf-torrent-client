namespace Torrent.Helpers.ExtensionMethods
{
    public static class MessageExtensions
    {
        /// <summary>
        /// This function it is used as extension method for extracting the proper property value from message
        /// </summary>
        /// <typeparam name="T">type of the property</typeparam>
        /// <param name="message">the message itself</param>
        /// <returns>the property of type T or null otherwise</returns>
        public static T As<T>(this Message message) where T : class
        {
            //iterate through all properties and get the desired one
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var propertyInfo in message.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType != typeof(T))
                {
                    continue;
                }

                return propertyInfo.GetValue(message) as T;
            }

            return null;
        }
    }
}
