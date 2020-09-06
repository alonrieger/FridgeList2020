# FridgeList

#A description of the app as a finite state machine
The following state machine describes the app flow, and (almost) all the app related classes in our code

![image of state machine](https://github.com/alonrieger/FridgeList2020/blob/master/statemachine.png)

In addition to the view models, the app related code also includes a Fridge.cs class, a ProductInFridge.cs class (both very basic and hold necessary fields), and tableFunctions.cs, which handles all our calls to table related azure functions.

#tableFunctions.cs
Every operation in the above state machine, which requires access to a table in azure, calls a method from tableFunctions.cs. The methods in tableFunction.cs are mostly generic (we have a few exceptions), and so they are referenced by many functions in the view models, and perform very specific tasks, thus leaving the burden of checking and veriftying stuff to the view models. The following is a list of all the table functions, and their references:

1. delete_from_table(String tableName, String field, String columnName): accesses the table named 'tableName', and deletes the row which holds the name 'field' under column 'colimnName'. References:  
1.1. InsertPageViewModel: InsertToCloudTable: handles the case where a product we inserted is in the shopping list, and thus should be deleted from it (if the amount inserted exceeds the amount in the shopping list).  
1.2. MainViewModel: free_user_from_fridge: handles the case where a user decided to log-out, or leave the fridge. In that case the user is deleted from the list of the fridge users.  
1.3. ShoppingListViewModel: decrementCmd: handles the case where the '-' button is pressed, and the number in the list is reduced to 0.  
1.4. ViewContentViewModel: incrementCmd: handles the case where the '+' button is pressed, and the product is in the shopping list with quantity 1, in which case it should be deleted from the shopping list.  
decrementCmd: handles the case where the '-' button is pressed, and the number in the list is reduced to 0.
