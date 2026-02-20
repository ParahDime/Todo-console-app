# To do List App

# Overview
	The project acts as a to do list, built on c# alongside SQLite. C# communicates to the database via a package, Microsoft.Data.SQLite.

	Data is already created within seed.sql, formatted via schema.sql, allowing for usable data for the program
# What i learnt
	I've learned a lot from this project, as it's my first time building a project with not only 2 programming langauges, but also communication between them.
	I've also learned about other ways of storing data outside of a file, such as with SQL. Not only this but choosing the right type of SQL for the project was important.
	SQLite was chosen due to being lightweight and serverless, allowing me to get back into understanding SQL without any complexities.

	I've also learned a lot about how industry standards can affect code.
	An example of this is at university learning about camelCase, but not a lot about Pascalcase.
	In addition to this, I also took a lot of time to learn about SQL commands, having not done them in a while
	which lead to learning about how to make proper calls, especially when it comes to authenticating users.
	Another part I've learned is about various methods of password protection, such as privacy

	I've also learned about persistent data within SQL. Part of my program would drop all tables as a way to bypass errors, however this was rectified when i was debugging over persistent data

	I've also learned about how to communicate to a server/back end, using calls
	While not the same as an API, it does help me understand and build back up the knowledge to make calls and interact and modify data

# How it works
	The project starts by beginning to initialise the data. It runs a check within the data file itself. If the tables exist, they aren't dropped or gone over.
	THEN it checks if the tables are already populated. If yes, move on. If not, it accesses seed.sql via Data.cs to populate the data
	Should this fail, the user will be given a reason as to why it can't populate the data or seed the data

	The user is initially given two options, login as well as create an account. 
	To login, the data undergoes hashing and salting in order to protect the data, using that to compare it, rather than decryption. 
