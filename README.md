# Instructions

1. Update the connection string with the credentials and location of your DB
2. Build and run the application.
3. EXPECTED: An DbUpdateConcurrencyException is thrown with the following message: "The record you were trying to delete does not exist"
4. ACTUAL: No exception is thrown and the following message is printed in the console: "the number of state entries written to the database: 1"
