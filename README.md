# IP-API
A simple API created using ASP.NET that exposes an endpoint to retrieve IP details (CountryName, TwoLetterCode, ThreeLetterCode) for a specific IP. The specific requirements for this API can be found in REQUIREMENTS.pdf 

The API utilizes the following NuGet packages:
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
Newtonsoft.Json

## Installation and Setup
1. Clone this repository.
2. Install the required dependencies using Package Manager Console with the following command:
  Install-Package Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Tools, Newtonsoft.Json
3. Run the API locally by opening the solution in Visual Studio and pressing F5
  
## Usage
To get details for a specific IP, make a GET request to the following endpoint: {localhost:port}/api/ip/{ipAddress}
