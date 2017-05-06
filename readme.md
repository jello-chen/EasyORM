# EasyORM

This is a lightweight ORM framework and Support .NET Framework 4.5 and above.

# Usage

1. Add the reference of `EasyORM.dll` to your project.

2. Import namespace `using EasyORM;`.

3. Configurate the ConnectionString in app/web.config.
  	
4.   	 	 
        var dataContext = new DataContext("SQLServer");  
	    var query = from q in dataContext.Set<T_User>()
	    		    select q;
    	 foreach (var item in query)
    	 {
    	      Console.WriteLine("ID:{0},Name:{1}", item.ID, item.Name);
    	 }