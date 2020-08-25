using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing;

namespace CounterApp
{
    public class tableFunctions
    {
        public static HttpClient client = new HttpClient();
        public static async Task<string> delete_from_table(String tableName, String field, String columnName)
        {
            string request = tableName + "$" + field + "$" + columnName;
            string url =  "https://counterfunctions20200502095136.azurewebsites.net/api/delete-from-table/" + request;
            var getStringTask = await client.GetStringAsync(url);
            string response = getStringTask.ToString()/*.Result*/;
            return response;
        }

        public static async Task<bool> add_to_last_activity_list(string tableName, string username,string actionAndQuantity,string product)
        {
            string request = tableName + "$" + username + "$" + actionAndQuantity + "$" + product;
            string url = "https://counterfunctions20200502095136.azurewebsites.net/api/add-to-last-activity-list/" + request;
            var getStringTask = await client.GetStringAsync(url);
            string response = getStringTask.ToString()/*.Result*/;
            return true;
        }

        
        public static async Task<bool> add_to_table(String table_type, String tableName, List<String> arg)
        {
            string request = table_type + "$" + tableName;
            for (int i = 0; i < arg.Count; i++)
            {
                request = request + "$" + arg[i];
            }
            string url = "https://counterfunctions20200502095136.azurewebsites.net/api/add-to-table/" + request;
            var getStringTask = await client.GetStringAsync(url);
            return true;
        }

        public static async Task<bool> new_tables(String FName, String Uname)
        {
            string request = FName;
            string url = "https://counterfunctions20200502095136.azurewebsites.net/api/new-tables/" + request;
            var getStringTask = await client.GetStringAsync(url);

            List<String> arg = new List<String>();
            arg.Add(Uname);
            arg.Add("");
            bool result = await add_to_table("usersInFridge", FName + "Users", arg);

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            arg.Clear();
            arg.Add(FName);
            arg.Add(new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray()));
            result = await add_to_table("allRefrigirators", "FridgesTable", arg);
            return result;
        }

        public static async Task<List<String>> get_column(String tableName, String columnName)
        {
            string request = tableName + "$" + columnName; ;
            string url = "https://counterfunctions20200502095136.azurewebsites.net/api/get-column/" + request;
            var getStringTask = await client.GetStringAsync(url);
            String responseContent = getStringTask.ToString();
            List<string> Result = JsonConvert.DeserializeObject<List<string>>(responseContent);
            return Result;
        }

        public static async Task<String> get_value(String tableName, String columnName, String rowName) //TODO: change name to get_value
        {
            string request = tableName + "$" + columnName + "$" + rowName;
            string url = "https://counterfunctions20200502095136.azurewebsites.net/api/get-value/" + request;
            var getStringTask = await client.GetStringAsync(url);
            string responseContent = getStringTask.ToString();
            return responseContent;
        }

        public static async Task<bool> table_exists(String tableName)
        {
            string request = tableName;
            string url = "https://counterfunctions20200502095136.azurewebsites.net/api/table-exists/" + request;
            var getStringTask = await client.GetStringAsync(url);
            string responseContent = getStringTask.ToString();
            if (responseContent.Equals("true"))
            {
                return true;
            }
            return false;
        }

        public static async Task<bool> set_value(String tableName, String columnName, String rowName, String newValue)
        {
            string request = tableName + "$" + columnName + "$" + rowName + "$" + newValue;
            string url = "https://counterfunctions20200502095136.azurewebsites.net/api/set-value/" + request;
            var getStringTask = await client.GetStringAsync(url);
            string responseContent = getStringTask.ToString();
            return true;
        }    
    }
}
