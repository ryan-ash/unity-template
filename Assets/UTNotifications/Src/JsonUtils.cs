using System.Collections.Generic;

namespace UTNotifications
{
    public sealed class JsonUtils
    {
        public static JSONArray ToJson(ICollection<Button> buttons)
        {
            if (buttons == null || buttons.Count == 0)
            {
                return null;
            }

            JSONArray json = new JSONArray();

            foreach (Button it in buttons)
            {
                JSONClass button = new JSONClass();
                button.Add("title", it.title);

                JSONNode userData = ToJson(it.userData);
                if (userData != null)
                {
                    button.Add("userData", userData);
                }

                json.Add(button);
            }

            return json;
        }

        public static JSONNode ToJson(IDictionary<string, string> userData)
        {
            if (userData == null || userData.Count == 0)
            {
                return null;
            }

            JSONClass json = new JSONClass();
            foreach (KeyValuePair<string, string> it in userData)
            {
                json.Add(it.Key, new JSONData(it.Value));
            }

            return json;
        }
    }
}