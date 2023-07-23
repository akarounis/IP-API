using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NovibetProject.Models;
using Microsoft.Extensions.Caching.Memory;

namespace NovibetProject.Services
{
    public class IpDetailsService
    {
        private readonly HttpClient client_;
        private readonly IpDetailsContext context_;
        private readonly IMemoryCache cache_;

        public IpDetailsService(IpDetailsContext context, IMemoryCache cache)
        {
            client_ = new HttpClient();
            context_ = context;
            cache_ = cache; ;
        }        

        // Public method used to update IpAddresses in batches of 100 from db
        public async Task UpdateIpDetails()
        {
            // fetch Ip details from db in batches of 100           
            var ips = await context_.IPAddresses.OrderBy(d => d.Id).ToListAsync();
            var batch = new List<IPAddresses>();
            foreach (var ip in ips)
            {
                batch.Add(ip);
                if (batch.Count == 100)
                {
                    await FetchAndUpdateIpDetails(batch);
                    batch.Clear();
                }
            }

            if (batch.Count > 0) await FetchAndUpdateIpDetails(batch);
        }

        // Method that return IpDetails object from given Ip address
        public async Task<JObject> GetIpDetailsAsync(string ipAddress)
        {
            IPAddresses ipDetails;

            // Check if IPAddress exists in cache, if not, try to find it in db
            if(cache_.TryGetValue(ipAddress, out IPAddresses cachedObject))
            {
                ipDetails = cachedObject as IPAddresses;
            }
            else
            {
                ipDetails = await context_.IPAddresses.FirstOrDefaultAsync(d => d.IP == ipAddress);
            }            

            if (ipDetails == null) return null; // No IPAddress with given IP exists

            var country = await context_.Countries.FirstOrDefaultAsync(d => d.Id == ipDetails.CountryId);

            var jsonObject = new JObject
            {
                { "IP", ipAddress },
                { "Country", country.Name },
                { "Country three letter code", country.ThreeLetterCode },
                { "Country two letter code", country.TwoLetterCode }
            };

            return jsonObject;
        }

        public async Task<JArray> GetReportAsync()
        {
            var addressesPerCountry = await context_.AddressesPerCountry.ToListAsync();

            if (addressesPerCountry == null) return null;

            JArray jArray = new JArray();

            foreach(var country in addressesPerCountry)
            {
                jArray.Add(new JObject
                {
                    {"Country", country.CountryName },
                    {"No. of addresses", country.AddressesCount },
                    {"Last updated", country.LastAddressUpdated }
                });
            }

            return jArray;
        }

        // Method that fetches IP details from IP2C and updates db
        private async Task FetchAndUpdateIpDetails(List<IPAddresses> ipDetails)
        {
            var countries = await context_.Countries.OrderBy(d => d.Id).ToListAsync();            

            // Fetch latest IPs from IP2C for the batch of IP addresses
            foreach (var ipAddress in ipDetails)
            {
                var apiUrl = $"https://ip2c.org/{ipAddress.IP}";
                var response = await client_.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var jsonParameters = json.Split(";");

                    if (jsonParameters[0] != "1") continue; // Response for the given IP is not valid

                    var twoLetterCode = jsonParameters[1];
                    var threeLetterCode = jsonParameters[2];
                    var countryName = jsonParameters[3];
                            
                    var country = countries.FirstOrDefault(d => d.TwoLetterCode == twoLetterCode); ;

                    // If the given country doesn't exist in the database, add it
                    if(country == null) {
                        var newCountry = new Countries
                        {
                            Name = countryName.Substring(0, Math.Min(countryName.Length, 50)), // SQL table only allows characters up to 50 characters
                            TwoLetterCode = twoLetterCode,
                            ThreeLetterCode = threeLetterCode,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        countries.Add(newCountry);
                        await context_.SaveChangesAsync();

                        country = newCountry;
                    }

                    // Update ipAddress in the db                    
                    ipAddress.CountryId = country.Id;
                    ipAddress.UpdatedAt = DateTime.UtcNow;

                    cache_.Set(ipAddress.IP, ipAddress);
                }
            }

            await context_.SaveChangesAsync();
        }
    }
}
