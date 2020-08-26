using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Reflection.Metadata.Ecma335;
using System.Drawing.Printing;
//using Microsoft.Azure.Management.ContainerRegistry.Fluent;

namespace CounterFunctions
{
    public static class CounterFunctions
    {
        static ServiceClient serviceClient;

        static string ctd_connectionString = "HostName=skeleton-michael.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=LmmZcwXs9ODxlSQzQNNZh49st2QWtuSYGxBUiakyTnI=";

        public static int GLOBAL_COUNTER = 0;
        private const string c_eventHubConnectionString = "eventHubConnectionString";
        private const string c_tableName = "myTable";
        private const string c_eventHubName = "skeleton";
        private const string c_signalRConnectionString = "AzureSignalRConnectionString";
        private static readonly AzureSignalR SignalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));



        [FunctionName("delete-from-table")]
        public static async Task<string> DeleteFromTable(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "delete-from-table/{id}")] HttpRequestMessage request,
[Table(c_refrigiratorTableName)] CloudTable cloudTable,
[SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
string id,
ILogger log)
        {
            //table name$field$columnname
            string remaining_text = id;
            int end_of_table_name = remaining_text.IndexOf('$');
            string table_name = remaining_text.Substring(0, end_of_table_name);
            remaining_text = remaining_text.Substring(end_of_table_name + 1);
            int end_of_field = remaining_text.IndexOf('$');
            string field = remaining_text.Substring(0, end_of_field);
            string column_name = remaining_text.Substring(end_of_field + 1);

            /*
            Uri table_address = new Uri(table_name);
            CloudTable current_table = new CloudTable(table_address);
            */
            var storageAccount = new CloudStorageAccount(new StorageCredentials("skeletonstorage9", "JkIOaYdh37Js2i8QJVuYeq/LPjJPsUZoQwdCsxPHuBPlBPk8RUiZdVe08Ts+Jz8QAeH7x38HUZyLuL/MHtCN4g =="), true);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table_received_as_input = tableClient.GetTableReference(table_name);
            await table_received_as_input.CreateIfNotExistsAsync();

            if (table_name == "UsersTable")
            {


                TableQuery<UserInUsersTable> idQuery = new TableQuery<UserInUsersTable>()
.Where(TableQuery.GenerateFilterCondition(column_name, QueryComparisons.Equal, field));
                TableQuerySegment<UserInUsersTable> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                UserInUsersTable userInUsersTable = queryResult.FirstOrDefault();
                TableOperation deleteOperation = TableOperation.Delete(userInUsersTable);
                await table_received_as_input.ExecuteAsync(deleteOperation);
                return userInUsersTable.RowKey+"$";
            }
            else if (table_name == "FridgesTable")
            {

                TableQuery<Fridge> idQuery = new TableQuery<Fridge>()
    .Where(TableQuery.GenerateFilterCondition(column_name, QueryComparisons.Equal, field));
                TableQuerySegment<Fridge> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                Fridge fridge = queryResult.FirstOrDefault();
                TableOperation deleteOperation = TableOperation.Delete(fridge);
                await table_received_as_input.ExecuteAsync(deleteOperation);
                return fridge.RowKey + "$";
            }
            else if (table_name.EndsWith("Users"))
            {

                TableQuery<UserInFridge> idQuery = new TableQuery<UserInFridge>()
                    .Where(TableQuery.GenerateFilterCondition(column_name, QueryComparisons.Equal, field));
                TableQuerySegment<UserInFridge> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                UserInFridge userInFridge = queryResult.FirstOrDefault();
                TableOperation deleteOperation = TableOperation.Delete(userInFridge);
                await table_received_as_input.ExecuteAsync(deleteOperation);
                return userInFridge + "$";

            }
            else if (table_name.EndsWith("Shopping"))
            {

                TableQuery<productInShoppingList> idQuery = new TableQuery<productInShoppingList>()
 .Where(TableQuery.GenerateFilterCondition(column_name, QueryComparisons.Equal, field));
                TableQuerySegment<productInShoppingList> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                productInShoppingList productInShoppingList = queryResult.FirstOrDefault();
                TableOperation deleteOperation = TableOperation.Delete(productInShoppingList);
                await table_received_as_input.ExecuteAsync(deleteOperation);
                return productInShoppingList.RowKey + "$";
            }

            else if (table_name.EndsWith("Content"))
            {
                TableQuery<Product> idQuery = new TableQuery<Product>()
    .Where(TableQuery.GenerateFilterCondition(column_name, QueryComparisons.Equal, field));
                TableQuerySegment<Product> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                Product productInTable = queryResult.FirstOrDefault();
                TableOperation deleteOperation = TableOperation.Delete(productInTable);
                await table_received_as_input.ExecuteAsync(deleteOperation);
                return productInTable.RowKey + "$" + productInTable.count;

            }
            return field;
        }


        //input: product$count$qr   maybe only product and count

        [FunctionName("add-to-table")]
        public static async Task<string> AddToTable(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "add-to-table/{id}")] HttpRequestMessage request,
[Table(c_refrigiratorTableName)] CloudTable cloudTable,
[SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
string id,
ILogger log)
        {
            string remaining_text = id;
            int end_of_table_type = remaining_text.IndexOf('$');
            string table_type = remaining_text.Substring(0, end_of_table_type);
            remaining_text = id.Substring(end_of_table_type + 1);
            int end_of_table_name = remaining_text.IndexOf('$');
            string table_name = remaining_text.Substring(0, end_of_table_name);

            var storageAccount = new CloudStorageAccount(new StorageCredentials("skeletonstorage9", "JkIOaYdh37Js2i8QJVuYeq/LPjJPsUZoQwdCsxPHuBPlBPk8RUiZdVe08Ts+Jz8QAeH7x38HUZyLuL/MHtCN4g =="), true);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table_received_as_input = tableClient.GetTableReference(table_name);
            await table_received_as_input.CreateIfNotExistsAsync();


            if (table_type == "contentWithQR")
            {
                remaining_text = remaining_text.Substring(end_of_table_name + 1);
                int end_of_product = remaining_text.IndexOf('$');
                string product = remaining_text.Substring(0, end_of_product);
                remaining_text = remaining_text.Substring(end_of_product + 1);
                int end_of_count = remaining_text.IndexOf('$');
                string count = remaining_text.Substring(0, end_of_count);
                remaining_text = remaining_text.Substring(end_of_count + 1);
                string qr = remaining_text;
                Product product_to_insert = new Product();
                product_to_insert.count = int.Parse(count);
                product_to_insert.qr = qr;
                product_to_insert.RowKey = product;
                product_to_insert.PartitionKey = product;
                TableOperation insertOperation = TableOperation.InsertOrReplace(product_to_insert);
                await table_received_as_input.ExecuteAsync(insertOperation);

            }
            else if (table_type == "contentWithoutQR")
            {
                remaining_text = remaining_text.Substring(end_of_table_name + 1);
                int end_of_product = remaining_text.IndexOf('$');
                string product = remaining_text.Substring(0, end_of_product);
                string count = remaining_text.Substring(end_of_product + 1);
                Product product_to_insert = new Product();
                product_to_insert.count = int.Parse(count);
                product_to_insert.qr = null;
                product_to_insert.RowKey = product;
                product_to_insert.PartitionKey = product;
                TableQuery<Product> idQuery = new TableQuery<Product>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, product));
                TableQuerySegment<Product> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                Product productInTable = queryResult.FirstOrDefault();
                product_to_insert.count = product_to_insert.count + productInTable.count;
                product_to_insert.qr = productInTable.qr;
                TableOperation insertOperation = TableOperation.InsertOrReplace(product_to_insert);
                await table_received_as_input.ExecuteAsync(insertOperation);

            }
            else if (table_type == "shopping")
            {
                remaining_text = remaining_text.Substring(end_of_table_name + 1);
                int end_of_product = remaining_text.IndexOf('$');
                string product = remaining_text.Substring(0, end_of_product);
                string count = remaining_text.Substring(end_of_product + 1);
                productInShoppingList product_to_insert = new productInShoppingList();
                product_to_insert.count = int.Parse(count);
                product_to_insert.RowKey = product;
                product_to_insert.PartitionKey = product;
                TableOperation insertOperation = TableOperation.InsertOrReplace(product_to_insert);
                await table_received_as_input.ExecuteAsync(insertOperation);

            }
            else if (table_type == "usersInFridge") //tablename$username$identifier
            {
                remaining_text = remaining_text.Substring(end_of_table_name + 1);
                int end_of_username = remaining_text.IndexOf('$');
                string username = remaining_text.Substring(0, end_of_username);
                string identifier = remaining_text.Substring(end_of_username + 1);
                UserInFridge user = new UserInFridge();
                user.RowKey = username;
                user.PartitionKey = username;
                user.Identifier = identifier;
                TableOperation insertOperation = TableOperation.InsertOrReplace(user);
                await table_received_as_input.ExecuteAsync(insertOperation);


            }
            else if (table_type == "allUsers")
            {
                remaining_text = remaining_text.Substring(end_of_table_name + 1);
                string before = remaining_text;
                int end_of_username = remaining_text.IndexOf('$');
                string username = remaining_text.Substring(0, end_of_username);
                string password = remaining_text.Substring(end_of_username + 1);
                UserInUsersTable user = new UserInUsersTable();
                user.RowKey = username;
                user.PartitionKey = username;
                user.Password = password;
                TableOperation insertOperation = TableOperation.InsertOrReplace(user);
                await table_received_as_input.ExecuteAsync(insertOperation);
            }
            else if (table_type == "allRefrigirators") //tablename$fridgename$fridgeid
            {
                remaining_text = remaining_text.Substring(end_of_table_name + 1);
                int end_of_fridge_name = remaining_text.IndexOf('$');
                string fridge_name = remaining_text.Substring(0, end_of_fridge_name);
                string fridge_id = remaining_text.Substring(end_of_fridge_name + 1);
                Fridge fridge = new Fridge();
                fridge.RowKey = fridge_name;
                fridge.PartitionKey = fridge_name;
                fridge.Id = fridge_id;
                TableOperation insertOperation = TableOperation.InsertOrReplace(fridge);
                await table_received_as_input.ExecuteAsync(insertOperation);
            }
            return table_type + " " + table_name;
        }



        [FunctionName("new-tables")]  //tablename
        public static async Task<string> NewTables(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "new-tables/{id}")] HttpRequestMessage request,
[Table(c_refrigiratorTableName)] CloudTable cloudTable,
[SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
string id,
ILogger log)
        {
            string table_name = id;
            var storageAccount = new CloudStorageAccount(new StorageCredentials("skeletonstorage9", "JkIOaYdh37Js2i8QJVuYeq/LPjJPsUZoQwdCsxPHuBPlBPk8RUiZdVe08Ts+Jz8QAeH7x38HUZyLuL/MHtCN4g =="), true);
            var tableClient = storageAccount.CreateCloudTableClient();
            var content_table = tableClient.GetTableReference(table_name + "Content");
            var shopping_table = tableClient.GetTableReference(table_name + "Shopping");
            var users_table = tableClient.GetTableReference(table_name + "Users");
            var last_activity_table = tableClient.GetTableReference(table_name + "LastActivity");
            await content_table.CreateIfNotExistsAsync();
            await shopping_table.CreateIfNotExistsAsync();
            await users_table.CreateIfNotExistsAsync();
            await last_activity_table.CreateIfNotExistsAsync();

            return table_name;
        }




        [FunctionName("get-column")]  //tablename$columnname
        public static async Task<List<string>> GetColumn(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-column/{id}")] HttpRequestMessage request,
[Table(c_refrigiratorTableName)] CloudTable cloudTable,
[SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
string id,
ILogger log)
        {
            string remaining_text = id;
            int end_of_table_name = remaining_text.IndexOf('$');
            string table_name = remaining_text.Substring(0, end_of_table_name);
            string column_name = remaining_text.Substring(end_of_table_name + 1);
            var storageAccount = new CloudStorageAccount(new StorageCredentials("skeletonstorage9", "JkIOaYdh37Js2i8QJVuYeq/LPjJPsUZoQwdCsxPHuBPlBPk8RUiZdVe08Ts+Jz8QAeH7x38HUZyLuL/MHtCN4g =="), true);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table_received_as_input = tableClient.GetTableReference(table_name);
            await table_received_as_input.CreateIfNotExistsAsync();

            List<string> column_content = new List<string>();
            

            if (table_name == "UsersTable")
            {
                TableQuery<UserInUsersTable> idQuery = new TableQuery<UserInUsersTable>()
.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "0"));
                TableQuerySegment<UserInUsersTable> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return column_content;
                }
                foreach (UserInUsersTable p in queryResult)
                {
                    try
                    {
                        if (column_name == "RowKey")
                        {
                            column_content.Add(p.RowKey);
                        }
                        if (column_name == "Password")
                        {
                            column_content.Add(p.Password);
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            else if (table_name == "FridgesTable")
            {
                TableQuery<Fridge> idQuery = new TableQuery<Fridge>()
.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "0"));
                TableQuerySegment<Fridge> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return column_content;
                }
                foreach (Fridge p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            column_content.Add(p.RowKey);
                        }
                        if (column_name == "Id")
                        {
                            column_content.Add(p.Id);
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            else if (table_name.EndsWith("Users"))
            {
                TableQuery<UserInFridge> idQuery = new TableQuery<UserInFridge>()
.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "0"));
                TableQuerySegment<UserInFridge> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return column_content;
                }
                foreach (UserInFridge p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            column_content.Add(p.RowKey);
                        }
                        if (column_name == "Identifier")
                        {
                            column_content.Add(p.Identifier);
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            else if (table_name.EndsWith("Content"))
            {
                TableQuery<Product> idQuery = new TableQuery<Product>()
   .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "0"));
                TableQuerySegment<Product> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return column_content;
                }
                foreach (Product p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            column_content.Add(p.RowKey);
                        }
                        if (column_name == "count")
                        {
                            column_content.Add(p.count.ToString());
                        }
                        if ("qr" == column_name)
                        {
                            column_content.Add(p.qr);
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }

            }
            else if (table_name.EndsWith("Shopping"))
            {
                TableQuery<productInShoppingList> idQuery = new TableQuery<productInShoppingList>()
.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "0"));
                TableQuerySegment<productInShoppingList> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return column_content;
                }
                foreach (productInShoppingList p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            column_content.Add(p.RowKey);
                        }
                        if (column_name == "count")
                        {
                            column_content.Add(p.count.ToString());
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }


            else if (table_name.EndsWith("Activity"))
            {
                TableQuery<RowInLastActivity> idQuery = new TableQuery<RowInLastActivity>()
.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "-1000"));
                TableQuerySegment<RowInLastActivity> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return column_content;
                }
                List<int> rowkeyValue = new List<int>();

                foreach (RowInLastActivity r in queryResult)
                {
                    try
                    {
                        rowkeyValue.Add(int.Parse(r.RowKey));
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                int max = -1000;
                int second = -1000;
                foreach (int value in rowkeyValue)
                {
                    if (value > max)
                    {
                        second = max;
                        max = value;
                    }
                    else if (value > second)
                    {
                        second = value;
                    }
                }
                string firstString = "";
                string secondString = "";
                string thirdString = "";
                foreach (RowInLastActivity r in queryResult)
                {
                    try
                    {
                        if (column_name == "content" && int.Parse(r.RowKey)==max)
                        {
                            firstString = r.content;
                        }
                        else if (column_name == "content" && int.Parse(r.RowKey) == second)
                        {
                            secondString = r.content;
                        }
                        else if (column_name == "content")
                        {
                            thirdString = r.content;
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                if (thirdString.Length > 0)
                {
                    column_content.Add(firstString);
                    column_content.Add(secondString);
                    column_content.Add(thirdString);
                    return column_content;
                }
                else if (secondString.Length > 0)
                {
                    column_content.Add(firstString);
                    column_content.Add(secondString);
                    return column_content;
                }
                else
                {
                    column_content.Add(firstString);
                    return column_content;
                }
            }
            return column_content;
        }

        [FunctionName("get-value")]  //tablename$columnname$rowname
        public static async Task<string> GetValue(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-value/{id}")] HttpRequestMessage request,
[Table(c_refrigiratorTableName)] CloudTable cloudTable,
[SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
string id,
ILogger log)
        {
            string remaining_text = id;
            int end_of_table_name = remaining_text.IndexOf('$');
            string table_name = remaining_text.Substring(0, end_of_table_name);
            remaining_text = remaining_text.Substring(end_of_table_name + 1);
            int end_of_column_name = remaining_text.IndexOf('$');
            string column_name = remaining_text.Substring(0, end_of_column_name);
            string row_key_content = remaining_text.Substring(end_of_column_name + 1);
            var storageAccount = new CloudStorageAccount(new StorageCredentials("skeletonstorage9", "JkIOaYdh37Js2i8QJVuYeq/LPjJPsUZoQwdCsxPHuBPlBPk8RUiZdVe08Ts+Jz8QAeH7x38HUZyLuL/MHtCN4g =="), true);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table_received_as_input = tableClient.GetTableReference(table_name);
            await table_received_as_input.CreateIfNotExistsAsync();

            string result = "";

            if (table_name == "UsersTable")
            {
                TableQuery<UserInUsersTable> idQuery = new TableQuery<UserInUsersTable>()
.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row_key_content));
                TableQuerySegment<UserInUsersTable> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return result;
                }
                foreach (UserInUsersTable p in queryResult)
                {
                    try
                    {
                        if (column_name == "RowKey")
                        {
                            return p.RowKey;
                        }
                        if (column_name == "Password")
                        {
                            return p.Password;
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            else if (table_name == "FridgesTable")
            {
                TableQuery<Fridge> idQuery = new TableQuery<Fridge>()
.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row_key_content));
                TableQuerySegment<Fridge> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return result;
                }
                foreach (Fridge p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            return p.RowKey;
                        }
                        if (column_name == "Id")
                        {
                            return p.Id;
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            else if (table_name.EndsWith("Users"))
            {
                TableQuery<UserInFridge> idQuery = new TableQuery<UserInFridge>()
.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row_key_content));
                TableQuerySegment<UserInFridge> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return result;
                }
                foreach (UserInFridge p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            return p.RowKey;
                        }
                        if (column_name == "Identifier")
                        {
                            return p.Identifier;
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            else if (table_name.EndsWith("Content"))
            {
                TableQuery<Product> idQuery = new TableQuery<Product>()
   .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row_key_content));
                TableQuerySegment<Product> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return result;
                }
                foreach (Product p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            return p.RowKey;
                        }
                        if (column_name == "count")
                        {
                            return p.count.ToString();
                        }
                        if ("qr" == column_name)
                        {
                            return p.qr;
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }

            }
            else if (table_name.EndsWith("Shopping"))
            {
                TableQuery<productInShoppingList> idQuery = new TableQuery<productInShoppingList>()
.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row_key_content));
                TableQuerySegment<productInShoppingList> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return result;
                }
                foreach (productInShoppingList p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            return p.RowKey;
                        }
                        if (column_name == "count")
                        {
                            return p.count.ToString();
                        }

                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            return result;
        }





        [FunctionName("table-exists")]  //tablename$columnname$rowname
        public static async Task<bool> TableExist(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "table-exists/{id}")] HttpRequestMessage request,
[Table(c_refrigiratorTableName)] CloudTable cloudTable,
[SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
string id,
ILogger log)
        {
            string table_name = id;

            var storageAccount = new CloudStorageAccount(new StorageCredentials("skeletonstorage9", "JkIOaYdh37Js2i8QJVuYeq/LPjJPsUZoQwdCsxPHuBPlBPk8RUiZdVe08Ts+Jz8QAeH7x38HUZyLuL/MHtCN4g =="), true);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table_received_as_input = tableClient.GetTableReference(table_name);
            return await table_received_as_input.ExistsAsync();
        }







        [FunctionName("set-value")]  //tablename$columnname$rowname$new value
        public static async Task<string> SetValue(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "set-value/{id}")] HttpRequestMessage request,
    [Table(c_refrigiratorTableName)] CloudTable cloudTable,
    [SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
    string id,
    ILogger log)
        {
            string remaining_text = id;
            int end_of_table_name = remaining_text.IndexOf('$');
            string table_name = remaining_text.Substring(0, end_of_table_name);
            remaining_text = remaining_text.Substring(end_of_table_name + 1);
            int end_of_column_name = remaining_text.IndexOf('$');
            string column_name = remaining_text.Substring(0, end_of_column_name);
            remaining_text = remaining_text.Substring(end_of_column_name + 1);
            int end_of_row_key_content = remaining_text.IndexOf('$');
            string row_key_content = remaining_text.Substring(0, end_of_row_key_content);
            string new_value = remaining_text.Substring(end_of_row_key_content + 1);
            //return table_name + " " + column_name + " " + row_key_content + " " + new_value;
            var storageAccount = new CloudStorageAccount(new StorageCredentials("skeletonstorage9", "JkIOaYdh37Js2i8QJVuYeq/LPjJPsUZoQwdCsxPHuBPlBPk8RUiZdVe08Ts+Jz8QAeH7x38HUZyLuL/MHtCN4g =="), true);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table_received_as_input = tableClient.GetTableReference(table_name);
            await table_received_as_input.CreateIfNotExistsAsync();

            string result = "";

            //stopped here
            if (table_name == "UsersTable")
            {
                TableQuery<UserInUsersTable> idQuery = new TableQuery<UserInUsersTable>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row_key_content));
                TableQuerySegment<UserInUsersTable> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return result;
                }
                foreach (UserInUsersTable p in queryResult)
                {
                    try
                    {
                        if (column_name == "RowKey")
                        {
                            p.RowKey = new_value;
                        }
                        if (column_name == "Password")
                        {
                            p.Password = new_value;
                        }
                        TableOperation insertOperation = TableOperation.InsertOrReplace(p);
                        await table_received_as_input.ExecuteAsync(insertOperation);

                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            else if (table_name == "FridgesTable")
            {
                TableQuery<Fridge> idQuery = new TableQuery<Fridge>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row_key_content));
                TableQuerySegment<Fridge> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return result;
                }
                foreach (Fridge p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            p.RowKey = new_value;
                        }
                        if (column_name == "Id")
                        {
                            p.Id = new_value;
                        }
                        TableOperation insertOperation = TableOperation.InsertOrReplace(p);
                        await table_received_as_input.ExecuteAsync(insertOperation);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            else if (table_name.EndsWith("Users"))
            {
                TableQuery<UserInFridge> idQuery = new TableQuery<UserInFridge>()
.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row_key_content));
                TableQuerySegment<UserInFridge> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return result;
                }
                foreach (UserInFridge p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            p.RowKey = new_value;
                        }
                        if (column_name == "Identifier")
                        {
                            p.Identifier = new_value;
                        }
                        TableOperation insertOperation = TableOperation.InsertOrReplace(p);
                        await table_received_as_input.ExecuteAsync(insertOperation);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            else if (table_name.EndsWith("Content"))
            {
                TableQuery<Product> idQuery = new TableQuery<Product>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row_key_content));
                TableQuerySegment<Product> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return result;
                }
                foreach (Product p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            p.RowKey = new_value;
                        }
                        if (column_name == "count")
                        {
                            p.count = int.Parse(new_value);
                        }
                        if ("qr" == column_name)
                        {
                            p.qr = new_value;
                        }
                        TableOperation insertOperation = TableOperation.InsertOrReplace(p);
                        await table_received_as_input.ExecuteAsync(insertOperation);
                    }
                    catch (ArgumentException)
                    {
                    }
                }

            }
            else if (table_name.EndsWith("Shopping"))
            {
                TableQuery<productInShoppingList> idQuery = new TableQuery<productInShoppingList>()
    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, row_key_content));
                TableQuerySegment<productInShoppingList> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
                if (queryResult == null)
                {
                    return result;
                }
                foreach (productInShoppingList p in queryResult)
                {
                    try
                    {
                        if ("RowKey" == column_name)
                        {
                            p.RowKey = new_value;
                        }
                        if (column_name == "count")
                        {
                            p.count = int.Parse(new_value);
                        }
                        TableOperation insertOperation = TableOperation.InsertOrReplace(p);
                        await table_received_as_input.ExecuteAsync(insertOperation);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            return new_value;
        }




        [FunctionName("add-to-last-activity-list")]  //tablename$username$actionName$productName
        public static async Task<string> AddToLastActivityList(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "add-to-last-activity-list/{id}")] HttpRequestMessage request,
    [Table(c_refrigiratorTableName)] CloudTable cloudTable,
    [SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
    string id,
    ILogger log)
        {
            string remaining_text = id;
            int end_of_table_name = remaining_text.IndexOf('$');
            string table_name = remaining_text.Substring(0, end_of_table_name);
            remaining_text = remaining_text.Substring(end_of_table_name + 1);
            int end_of_user_name = remaining_text.IndexOf('$');
            string user_name = remaining_text.Substring(0, end_of_user_name);
            remaining_text = remaining_text.Substring(end_of_user_name + 1);
            int end_of_action_name = remaining_text.IndexOf('$');
            string action_name = remaining_text.Substring(0, end_of_action_name);
            string product_name = remaining_text.Substring(end_of_action_name + 1);
            //return table_name + " " + column_name + " " + row_key_content + " " + new_value;
            var storageAccount = new CloudStorageAccount(new StorageCredentials("skeletonstorage9", "JkIOaYdh37Js2i8QJVuYeq/LPjJPsUZoQwdCsxPHuBPlBPk8RUiZdVe08Ts+Jz8QAeH7x38HUZyLuL/MHtCN4g =="), true);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table_received_as_input = tableClient.GetTableReference(table_name);
            await table_received_as_input.CreateIfNotExistsAsync();

            string result = "";


            TableQuery<RowInLastActivity> idQuery = new TableQuery<RowInLastActivity>()
     .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "aaa"));
            TableQuerySegment<RowInLastActivity> queryResult = await table_received_as_input.ExecuteQuerySegmentedAsync(idQuery, null);
            if (queryResult == null)
            {
                return result;
            }
            int min_counter = -1;
            int max_counter = -1;
            int number_of_rows = 0;
            foreach (RowInLastActivity r in queryResult)
            {
                number_of_rows += 1;
                if (min_counter == -1)
                {
                    min_counter = int.Parse(r.RowKey);
                }
                if (max_counter == -1)
                {
                    max_counter = int.Parse(r.RowKey);
                }
                if (int.Parse(r.RowKey) < min_counter)
                {
                    min_counter = int.Parse(r.RowKey);
                }
                if (int.Parse(r.RowKey) > max_counter)
                {
                    max_counter = int.Parse(r.RowKey);
                }
            }

            result +=min_counter+" "+max_counter+ " " + number_of_rows + " ";
            if (number_of_rows >= 3)
            {
                foreach (RowInLastActivity r in queryResult)
                {
                    if (int.Parse(r.RowKey) == min_counter)
                    {
                        TableOperation deleteOperation = TableOperation.Delete(r);
                        await table_received_as_input.ExecuteAsync(deleteOperation);
                    }
                }
            }
            RowInLastActivity rowInLastActivity = new RowInLastActivity();
            if (max_counter == -1)
            {
                max_counter = 0;
            }
            int counter = max_counter + 1;
            string forth_word = "from";
            if (action_name.Contains("added"))
            {
                forth_word = "to";
            }
            rowInLastActivity.content = user_name + " " + action_name + " " + product_name + " " + forth_word + " fridge";
            rowInLastActivity.RowKey = counter.ToString();
            rowInLastActivity.PartitionKey = counter.ToString();
            TableOperation insertOperation = TableOperation.Insert(rowInLastActivity);
            await table_received_as_input.ExecuteAsync(insertOperation);
            return result;



        }



        // refrigirtor variables:

        private const string c_refrigiratorTableName = "refrigiratorContentTable2";
        [FunctionName("negotiate")]
        public static async Task<SignalRConnectionInfo> NegotiateConnection(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage request,
                    ILogger log)
        {
            try
            {
                ConnectionRequest connectionRequest = await ExtractContent<ConnectionRequest>(request);
                log.LogInformation($"request is:<{request}>");
                //  log.LogInformation($"Negotiating connection for user: <{connectionRequest.UserId}>.");
                //   log.LogInformation(Environment.GetEnvironmentVariable(c_signalRConnectionString));
                string clientHubUrl = SignalR.GetClientHubUrl(c_eventHubName);
                string accessToken = SignalR.GenerateAccessToken(clientHubUrl);
                // string clientHubUrl = "abc";
                // string accessToken = "abc";
                return new SignalRConnectionInfo { AccessToken = accessToken, Url = clientHubUrl };
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to negotiate connection.");
                throw;
            }
        }


        [FunctionName("MessageReceiver")]
        public static async Task MessageReceiver(
            [EventHubTrigger(c_eventHubName, Connection = c_eventHubConnectionString)]EventData message,
            [Table(c_tableName)] CloudTable cloudTable,
            [SignalR(HubName = c_eventHubName, ConnectionStringSetting = c_signalRConnectionString)] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Incoming request to {nameof(MessageReceiver)}");
                Counter cloudcounter = await GetOrCreateCounter(cloudTable, GLOBAL_COUNTER);
                log.LogInformation("counter=" + cloudcounter.Count);
                cloudcounter.Count++;
                TableOperation updateOperation = TableOperation.Replace(cloudcounter);
                await cloudTable.ExecuteAsync(updateOperation);

                /*"brodcast the message to all connected client applications"*/
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "CounterUpdate",
                        Arguments = new object[] { cloudcounter }
                    }
                    );
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Exception in {nameof(MessageReceiver)}: {ex.Message}");
            }
        }


        [FunctionName("update-counter")]
        public static async Task<int> UpdateCounter(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage request,
           [Table(c_tableName)] CloudTable cloudTable,
           [SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
           ILogger log)
        {
            log.LogInformation("Updating counter.");

            Counter counterRequest = await ExtractContent<Counter>(request);

            Counter cloudCounter = await GetOrCreateCounter(cloudTable, GLOBAL_COUNTER);
            cloudCounter.Count++;
            TableOperation updateOperation = TableOperation.Replace(cloudCounter);
            await cloudTable.ExecuteAsync(updateOperation);

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "CounterUpdate",
                    Arguments = new object[] { cloudCounter }
                });
            return cloudCounter.Count;
        }

        [FunctionName("get-counter")]
        public static async Task<int> GetCounter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-counter/{id}")] HttpRequestMessage request,
            [Table(c_tableName)] CloudTable cloudTable,
            [SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
            string id,
            ILogger log)
        {
            log.LogInformation("getting counter.");

            Counter counterRequest = await ExtractContent<Counter>(request);

            Counter cloudCounter = await GetOrCreateCounter(cloudTable, GLOBAL_COUNTER);
            TableOperation updateOperation = TableOperation.Replace(cloudCounter);
            await cloudTable.ExecuteAsync(updateOperation);

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "CounterUpdate",
                    Arguments = new object[] { cloudCounter }
                });
            return cloudCounter.Count;
        }

        [FunctionName("get-devices")]
        public static async Task<Device[]> getDevices(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-devices")] HttpRequestMessage request,
            [Table(c_tableName)] CloudTable cloudTable,
            [SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            var listofdevices = new List<Device>();
            var listOfStrings = new List<string>();
            RegistryManager registry = RegistryManager.CreateFromConnectionString("HostName=skeleton-michael.azure-devices.net;SharedAccessKeyName=registryRead;SharedAccessKey=I0trOFz8zhHCz6n5/aD33jhykgMBONNzfWES5lk/wGU=");
            var query = registry.CreateQuery("SELECT * FROM devices");
            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync();
                foreach (var twin in page)
                {

                    Device d = new Device(twin.DeviceId);
                    listofdevices.Add(d);
                }
            }
            Device[] devices = listofdevices.ToArray();
            //  var listofdevices2 = new List<Device>();
            //   Device[] devices2 = listofdevices2.ToArray();
            return devices;

        }
        [FunctionName("cloud-to-device")]
        public static async Task<int> CloudToDevice(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "cloud-to-device")] HttpRequestMessage request,
        [Table(c_tableName)] CloudTable cloudTable,
        [SignalR(HubName = c_eventHubName)] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
        {
            //Console.WriteLine("Send Cloud-to-Device message\n");
            serviceClient = ServiceClient.CreateFromConnectionString(ctd_connectionString);
            //Console.WriteLine("Press any key to send a C2D message.");
            // Console.ReadLine();
            SendCloudToDeviceMessageAsync().Wait();
            return 0;
        }
        private async static Task SendCloudToDeviceMessageAsync()
        {
            var commandMessage = new
             Message(Encoding.ASCII.GetBytes("Cloud to device message."));
            await serviceClient.SendAsync("MyCDevice", commandMessage);
        }

        private static async Task<T> ExtractContent<T>(HttpRequestMessage request)
        {
            string connectionRequestJson = await request.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(connectionRequestJson);
        }

        private static async Task<Counter> GetOrCreateCounter(CloudTable cloudTable, int counterId)
        {
            TableQuery<Counter> idQuery = new TableQuery<Counter>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, counterId.ToString()));

            TableQuerySegment<Counter> queryResult = await cloudTable.ExecuteQuerySegmentedAsync(idQuery, null);
            Counter cloudCounter = queryResult.FirstOrDefault();
            if (cloudCounter == null)
            {
                cloudCounter = new Counter { Id = counterId };

                TableOperation insertOperation = TableOperation.InsertOrReplace(cloudCounter);
                cloudCounter.PartitionKey = "counter";
                cloudCounter.RowKey = cloudCounter.Id.ToString();
                TableResult tableResult = await cloudTable.ExecuteAsync(insertOperation);
                return await GetOrCreateCounter(cloudTable, counterId);
            }

            return cloudCounter;
        }



    }




}

