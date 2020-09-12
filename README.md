# FridgeList

**A description of the app as a finite state machine**
The following state machine describes the app flow, and (almost) all the app related classes in our code

![image of state machine](https://github.com/alonrieger/FridgeList2020/blob/master/statemachine.png)

In addition to the view models, the app related code also includes a Fridge.cs class, a ProductInFridge.cs class (both very basic and hold necessary fields), and tableFunctions.cs, which handles all our calls to table related azure functions.

#Cloud Tables
The following are the cloud tables we hold:
1. Users table: holds all the usernames and passwords
2. Fridges table: holds all the fridges and their id's.
3. For each fridge in the fridges table, we have four tables:
3.1. Users table: holds the users logged in to the fridge.
3.2. Content table: holds the products in the fridge, with their respective number of units and Qr code.
3.3. Shopping table: holds the products in the shopping list, with their respective number of units.
3.4. Last activity table: holds strings that describe the last 3 changes made in the fridge.

#tableFunctions.cs
Every operation in the above state machine, which requires access to a table in azure, calls a method from tableFunctions.cs. The methods in tableFunction.cs are mostly generic (we have a few exceptions), and so they are referenced by many functions in the view models, and perform very specific tasks, thus leaving the burden of checking and veriftying stuff to the view models. The following is a list of all the table functions, and their references:

1. delete_from_table(String tableName, String field, String columnName): accesses the table named 'tableName', and deletes the row which holds the name 'field' under column 'colimnName'. References:  
1.1. InsertPageViewModel: InsertToCloudTable: handles the case where a product we inserted is in the shopping list, and thus should be deleted from it (if the amount inserted exceeds the amount in the shopping list).  
1.2. MainViewModel: free_user_from_fridge: handles the case where a user decided to log-out, or leave the fridge. In that case the user is deleted from the list of the fridge users.  
Delete: handles the case where the user deletes a product from the fridge by scanning its Qr code. 
1.3. ShoppingListViewModel: decrementCmd: handles the case where the '-' button is pressed, and the number in the list is reduced to 0.  
1.4. ViewContentViewModel: incrementCmd: handles the case where the '+' button is pressed, and the product is in the shopping list with quantity 1, in which case it should be deleted from the shopping list.  
decrementCmd: handles the case where the '-' button is pressed, and the number in the list is reduced to 0.

2. add_to_last_activity_list(string tableName, string username,string actionAndQuantity,string product): This function takes arguments that describe the changes made by users of the fridge, creates a sentence that describes the change, saves it to the fridge's activity list, and deletes older actions. It is called whenever a user changes the content of the fridge (InsertPageViewModel: insertToCloudTable, MainViewModel: delete, ViewContentViewModel:decrementCmd, incrementCmd).

3. new_tables(String FName, String Uname): This function is called from NewFridgePageViewModel, after the user gives a valid new fridge name. The function generates a random id for the fridge, and adds 'Fname' and the id to the fridges table. Then it creates four tables for the fridge (Users, content,shopping list, and last activity), and adds 'Uname' to the fridge's users table.

4. add_to_table(String table_type, String tableName, List<String> arg): this function inserts the data in 'arg' as a new row in the azure table called 'tablename'. References:
4.1. SignUpPageViewModel: save_password_and_username: after verifying that the username and password are valid, they are added to the users table.
4.2. tableFunctions: add_to_table: the method is called twice- to add the fridge to the fridges table, and to add the user to the fridge's users table.
4.3. JoinFridgePageViewModel: JoinFridgeCmd: once the user submits valid fridge name and id, the user is added to the fridge's users list.
4.4. ShoppingListViewModel: insertCmd: when a user wishes to add a product to the shopping list.
4.5. InsertPageViewModel: insertToCloudTable: after the user scans a product, and assigns a name and number, those are added to the fridge's content table.
  
5. get_column(String tableName, String columnName): this function accesses the column denoted by 'columnName' in the table denoted by 'tableName', and returns it as a list. This function is very useful for cases where we need to verify if something exists. References:
5.1. SignUpPageViewModel: username_already_exists: this functions verifies if the username exists. If it does, the app instructs the user to choose another name. 
5.2. LoginPageViewModel: username_already_exists: same as above. If it does not exists, the user is alerted that there is no such user.
5.3. newFridgePageViewModel: fridgename_already_exists: same as above, only now the function verifies if the fridge exists. If it does, the app instructs the user to choose another name.
5.4. InsertPageViewModel: product_exists: same as above, only now the function verifies if the product already exists in the fridge. If it does, it lets the user know that a Qr code is already assigned to the product.
insertToCloudTable: when a product is inserted to the fridge, the app checks if the product is in the shopping list, and if so it decrements it there. In order to see if the product is in the shopping list, get_column is called.
5.5. ViewContentViewModel: When the page starts, the function is called twice: to get a list of all the products in the fridge, and a list of all its number of units.
incrementCmd: same as in insertToCloudTable.
5.6. ShoppingListViewModel: when the page starts, the function is called in the same logic as in ViewContentViewModel.
5.7. MainViewModel: qr_code_exists: Same as in many functions above, when we wish to delete a product, we have to verify that the Qr code we just scanned is assigned to a product in the fridge.
changesLogIsEmpty, writeChangesInFridge: the function is called in order to access the activity changes. The first function verifies if it is empty (in which case no message would be prompted when the main page opens). The second function accesses all the sentences in order to delete old ones (if there are more than 3), and insert a sentence which explains the last change.
 
 6. get_value(String tableName, String columnName, String rowName)
  

