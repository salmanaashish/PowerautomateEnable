using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PoweautomateEnable
{
    class Program
    {
        private static string newConnectionReferenceId = "2f14e521-fc5a-4453-a3ae-c67ec710afd1"; // Connection reference ID to update


        // List of Flow IDs to enable, disable, or update connection reference
      private static List<Guid> flowIds = new List<Guid>
    {
       Guid.Parse("98c98f15-a497-ef11-8a69-6045bdceb6c0"),
      Guid.Parse("209a9273-8b97-ef11-8a69-6045bdceb6c0"),
    };

        static async Task Main(string[] args)
        {
            try
            {
                //// Acquire token from Azure AD using MSAL
                //var authResult = await GetAuthenticationToken();
                //if (authResult == null)
                //{
                //    Console.WriteLine("Failed to acquire token.");
                //    return;
                //}

                // Initialize the Dataverse ServiceClient with acquired token
                //var connectionString = $"AuthType=OAuth;Url={environmentUrl};ClientId={clientId};ClientSecret={clientSecret};TenantId={tenantId};";
                var connectionString = "AuthType=OAuth;" +
                               "Url=https://or8.dynamics.com/;" +
                               "Username=oft.com;" +
                               "Password=]3;" +
                               "AppId=0ff6e7ddc27f;" +
                               "RedirectUri=http://localhost;"; // Required for OAuth



                using (var serviceClient = new ServiceClient(connectionString))
                {
                    if (serviceClient.IsReady)
                    {
                        Console.WriteLine("Connected to Dataverse.");

                        // Retrieve flows based on IDs
                        var flows = GetFlowsByIds(serviceClient, flowIds);

                        // Enable, Disable, and Update Connection References
                        foreach (var flow in flows.Entities)
                        {
                            Console.WriteLine($"Processing flow: {flow.GetAttributeValue<string>("name")} with ID: {flow.Id}");

                           // await SetFlowState(serviceClient, flow, false);  
                            await SetFlowState(serviceClient, flow, false); 
                            //Disable the flow - false
                            //Enable the flow - true

                           await UpdateConnectionReference(serviceClient, flow, newConnectionReferenceId);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect to Dataverse.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to get flows based on IDs
        static EntityCollection GetFlowsByIds(ServiceClient serviceClient, List<Guid> flowIds)
        {
            var query = new QueryExpression("workflow")
            {
                ColumnSet = new ColumnSet("workflowid", "name", "statecode"),
                Criteria = new FilterExpression
                {
                    Conditions =
                {
                    new ConditionExpression("workflowid", ConditionOperator.In, flowIds.ToArray())
                }
                }
            };
            return serviceClient.RetrieveMultiple(query);
        }

        // Enable or disable flow
        static async Task SetFlowState(ServiceClient serviceClient, Entity flow, bool enable)
        {
            flow["statecode"] = new OptionSetValue(enable ? 1 : 0); // 0 for Enabled, 1 for Disabled
            serviceClient.Update(flow);
            //Console.ReadLine();
            Console.WriteLine($"Flow '{flow.GetAttributeValue<string>("name")}' with ID '{flow.Id}' has been {(enable ? "enabled" : "disabled")}.");
        }

        // Update connection reference for the flow
        static async Task UpdateConnectionReference(ServiceClient serviceClient, Entity flow, string newConnectionReferenceId)
        {
            flow["connectionreferenceid"] = new EntityReference("connectionreference", Guid.Parse(newConnectionReferenceId));
            serviceClient.Update(flow);

            Console.WriteLine($"Connection reference for flow '{flow.GetAttributeValue<string>("name")}' with ID '{flow.Id}' has been updated.");
        }

        //// MSAL Authentication to get token
        //static async Task<AuthenticationResult> GetAuthenticationToken()
        //{
        //    var app = ConfidentialClientApplicationBuilder.Create(clientId)
        //        .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
        //        .WithClientSecret(clientSecret)
        //        .Build();

        //    try
        //    {
        //        // Request token for the Dataverse scope
        //        return await app.AcquireTokenForClient(new string[] { $"{environmentUrl}/.default" }).ExecuteAsync();
        //    }
        //    catch (MsalException ex)
        //    {
        //        Console.WriteLine($"Authentication failed: {ex.Message}");
        //        return null;
        //    }
        //}
    }

}